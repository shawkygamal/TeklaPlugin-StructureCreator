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
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;

            for (int i = 0; i < lamelar.NumberOfColumns; i++)
            {
                // Rotated by the origin coordinates (swapped X/Y logic for opposite direction)
                var pozX_rotated_o = Math.Cos(rotationAngleRadians) * (lamelar.OffsetX + lamelar.DistanceBetweenColumns * i - ((lamelar.NumberOfColumns - 1) * lamelar.DistanceBetweenColumns) / 2) - Math.Sin(rotationAngleRadians) * lamelar.OffsetY;
                var pozY_rotated_o = Math.Sin(rotationAngleRadians) * (lamelar.OffsetX + lamelar.DistanceBetweenColumns * i - ((lamelar.NumberOfColumns - 1) * lamelar.DistanceBetweenColumns) / 2) + Math.Cos(rotationAngleRadians) * lamelar.OffsetY;

                // Rotated coordinates
                var pozX_rotated = pozX_rotated_o + global.PositionX;
                var pozY_rotated = pozY_rotated_o + global.PositionY;

                Beam elevationLamelar = new Beam();
                elevationLamelar.StartPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ);
                elevationLamelar.EndPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ + lamelar.Height);
                elevationLamelar.Profile = new Profile { ProfileString = $"{lamelar.Width}*{lamelar.Thickness}" };
                elevationLamelar.Material = new Material { MaterialString = lamelar.Material };
                elevationLamelar.Class = lamelar.Class;
                elevationLamelar.Position.Rotation = Position.RotationEnum.FRONT;
                elevationLamelar.Position.RotationOffset = global.RotationAngle;
                elevationLamelar.Position.Plane = Position.PlaneEnum.MIDDLE;
                elevationLamelar.Position.Depth = Position.DepthEnum.MIDDLE;
                elevationLamelar.Name = $"Elevation_Lamelar_{i}";

                elevationLamelar.Insert();
            }
        }

        public void CreateElevationCircular(TeklaPlugin.Services.Core.Models.GlobalParameters global, Models.CircularElevationParameters circular)
        {
            double rotationAngleRadians = global.RotationAngle * Math.PI / 180;

            for (int i = 0; i < circular.NumberOfColumns; i++)
            {
                // Rotated by the origin coordinates (swapped X/Y logic)
                var pozX_rotated_o = Math.Cos(rotationAngleRadians) * (circular.OffsetX + circular.DistanceBetweenColumns * i - ((circular.NumberOfColumns - 1) * circular.DistanceBetweenColumns) / 2) - Math.Sin(rotationAngleRadians) * circular.OffsetY;
                var pozY_rotated_o = Math.Sin(rotationAngleRadians) * (circular.OffsetX + circular.DistanceBetweenColumns * i - ((circular.NumberOfColumns - 1) * circular.DistanceBetweenColumns) / 2) + Math.Cos(rotationAngleRadians) * circular.OffsetY;

                // Rotated coordinates
                var pozX_rotated = pozX_rotated_o + global.PositionX;
                var pozY_rotated = pozY_rotated_o + global.PositionY;

                Beam elevationCircular = new Beam();
                elevationCircular.StartPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ);
                elevationCircular.EndPoint = new Point(pozX_rotated, pozY_rotated, global.PositionZ + circular.Height);
                elevationCircular.Profile = new Profile { ProfileString = $"D{circular.Diameter}" };
                elevationCircular.Material = new Material { MaterialString = circular.Material };
                elevationCircular.Class = circular.Class;
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