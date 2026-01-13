using System;

namespace TeklaPlugin.Services.Piles.Models
{
    public class PileParameters
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public double RowDistance { get; set; }
        public double ColumnDistance { get; set; }
        public double Length { get; set; }
        public double Diameter { get; set; }
        public double EmbeddedLength { get; set; }
        public string Material { get; set; } = "C50/60"; // Default concrete material for piles
        public string Class { get; set; } = "8"; // Default class for piles
    }
}