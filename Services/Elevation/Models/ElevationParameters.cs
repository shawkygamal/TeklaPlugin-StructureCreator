using System;

namespace TeklaPlugin.Services.Elevation.Models
{
    public enum ElevationType
    {
        Lamelar,
        Circular
    }

    // Lamelar elevation parameters
    public class LamelarElevationParameters
    {
        public double Width { get; set; }
        public double Thickness { get; set; }
        public double Height { get; set; }
    }

    // Circular elevation parameters
    public class CircularElevationParameters
    {
        public double Diameter { get; set; }
        public double Height { get; set; }
        public int NumberOfColumns { get; set; }
        public double DistanceBetweenColumns { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
    }
}