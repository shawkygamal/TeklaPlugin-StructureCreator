using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;

namespace TeklaPlugin.Services.Foundation
{
    /// <summary>
    /// Creates reinforcement for pad footings.
    /// 
    /// Beam local coordinate system (vertical beam):
    ///   X = along beam axis  →  vertical (MinX = top of footing, MaxX = bottom)
    ///   Y = 1st profile dim  →  widthi  (adjusted to MaxY-2a after skew cut)
    ///   Z = 2nd profile dim  →  Length
    ///   
    /// Cover convention:
    ///   cover = distance from concrete face to outer surface of nearest bar.
    ///   Bar center offset from face = cover + dia/2.
    ///   
    /// Face inset:
    ///   B1/T1 (outer) face inset by sideCover  → bars have end cover.
    ///   B2/T2 (inner) face inset by sideCover + outer dia → bars sit inside outer grid.
    /// </summary>
    public class FoundationReinforcementService
    {
        private readonly Model _model;

        public FoundationReinforcementService(Model model)
        {
            _model = model;
        }

        public void CreateReinforcement(Beam padFooting, GlobalParameters global, FoundationParameters foundation, double a)
        {
            var rebar = foundation.Reinforcement;

            TransformationPlane currentPlane = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane localPlane = new TransformationPlane(padFooting.GetCoordinateSystem());
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localPlane);

            Solid solid = padFooting.GetSolid();
            if (solid == null)
            {
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentPlane);
                return;
            }

            double hookLen = rebar.HookLength;
            double sideCover = rebar.BottomCover; // side cover for hooks and lateral inset

            // B1/T1 = outer layers → face inset by sideCover (end cover from concrete edge)
            // B2/T2 = inner layers → face inset by sideCover + outer layer dia (sits inside B1/T1)
            double outerInset = sideCover;
            double bottomInnerInset = sideCover + rebar.B1.Diameter;
            double topInnerInset = sideCover + rebar.T1.Diameter;

            CreateMainLayer(solid, rebar.B1,
                rebar.BottomCover + rebar.B1.Diameter / 2.0,
                false, a, "B1", hookLen, outerInset);

            CreateMainLayer(solid, rebar.B2,
                rebar.BottomCover + rebar.B1.Diameter + rebar.B2.Diameter / 2.0,
                false, a, "B2", hookLen, bottomInnerInset);

            CreateMainLayer(solid, rebar.T1,
                rebar.TopCover + rebar.T1.Diameter / 2.0,
                true, a, "T1", hookLen, outerInset);

            CreateMainLayer(solid, rebar.T2,
                rebar.TopCover + rebar.T1.Diameter + rebar.T2.Diameter / 2.0,
                true, a, "T2", hookLen, topInnerInset);

            // Side reinforcement (closed stirrups)
            CreateSideReinforcement(solid, rebar, foundation, a);

