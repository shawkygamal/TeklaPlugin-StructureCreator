using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Solid;
using TeklaPlugin.Services.Piles.Models;

namespace TeklaPlugin.Services.Piles
{
    /// <summary>
    /// Creates reinforcement for circular piles.
    /// 
    /// All radii computed directly from:
    ///   R = pile radius, c = cover, s = spiral dia, d = main bar dia, sp = spacer dia
    /// </summary>
    public class PileReinforcementService
    {
        private readonly Model _model;
        private const int Seg = 36;

        public PileReinforcementService(Model model)
        {
            _model = model;
        }

        public void CreateReinforcement(Beam pile, double pileDiameter, PileReinforcementParameters p)
        {
            TransformationPlane saved = _model.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(
                new TransformationPlane(pile.GetCoordinateSystem()));

            try
            {
                Solid solid = pile.GetSolid();
                if (solid == null) return;

                double minX = solid.MinimumPoint.X;
                double maxX = solid.MaximumPoint.X;
                double R = pileDiameter / 2.0;

                int[] bars = p.GetBarsPerLayerArray();
                int n = bars.Length;

                // ── 1. Spiral (continuous helix) ──
                double spiralR = R - p.Cover - p.SpiralDiameter / 2.0;
                CreateSpiral(pile, spiralR, p.SpiralDiameter, p.SpiralPitch,
                             minX, maxX, "Pile Spiral", 7);

                // ── 2. Main longitudinal bars per layer ──
                for (int k = 0; k < n; k++)
                {
                    double layerR = R - p.Cover - p.SpiralDiameter
                                    - (k + 0.5) * p.MainBarDiameter
                                    - k * p.SpacerDiameter;
                    if (layerR <= 0) break;
                    CreateMainBars(pile, p.MainBarDiameter, bars[k], layerR, minX, maxX, k);
                }

                // ── 3. Spacer hoops between adjacent layers ──
                for (int k = 0; k < n - 1; k++)
                {
                    double spacerR = R - p.Cover - p.SpiralDiameter
                                     - (k + 1) * p.MainBarDiameter
                                     - (k + 0.5) * p.SpacerDiameter;
                    if (spacerR <= 0) break;
                    CreateHoops(pile, spacerR, p.SpacerDiameter, p.SpiralPitch,
                                minX, maxX, "Pile Spacer", 4);
                }

                // ── 4. Circular stirrup inside innermost layer ──
                if (p.CircStirrupEnabled && n > 0)
                {
                    double circR = R - p.Cover - p.SpiralDiameter
                                   - n * p.MainBarDiameter
                                   - (n - 1) * p.SpacerDiameter;
                    if (circR > 0)
                    {
                        CreateHoops(pile, circR, p.CircStirrupDiameter, p.CircStirrupPitch,
                                    minX, maxX, "Pile Stirrup", 3);
                    }
                }
            }
            finally
            {
                _model.GetWorkPlaneHandler().SetCurrentTransformationPlane(saved);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Continuous spiral (multi-turn helix)
        // ─────────────────────────────────────────────────────────────
        private void CreateSpiral(Beam pile, double radius, double barDia,
            double pitch, double minX, double maxX, string name, int cls)
        {
            if (radius <= 0) return;

            int turns = (int)Math.Ceiling((maxX - minX) / pitch);
            int segs = turns * Seg;
            if (segs < 2) return;

            var bar = new SingleRebar();
            bar.Father = pile;
            bar.Name = name;
            bar.Grade = "Undefined";
            bar.Size = barDia.ToString();
            bar.Class = cls;

            var poly = new Polygon();
            double da = 2.0 * Math.PI / Seg;
            double dx = pitch / Seg;

            for (int i = 0; i <= segs; i++)
            {
                double x = minX + i * dx;
                if (x > maxX) break;
                double a = i * da;
                poly.Points.Add(new Point(x, radius * Math.Cos(a), radius * Math.Sin(a)));
            }

            bar.Polygon = poly;
            bar.Insert();
        }

        // ─────────────────────────────────────────────────────────────
        //  Individual circular hoops at regular spacing.
        //  Each hoop = 1-turn helix with 50 mm advance so Tekla renders it.
        // ─────────────────────────────────────────────────────────────
        private void CreateHoops(Beam pile, double radius, double barDia,
            double spacing, double minX, double maxX, string name, int cls)
        {
            if (radius <= 0) return;

            double length = maxX - minX;
            int count = Math.Max(1, (int)Math.Floor(length / spacing));
            double start = minX + (length - (count - 1) * spacing) / 2.0;

            for (int h = 0; h < count; h++)
            {
                double xc = start + h * spacing;

                var bar = new SingleRebar();
                bar.Father = pile;
                bar.Name = name;
                bar.Grade = "Undefined";
                bar.Size = barDia.ToString();
                bar.Class = cls;

                var poly = new Polygon();
                double da = 2.0 * Math.PI / Seg;
                double dx = 50.0 / Seg;              // ~1.4 mm per segment
                double x0 = xc - 25.0;               // center hoop at xc

                for (int i = 0; i <= Seg; i++)
                {
                    double a = i * da;
                    poly.Points.Add(new Point(
                        x0 + i * dx,
                        radius * Math.Cos(a),
                        radius * Math.Sin(a)));
                }

                bar.Polygon = poly;
                bar.Insert();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Longitudinal bars in concentric circular layers
        // ─────────────────────────────────────────────────────────────
        private void CreateMainBars(Beam pile, double barDia, int count,
            double layerR, double minX, double maxX, int layerIdx)
        {
            if (count <= 0) return;

            double da = 2.0 * Math.PI / count;

            for (int i = 0; i < count; i++)
            {
                double a = i * da;
                double y = layerR * Math.Cos(a);
                double z = layerR * Math.Sin(a);

                var bar = new SingleRebar();
                bar.Father = pile;
                bar.Name = $"Pile Main L{layerIdx + 1}";
                bar.Grade = "Undefined";
                bar.Size = barDia.ToString();
                bar.Class = 7;

                var poly = new Polygon();
                poly.Points.Add(new Point(minX, y, z));
                poly.Points.Add(new Point(maxX, y, z));
                bar.Polygon = poly;
                bar.Insert();
            }
        }
    }
}
