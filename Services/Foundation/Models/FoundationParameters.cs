using System;

namespace TeklaPlugin.Services.Foundation.Models
{
    public class FoundationParameters
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public string Material { get; set; } = "C50/60"; // Default concrete material
        public string Class { get; set; } = "8"; // Default class for foundations
    }
}