            // Intermediate mesh layers (Width first / bottom, Length second / top)
            CreateIntermediateLayers(solid, rebar, foundation, a);

            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentPlane);
            _model.CommitChanges();
        }

        // ────────────────────────────────────────────────────────────
        //  Main layers  (B1, B2, T1, T2)  with optional hooks
        // ────────────────────────────────────────────────────────────

        /// <param name="faceInset">How much to shrink the face in both Y and Z from each edge.</param>
        private void CreateMainLayer(Solid solid, RebarLayer layer, double centerOffset,
            bool isTop, double a, string name, double hookLength, double faceInset)
        {
            double minX = solid.MinimumPoint.X;
            double maxX = solid.MaximumPoint.X;
            double minY = solid.MinimumPoint.Y + faceInset;
            double maxY = solid.MaximumPoint.Y - 2 * a - faceInset;
            double minZ = solid.MinimumPoint.Z + faceInset;
            double maxZ = solid.MaximumPoint.Z - faceInset;

            RebarSet rebarSet = new RebarSet();
            rebarSet.RebarProperties.Name = $"Foundation {name}";
            rebarSet.RebarProperties.Grade = "Undefined";
            rebarSet.RebarProperties.BendingRadius = 4 * layer.Diameter;
            rebarSet.RebarProperties.Class = 7;
            rebarSet.RebarProperties.Size = layer.Diameter.ToString();

            bool hasHooks = hookLength > 0;

            // Hook X extents
            double hookHighX, hookLowX;
            if (isTop)
            {
                hookHighX = minX + hookLength;
                hookLowX = minX;
            }
            else
            {
                hookHighX = maxX;
                hookLowX = maxX - hookLength;
            }

            int legOrder = 1;

            // ── First hook face (offset=0: faceInset already provides side cover) ──
            if (hasHooks)
            {
                rebarSet.LegFaces.Add(CreateHookFace(legOrder++, 0,
                    hookHighX, hookLowX, layer.Direction, true, minY, maxY, minZ, maxZ));
            }

            // ── Main horizontal face ──
            var mainFace = new RebarLegFace();
            mainFace.LayerOrderNumber = legOrder++;
            mainFace.Reversed = false;
            mainFace.AdditonalOffset = centerOffset;

            if (isTop)
            {
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, minZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, minZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, maxZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, maxZ), null));
            }
            else
            {
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, minZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, maxZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, maxZ), null));
                mainFace.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, minZ), null));
            }

            rebarSet.LegFaces.Add(mainFace);

            // ── Second hook face (offset=0: faceInset already provides side cover) ──
            if (hasHooks)
            {
                rebarSet.LegFaces.Add(CreateHookFace(legOrder++, 0,
                    hookHighX, hookLowX, layer.Direction, false, minY, maxY, minZ, maxZ));
            }

            // ── Guideline (distribution direction) ──
            double faceX = isTop ? minX : maxX;

            var guideline = new RebarGuideline();
            guideline.Spacing = RebarSpacing.Create(
                RebarSpacing.SpacingType.EXACT_FLEXIBLE_FIRST_AND_LAST,
                new RebarSpacing.Offset(true, 0),
                new RebarSpacing.Offset(true, 0),
                layer.Spacing);

            if (layer.Direction == RebarDirection.Length)
            {
                // Bars run Z → guideline spans Y (inset values already applied)
                guideline.Curve.AddContourPoint(new ContourPoint(new Point(faceX, minY, minZ), null));
                guideline.Curve.AddContourPoint(new ContourPoint(new Point(faceX, maxY, minZ), null));
            }
            else
            {
                // Bars run Y → guideline spans Z (inset values already applied)
                guideline.Curve.AddContourPoint(new ContourPoint(new Point(faceX, minY, minZ), null));
                guideline.Curve.AddContourPoint(new ContourPoint(new Point(faceX, minY, maxZ), null));
            }

            rebarSet.Guidelines.Add(guideline);
            rebarSet.Insert();
        }

        /// <summary>
        /// Creates a hook leg face on a side of the footing.
        /// hookHighX / hookLowX define the X extent of the hook (highX > lowX).
        /// minY/maxY/minZ/maxZ are already inset by faceInset.
        /// </summary>
        private RebarLegFace CreateHookFace(int layerOrder, double sideCover,
            double hookHighX, double hookLowX,
            RebarDirection barDirection, bool isStartHook,
            double minY, double maxY, double minZ, double maxZ)
        {
            var face = new RebarLegFace();
            face.LayerOrderNumber = layerOrder;
            face.AdditonalOffset = sideCover;
            face.Reversed = false;

            if (barDirection == RebarDirection.Length)
            {
                if (isStartHook)
                {
                    // Z = MinZ, normal → +Z (inward)
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, minY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, maxY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, maxY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, minY, minZ), null));
                }
                else
                {
                    // Z = MaxZ, normal → −Z (inward)
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, minY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, minY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, maxY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, maxY, maxZ), null));
                }
            }
            else // Width direction
            {
                if (isStartHook)
                {
                    // Y = MinY, normal → +Y (inward)
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, minY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, minY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, minY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, minY, maxZ), null));
                }
                else
                {
                    // Y = MaxY, normal → −Y (inward)
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, maxY, minZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookHighX, maxY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, maxY, maxZ), null));
                    face.Contour.AddContourPoint(new ContourPoint(new Point(hookLowX, maxY, minZ), null));
                }
            }

            return face;
        }

        // ────────────────────────────────────────────────────────────
        //  Side reinforcement  (closed stirrups around perimeter)
        // ────────────────────────────────────────────────────────────

        private void CreateSideReinforcement(Solid solid, FoundationReinforcementParameters rebar,
            FoundationParameters foundation, double a)
        {
            int sideBarCount = rebar.CalculateSideBarCount(foundation.Height);
            if (sideBarCount <= 0) return;

            double minX = solid.MinimumPoint.X;
            double maxX = solid.MaximumPoint.X;
            double minY = solid.MinimumPoint.Y;
            double maxY = solid.MaximumPoint.Y - 2 * a;
            double minZ = solid.MinimumPoint.Z;
            double maxZ = solid.MaximumPoint.Z;

            double sideCover = rebar.BottomCover;

            double bottomEdge = maxX - rebar.BottomCover - rebar.B1.Diameter - rebar.B2.Diameter;
            double topEdge = minX + rebar.TopCover + rebar.T1.Diameter + rebar.T2.Diameter;

            double guideBottom = bottomEdge - 50;
            double guideTop = topEdge + 50;

            if (guideBottom <= guideTop) return;

            RebarSet rebarSet = new RebarSet();
            rebarSet.RebarProperties.Name = "Foundation Side Reinforcement";
            rebarSet.RebarProperties.Grade = "Undefined";
            rebarSet.RebarProperties.BendingRadius = 4 * rebar.Side.Diameter;
            rebarSet.RebarProperties.Class = 7;
            rebarSet.RebarProperties.Size = rebar.Side.Diameter.ToString();

            // Face 1: front (Z=MinZ), normal +Z
            var face1 = new RebarLegFace();
            face1.LayerOrderNumber = 1;
            face1.AdditonalOffset = sideCover;
            face1.Reversed = false;
            face1.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, minZ), null));
            face1.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, minZ), null));
            face1.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, minZ), null));
            face1.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, minZ), null));
            rebarSet.LegFaces.Add(face1);

            // Face 2: right (Y=MaxY adj), normal −Y
            var face2 = new RebarLegFace();
            face2.LayerOrderNumber = 2;
            face2.AdditonalOffset = sideCover;
            face2.Reversed = false;
            face2.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, minZ), null));
            face2.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, minZ), null));
            face2.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, maxZ), null));
            face2.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, maxZ), null));
            rebarSet.LegFaces.Add(face2);

            // Face 3: back (Z=MaxZ), normal −Z
            var face3 = new RebarLegFace();
            face3.LayerOrderNumber = 3;
            face3.AdditonalOffset = sideCover;
            face3.Reversed = false;
            face3.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, maxZ), null));
            face3.Contour.AddContourPoint(new ContourPoint(new Point(minX, maxY, maxZ), null));
            face3.Contour.AddContourPoint(new ContourPoint(new Point(maxX, maxY, maxZ), null));
            face3.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, maxZ), null));
            rebarSet.LegFaces.Add(face3);

            // Face 4: left (Y=MinY), normal +Y
            var face4 = new RebarLegFace();
            face4.LayerOrderNumber = 4;
            face4.AdditonalOffset = sideCover;
            face4.Reversed = false;
            face4.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, minZ), null));
            face4.Contour.AddContourPoint(new ContourPoint(new Point(minX, minY, maxZ), null));
            face4.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, maxZ), null));
            face4.Contour.AddContourPoint(new ContourPoint(new Point(maxX, minY, minZ), null));
            rebarSet.LegFaces.Add(face4);

            var guideline = new RebarGuideline();
            guideline.Spacing = RebarSpacing.Create(
                RebarSpacing.SpacingType.EXACT_FLEXIBLE_FIRST_AND_LAST,
                new RebarSpacing.Offset(true, 0),
                new RebarSpacing.Offset(true, 0),
                rebar.Side.Spacing);

            guideline.Curve.AddContourPoint(new ContourPoint(new Point(guideBottom, minY, minZ), null));
            guideline.Curve.AddContourPoint(new ContourPoint(new Point(guideTop, minY, minZ), null));

            rebarSet.Guidelines.Add(guideline);
            rebarSet.Insert();
        }

        // ────────────────────────────────────────────────────────────
        //  Intermediate layers  (Width first / bottom, Length second / top)
        // ────────────────────────────────────────────────────────────

        private void CreateIntermediateLayers(Solid solid, FoundationReinforcementParameters rebar,
            FoundationParameters foundation, double a)
        {
            if (rebar.IntermediateLayers == null || rebar.IntermediateLayers.Count == 0) return;

            double minX = solid.MinimumPoint.X;
            double maxX = solid.MaximumPoint.X;
            double sideCover = rebar.BottomCover;

            // Intermediate faces inset by sideCover + max outer dia (always inside outer grid)
            double intInset = sideCover + Math.Max(rebar.B1.Diameter, rebar.T1.Diameter);
            double minY = solid.MinimumPoint.Y + intInset;
            double maxY = solid.MaximumPoint.Y - 2 * a - intInset;
            double minZ = solid.MinimumPoint.Z + intInset;
            double maxZ = solid.MaximumPoint.Z - intInset;

            double bottomLevel = maxX - rebar.BottomCover - rebar.B1.Diameter - rebar.B2.Diameter;
            double topLevel = minX + rebar.TopCover + rebar.T1.Diameter + rebar.T2.Diameter;
            double freeSpace = bottomLevel - topLevel;

            if (freeSpace <= 0) return;

            // Collect layers in order: Width first (near bottom), then Length (near top)
            var orderedDiameters = new List<double>();
            var orderedDirections = new List<RebarDirection>();
            var orderedSpacings = new List<double>();

            foreach (var il in rebar.IntermediateLayers)
            {
                if (il.NumberOfLayers <= 0 || il.Direction != RebarDirection.Width) continue;
                for (int n = 0; n < il.NumberOfLayers; n++)
                {
                    orderedDiameters.Add(il.Diameter);
                    orderedDirections.Add(RebarDirection.Width);
                    orderedSpacings.Add(il.Spacing);
                }
            }

            foreach (var il in rebar.IntermediateLayers)
            {
                if (il.NumberOfLayers <= 0 || il.Direction != RebarDirection.Length) continue;
                for (int n = 0; n < il.NumberOfLayers; n++)
                {
                    orderedDiameters.Add(il.Diameter);
                    orderedDirections.Add(RebarDirection.Length);
                    orderedSpacings.Add(il.Spacing);
                }
            }

            int totalCount = orderedDiameters.Count;
            if (totalCount == 0) return;

            double layerSpacing = freeSpace / (totalCount + 1);

            for (int idx = 0; idx < totalCount; idx++)
            {
                double dia = orderedDiameters[idx];
                RebarDirection dir = orderedDirections[idx];
                double spc = orderedSpacings[idx];

                double xPos = bottomLevel - layerSpacing * (idx + 1);

                RebarSet rebarSet = new RebarSet();
                rebarSet.RebarProperties.Name = $"Foundation Int {dir}-{idx + 1}";
                rebarSet.RebarProperties.Grade = "Undefined";
                rebarSet.RebarProperties.BendingRadius = 4 * dia;
                rebarSet.RebarProperties.Class = 7;
                rebarSet.RebarProperties.Size = dia.ToString();

                var legFace = new RebarLegFace();
                legFace.LayerOrderNumber = 1;
                legFace.AdditonalOffset = 0;
                legFace.Reversed = false;
                legFace.Contour.AddContourPoint(new ContourPoint(new Point(xPos, minY, minZ), null));
                legFace.Contour.AddContourPoint(new ContourPoint(new Point(xPos, minY, maxZ), null));
                legFace.Contour.AddContourPoint(new ContourPoint(new Point(xPos, maxY, maxZ), null));
                legFace.Contour.AddContourPoint(new ContourPoint(new Point(xPos, maxY, minZ), null));
                rebarSet.LegFaces.Add(legFace);

                var guideline = new RebarGuideline();
                guideline.Spacing = RebarSpacing.Create(
                    RebarSpacing.SpacingType.EXACT_FLEXIBLE_FIRST_AND_LAST,
                    new RebarSpacing.Offset(true, 0),
                    new RebarSpacing.Offset(true, 0),
                    spc);

                if (dir == RebarDirection.Length)
                {
                    guideline.Curve.AddContourPoint(new ContourPoint(new Point(xPos, minY, minZ), null));
                    guideline.Curve.AddContourPoint(new ContourPoint(new Point(xPos, maxY, minZ), null));
                }
                else
                {
                    guideline.Curve.AddContourPoint(new ContourPoint(new Point(xPos, minY, minZ), null));
                    guideline.Curve.AddContourPoint(new ContourPoint(new Point(xPos, minY, maxZ), null));
                }

                rebarSet.Guidelines.Add(guideline);
                rebarSet.Insert();
            }
        }
    }
}
