using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;
using TeklaPlugin.Services.Piles.Models;

namespace TeklaPlugin.Services.Piles
{
    public class PilesService
    {
        private readonly Model _model;

        public PilesService(Model model)
        {
            _model = model;
        }

        public void CreatePiles(TeklaPlugin.Services.Core.Models.GlobalParameters global, Foundation.Models.FoundationParameters foundation, Models.PileParameters piles)
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
    }
}