using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Elevation.Models;

namespace TeklaPlugin.Services.Elevation
{
    public class ElevationService
    {
        private readonly Model _model;

        public ElevationService(Model model)
        {
            _model = model;
        }

        public void CreateElevationLamelar(TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.LamelarElevationParameters lamelar)
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

        public void CreateElevationCircular(TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CircularElevationParameters circular)
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
    }
}