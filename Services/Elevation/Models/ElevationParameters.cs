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
        public int NumberOfColumns { get; set; }
        public double DistanceBetweenColumns { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public string Material { get; set; } = "C50/60"; // Default concrete material
        public string Class { get; set; } = "8"; // Default class for elevation
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
        public string Material { get; set; } = "C50/60"; // Default concrete material
        public string Class { get; set; } = "8"; // Default class for elevation
    }
}