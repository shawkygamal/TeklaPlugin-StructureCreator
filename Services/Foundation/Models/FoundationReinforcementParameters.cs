using System;
using System.Collections.Generic;

namespace TeklaPlugin.Services.Foundation.Models
{
    public enum RebarDirection
    {
        Width,
        Length
    }

    public class RebarLayer
    {
        public double Diameter { get; set; } = 25;
        public RebarDirection Direction { get; set; } = RebarDirection.Length;
        public double Spacing { get; set; } = 200;
    }

    public class SideReinforcement
    {
        public double Diameter { get; set; } = 12;
        public double Spacing { get; set; } = 200;
    }

    public class IntermediateReinforcement
    {
        public double Diameter { get; set; } = 16;
        public RebarDirection Direction { get; set; } = RebarDirection.Length;
        public int NumberOfLayers { get; set; } = 0;
        public double Spacing { get; set; } = 200;
    }

    public class FoundationReinforcementParameters
    {
        // Top reinforcement (T1 = outermost top, T2 = second top layer)
        public RebarLayer T1 { get; set; } = new RebarLayer { Diameter = 25, Direction = RebarDirection.Length };
        public RebarLayer T2 { get; set; } = new RebarLayer { Diameter = 25, Direction = RebarDirection.Width };

        // Bottom reinforcement (B1 = outermost bottom, B2 = second bottom layer)
        public RebarLayer B1 { get; set; } = new RebarLayer { Diameter = 25, Direction = RebarDirection.Length };
        public RebarLayer B2 { get; set; } = new RebarLayer { Diameter = 25, Direction = RebarDirection.Width };

        // Cover from concrete face to first rebar surface (not center)
        // Distance from concrete edge to bar center = Cover + Dia/2
        public double TopCover { get; set; } = 50;
        public double BottomCover { get; set; } = 50;

        // Hook length for T1, T2, B1, B2 (0 = no hooks)
        public double HookLength { get; set; } = 0;

        // Side reinforcement around the whole footing perimeter
        public SideReinforcement Side { get; set; } = new SideReinforcement();

        // Intermediate layers between B2 and T2
        public List<IntermediateReinforcement> IntermediateLayers { get; set; } = new List<IntermediateReinforcement>();

        /// <summary>
        /// Calculate how many side bars fit in the free space between B2 and T2,
        /// minus 50 mm inset from each end.
        /// </summary>
        public int CalculateSideBarCount(double footingHeight)
        {
            double freeSpace = footingHeight
                - TopCover - T1.Diameter - T2.Diameter
                - BottomCover - B1.Diameter - B2.Diameter
                - 100; // 50 mm inset from top and bottom

            if (freeSpace <= 0 || Side.Spacing <= 0) return 0;

            int count = (int)Math.Floor(freeSpace / Side.Spacing) - 1;
            return Math.Max(count, 0);
        }
    }
}
