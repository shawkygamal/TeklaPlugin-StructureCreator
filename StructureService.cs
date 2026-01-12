using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using Tekla.Structures.Model.UI;
using Material = Tekla.Structures.Model.Material;
using System.Collections.Generic;

namespace TeklaPlugin
{
    public class StructureService
    {
        private readonly Model _model;

        public StructureService(Model model)
        {
            _model = model;
        }

        public void CreateFoundation(GlobalParameters global, FoundationParameters foundation)
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
                Example1(foundationBeam, global, foundation, a);
                DrawTwoCircles(foundationBeam, a, global, foundation);
            }
        }

        private void CreateCutPlane(Beam foundation, GlobalParameters global, FoundationParameters fParams, double a)
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

        public void CreateMat(GlobalParameters global, FoundationParameters foundation, MatParameters mat)
        {
            // Calculations for the skew of the mat
            double a_mat = Math.Tan(global.SkewAngle * Math.PI / 180) * (foundation.Length + 2 * mat.Cantilever) / 2;
            double widthi_mat = foundation.Width + 2 * mat.Cantilever + 2 * a_mat;

            Beam matBeam = new Beam();
            matBeam.StartPoint = new Point(global.PositionX, global.PositionY, global.PositionZ - foundation.Height);
            matBeam.EndPoint = new Point(global.PositionX, global.PositionY, global.PositionZ - foundation.Height - mat.Thickness);
            matBeam.Profile = new Profile { ProfileString = $"{widthi_mat + 2 * mat.Cantilever}*{foundation.Length + 2 * mat.Cantilever}" };
            matBeam.Material = new Material { MaterialString = "C12/15" };
            matBeam.Class = "1";
            matBeam.Position.Rotation = Position.RotationEnum.FRONT;
            matBeam.Position.RotationOffset = -global.RotationAngle;
            matBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            matBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (matBeam.Insert())
            {
                CreateCutPlaneMat(matBeam, global, foundation, mat);
            }
        }

        private void CreateCutPlaneMat(Beam matBeam, GlobalParameters global, FoundationParameters foundation, MatParameters mat)
        {
            double angleRadians = global.SkewAngle * Math.PI / 180.0;
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180.0;

            // FOR CUTPLANE 1
            double translatedX_cut1 = 0;
            double translatedY_cut1 = -foundation.Width / 2 - mat.Cantilever;

            double rotatedX_cut1 = translatedX_cut1 * Math.Cos(rotationAngleRadians) - translatedY_cut1 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut1 = translatedX_cut1 * Math.Sin(rotationAngleRadians) + translatedY_cut1 * Math.Cos(rotationAngleRadians);

            double finalX_cut1 = rotatedX_cut1 + global.PositionX;
            double finalY_cut1 = rotatedY_cut1 + global.PositionY;

            // FOR CUTPLANE 2
            double translatedX_cut2 = 0;
            double translatedY_cut2 = +foundation.Width / 2 + mat.Cantilever;

            double rotatedX_cut2 = translatedX_cut2 * Math.Cos(rotationAngleRadians) - translatedY_cut2 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut2 = translatedX_cut2 * Math.Sin(rotationAngleRadians) + translatedY_cut2 * Math.Cos(rotationAngleRadians);

            double finalX_cut2 = rotatedX_cut2 + global.PositionX;
            double finalY_cut2 = rotatedY_cut2 + global.PositionY;

            CutPlane cutPlane = new CutPlane
            {
                Father = matBeam,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut1, finalY_cut1, 0),
                    AxisX = new Vector(Math.Cos(angleRadians + rotationAngleRadians), Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 0, 1)
                }
            };

            CutPlane cutPlane2 = new CutPlane
            {
                Father = matBeam,
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

        public void CreatePiles(GlobalParameters global, FoundationParameters foundation, PileParameters piles)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;
            double a = Math.Tan(global.SkewAngle * Math.PI / 180) * foundation.Length / 2;

            for (int i = 0; i < piles.Columns; i++)
            {
                for (int j = 0; j < piles.Rows; j++)
                {
                    // Unrotated coordinates
                    var pozX = piles.RowDistance * j - ((piles.Rows - 1) * piles.RowDistance) / 2;
                    var pozY = piles.ColumnDistance * i - ((piles.Columns - 1) * piles.ColumnDistance) / 2;
                    var pozZ_upper = global.PositionZ - foundation.Height + piles.EmbeddedLength;
                    var pozZ_lower = global.PositionZ - foundation.Height - piles.Length;

                    // Skewed coefficient
                    var c = 2 * a * pozX / foundation.Length;

                    // Rotated coordinates by the origin
                    var pozX_rotated_o = Math.Cos(rotationAngleRadians) * pozX - Math.Sin(rotationAngleRadians) * (pozY + c);
                    var pozY_rotated_o = Math.Sin(rotationAngleRadians) * pozX + Math.Cos(rotationAngleRadians) * (pozY + c);

                    // Rotated coordinates
                    var pozX_rotated = pozX_rotated_o + global.PositionX;
                    var pozY_rotated = pozY_rotated_o + global.PositionY;

                    Beam pile = new Beam();
                    pile.StartPoint = new Point(pozX_rotated, pozY_rotated, pozZ_upper);
                    pile.EndPoint = new Point(pozX_rotated, pozY_rotated, pozZ_lower);
                    pile.Profile = new Profile { ProfileString = $"D{piles.Diameter}" };
                    pile.Material = new Material { MaterialString = "C50/60" };
                    pile.Class = "8";
                    pile.Position.Rotation = Position.RotationEnum.FRONT;
                    pile.Position.RotationOffset = global.RotationAngle;
                    pile.Position.Plane = Position.PlaneEnum.MIDDLE;
                    pile.Position.Depth = Position.DepthEnum.MIDDLE;
                    pile.Name = $"Pile_{i}_{j}";

                    pile.Insert();
                }
            }
        }

        public void CreateElevationLamelar(GlobalParameters global, LamelarElevationParameters lamelar)
        {
            Beam elevationLamelar = new Beam();
            elevationLamelar.StartPoint = new Point(global.PositionX, global.PositionY, global.PositionZ);
            elevationLamelar.EndPoint = new Point(global.PositionX, global.PositionY, global.PositionZ + lamelar.Height);
            elevationLamelar.Profile = new Profile { ProfileString = $"{lamelar.Width}*{lamelar.Thickness}" };
            elevationLamelar.Material = new Material { MaterialString = "C50/60" };
            elevationLamelar.Class = "8";
            elevationLamelar.Position.Rotation = Position.RotationEnum.FRONT;
            elevationLamelar.Position.RotationOffset = global.RotationAngle;
            elevationLamelar.Position.Plane = Position.PlaneEnum.MIDDLE;
            elevationLamelar.Position.Depth = Position.DepthEnum.MIDDLE;

            elevationLamelar.Insert();
        }

        public void CreateElevationCircular(GlobalParameters global, CircularElevationParameters circular)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;

            for (int i = 0; i < circular.NumberOfColumns; i++)
            {
                // Rotated by the origin coordinates
                var pozX_rotated_o = Math.Cos(rotationAngleRadians) * circular.OffsetX - Math.Sin(rotationAngleRadians) * (circular.OffsetY + circular.DistanceBetweenColumns * i - ((circular.NumberOfColumns - 1) * circular.DistanceBetweenColumns) / 2);
                var pozY_rotated_o = Math.Sin(rotationAngleRadians) * circular.OffsetX + Math.Cos(rotationAngleRadians) * (circular.OffsetY + circular.DistanceBetweenColumns * i - ((circular.NumberOfColumns - 1) * circular.DistanceBetweenColumns) / 2);

                // Rotated coordinates
                var pozX_rotated = pozX_rotated_o + global.PositionX;
                var pozY_rotated = pozY_rotated_o + global.PositionY;

                Beam elevationCircular = new Beam();
                elevationCircular.StartPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ);
                elevationCircular.EndPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ + circular.Height);
                elevationCircular.Profile = new Profile { ProfileString = $"D{circular.Diameter}" };
                elevationCircular.Material = new Material { MaterialString = "C50/60" };
                elevationCircular.Class = "8";
                elevationCircular.Position.Rotation = Position.RotationEnum.FRONT;
                elevationCircular.Position.RotationOffset = global.RotationAngle;
                elevationCircular.Position.Plane = Position.PlaneEnum.MIDDLE;
                elevationCircular.Position.Depth = Position.DepthEnum.MIDDLE;
                elevationCircular.Name = $"Elevation_{i}";

                elevationCircular.Insert();
            }
        }

        public void CreateCap(GlobalParameters global, CapParameters cap, double elevationCircularHeight)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;
            
            // Calculate skew offset based on W (longitudinal dimension)
            double a = Math.Tan(global.SkewAngle * Math.PI / 180) * cap.W / 2;
            double widthi_cap = cap.B + 2 * a; // Adjusted width including skew
            
            // Calculate position with P offset from column center
            // P offset is applied in the local X direction (along W dimension)
            double offsetX = cap.P;
            double offsetY = 0;
            
            // Rotate the offset by the global rotation angle
            double rotatedOffsetX = offsetX * Math.Cos(rotationAngleRadians) - offsetY * Math.Sin(rotationAngleRadians);
            double rotatedOffsetY = offsetX * Math.Sin(rotationAngleRadians) + offsetY * Math.Cos(rotationAngleRadians);
            
            // Final position centered on column with P offset
            double finalX = global.PositionX + rotatedOffsetX;
            double finalY = global.PositionY + rotatedOffsetY;
            double finalZ_top = global.PositionZ + elevationCircularHeight + cap.H;
            double finalZ_bottom = global.PositionZ + elevationCircularHeight;

            Beam capBeam = new Beam();
            capBeam.StartPoint = new Point(finalX, finalY, finalZ_top);
            capBeam.EndPoint = new Point(finalX, finalY, finalZ_bottom);
            capBeam.Profile = new Profile { ProfileString = $"{widthi_cap}*{cap.W}" }; // B (width) x W (depth)
            capBeam.Material = new Material { MaterialString = "C12/15" };
            capBeam.Class = "8";
            capBeam.Position.Rotation = Position.RotationEnum.FRONT;
            capBeam.Position.RotationOffset = -global.RotationAngle;
            capBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            capBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (capBeam.Insert())
            {
                CreateCutPlaneCap(capBeam, global, cap, elevationCircularHeight, finalX, finalY);
            }
        }

        private void CreateCutPlaneCap(Beam cap, GlobalParameters global, CapParameters capParams, double elevationCircularHeight, double centerX, double centerY)
        {
            double angleRadians = global.SkewAngle * Math.PI / 180.0;
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180.0;

            // FOR CUTPLANE 1 (using B for width in transverse direction)
            double translatedX_cut1 = 0;
            double translatedY_cut1 = -capParams.B / 2;

            double rotatedX_cut1 = translatedX_cut1 * Math.Cos(rotationAngleRadians) - translatedY_cut1 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut1 = translatedX_cut1 * Math.Sin(rotationAngleRadians) + translatedY_cut1 * Math.Cos(rotationAngleRadians);

            double finalX_cut1 = rotatedX_cut1 + centerX;
            double finalY_cut1 = rotatedY_cut1 + centerY;

            // FOR CUTPLANE 2
            double translatedX_cut2 = 0;
            double translatedY_cut2 = +capParams.B / 2;

            double rotatedX_cut2 = translatedX_cut2 * Math.Cos(rotationAngleRadians) - translatedY_cut2 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut2 = translatedX_cut2 * Math.Sin(rotationAngleRadians) + translatedY_cut2 * Math.Cos(rotationAngleRadians);

            double finalX_cut2 = rotatedX_cut2 + centerX;
            double finalY_cut2 = rotatedY_cut2 + centerY;

            // Z position for lateral cuts based on SlopeHeight parameter
            double slopeZ = global.PositionZ + elevationCircularHeight + capParams.SlopeHeight;

            // Skew Cut Planes (for trapezoidal shape in plan view)
            CutPlane cutPlane_skew_1 = new CutPlane
            {
                Father = cap,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut1, finalY_cut1, 0),
                    AxisX = new Vector(Math.Cos(angleRadians + rotationAngleRadians), Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 0, 1)
                }
            };

            CutPlane cutPlane_skew_2 = new CutPlane
            {
                Father = cap,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut2, finalY_cut2, 0),
                    AxisX = new Vector(-Math.Cos(angleRadians + rotationAngleRadians), -Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 0, 1)
                }
            };

            // Lateral Cut Planes (for trapezoidal shape in elevation - the sloped sides)
            CutPlane cutPlane_lateral_1 = new CutPlane
            {
                Father = cap,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut1, finalY_cut1, slopeZ),
                    AxisX = new Vector(-Math.Cos(angleRadians + rotationAngleRadians), -Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 1, -0.4), // Slope inward (negative Z component)
                }
            };

            CutPlane cutPlane_lateral_2 = new CutPlane
            {
                Father = cap,
                Plane = new Plane
                {
                    Origin = new Point(finalX_cut2, finalY_cut2, slopeZ),
                    AxisX = new Vector(-Math.Cos(angleRadians + rotationAngleRadians), -Math.Sin(angleRadians + rotationAngleRadians), 0),
                    AxisY = new Vector(0, 1, 0.4), // Slope inward (positive Z component)
                }
            };

            cutPlane_skew_1.Insert();
            cutPlane_skew_2.Insert();
            cutPlane_lateral_1.Insert();
            cutPlane_lateral_2.Insert();
        }

        // Rebar Logic (Copied and adapted)
        private void Example1(Beam padFooting, GlobalParameters global, FoundationParameters foundation, double a)
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



        // Overload to fix the content above
        private void DrawTwoCircles(Beam padFooting, double a, GlobalParameters global, FoundationParameters fParams)
        {
            TransformationPlane currentPlane = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane localPlane = new TransformationPlane(padFooting.GetCoordinateSystem());
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(localPlane);

            Solid solid = padFooting.GetSolid();
            if (solid == null) return;

            double radius = 100.0;

            Point center1 = new Point(new Point(solid.MaximumPoint.X, solid.MaximumPoint.Y - 2 * a, solid.MinimumPoint.Z));
            DrawCircle(center1, radius);

            double l = Math.Cos(global.SkewAngle * Math.PI / 180) * fParams.Width; // correct?
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
