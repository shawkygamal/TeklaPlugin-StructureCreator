using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using Tekla.Structures.Model.UI;
using Material = Tekla.Structures.Model.Material;
using System.Collections.Generic;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;

namespace TeklaPlugin.Services.Foundation
{
    public class FoundationService
    {
        private readonly Model _model;

        public FoundationService(Model model)
        {
            _model = model;
        }

        public void CreateFoundation(TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.FoundationParameters foundation)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;

            // Calculations for the skew
            // a = half of the extra width necessary to make the oblique cut
            double a = Math.Tan(global.SkewAngle * Math.PI / 180) * foundation.Length / 2;
            double widthi = foundation.Width + 2 * a; // imaginary width

            Beam foundationBeam = new Beam();
            foundationBeam.StartPoint = new Point(global.PositionX, global.PositionY, global.PositionZ);
            foundationBeam.EndPoint = new Point(global.PositionX, global.PositionY, global.PositionZ - foundation.Height);
            foundationBeam.Profile = new Profile { ProfileString = $"{widthi}*{foundation.Length}" };
            foundationBeam.Material = new Material { MaterialString = "C50/60" };
            foundationBeam.Class = "8";
            foundationBeam.Position.Rotation = Position.RotationEnum.FRONT;
            foundationBeam.Position.RotationOffset = -global.RotationAngle;
            foundationBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            foundationBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (foundationBeam.Insert())
            {
                CreateCutPlane(foundationBeam, global, foundation, a);
                CreateRebarSet(foundationBeam, global, foundation, a);
                DrawCircles(foundationBeam, a, global, foundation);
            }
        }

