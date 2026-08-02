using System;

namespace TeklaPlugin.Services.Elevation.Models
{
    public enum ColumnShapeType
    {
        Circular,
        Rectangular
    }

    public enum InternalStirrupShape
    {
        Rectangular,  // Standard rectangular tie
        Diamond,      // Diamond-shaped cross-tie
        Cross,        // Plus-shaped (cross ties)
        Circular      // For circular columns only
    }

    /// <summary>
    /// Reinforcement parameters for columns (circular and rectangular).
    /// 
    /// For rectangular columns:
    ///   External ties wrap around the perimeter
    ///   Main bars placed at corners and along edges
    ///   Internal ties can be rectangular, diamond, or cross-shaped
    ///   
    /// For circular columns:
    ///   External ties are circular hoops (not spirals)
    ///   Main bars placed in concentric rings
    ///   Internal ties are circular
    /// </summary>
    public class ColumnReinforcementParameters
    {
        public double Cover { get; set; } = 40;     // mm (typical for columns)

        // ── Main longitudinal bars (vertical reinforcement) ──
        public double MainBarDiameter { get; set; } = 16;   // mm

        /// <summary>
        /// Optional splice (lap) length for main bars along column length (mm).
        /// 0 = no splice (single full-length bar).
        /// </summary>
        public double MainBarSpliceLength { get; set; } = 0;

        /// <summary>
        /// Comma-separated bar counts per layer (outside → inside).
        /// For rectangular: "12,8" → outer layer 12 bars, inner layer 8 bars
        /// For circular: "16,12" → outer ring 16 bars, inner ring 12 bars
        /// </summary>
        public string BarsPerLayer { get; set; } = "8";

        /// <summary>
        /// Spacing between layers (mm). For concentric rings in circular columns
        /// or nested rectangles in rectangular columns.
        /// </summary>
        public double LayerSpacing { get; set; } = 50;   // mm

        public double SpacerDiameter { get; set; } = 10; // mm (for maintaining spacing)

        // ── External stirrups/ties (outside after cover) ──
        public bool ExternalStirrupsEnabled { get; set; } = true;
        public double ExternalStirrupDiameter { get; set; } = 10;   // mm
        public double ExternalStirrupSpacing { get; set; } = 150;   // mm (vertical spacing)

        // For rectangular columns: tie configuration
        public bool IncludeSeismicHooks { get; set; } = true;  // 135° hooks instead of 90°

        /// <summary>
        /// Hook extension length beyond the bend at the end of stirrups (mm).
        /// Typically 6*stirrupDia to 10*stirrupDia per code requirements.
        /// 0 = use default (10 * ExternalStirrupDiameter).
        /// </summary>
        public double HookLength { get; set; } = 0;

        // For circular columns: can also use circular ties (not spiral)
        public bool UseCircularTies { get; set; } = true;  // For circular columns only

        // ── Internal collar stirrups (inside the innermost reinforcement layer) ──
        public bool InternalStirrupsEnabled { get; set; } = false;
        public double InternalStirrupDiameter { get; set; } = 8;    // mm
        public double InternalStirrupSpacing { get; set; } = 300;   // mm

        /// <summary>
        /// For rectangular columns: shape of internal stirrups (e.g., rectangular, diamond, cross-ties)
        /// </summary>
        public InternalStirrupShape InternalShape { get; set; } = InternalStirrupShape.Rectangular;

        // Helper methods
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
