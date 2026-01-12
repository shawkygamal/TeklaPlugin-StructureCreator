using System;

namespace TeklaPlugin
{
    public class GlobalParameters
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double RotationAngle { get; set; }
        public double SkewAngle { get; set; }
    }

    public class FoundationParameters
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
    }

    public class MatParameters
    {
        public double Cantilever { get; set; }
        public double Thickness { get; set; }
    }

    public class PileParameters
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public double RowDistance { get; set; }
        public double ColumnDistance { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public double EmbeddedLength { get; set; }
    }

    public class LamelarElevationParameters
    {
        public double Width { get; set; }
        public double Thickness { get; set; }
        public double Height { get; set; }
    }

    public class CircularElevationParameters
    {
        public double Diameter { get; set; }
        public double Height { get; set; }
        public int NumberOfColumns { get; set; }
        public double DistanceBetweenColumns { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
    }

    public class CapParameters
    {
        public double H { get; set; } // Height - Vertical direction (Z)
        public double B { get; set; } // Top width - Transverse direction (Y) 
        public double W { get; set; } // Depth/Width - Longitudinal direction (X) - not visible in 2D front view
        public double P { get; set; } // Offset from column center for positioning
        public double SlopeHeight { get; set; } // Height where slope starts (for trapezoid shape)
    }
}
