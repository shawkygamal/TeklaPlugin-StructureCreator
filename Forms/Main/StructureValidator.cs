using System;
using System.Collections.Generic;
using System.Linq;
using TeklaPlugin.Services.Cap.Models;
using TeklaPlugin.Services.Elevation.Models;
using TeklaPlugin.Services.Foundation.Models;
using TeklaPlugin.Services.Piles.Models;
using TeklaPlugin.Services.Buffer.Models;

namespace TeklaPlugin.Forms.Main
{
    /// <summary>
    /// Holds the result of a cross-object validation pass.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; } = true;
        public List<string> Errors { get; } = new List<string>();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        /// <summary>
        /// Merge another ValidationResult into this one.
        /// </summary>
        public void Merge(ValidationResult other)
        {
            if (other == null) return;
            foreach (var err in other.Errors)
                AddError(err);
        }

        /// <summary>
        /// Returns all errors as a single string separated by newlines.
        /// </summary>
        public string GetSummary()
        {
            return string.Join(Environment.NewLine, Errors.Select((e, i) => $"{i + 1}. {e}"));
        }
    }

    /// <summary>
    /// Cross-object geometry validations for the structure creator.
    /// Ensures that child elements (columns, piles) do not exceed the
    /// boundaries of their parent elements (cap beam, foundation).
    /// </summary>
    public class StructureValidator
    {
        // ──────────────────────────────────────────────────────────────
        //  PUBLIC: run every validation at once
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Run all cross-object validations and return a combined result.
        /// </summary>
        public ValidationResult ValidateAll(
            ElevationType elevationType,
            LamelarElevationParameters lamelarParams,
            CircularElevationParameters circularParams,
            CapParameters capParams,
            PileParameters pileParams,
            FoundationParameters foundationParams,
            BufferParameters bufferParams)
        {
            var result = new ValidationResult();
            result.Merge(ValidateColumnsVsCapBeam(elevationType, lamelarParams, circularParams, capParams));
            result.Merge(ValidatePilesVsFoundation(pileParams, foundationParams));
            result.Merge(ValidateBuffersVsCapBeam(bufferParams, capParams));
            return result;
        }

        // ──────────────────────────────────────────────────────────────
        //  1. Columns vs Cap Beam
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that:
        ///   a) Column cross-section does not exceed cap beam width.
        ///   b) Adjacent columns do not overlap (spacing &lt; column size).
        ///   c) No column extends outside the cap beam bottom length.
        ///
        /// Geometry reference (local frame, before rotation):
        ///   Column i center X = OffsetX + Spacing * i − ((N−1) * Spacing) / 2
        ///   Cap beam bottom edge = [P − BottomLength/2  …  P + BottomLength/2]
        /// </summary>
        public ValidationResult ValidateColumnsVsCapBeam(
            ElevationType elevationType,
            LamelarElevationParameters lamelarParams,
            CircularElevationParameters circularParams,
            CapParameters capParams)
        {
            var result = new ValidationResult();

            // Resolve the active column parameters based on type
            int numColumns;
            double spacing;
            double columnSize;   // Width for lamelar, Diameter for circular
            double offsetX;
            string sizeName;

            if (elevationType == ElevationType.Lamelar)
            {
                numColumns = lamelarParams.NumberOfColumns;
                spacing = lamelarParams.DistanceBetweenColumns;
                columnSize = lamelarParams.Width;
                offsetX = lamelarParams.OffsetX;
                sizeName = "width";
            }
            else
            {
                numColumns = circularParams.NumberOfColumns;
                spacing = circularParams.DistanceBetweenColumns;
                columnSize = circularParams.Diameter;
                offsetX = circularParams.OffsetX;
                sizeName = "diameter";
            }

            // ── (a) Column cross-section vs cap width ─────────────────
            if (elevationType == ElevationType.Lamelar)
            {
                // For lamelar, check both width and thickness against cap width
                double maxDimension = Math.Max(lamelarParams.Width, lamelarParams.Thickness);
                if (maxDimension > capParams.Width)
                {
                    result.AddError(
                        $"Lamelar column cross-section ({lamelarParams.Width:F0} × {lamelarParams.Thickness:F0} mm) " +
                        $"exceeds cap beam width ({capParams.Width:F0} mm). " +
                        $"Reduce column width/thickness or increase cap beam width.");
                }
            }
            else
            {
                // For circular, check diameter against cap width
                if (circularParams.Diameter > capParams.Width)
                {
                    result.AddError(
                        $"Circular column diameter ({circularParams.Diameter:F0} mm) exceeds cap beam width " +
                        $"({capParams.Width:F0} mm). Reduce column diameter or increase cap beam width.");
                }
            }

            // ── (b) Column overlap check ──────────────────────────────
            if (numColumns > 1 && spacing < columnSize)
            {
                result.AddError(
                    $"Column overlap: spacing ({spacing:F0} mm) is less than column {sizeName} " +
                    $"({columnSize:F0} mm). Minimum spacing must be ≥ {columnSize:F0} mm.");
            }

            // ── (c) Columns inside cap beam bottom length ─────────────
            double capLeft = capParams.P - capParams.BottomLength / 2.0;
            double capRight = capParams.P + capParams.BottomLength / 2.0;

            for (int i = 0; i < numColumns; i++)
            {
                double colCenter = offsetX + spacing * i
                                   - ((numColumns - 1) * spacing) / 2.0;
                double colLeft = colCenter - columnSize / 2.0;
                double colRight = colCenter + columnSize / 2.0;

                if (colLeft < capLeft)
                {
                    result.AddError(
                        $"Column {i + 1} left edge ({colLeft:F0} mm) is outside cap beam " +
                        $"bottom left edge ({capLeft:F0} mm). " +
                        $"Reduce column count/spacing, adjust offset, or increase cap bottom length.");
                }

                if (colRight > capRight)
                {
                    result.AddError(
                        $"Column {i + 1} right edge ({colRight:F0} mm) is outside cap beam " +
                        $"bottom right edge ({capRight:F0} mm). " +
                        $"Reduce column count/spacing, adjust offset, or increase cap bottom length.");
                }
            }

            return result;
        }

        // ──────────────────────────────────────────────────────────────
        //  2. Piles vs Foundation
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that the pile grid (including pile diameter) fits
        /// within the foundation footprint.
        ///
        /// Geometry reference (local frame, before rotation):
        ///   Pile rows    spread along X → compared with Foundation.Length
        ///   Pile columns spread along Y → compared with Foundation.Width
        ///
        ///   Grid extent X = (Rows − 1) × RowDistance + Diameter
        ///   Grid extent Y = (Columns − 1) × ColumnDistance + Diameter
        /// </summary>
        public ValidationResult ValidatePilesVsFoundation(
            PileParameters pileParams,
            FoundationParameters foundationParams)
        {
            var result = new ValidationResult();

            // X direction: rows along foundation length
            double pileExtentX = (pileParams.Rows - 1) * pileParams.RowDistance
                                 + pileParams.Diameter;

            if (pileExtentX > foundationParams.Length)
            {
                result.AddError(
                    $"Pile grid row extent ({pileExtentX:F0} mm) exceeds foundation length " +
                    $"({foundationParams.Length:F0} mm). " +
                    $"Reduce rows ({pileParams.Rows}), row distance ({pileParams.RowDistance:F0} mm), " +
                    $"pile diameter ({pileParams.Diameter:F0} mm), or increase foundation length.");
            }

            // Y direction: columns along foundation width
            double pileExtentY = (pileParams.Columns - 1) * pileParams.ColumnDistance
                                 + pileParams.Diameter;

            if (pileExtentY > foundationParams.Width)
            {
                result.AddError(
                    $"Pile grid column extent ({pileExtentY:F0} mm) exceeds foundation width " +
                    $"({foundationParams.Width:F0} mm). " +
                    $"Reduce columns ({pileParams.Columns}), column distance ({pileParams.ColumnDistance:F0} mm), " +
                    $"pile diameter ({pileParams.Diameter:F0} mm), or increase foundation width.");
            }

            // Embedded length cannot exceed foundation height (thickness)
            if (pileParams.EmbeddedLength > foundationParams.Height)
            {
                result.AddError(
                    $"Pile embedded length ({pileParams.EmbeddedLength:F0} mm) exceeds foundation " +
                    $"thickness ({foundationParams.Height:F0} mm). " +
                    $"Reduce embedded length or increase foundation height.");
            }

            return result;
        }

        // ──────────────────────────────────────────────────────────────
        //  3. Buffers vs Cap Beam
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates that buffers fit within the cap beam length.
        /// Checks: leftOffset + N × Width + (N-1) × Spacing + rightOffset ≤ TopLength
        /// </summary>
        public ValidationResult ValidateBuffersVsCapBeam(
            BufferParameters bufferParams,
            CapParameters capParams)
        {
            var result = new ValidationResult();

            if (bufferParams.Number <= 0) return result;

            // Calculate total space needed with normal spacing
            double totalSpaceNeeded = bufferParams.LeftOffset 
                                    + bufferParams.Number * bufferParams.Width 
                                    + (bufferParams.Number - 1) * bufferParams.Spacing 
                                    + bufferParams.RightOffset;

            if (totalSpaceNeeded > capParams.TopLength)
            {
                result.AddError(
                    $"Buffers exceed cap beam: {bufferParams.Number} buffers with width {bufferParams.Width:F0} mm, " +
                    $"spacing {bufferParams.Spacing:F0} mm, left offset {bufferParams.LeftOffset:F0} mm, " +
                    $"and right offset {bufferParams.RightOffset:F0} mm require {totalSpaceNeeded:F0} mm " +
                    $"but cap top length is only {capParams.TopLength:F0} mm. " +
                    $"Reduce number of buffers, spacing, offsets, or buffer width.");
            }

            // Check minimum space needed (buffers touching, no spacing)
            double minSpaceNeeded = bufferParams.LeftOffset 
                                  + bufferParams.Number * bufferParams.Width 
                                  + bufferParams.RightOffset;

            if (minSpaceNeeded > capParams.TopLength)
            {
                result.AddError(
                    $"Too many buffers: Even with zero spacing, {bufferParams.Number} buffers of width " +
                    $"{bufferParams.Width:F0} mm with left offset {bufferParams.LeftOffset:F0} mm and " +
                    $"right offset {bufferParams.RightOffset:F0} mm require {minSpaceNeeded:F0} mm " +
                    $"but cap top length is only {capParams.TopLength:F0} mm. " +
                    $"Reduce number of buffers or buffer width.");
            }

            return result;
        }
    }
}
