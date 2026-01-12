using System;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;

namespace Fundatie_Cub
{
    public class SimpleBeamCut
    {
        private readonly Model _model;

        public SimpleBeamCut()
        {
            _model = new Model();
        }

        public void CreateTaperedBeam(Point startPos, double lengthTop, double lengthBottom, double width, double height, double rotationAngle)
        {
            // 1. Create the Base Beam
            // We use the Maximum length to ensure we have enough material to cut from.
            double maxLength = Math.Max(lengthTop, lengthBottom);

            Beam beam = new Beam();
            beam.StartPoint = startPos;
            beam.EndPoint = new Point(startPos.X, startPos.Y, startPos.Z - height);

            // Profile: Width (Y) * MaxLength (X)
            // Note: We adhere to the user's implicit convention that "Length" (tapering dim) is X-axis
            beam.Profile = new Profile { ProfileString = $"{width}*{maxLength}" };
            beam.Material = new Material { MaterialString = "C50/60" };
            beam.Class = "8";

            // Positioning
            beam.Position.Rotation = Position.RotationEnum.FRONT;
            beam.Position.RotationOffset = -rotationAngle;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Name = "SimpleTaperedBeam";

            if (beam.Insert())
            {
                // 2. Apply Taper Cuts
                ApplyTaperCuts(beam, lengthTop, lengthBottom, height);

                _model.CommitChanges();
            }
        }

        private void ApplyTaperCuts(Part part, double lenTop, double lenBottom, double height)
        {
            Beam beam = part as Beam;
            if (beam == null) return;

            Point center = beam.StartPoint;
            // Rotation logic matches the Position.RotationOffset text
            double rotRad = -beam.Position.RotationOffset * Math.PI / 180.0;

            double topHalf = lenTop / 2.0;
            double botHalf = lenBottom / 2.0;

            // Cut Positive Side (Right side of X-axis)
            CreateTaperCutPlane(part, center, rotRad, topHalf, botHalf, height, isPositiveSide: true);

            // Cut Negative Side (Left side of X-axis)
            CreateTaperCutPlane(part, center, rotRad, topHalf, botHalf, height, isPositiveSide: false);
        }

        private void CreateTaperCutPlane(Part part, Point center, double rotRad, double topHalf, double botHalf, double height, bool isPositiveSide)
        {
            double sign = isPositiveSide ? 1.0 : -1.0;

            // 1. Define 3 points in Local Coordinates (X along Length, Z down)
            // Point A (Top Edge)
            double xA = sign * topHalf;
            double yA = 0;
            double zA = 0;

            // Point B (Bottom Edge)
            double xB = sign * botHalf;
            double yB = 0;
            double zB = -height;

            // Point C (Width definer - parallel to Y)
            double xC = xA;
            double yC = 500.0; // Offset in Width direction
            double zC = zA;

            // 2. Rotate to Global Coordinates
            Point pA = RotateAndTranslate(center, rotRad, xA, yA, zA);
            Point pB = RotateAndTranslate(center, rotRad, xB, yB, zB);
            Point pC = RotateAndTranslate(center, rotRad, xC, yC, zC);

            // 3. Define Vector Axes for Plane
            // Axis X: Along the slope (A -> B)
            Vector vecAB = new Vector(pB.X - pA.X, pB.Y - pA.Y, pB.Z - pA.Z);
            // Axis Y: Along width (A -> C)
            Vector vecAC = new Vector(pC.X - pA.X, pC.Y - pA.Y, pC.Z - pA.Z);

            // 4. Ensure Normal points Outward (to cut the excess)
            // Calculate normal direction
            Vector normal = vecAB.Cross(vecAC);

            // Check direction relative to "Outward" vector from center
            // Simple check: Is the normal pointing roughly in the 'sign' direction of X?
            Vector centerToOut = new Vector(pA.X - center.X, pA.Y - center.Y, 0); // Radial vector

            if (normal.Dot(centerToOut) < 0)
            {
                // Pointing Inward, valid material side. We want to remove the OTHER side.
                // Tekla CutPlane removes material on the POSITIVE Z of its local system.
                // So strictly speaking, we want the Normal to point OUTWARDS (towards the air).

                // If Dot < 0 (Inwards), flip Y axis to flip Normal.
                vecAC = new Vector(-vecAC.X, -vecAC.Y, -vecAC.Z);
            }

            // 5. Insert CutPlane
            CutPlane cut = new CutPlane();
            cut.Father = part;
            cut.Plane = new Plane();
            cut.Plane.Origin = pA;
            cut.Plane.AxisX = vecAB;
            cut.Plane.AxisY = vecAC;

            cut.Insert();
        }

        private Point RotateAndTranslate(Point center, double rotRad, double x, double y, double z)
        {
            // Rotation around Z-axis
            double xRot = x * Math.Cos(rotRad) - y * Math.Sin(rotRad);
            double yRot = x * Math.Sin(rotRad) + y * Math.Cos(rotRad);

            return new Point(center.X + xRot, center.Y + yRot, center.Z + z);
        }
    }
}
