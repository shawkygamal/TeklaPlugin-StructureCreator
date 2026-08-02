using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using TeklaPlugin.Services.Elevation.Models;

namespace TeklaPlugin.Services.Elevation
{
    /// <summary>
    /// Creates reinforcement for columns (circular and rectangular).
    /// 
    /// Unlike piles which use continuous spiral stirrups, columns use
    /// discrete closed ties/stirrups at regular intervals.
    /// </summary>
    public class ColumnReinforcementService
    {
        private readonly Model _model;
        private const int Seg = 36; // Segments for circular shapes

        public ColumnReinforcementService(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Creates reinforcement for a circular column.
        /// </summary>
        public void CreateCircularReinforcement(Beam column, double diameter, ColumnReinforcementParameters p)
        {
            if (p == null) return;

            TransformationPlane saved = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
                new TransformationPlane(column.GetCoordinateSystem()));

            try
            {
                Solid solid = column.GetSolid();
                if (solid == null) return;

                double minX = solid.MinimumPoint.X;
                double maxX = solid.MaximumPoint.X;

                CreateCircularColumnReinforcement(column, diameter, p, minX, maxX);
            }
            finally
            {
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(saved);
            }
        }

        /// <summary>
        /// Creates reinforcement for a rectangular (lamelar) column.
        /// </summary>
        public void CreateRectangularReinforcement(Beam column, double width, double depth, ColumnReinforcementParameters p)
        {
            if (p == null) return;

            TransformationPlane saved = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
                new TransformationPlane(column.GetCoordinateSystem()));

