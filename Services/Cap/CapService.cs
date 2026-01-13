using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
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
            capBeam.Material = new Material { MaterialString = cap.Material };
            capBeam.Class = cap.Class;
            capBeam.Position.Rotation = Position.RotationEnum.FRONT;
            capBeam.Position.RotationOffset = -global.RotationAngle;
            capBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            capBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (capBeam.Insert())
            {
                CreateCutPlaneCap(capBeam, global, cap, elevationCircularHeight, finalX, finalY);
            }
        }

        private void CreateCutPlaneCap(Beam cap, TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CapParameters capParams, double elevationCircularHeight, double centerX, double centerY)
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
    }
}