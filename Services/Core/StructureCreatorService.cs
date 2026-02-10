using System;
using Tekla.Structures.Model;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation;
using TeklaPlugin.Services.Mat;
using TeklaPlugin.Services.Piles;
using TeklaPlugin.Services.Elevation;
using TeklaPlugin.Services.Cap;
using TeklaPlugin.Services.Buffer;
using TeklaPlugin.Services.Foundation.Models;
using TeklaPlugin.Services.Mat.Models;
using TeklaPlugin.Services.Piles.Models;
using TeklaPlugin.Services.Elevation.Models;
using TeklaPlugin.Services.Cap.Models;
using TeklaPlugin.Services.Buffer.Models;

namespace TeklaPlugin.Services.Core
{
    public class StructureCreatorService
    {
        private readonly Model _model;
        private readonly FoundationService _foundationService;
        private readonly MatService _matService;
        private readonly PilesService _pilesService;
        private readonly ElevationService _elevationService;
        private readonly CapService _capService;
        private readonly BufferService _bufferService;

        public StructureCreatorService(Model model)
        {
            _model = model;
            _foundationService = new FoundationService(model);
            _matService = new MatService(model);
            _pilesService = new PilesService(model);
            _elevationService = new ElevationService(model);
            _capService = new CapService(model);
            _bufferService = new BufferService(model);
        }

        public void CreateStructure(
            GlobalParameters global,
            TeklaPlugin.Services.Foundation.Models.FoundationParameters foundation,
            TeklaPlugin.Services.Mat.Models.MatParameters mat,
            TeklaPlugin.Services.Piles.Models.PileParameters piles,
            ElevationType elevationType,
            TeklaPlugin.Services.Elevation.Models.LamelarElevationParameters lamelarElevation,
            TeklaPlugin.Services.Elevation.Models.CircularElevationParameters circularElevation,
            TeklaPlugin.Services.Cap.Models.CapParameters cap,
            TeklaPlugin.Services.Buffer.Models.BufferParameters buffer)
        {
            try
            {
                // Create foundation
                _foundationService.CreateFoundation(global, foundation);

                // Create mat
                _matService.CreateMat(global, foundation, mat);

                // Create piles
                _pilesService.CreatePiles(global, foundation, piles);

                // Create elevation and get height for cap positioning
                double elevationHeight;
                if (elevationType == ElevationType.Lamelar)
                {
                    _elevationService.CreateElevationLamelar(global, lamelarElevation);
                    elevationHeight = lamelarElevation.Height;
                }
                else
                {
                    _elevationService.CreateElevationCircular(global, circularElevation);
                    elevationHeight = circularElevation.Height;
                }

                // Create cap
                _capService.CreateCap(global, cap, elevationHeight);

                // Create buffers on top of cap
                _bufferService.CreateBuffers(global, cap, buffer, elevationHeight);

                _model.CommitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating structure: {ex.Message}", ex);
            }
        }
    }
}
