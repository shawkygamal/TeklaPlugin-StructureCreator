using System;

namespace TeklaPlugin.Services.Buffer.Models
{
    public class BufferParameters
    {
        public int Number { get; set; } = 3;
        public double Spacing { get; set; } = 500; // S - spacing between buffers
        public double LeftOffset { get; set; } = 200; // From left edge of cap to first buffer
        public double RightOffset { get; set; } = 200; // From right edge of cap to last buffer
        public double Width { get; set; } = 400; // W - along cap length direction
        public double Breadth { get; set; } = 300; // B - along cap width direction
        public string Heights { get; set; } = "50,50,50"; // Comma-separated height per buffer
        public string Material { get; set; } = "C12/15";
        public string Class { get; set; } = "8";
    }
}