        private void CreateCutPlane(Beam foundation, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.FoundationParameters fParams, double a)
        {
            double angleRadians = global.SkewAngle * Math.PI / 180.0;
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180.0;

            // FOR CUTPLANE 1
            double translatedX_cut1 = 0;
            double translatedY_cut1 = -fParams.Width / 2;

            double rotatedX_cut1 = translatedX_cut1 * Math.Cos(rotationAngleRadians) - translatedY_cut1 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut1 = translatedX_cut1 * Math.Sin(rotationAngleRadians) + translatedY_cut1 * Math.Cos(rotationAngleRadians);

            double finalX_cut1 = rotatedX_cut1 + global.PositionX;
            double finalY_cut1 = rotatedY_cut1 + global.PositionY;

            // FOR CUTPLANE 2
            double translatedX_cut2 = 0;
            double translatedY_cut2 = +fParams.Width / 2;

            double rotatedX_cut2 = translatedX_cut2 * Math.Cos(rotationAngleRadians) - translatedY_cut2 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut2 = translatedX_cut2 * Math.Sin(rotationAngleRadians) + translatedY_cut2 * Math.Cos(rotationAngleRadians);

            double finalX_cut2 = rotatedX_cut2 + global.PositionX;
            double finalY_cut2 = rotatedY_cut2 + global.PositionY;

            CutPlane cutPlane = new CutPlane
            {
                Father = foundation,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut1, finalY_cut1, 0),
                    AxisX = new Vector(Math.Cos(angleRadians + rotationAngleRadians), Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 0, 1)
                }
            };

            CutPlane cutPlane2 = new CutPlane
            {
                Father = foundation,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut2, finalY_cut2, 0),
                    AxisX = new Vector(-Math.Cos(angleRadians + rotationAngleRadians), -Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 0, 1)
                }
            };

            cutPlane.Insert();
            cutPlane2.Insert();
        }

        private void CreateRebarSet(Beam padFooting, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.FoundationParameters foundation, double a)
        {
            TransformationPlane currentPlane = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane localPlane = new TransformationPlane(padFooting.GetCoordinateSystem());
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localPlane);

            Solid solid = padFooting.GetSolid();

            RebarSet rebarSet = new RebarSet();
            rebarSet.RebarProperties.Name = "RebarSet Test";
            rebarSet.RebarProperties.Grade = "Undefined";
            rebarSet.RebarProperties.BendingRadius = 50;
            rebarSet.RebarProperties.Class = 7;
            rebarSet.RebarProperties.Size = "25";

            var legFace1 = new RebarLegFace
            {
                LayerOrderNumber = 2,
                AdditonalOffset = -50,
                Reversed = false
            };
            legFace1.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z), null));
            legFace1.Contour.AddContourPoint(new ContourPoint(new Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z), null));
            legFace1.Contour.AddContourPoint(new ContourPoint(new Point(solid.MinimumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z), null));
            legFace1.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z), null));
            rebarSet.LegFaces.Add(legFace1);

            var legFace2 = new RebarLegFace
            {
                LayerOrderNumber = 2,
                AdditonalOffset = 50,
                Reversed = false
            };
            legFace2.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z), null));
            legFace2.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MinimumPoint.Y + 2 * a, solid.MaximumPoint.Z), null));
            legFace2.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z), null));
            legFace2.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z), null));
            rebarSet.LegFaces.Add(legFace2);

            var legFace3 = new RebarLegFace
            {
                LayerOrderNumber = 2,
                AdditonalOffset = 50,
                Reversed = false
            };
            legFace3.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MinimumPoint.Y + 2 * a, solid.MaximumPoint.Z), null));
            legFace3.Contour.AddContourPoint(new ContourPoint(new Point(solid.MinimumPoint.X, solid.MinimumPoint.Y + 2 * a, solid.MaximumPoint.Z), null));
            legFace3.Contour.AddContourPoint(new ContourPoint(new Point(solid.MinimumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z), null));
            legFace3.Contour.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z), null));
            rebarSet.LegFaces.Add(legFace3);

            var guideline = new RebarGuideline();
            guideline.Spacing = RebarSpacing.Create(RebarSpacing.SpacingType.EXACT_FLEXIBLE_FIRST_AND_LAST, new RebarSpacing.Offset(true, 0), new RebarSpacing.Offset(true, 0), 200);

            double l = Math.Cos(global.SkewAngle * Math.PI / 180) * foundation.Width;
            double y = Math.Sin(global.SkewAngle * Math.PI / 180) * l;
            double x = Math.Sqrt(l * l - y * y);

            guideline.Curve.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z), null));
            guideline.Curve.AddContourPoint(new ContourPoint(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a - x, solid.MinimumPoint.Z + y), null));
            rebarSet.Guidelines.Add(guideline);

            rebarSet.Insert();

            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentPlane);
            _model.CommitChanges();
        }

        private void DrawCircles(Beam padFooting, double a, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.FoundationParameters fParams)
        {
            TransformationPlane currentPlane = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane localPlane = new TransformationPlane(padFooting.GetCoordinateSystem());
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localPlane);

            Solid solid = padFooting.GetSolid();
            if (solid == null) return;

            double radius = 100.0;

            Point center1 = new Point(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z));
            DrawCircle(center1, radius);

            double l = Math.Cos(global.SkewAngle * Math.PI / 180) * fParams.Width;
            double y = Math.Sin(global.SkewAngle * Math.PI / 180) * l;
            double x = Math.Sqrt(l * l - y * y);

            Point center2 = new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a - x, solid.MaximumPoint.Z);
            DrawCircle(center2, radius);

            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentPlane);
        }

        private void DrawCircle(Point center, double radius)
        {
            int numPoints = 36;
            double angleStep = 2 * Math.PI / numPoints;

            RebarSet rebarSet = new RebarSet();
            rebarSet.RebarProperties.Name = "RebarSet Cerc";
            rebarSet.RebarProperties.Grade = "Undefined";
            rebarSet.RebarProperties.BendingRadius = 50;
            rebarSet.RebarProperties.Class = 7;
            rebarSet.RebarProperties.Size = "25";

            var guideline = new RebarGuideline();
            guideline.Spacing = RebarSpacing.Create(RebarSpacing.SpacingType.EXACT_FLEXIBLE_LAST, new RebarSpacing.Offset(true, 0), new RebarSpacing.Offset(true, 0), 100);

            for (int i = 0; i < numPoints; i++)
            {
                double angle = i * angleStep;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                Point point = new Point(x, y, center.Z);
                guideline.Curve.AddContourPoint(new ContourPoint(point, null));
            }

            rebarSet.Guidelines.Add(guideline);
            rebarSet.Insert();
            _model.CommitChanges();
        }
    }
}