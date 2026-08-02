using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using Tekla.Structures.Model.UI;
using System.Collections.Generic;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;

namespace TeklaPlugin.Services.Foundation
{
    public class FoundationService
    {
        private readonly Model _model;
        private readonly FoundationReinforcementService _rebarService;

        public FoundationService(Model model)
        {
            _model = model;
            _rebarService = new FoundationReinforcementService(model);
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
            foundationBeam.Material = new Material { MaterialString = foundation.Material };
            foundationBeam.Class = foundation.Class;
            foundationBeam.Position.Rotation = Position.RotationEnum.FRONT;
            foundationBeam.Position.RotationOffset = -global.RotationAngle;
            foundationBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
            foundationBeam.Position.Depth = Position.DepthEnum.MIDDLE;

            if (foundationBeam.Insert())
            {
                CreateCutPlane(foundationBeam, global, foundation, a);

                // Use the separate reinforcement service
                _rebarService.CreateReinforcement(foundationBeam, global, foundation, a);
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
    }
}
