using System;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Datatype;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Cap.Models;

namespace TeklaPlugin.Services.Cap
{
    public class CapService
    {
        private readonly Model _model;

        public CapService(Model model)
        {
            _model = model;
        }

        public void CreateCap(TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CapParameters cap, double elevationCircularHeight)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;

            // Calculate skew offset based on maximum length (for skew cuts)
            double maxLength = Math.Max(cap.TopLength, cap.BottomLength);
            double a = Math.Tan(global.SkewAngle * Math.PI / 180) * maxLength / 2;
            double widthi_cap = maxLength + 2 * a; // Adjusted width including skew

            // Calculate position with P offset from column center
            double offsetX = cap.P;
            double offsetY = 0;

            double rotatedOffsetX = offsetX * Math.Cos(rotationAngleRadians) - offsetY * Math.Sin(rotationAngleRadians);
            double rotatedOffsetY = offsetX * Math.Sin(rotationAngleRadians) + offsetY * Math.Cos(rotationAngleRadians);

            double finalX = global.PositionX + rotatedOffsetX;
            double finalY = global.PositionY + rotatedOffsetY;

            // Total beam height = Depth (rectangular top) + HeightDiff (sloped bottom)
            double totalHeight = cap.Depth + cap.HeightDiff;
            double finalZ_top = global.PositionZ + elevationCircularHeight + totalHeight;
            double finalZ_bottom = global.PositionZ + elevationCircularHeight;

            Beam capBeam = new Beam();
            capBeam.StartPoint = new Point(finalX, finalY, finalZ_top);
            capBeam.EndPoint = new Point(finalX, finalY, finalZ_bottom);
            capBeam.Profile = new Profile { ProfileString = $"{cap.Width}*{widthi_cap}" }; // Width (thickness) x MaxLength (adjusted for skew)
            capBeam.Material = new Material { MaterialString = cap.Material };
            capBeam.Class = cap.Class;
            capBeam.Position.Rotation = Position.RotationEnum.FRONT;
            capBeam.Position.RotationOffset = -global.RotationAngle;
            capBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            capBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (capBeam.Insert())
            {
                CreateCutPlaneCap(capBeam, global, cap, elevationCircularHeight, finalX, finalY);
                CreateCrossSectionCut(capBeam, global, cap, finalX, finalY, finalZ_top, widthi_cap);
            }
        }

        private void CreateCutPlaneCap(Beam cap, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CapParameters capParams, double elevationCircularHeight, double centerX, double centerY)
        {
            // Handle skew cuts first (like foundation service)
            if (global.SkewAngle != 0)
            {
                CreateSkewCutPlanes(cap, global, capParams, centerX, centerY);
            }

            // Apply tapered cuts for trapezoidal shape (only in the HeightDiff portion)
            Point beamCenter = cap.StartPoint;
            double rotRad = -cap.Position.RotationOffset * Math.PI / 180.0;

            double topHalf = capParams.TopLength / 2.0;
            double botHalf = capParams.BottomLength / 2.0;

            CreateTaperCutPlane(cap, beamCenter, rotRad, topHalf, botHalf, capParams.Depth, capParams.HeightDiff, isPositiveSide: true);
            CreateTaperCutPlane(cap, beamCenter, rotRad, topHalf, botHalf, capParams.Depth, capParams.HeightDiff, isPositiveSide: false);
        }

        private void CreateSkewCutPlanes(Beam cap, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CapParameters capParams, double centerX, double centerY)
        {
            double angleRadians = global.SkewAngle * Math.PI / 180.0;
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180.0;

            // Calculate the maximum half-width for skew cuts
            double maxHalfWidth = Math.Max(capParams.TopLength, capParams.BottomLength) / 2;

            // FOR CUTPLANE 1 (left side)
            double translatedX_cut1 = 0;
            double translatedY_cut1 = -maxHalfWidth;

            double rotatedX_cut1 = translatedX_cut1 * Math.Cos(rotationAngleRadians) - translatedY_cut1 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut1 = translatedX_cut1 * Math.Sin(rotationAngleRadians) + translatedY_cut1 * Math.Cos(rotationAngleRadians);

            double finalX_cut1 = rotatedX_cut1 + centerX;
            double finalY_cut1 = rotatedY_cut1 + centerY;

            // FOR CUTPLANE 2 (right side)
            double translatedX_cut2 = 0;
            double translatedY_cut2 = +maxHalfWidth;

            double rotatedX_cut2 = translatedX_cut2 * Math.Cos(rotationAngleRadians) - translatedY_cut2 * Math.Sin(rotationAngleRadians);
            double rotatedY_cut2 = translatedX_cut2 * Math.Sin(rotationAngleRadians) + translatedY_cut2 * Math.Cos(rotationAngleRadians);

            double finalX_cut2 = rotatedX_cut2 + centerX;
            double finalY_cut2 = rotatedY_cut2 + centerY;

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

            cutPlane_skew_1.Insert();
            cutPlane_skew_2.Insert();
        }

