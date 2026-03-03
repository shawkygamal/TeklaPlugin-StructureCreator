using System;

namespace TeklaPlugin.Services.Piles.Models
{
    /// <summary>
    /// Reinforcement parameters for circular piles.
    /// 
    /// Cross-section (outside → inside):
    ///   Concrete → Cover → Spiral → Layer 1 → Spacer hoop → Layer 2 → … → Circular stirrup
    ///   
    /// Radii (R = pile radius, n = number of layers):
    ///   Spiral        = R - Cover - SpiralDia/2
    ///   Layer k       = R - Cover - SpiralDia - (k+0.5)*MainBarDia - k*SpacerDia
    ///   Spacer k→k+1  = R - Cover - SpiralDia - (k+1)*MainBarDia - (k+0.5)*SpacerDia
    ///   Circ stirrup  = R - Cover - SpiralDia - n*MainBarDia - (n-1)*SpacerDia
    /// </summary>
    public class PileReinforcementParameters
    {
        public double Cover { get; set; } = 50;

        // ── Spiral stirrup (continuous helix, outside after cover) ──
        public double SpiralDiameter { get; set; } = 12;
        public double SpiralPitch { get; set; } = 100;

        // ── Main longitudinal bars ──
        public double MainBarDiameter { get; set; } = 20;

        /// <summary>
        /// Comma-separated bar counts per layer (outside → inside).
        /// "20,20,10" → 3 layers with 20, 20, 10 bars.
        /// </summary>
        public string BarsPerLayer { get; set; } = "10";

        public double SpacerDiameter { get; set; } = 12;

        // ── Circular stirrup (inside innermost layer) ──
        public bool CircStirrupEnabled { get; set; } = false;
        public double CircStirrupDiameter { get; set; } = 10;
        public double CircStirrupPitch { get; set; } = 200;

        public int[] GetBarsPerLayerArray()
        {
            if (string.IsNullOrWhiteSpace(BarsPerLayer))
                return new int[] { 0 };

            var parts = BarsPerLayer.Split(',');
            var result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                result[i] = int.TryParse(parts[i].Trim(), out int val) ? val : 0;
            return result;
        }

        public int NumberOfLayers
        {
            get { return GetBarsPerLayerArray().Length; }
        }
    }
}
