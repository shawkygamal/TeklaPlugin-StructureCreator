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
        public double CutX { get; set; } // L-cut horizontal dimension (width direction), 0 = no cut
        public double CutY { get; set; } // L-cut vertical dimension (height direction), 0 = no cut
        public string CutSide { get; set; } = "Right"; // Which side to cut: "Right" or "Left"
        public double F { get; set; } // Cross-section width taper in HeightDiff portion (0 = no taper)
        public double D1 { get; set; } // Plan taper: width above centerline at left end (0 = no taper, use Width)
        public double D2 { get; set; } // Plan taper: width below centerline at left end
        public double D3 { get; set; } // Plan taper: width above centerline at right end
        public double D4 { get; set; } // Plan taper: width below centerline at right end
        public string Material { get; set; } = "C12/15"; // Default concrete material for cap
        public string Class { get; set; } = "8"; // Default class for cap
    }
}
