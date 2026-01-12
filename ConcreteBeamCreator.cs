using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Plugins;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;

namespace TeklaPlugin
{
    public class StructureCreator
    {
        private readonly Model _model;
        private readonly StructureService _structureService;

        public StructureCreator(Model model)
        {
            _model = model;
            _structureService = new StructureService(model);
        }

        public void CreateStructure(GlobalParameters global, FoundationParameters foundation,
                                   MatParameters mat, PileParameters piles,
                                   bool isLamelarElevation, LamelarElevationParameters lamelarElevation,
                                   CircularElevationParameters circularElevation, CapParameters cap)
        {
            try
            {
                // Create foundation
                _structureService.CreateFoundation(global, foundation);

                // Create mat
                _structureService.CreateMat(global, foundation, mat);

                // Create piles
                _structureService.CreatePiles(global, foundation, piles);

                // Create elevation
                double elevationHeight;
                if (isLamelarElevation)
                {
                    _structureService.CreateElevationLamelar(global, lamelarElevation);
                    elevationHeight = lamelarElevation.Height;
                }
                else
                {
                    _structureService.CreateElevationCircular(global, circularElevation);
                    elevationHeight = circularElevation.Height;
                }

                // Create cap
                _structureService.CreateCap(global, cap, elevationHeight);

                _model.CommitChanges();
                MessageBox.Show("Structure created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating structure: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
