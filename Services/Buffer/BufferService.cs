using System;
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Material = Tekla.Structures.Model.Material;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Cap.Models;
using TeklaPlugin.Services.Buffer.Models;

namespace TeklaPlugin.Services.Buffer
{
    public class BufferService
    {
        private readonly Model _model;

        public BufferService(Model model)
        {
            _model = model;
        }

        public void CreateBuffers(GlobalParameters global, CapParameters cap, BufferParameters buffer, double elevationHeight)
        {
            if (buffer.Number <= 0) return;

            double rotRad = global.RotationAngle * Math.PI / 180.0;
            double totalCapHeight = cap.Depth + cap.HeightDiff;
            double capTopZ = global.PositionZ + elevationHeight + totalCapHeight;

            // Cap center position (same offset as cap beam)
            double capOffsetX = cap.P * Math.Cos(rotRad);
            double capOffsetY = cap.P * Math.Sin(rotRad);
            double capCenterX = global.PositionX + capOffsetX;
            double capCenterY = global.PositionY + capOffsetY;

            // Parse individual heights
            double[] heights = ParseHeights(buffer.Heights, buffer.Number);

            for (int i = 0; i < buffer.Number; i++)
            {
                // Position along cap length (local X direction)
                double localX;
                
                if (i < buffer.Number - 1)
                {
                    // Normal positioning from left offset + spacing
                    localX = -cap.TopLength / 2.0 + buffer.LeftOffset + buffer.Width / 2.0 + i * (buffer.Width + buffer.Spacing);
                }
                else
                {
                    // Last buffer: check if normal spacing would exceed right offset
                    double normalX = -cap.TopLength / 2.0 + buffer.LeftOffset + buffer.Width / 2.0 + i * (buffer.Width + buffer.Spacing);
                    double normalRightEdge = normalX + buffer.Width / 2.0;
                    double rightConstraint = cap.TopLength / 2.0 - buffer.RightOffset;
                    
                    if (normalRightEdge > rightConstraint)
                    {
                        // Adjust to respect right offset
                        localX = rightConstraint - buffer.Width / 2.0;
                    }
                    else
                    {
                        localX = normalX;
                    }
                }
                
                double localY = 0; // Centered in cap width

                // Rotate to global
                double dx = localX * Math.Cos(rotRad) - localY * Math.Sin(rotRad);
                double dy = localX * Math.Sin(rotRad) + localY * Math.Cos(rotRad);

                double bufX = capCenterX + dx;
                double bufY = capCenterY + dy;
                double h = heights[i];

                Beam bufferBeam = new Beam();
                bufferBeam.StartPoint = new Point(bufX, bufY, capTopZ + h);
                bufferBeam.EndPoint = new Point(bufX, bufY, capTopZ);
                bufferBeam.Profile = new Profile { ProfileString = $"{buffer.Breadth}*{buffer.Width}" };
                bufferBeam.Material = new Material { MaterialString = buffer.Material };
                bufferBeam.Class = buffer.Class;
                bufferBeam.Position.Rotation = Position.RotationEnum.FRONT;
                bufferBeam.Position.RotationOffset = -global.RotationAngle;
                bufferBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
                bufferBeam.Position.Depth = Position.DepthEnum.MIDDLE;

                bufferBeam.Insert();
            }
        }

        private double[] ParseHeights(string heightsStr, int count)
        {
            double[] heights = new double[count];
            string[] parts = heightsStr.Split(',');
            for (int i = 0; i < count; i++)
            {
                if (i < parts.Length && double.TryParse(parts[i].Trim(), out double h))
                    heights[i] = h;
                else if (i > 0)
                    heights[i] = heights[i - 1]; // Repeat last provided height
                else
                    heights[i] = 50; // Default 50mm
            }
            return heights;
        }
    }
}
