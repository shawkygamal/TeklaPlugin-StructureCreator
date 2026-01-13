using System;

namespace TeklaPlugin.Services.Cap.Models
{
    public class CapParameters
    {
        public double H { get; set; } // Height - Vertical direction (Z)
        public double B { get; set; } // Top width - Transverse direction (Y)
        public double W { get; set; } // Depth - Longitudinal direction (X) - not visible in 2D front view
        public double P { get; set; } // Offset from column center for positioning
        public double SlopeHeight { get; set; } // Height where slope starts (for trapezoid shape)
        public string Material { get; set; } = "C12/15"; // Default concrete material for cap
        public string Class { get; set; } = "8"; // Default class for cap
    }
}