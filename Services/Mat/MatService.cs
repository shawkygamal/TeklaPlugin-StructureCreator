using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;
using TeklaPlugin.Services.Mat.Models;

namespace TeklaPlugin.Services.Mat
{
    public class MatService
    {
        private readonly Model _model;

        public MatService(Model model)
        {
            _model = model;
        }

        public void CreateMat(TeklaPlugin.Services.Core.Models.GlobalParameters global, Foundation.Models.FoundationParameters foundation, Models.MatParameters mat)
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

        private void CreateCutPlaneMat(Beam matBeam, TeklaPlugin.Services.Core.Models.GlobalParameters global, Foundation.Models.FoundationParameters foundation, Models.MatParameters mat)
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
    }
}