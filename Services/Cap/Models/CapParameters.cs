using System;

namespace TeklaPlugin.Services.Cap.Models
{
    public class CapParameters
    {
        public double TopLength { get; set; } // Long top length of trapezoid
        public double BottomLength { get; set; } // Shorter bottom length of trapezoid
        public double Width { get; set; } // Width perpendicular to trapezoid plane (thickness)
        public double Depth { get; set; } // Vertical depth of the rectangular (straight-sided) top portion
        public double HeightDiff { get; set; } // Height of the inclined/sloped portion below the main depth
        public double P { get; set; } // Offset from column center for positioning
        public string Material { get; set; } = "C12/15"; // Default concrete material for cap
        public string Class { get; set; } = "8"; // Default class for cap
    }
}
