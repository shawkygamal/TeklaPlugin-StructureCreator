using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using TeklaPlugin.Services.Piles.Models;

namespace TeklaPlugin.Services.Piles
{
    /// <summary>
    /// Creates reinforcement for circular piles.
    /// 
    /// All radii computed directly from:
    ///   R = pile radius, c = cover, s = spiral dia, d = main bar dia, sp = spacer dia
    /// </summary>
    public class PileReinforcementService
    {
        private readonly Model _model;
        private const int Seg = 36;

        public PileReinforcementService(Model model)
        {
            _model = model;
        }

        public void CreateReinforcement(Beam pile, double pileDiameter, PileReinforcementParameters p)
        {
            TransformationPlane saved = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
                new TransformationPlane(pile.GetCoordinateSystem()));

            try
            {
                Solid solid = pile.GetSolid();
                if (solid == null) return;

                double minX = solid.MinimumPoint.X;
                double maxX = solid.MaximumPoint.X;
                double R = pileDiameter / 2.0;

                int[] bars = p.GetBarsPerLayerArray();
                int n = bars.Length;

                // ── 1. Spiral (continuous helix) ──
                double spiralR = R - p.Cover - p.SpiralDiameter / 2.0;
                CreateSpiral(pile, spiralR, p.SpiralDiameter, p.SpiralPitch,
                             minX, maxX, "Pile Spiral", 7);

                // ── 2. Main longitudinal bars per layer ──
                for (int k = 0; k < n; k++)
                {
                    double layerR = R - p.Cover - p.SpiralDiameter
                                    - (k + 0.5) * p.MainBarDiameter
                                    - k * p.SpacerDiameter;
                    if (layerR <= 0) break;
                    CreateMainBars(pile, p.MainBarDiameter, bars[k], layerR, minX, maxX, k);
                }

                // ── 3. Spacer hoops between adjacent layers ──
                for (int k = 0; k < n - 1; k++)
                {
                    double spacerR = R - p.Cover - p.SpiralDiameter
                                     - (k + 1) * p.MainBarDiameter
                                     - (k + 0.5) * p.SpacerDiameter;
                    if (spacerR <= 0) break;
                    CreateHoops(pile, spacerR, p.SpacerDiameter, p.SpiralPitch,
                                minX, maxX, "Pile Spacer", 4);
                }

                // ── 4. Circular stirrup inside innermost layer ──
                if (p.CircStirrupEnabled && n > 0)
                {
                    // Center of stirrup is inside the innermost main bars:
                    // lastLayerR = R - Cover - SpiralDia - (n-0.5)*MainBarDia - (n-1)*SpacerDia
                    // move in by MainBarDia/2 + CircStirrupDia/2
                    double lastLayerR = R - p.Cover - p.SpiralDiameter
                                        - (n - 0.5) * p.MainBarDiameter
                                        - (n - 1) * p.SpacerDiameter;
                    double circR = lastLayerR
                                   - p.MainBarDiameter / 2.0
                                   - p.CircStirrupDiameter / 2.0;
                    if (circR > 0)
                    {
                        CreateHoops(pile, circR, p.CircStirrupDiameter, p.CircStirrupPitch,
                                    minX, maxX, "Pile Stirrup", 3);
                    }
                }
            }
            finally
            {
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(saved);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Continuous spiral (multi-turn helix)
        // ─────────────────────────────────────────────────────────────
        private void CreateSpiral(Beam pile, double radius, double barDia,
            double pitch, double minX, double maxX, string name, int cls)
        {
            if (radius <= 0) return;

            int turns = (int)Math.Ceiling((maxX - minX) / pitch);
            int segs = turns * Seg;
            if (segs < 2) return;

            var bar = new SingleRebar();
            bar.Father = pile;
            bar.Name = name;
            bar.Grade = "Undefined";
            bar.Size = barDia.ToString();
            bar.Class = cls;

            var poly = new Polygon();
            double da = 2.0 * Math.PI / Seg;
            double dx = pitch / Seg;

            for (int i = 0; i <= segs; i++)
            {
                double x = minX + i * dx;
                if (x > maxX) break;
                double a = i * da;
                poly.Points.Add(new Point(x, radius * Math.Cos(a), radius * Math.Sin(a)));
            }

            bar.Polygon = poly;
            bar.Insert();
        }

        // ─────────────────────────────────────────────────────────────
        //  Circular hoops via RebarGroup – defines the hoop shape once
        //  and lets Tekla distribute copies along the pile axis.
        // ─────────────────────────────────────────────────────────────
        private void CreateHoops(Beam pile, double radius, double barDia,
            double spacing, double minX, double maxX, string name, int cls)
        {
            if (radius <= 0) return;

            var group = new RebarGroup();
            group.Father = pile;
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

            // Distribution along pile axis (X in local coords)
            group.StartPoint = new Point(minX, 0, 0);
            group.EndPoint = new Point(maxX, 0, 0);

            // Spacing
            group.Spacings.Add(spacing);
            group.SpacingType = RebarGroup.RebarGroupSpacingTypeEnum.SPACING_TYPE_TARGET_SPACE;

            group.Insert();
        }

        // ─────────────────────────────────────────────────────────────
        //  Longitudinal bars in concentric circular layers
        // ─────────────────────────────────────────────────────────────
        private void CreateMainBars(Beam pile, double barDia, int count,
            double layerR, double minX, double maxX, int layerIdx)
        {
            if (count <= 0) return;

            double da = 2.0 * Math.PI / count;

            for (int i = 0; i < count; i++)
            {
                double a = i * da;
                double y = layerR * Math.Cos(a);
                double z = layerR * Math.Sin(a);

                var bar = new SingleRebar();
                bar.Father = pile;
                bar.Name = $"Pile Main L{layerIdx + 1}";
                bar.Grade = "Undefined";
                bar.Size = barDia.ToString();
                bar.Class = 7;

                var poly = new Polygon();
                poly.Points.Add(new Point(minX, y, z));
                poly.Points.Add(new Point(maxX, y, z));
                bar.Polygon = poly;
                bar.Insert();
            }
        }
    }
}