            try
            {
                Solid solid = column.GetSolid();
                if (solid == null) return;

                double minX = solid.MinimumPoint.X;
                double maxX = solid.MaximumPoint.X;

                CreateRectangularColumnReinforcement(column, width, depth, p, minX, maxX);
            }
            finally
            {
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(saved);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  CIRCULAR COLUMN REINFORCEMENT
        //  
        //  Cross-section layout (outside → inside):
        //    Concrete edge → Cover → External Stirrup (wraps around outside) → Main Bars → Spacer → Inner Layers → Internal Stirrup
        //  
        //  Radii (R = column radius):
        //    Main Bar Layer 0 center = R - Cover - ExtStirrupDia - MainBarDia/2
        //    External Stirrup center = MainBar0_center + MainBarDia/2 + StirrupDia/2 (outside the bars)
        //    Internal Stirrup center = innermost layer center - MainBarDia/2 - IntStirrupDia/2 (inside the bars)
        // ═══════════════════════════════════════════════════════════════════

        private void CreateCircularColumnReinforcement(Beam column, double diameter, ColumnReinforcementParameters p,
            double minX, double maxX)
        {
            double R = diameter / 2.0;
            int[] bars = p.GetBarsPerLayerArray();
            int n = bars.Length;
            if (n == 0) return;

            // Calculate all layer radii first
            var layerRadii = new List<double>();
            for (int k = 0; k < n; k++)
            {
                double layerR = R - p.Cover - p.ExternalStirrupDiameter
                                - p.MainBarDiameter / 2.0
                                - k * (p.MainBarDiameter + p.LayerSpacing);
                if (layerR <= 0) break;
                layerRadii.Add(layerR);
            }

            if (layerRadii.Count == 0) return;

            // ── 1. Main longitudinal bars per layer ──
            for (int k = 0; k < layerRadii.Count; k++)
            {
                CreateCircularMainBars(column, p.MainBarDiameter, bars[k], layerRadii[k],
                    minX, maxX, k, p.MainBarSpliceLength);
            }

            // ── 2. External circular ties (wraps AROUND the outermost main bars) ──
            if (p.ExternalStirrupsEnabled)
            {
                // Stirrup center is outside the main bar: mainBarCenter + mainBarDia/2 + stirrupDia/2
                double tieR = layerRadii[0] + p.MainBarDiameter / 2.0 + p.ExternalStirrupDiameter / 2.0;
                CreateCircularHoops(column, tieR, p.ExternalStirrupDiameter,
                    p.ExternalStirrupSpacing, minX, maxX, "Column Tie", 4);
            }

            // ── 3. Spacer hoops between adjacent layers ──
            for (int k = 0; k < layerRadii.Count - 1; k++)
            {
                // Spacer at midpoint between two layers
                double spacerR = (layerRadii[k] + layerRadii[k + 1]) / 2.0;
                CreateCircularHoops(column, spacerR, p.SpacerDiameter, p.ExternalStirrupSpacing,
                    minX, maxX, "Column Spacer", 5);
            }

            // ── 4. Internal circular stirrups (inside the innermost main bars) ──
            if (p.InternalStirrupsEnabled)
            {
                int lastIdx = layerRadii.Count - 1;
                // Internal stirrup center is inside the innermost main bar
                double internalR = layerRadii[lastIdx] - p.MainBarDiameter / 2.0 - p.InternalStirrupDiameter / 2.0;
                if (internalR > 0)
                {
                    CreateCircularHoops(column, internalR, p.InternalStirrupDiameter,
                        p.InternalStirrupSpacing, minX, maxX, "Column Internal Tie", 3);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  RECTANGULAR COLUMN REINFORCEMENT
        //  
        //  Cross-section layout (outside → inside):
        //    Concrete edge → Cover → External Stirrup → Main Bars → Inner Layers → Internal Stirrup
        //  
        //  The stirrup polygon is NOT drawn at bar centers. Instead, the
        //  straight legs run along the OUTSIDE face of the corner bars, and
        //  the bend radius at each corner wraps around the corner bar.
        //  
        //  Key geometry:
        //    barCenterW/D = center-to-center distance of outermost bars
        //    Stirrup straight legs are at barCenter ± (mainBarDia/2 + stirrupDia/2)
        //    Corner bend radius = mainBarDia/2 + stirrupDia/2 (wraps around bar)
        // ═══════════════════════════════════════════════════════════════════

        private void CreateRectangularColumnReinforcement(Beam column, double width, double depth, ColumnReinforcementParameters p,
            double minX, double maxX)
        {
            int[] bars = p.GetBarsPerLayerArray();
            int n = bars.Length;
            if (n == 0) return;

            // Effective hook length: if user set 0, default to 10 * stirrupDia
            double hookLen = p.HookLength > 0 ? p.HookLength : 10.0 * p.ExternalStirrupDiameter;

            // Calculate all layer dimensions first (center-to-center of main bars)
            var layerW = new List<double>();
            var layerD = new List<double>();

            for (int k = 0; k < n; k++)
            {
                // Center-to-center distance for bars in this layer
                double lw = width - 2 * p.Cover - 2 * p.ExternalStirrupDiameter - p.MainBarDiameter
                            - 2 * k * (p.MainBarDiameter + p.LayerSpacing);
                double ld = depth - 2 * p.Cover - 2 * p.ExternalStirrupDiameter - p.MainBarDiameter
                            - 2 * k * (p.MainBarDiameter + p.LayerSpacing);
                if (lw <= 0 || ld <= 0) break;
                layerW.Add(lw);
                layerD.Add(ld);
            }

            if (layerW.Count == 0) return;

            // ── 1. Main longitudinal bars per layer ──
            for (int k = 0; k < layerW.Count; k++)
            {
                CreateRectangularMainBars(column, p.MainBarDiameter, bars[k],
                    layerW[k], layerD[k], minX, maxX, k, p.MainBarSpliceLength);
            }

            // ── 2. External rectangular ties (wraps AROUND the outermost main bars) ──
            if (p.ExternalStirrupsEnabled)
            {
                // The stirrup must wrap around the outside of the corner bars.
                // Corner bend radius = mainBarDia/2 + stirrupDia/2 (wraps around bar surface)
                double cornerBendR = p.MainBarDiameter / 2.0 + p.ExternalStirrupDiameter / 2.0;

                CreateRectangularTiesWithHooks(column, layerW[0], layerD[0],
                    p.MainBarDiameter, p.ExternalStirrupDiameter, p.ExternalStirrupSpacing,
                    minX, maxX, "Column Tie", 4,
                    p.IncludeSeismicHooks, cornerBendR, hookLen);
            }

            // ── 3. Internal stirrups (inside the innermost main bars) ──
            if (p.InternalStirrupsEnabled)
            {
                int lastIdx = layerW.Count - 1;
                // Internal stirrup wraps inside the innermost main bars
                double intCornerBendR = p.MainBarDiameter / 2.0 + p.InternalStirrupDiameter / 2.0;
                double intHookLen = p.HookLength > 0 ? p.HookLength : 10.0 * p.InternalStirrupDiameter;

                // For internal stirrups, the bar center dimensions are the innermost layer
                double innerBarW = layerW[lastIdx];
                double innerBarD = layerD[lastIdx];

                if (innerBarW > p.MainBarDiameter && innerBarD > p.MainBarDiameter)
                {
                    switch (p.InternalShape)
                    {
                        case InternalStirrupShape.Rectangular:
                            CreateInternalRectangularTies(column, innerBarW, innerBarD,
                                p.MainBarDiameter, p.InternalStirrupDiameter,
                                p.InternalStirrupSpacing, minX, maxX,
                                "Column Internal Tie", 3, intCornerBendR, intHookLen);
                            break;
                        case InternalStirrupShape.Diamond:
                            CreateDiamondTies(column, innerBarW, innerBarD, p.InternalStirrupDiameter,
                                p.InternalStirrupSpacing, minX, maxX, "Column Diamond Tie", 3);
                            break;
                        case InternalStirrupShape.Cross:
                            CreateCrossTies(column, innerBarW, innerBarD, p.InternalStirrupDiameter,
                                p.InternalStirrupSpacing, minX, maxX, "Column Cross Tie", 3);
                            break;
                        case InternalStirrupShape.Circular:
                            // For rectangular columns, use inscribed circle
                            double internalR = Math.Min(innerBarW, innerBarD) / 2.0
                                               - p.MainBarDiameter / 2.0 - p.InternalStirrupDiameter / 2.0;
                            if (internalR > 0)
                            {
                                CreateCircularHoops(column, internalR, p.InternalStirrupDiameter,
                                    p.InternalStirrupSpacing, minX, maxX, "Column Internal Tie", 3);
                            }
                            break;
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CIRCULAR HOOPS (discrete ties, not continuous spiral)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateCircularHoops(Beam column, double radius, double barDia,
            double spacing, double minX, double maxX, string name, int cls)
        {
            if (radius <= 0) return;

            var group = new RebarGroup();
            group.Father = column;
            group.Name = name;
            group.Grade = "Undefined";
            group.Size = barDia.ToString();
            group.Class = cls;

            // Circular polygon (closed ring in Y-Z plane)
            var poly = new Polygon();
            double da = 2.0 * Math.PI / Seg;
            for (int i = 0; i <= Seg; i++)
            {
                double a = i * da;
                poly.Points.Add(new Point(minX,
                    radius * Math.Cos(a),
                    radius * Math.Sin(a)));
            }
            group.Polygons.Add(poly);

            // Bend radii at each polygon corner
            for (int i = 0; i < Seg; i++)
                group.RadiusValues.Add(0.0);

            // Distribution along column axis (X in local coords)
            group.StartPoint = new Point(minX, 0, 0);
            group.EndPoint = new Point(maxX, 0, 0);

            // Spacing
            group.Spacings.Add(spacing);
            group.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            group.Insert();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  RECTANGULAR TIES WITH HOOKS (closed stirrups that wrap AROUND bars)
        //  
        //  The stirrup polygon is drawn so that the straight legs run along
        //  the OUTSIDE face of the corner bars.
        //  
        //  Geometry:
        //    barCenterW/D = center-to-center of main bars in the layer
        //    offset = mainBarDia/2 + stirrupDia/2
        //    Stirrup legs at: ±(barCenterW/2 + offset), ±(barCenterD/2 + offset)
        //    Corner bend radius = mainBarDia/2 + stirrupDia/2 (wraps around bar)
        //  
        //  Hook: A single straight tail extending from the closing corner.
        //    Polygon path:
        //      C1(BL) → C2(BR) → C3(TR) → C4(TL) → C1(BL) → hookTailEnd
        //    The stirrup closes back to C1, then a straight hook tail extends
        //    inward at 135° (seismic) or 90° (standard).
        // ─────────────────────────────────────────────────────────────────────
        private void CreateRectangularTiesWithHooks(Beam column,
            double barCenterW, double barCenterD,
            double mainBarDia, double stirrupDia, double spacing,
            double minX, double maxX, string name, int cls,
            bool seismicHooks, double cornerBendRadius, double hookLength)
        {
            if (barCenterW <= 0 || barCenterD <= 0) return;

            // Offset from bar center to stirrup wire center (outside the bar)
            double offset = mainBarDia / 2.0 + stirrupDia / 2.0;

            // Stirrup rectangle half-dimensions (center of stirrup wire)
            double halfW = barCenterW / 2.0 + offset;
            double halfD = barCenterD / 2.0 + offset;

            // Ensure bend radius wraps around the bar
            double bendR = Math.Max(cornerBendRadius, offset);
            double hookBendR = seismicHooks ? 3.0 * stirrupDia : 2.0 * stirrupDia;

            var group = new RebarGroup();
            group.Father = column;
            group.Name = name;
            group.Grade = "Undefined";
            group.Size = stirrupDia.ToString();
            group.Class = cls;

            var poly = new Polygon();

            // P0: Corner 1 bottom-left (start of stirrup)
            poly.Points.Add(new Point(minX, -halfW, -halfD));
            // P1: Corner 2 bottom-right
            poly.Points.Add(new Point(minX, halfW, -halfD));
            // P2: Corner 3 top-right
            poly.Points.Add(new Point(minX, halfW, halfD));
            // P3: Corner 4 top-left
            poly.Points.Add(new Point(minX, -halfW, halfD));
            // P4: Close back to Corner 1 bottom-left
            poly.Points.Add(new Point(minX, -halfW, -halfD));

            // P5: Hook tail — straight extension inward from Corner 1
            if (seismicHooks)
            {
                // 135° hook: tail goes diagonally inward at 45°
                double diag = hookLength / Math.Sqrt(2.0);
                poly.Points.Add(new Point(minX, -halfW + diag, -halfD + diag));
            }
            else
            {
                // 90° hook: tail goes straight up (along Z) from Corner 1
                poly.Points.Add(new Point(minX, -halfW, -halfD + hookLength));
            }

            group.Polygons.Add(poly);

            // Bend radii: 6 points → 5 bends at vertices P0..P4
            group.RadiusValues.Add(bendR);        // P0: corner 1 (bottom-left / start)
            group.RadiusValues.Add(bendR);        // P1: corner 2 (bottom-right)
            group.RadiusValues.Add(bendR);        // P2: corner 3 (top-right)
            group.RadiusValues.Add(bendR);        // P3: corner 4 (top-left)
            group.RadiusValues.Add(hookBendR);   // P4: closing corner → hook bend

            // Distribution along column axis
            group.StartPoint = new Point(minX, 0, 0);
            group.EndPoint = new Point(maxX, 0, 0);

            group.Spacings.Add(spacing);
            group.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            group.Insert();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  INTERNAL RECTANGULAR TIES (wraps INSIDE the innermost main bars)
        //  
        //  Similar to external ties, but the stirrup legs run along the
        //  INSIDE face of the inner-layer bars.
        //  
        //  Geometry:
        //    offset = mainBarDia/2 + stirrupDia/2
        //    Stirrup legs at: ±(barCenterW/2 - offset), ±(barCenterD/2 - offset)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateInternalRectangularTies(Beam column,
            double barCenterW, double barCenterD,
            double mainBarDia, double stirrupDia, double spacing,
            double minX, double maxX, string name, int cls,
            double cornerBendRadius, double hookLength)
        {
            // Offset from bar center to stirrup wire center (inside the bar)
            double offset = mainBarDia / 2.0 + stirrupDia / 2.0;

            // Stirrup rectangle half-dimensions — INSIDE the bars
            double halfW = barCenterW / 2.0 - offset;
            double halfD = barCenterD / 2.0 - offset;

            if (halfW <= 0 || halfD <= 0) return;

            double bendR = Math.Max(cornerBendRadius, 2.0 * stirrupDia);
            double hookBendR = 2.0 * stirrupDia;

            var group = new RebarGroup();
            group.Father = column;
            group.Name = name;
            group.Grade = "Undefined";
            group.Size = stirrupDia.ToString();
            group.Class = cls;

            var poly = new Polygon();

            // Closed rectangle with a hook tail
            poly.Points.Add(new Point(minX, -halfW, -halfD));
            poly.Points.Add(new Point(minX, halfW, -halfD));
            poly.Points.Add(new Point(minX, halfW, halfD));
            poly.Points.Add(new Point(minX, -halfW, halfD));
            poly.Points.Add(new Point(minX, -halfW, -halfD));  // close

            // Hook tail: 90° straight up from closing corner
            poly.Points.Add(new Point(minX, -halfW, -halfD + hookLength));

            group.Polygons.Add(poly);

            // Bend radii: 6 points → 5 bends
            group.RadiusValues.Add(bendR);
            group.RadiusValues.Add(bendR);
            group.RadiusValues.Add(bendR);
            group.RadiusValues.Add(bendR);
            group.RadiusValues.Add(hookBendR);

            group.StartPoint = new Point(minX, 0, 0);
            group.EndPoint = new Point(maxX, 0, 0);

            group.Spacings.Add(spacing);
            group.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            group.Insert();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DIAMOND TIES (rotated 45° rectangular ties)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateDiamondTies(Beam column, double width, double depth,
            double barDia, double spacing, double minX, double maxX, string name, int cls)
        {
            if (width <= 0 || depth <= 0) return;

            var group = new RebarGroup();
            group.Father = column;
            group.Name = name;
            group.Grade = "Undefined";
            group.Size = barDia.ToString();
            group.Class = cls;

            double halfW = width / 2.0;
            double halfD = depth / 2.0;

            // Diamond polygon (rotated 45° in Y-Z plane)
            var poly = new Polygon();
            poly.Points.Add(new Point(minX, 0, -halfD));      // Bottom
            poly.Points.Add(new Point(minX, halfW, 0));       // Right
            poly.Points.Add(new Point(minX, 0, halfD));       // Top
            poly.Points.Add(new Point(minX, -halfW, 0));      // Left
            poly.Points.Add(new Point(minX, 0, -halfD));      // Close the loop
            group.Polygons.Add(poly);

            // Bend radii at corners
            double bendRadius = 2 * barDia;
            for (int i = 0; i < 4; i++)
                group.RadiusValues.Add(bendRadius);

            // Distribution along column axis
            group.StartPoint = new Point(minX, 0, 0);
            group.EndPoint = new Point(maxX, 0, 0);

            // Spacing
            group.Spacings.Add(spacing);
            group.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            group.Insert();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CROSS TIES (plus-shaped, two perpendicular bars)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateCrossTies(Beam column, double width, double depth,
            double barDia, double spacing, double minX, double maxX, string name, int cls)
        {
            if (width <= 0 || depth <= 0) return;

            double halfW = width / 2.0;
            double halfD = depth / 2.0;

            // Horizontal cross-tie (along Y axis)
            var groupH = new RebarGroup();
            groupH.Father = column;
            groupH.Name = name + " H";
            groupH.Grade = "Undefined";
            groupH.Size = barDia.ToString();
            groupH.Class = cls;

            var polyH = new Polygon();
            polyH.Points.Add(new Point(minX, -halfW, 0));
            polyH.Points.Add(new Point(minX, halfW, 0));
            groupH.Polygons.Add(polyH);

            groupH.StartPoint = new Point(minX, 0, 0);
            groupH.EndPoint = new Point(maxX, 0, 0);
            groupH.Spacings.Add(spacing);
            groupH.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;
            groupH.Insert();

            // Vertical cross-tie (along Z axis)
            var groupV = new RebarGroup();
            groupV.Father = column;
            groupV.Name = name + " V";
            groupV.Grade = "Undefined";
            groupV.Size = barDia.ToString();
            groupV.Class = cls;

            var polyV = new Polygon();
            polyV.Points.Add(new Point(minX, 0, -halfD));
            polyV.Points.Add(new Point(minX, 0, halfD));
            groupV.Polygons.Add(polyV);

            groupV.StartPoint = new Point(minX, 0, 0);
            groupV.EndPoint = new Point(maxX, 0, 0);
            groupV.Spacings.Add(spacing);
            groupV.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;
            groupV.Insert();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CIRCULAR MAIN BARS (longitudinal bars in concentric rings)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateCircularMainBars(Beam column, double barDia, int count,
            double layerR, double minX, double maxX, int layerIdx, double spliceLength)
        {
            if (count <= 0) return;

            double da = 2.0 * Math.PI / count;
            double totalLen = maxX - minX;
            bool useSplice = spliceLength > 0 && spliceLength < totalLen;

            for (int i = 0; i < count; i++)
            {
                double a = i * da;
                double y = layerR * Math.Cos(a);
                double z = layerR * Math.Sin(a);

                if (!useSplice)
                {
                    CreateMainBarSegment(column, barDia, layerIdx, minX, maxX, y, z);
                }
                else
                {
                    double mid = (minX + maxX) / 2.0;
                    double halfLap = spliceLength / 2.0;

                    // Lower bar: full from bottom to mid + half lap
                    CreateMainBarSegment(column, barDia, layerIdx,
                        minX, mid + halfLap, y, z);

                    // Upper bar: from mid - half lap to top
                    CreateMainBarSegment(column, barDia, layerIdx,
                        mid - halfLap, maxX, y, z);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  RECTANGULAR MAIN BARS (longitudinal bars distributed around perimeter)
        // ─────────────────────────────────────────────────────────────────────
        private void CreateRectangularMainBars(Beam column, double barDia, int count,
            double layerW, double layerD, double minX, double maxX, int layerIdx, double spliceLength)
        {
            if (count <= 0) return;

            double halfW = layerW / 2.0;
            double halfD = layerD / 2.0;
            double totalLen = maxX - minX;
            bool useSplice = spliceLength > 0 && spliceLength < totalLen;

            // Distribute bars around the rectangular perimeter
            // Strategy: place bars at corners first, then distribute remaining along edges
            var positions = CalculateRectangularBarPositions(count, halfW, halfD);

            foreach (var pos in positions)
            {
                if (!useSplice)
                {
                    CreateMainBarSegment(column, barDia, layerIdx, minX, maxX, pos.Y, pos.Z);
                }
                else
                {
                    double mid = (minX + maxX) / 2.0;
                    double halfLap = spliceLength / 2.0;

                    // Lower bar
                    CreateMainBarSegment(column, barDia, layerIdx,
                        minX, mid + halfLap, pos.Y, pos.Z);

                    // Upper bar
                    CreateMainBarSegment(column, barDia, layerIdx,
                        mid - halfLap, maxX, pos.Y, pos.Z);
                }
            }
        }

        /// <summary>
        /// Calculates bar positions distributed around a rectangular perimeter.
        /// Places bars at corners first, then distributes remaining bars along edges.
        /// </summary>
        private Point[] CalculateRectangularBarPositions(int count, double halfW, double halfD)
        {
            var positions = new Point[count];

            if (count <= 4)
            {
                // Just corners
                positions[0] = new Point(0, -halfW, -halfD);
                if (count > 1) positions[1] = new Point(0, halfW, -halfD);
                if (count > 2) positions[2] = new Point(0, halfW, halfD);
                if (count > 3) positions[3] = new Point(0, -halfW, halfD);
                return positions;
            }

            // 4 corners + distribute remaining along edges
            int remaining = count - 4;
            double perimeter = 2 * (2 * halfW + 2 * halfD);
            double edgeW = 2 * halfW;
            double edgeD = 2 * halfD;

            // Distribute proportionally to edge lengths
            int barsPerWidthEdge = (int)Math.Round(remaining * edgeW / perimeter / 2.0);
            int barsPerDepthEdge = (remaining - 2 * barsPerWidthEdge) / 2;

            int idx = 0;

            // Corner 1 (-halfW, -halfD)
            positions[idx++] = new Point(0, -halfW, -halfD);

            // Bottom edge (Z = -halfD, Y varies)
            for (int i = 0; i < barsPerWidthEdge; i++)
            {
                double y = -halfW + (i + 1) * edgeW / (barsPerWidthEdge + 1);
                positions[idx++] = new Point(0, y, -halfD);
            }

            // Corner 2 (halfW, -halfD)
            positions[idx++] = new Point(0, halfW, -halfD);

            // Right edge (Y = halfW, Z varies)
            for (int i = 0; i < barsPerDepthEdge; i++)
            {
                double z = -halfD + (i + 1) * edgeD / (barsPerDepthEdge + 1);
                positions[idx++] = new Point(0, halfW, z);
            }

            // Corner 3 (halfW, halfD)
            positions[idx++] = new Point(0, halfW, halfD);

            // Top edge (Z = halfD, Y varies)
            for (int i = 0; i < barsPerWidthEdge; i++)
            {
                double y = halfW - (i + 1) * edgeW / (barsPerWidthEdge + 1);
                positions[idx++] = new Point(0, y, halfD);
            }

            // Corner 4 (-halfW, halfD)
            positions[idx++] = new Point(0, -halfW, halfD);

            // Left edge (Y = -halfW, Z varies)
            while (idx < count)
            {
                int leftBars = count - idx;
                for (int i = 0; i < leftBars; i++)
                {
                    double z = halfD - (i + 1) * edgeD / (leftBars + 1);
                    positions[idx++] = new Point(0, -halfW, z);
                }
            }

            return positions;
        }

        private void CreateMainBarSegment(Beam column, double barDia, int layerIdx,
            double xStart, double xEnd, double y, double z)
        {
            var bar = new SingleRebar();
            bar.Father = column;
            bar.Name = $"Column Main L{layerIdx + 1}";
            bar.Grade = "Undefined";
            bar.Size = barDia.ToString();
            bar.Class = 7;

            var poly = new Polygon();
            poly.Points.Add(new Point(xStart, y, z));
            poly.Points.Add(new Point(xEnd, y, z));
            bar.Polygon = poly;
            bar.Insert();
        }
    }
}