        private void CreateTaperCutPlane(Beam cap, Point center, double rotRad, double topHalf, double botHalf, double depth, double heightDiff, bool isPositiveSide)
        {
            double sign = isPositiveSide ? 1.0 : -1.0;

            // 1. Define 3 points in Local Coordinates (X along Length, Z down from beam top)
            // Point A (Start of slope - at Depth level below top)
            double xA = sign * topHalf;
            double yA = 0;
            double zA = -depth; // Slope starts at Depth below beam top

            // Point B (Bottom Edge - at Depth + HeightDiff below top)
            double xB = sign * botHalf;
            double yB = 0;
            double zB = -(depth + heightDiff); // Slope ends at bottom

            // Point C (Width definer - parallel to Y, at same level as A)
            double xC = xA;
            double yC = 500.0; // Offset in Width direction
            double zC = zA;

            // 2. Rotate to Global Coordinates
            Point pA = RotateAndTranslate(center, rotRad, xA, yA, zA);
            Point pB = RotateAndTranslate(center, rotRad, xB, yB, zB);
            Point pC = RotateAndTranslate(center, rotRad, xC, yC, zC);

            // 3. Define Vector Axes for Plane
            // Axis X: Along the slope (A -> B)
            Vector vecAB = new Vector(pB.X - pA.X, pB.Y - pA.Y, pB.Z - pA.Z);
            // Axis Y: Along width (A -> C)
            Vector vecAC = new Vector(pC.X - pA.X, pC.Y - pA.Y, pC.Z - pA.Z);

            // 4. Ensure Normal points Outward (to cut the excess)
            Vector normal = vecAB.Cross(vecAC);
            Vector centerToOut = new Vector(pA.X - center.X, pA.Y - center.Y, 0);

            if (normal.Dot(centerToOut) < 0)
            {
                // Pointing Inward, flip Y axis to flip Normal
                vecAC = new Vector(-vecAC.X, -vecAC.Y, -vecAC.Z);
            }

            // 5. Insert CutPlane
            CutPlane cut = new CutPlane();
            cut.Father = cap;
            cut.Plane = new Plane();
            cut.Plane.Origin = pA;
            cut.Plane.AxisX = vecAB;
            cut.Plane.AxisY = vecAC;

            cut.Insert();
        }

        private void CreateCrossSectionCut(Beam capBeam, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CapParameters capParams, double centerX, double centerY, double topZ, double widthi_cap)
        {
            if (capParams.CutX <= 0 || capParams.CutY <= 0) return;

            double rotRad = global.RotationAngle * Math.PI / 180.0;

            // Width direction in local coords = Y axis
            // After rotation: global direction = (-sin(rotRad), cos(rotRad))
            double widthOffset = (capParams.Width / 2.0) - (capParams.CutX / 2.0);
            if (capParams.CutSide == "Left")
                widthOffset = -widthOffset;

            double dx = widthOffset * (-Math.Sin(rotRad));
            double dy = widthOffset * Math.Cos(rotRad);

            // Cutting beam: vertical, from slightly above top down by CutY
            Beam cutBeam = new Beam();
            cutBeam.StartPoint = new Point(centerX + dx, centerY + dy, topZ + 50);
            cutBeam.EndPoint = new Point(centerX + dx, centerY + dy, topZ - capParams.CutY);
            cutBeam.Profile = new Profile { ProfileString = $"{capParams.CutX}*{widthi_cap + 200}" };
            cutBeam.Material = new Material { MaterialString = capParams.Material };
            cutBeam.Class = BooleanPart.BooleanOperativeClassName;
            cutBeam.Position.Rotation = Position.RotationEnum.FRONT;
            cutBeam.Position.RotationOffset = -global.RotationAngle;
            cutBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            cutBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (cutBeam.Insert())
            {
                BooleanPart bp = new BooleanPart();
                bp.Father = capBeam;
                bp.SetOperativePart(cutBeam);
                bp.Type = BooleanPart.BooleanTypeEnum.BOOLEAN_CUT;

                if (!bp.Insert())
                {
                    // Boolean failed — remove leftover cutting beam
                    cutBeam.Delete();
                }
            }
        }

        private Point RotateAndTranslate(Point center, double rotRad, double x, double y, double z)
        {
            // Rotation around Z-axis
            double xRot = x * Math.Cos(rotRad) - y * Math.Sin(rotRad);
            double yRot = x * Math.Sin(rotRad) + y * Math.Cos(rotRad);

            return new Point(center.X + xRot, center.Y + yRot, center.Z + z);
        }
    }
}
