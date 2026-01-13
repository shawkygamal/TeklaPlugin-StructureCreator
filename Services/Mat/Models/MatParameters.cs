using System;

namespace TeklaPlugin.Services.Mat.Models
{
    public class MatParameters
    {
        public double Cantilever { get; set; }
        public double Thickness { get; set; }
        public string Material { get; set; } = "C12/15"; // Default concrete material for mat
        public string Class { get; set; } = "1"; // Default class for mat foundations
    }
}