using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Plugins;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using TeklaPlugin.Forms.Main;

namespace TeklaPlugin.Forms.Plugins
{
    [Plugin("StructureCreator")]
    [PluginUserInterface("TeklaPlugin.Forms.Main.StructureCreatorForm")]
    public class StructureCreatorPlugin : PluginBase
    {
        public StructureCreatorPlugin()
        {
            // Plugin initialization
        }

        public override List<InputDefinition> DefineInput()
        {
            // Define input for the plugin - no specific input needed for this plugin
            return new List<InputDefinition>();
        }

        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                // Show the form and create structure when button is clicked
                StructureCreatorForm form = new StructureCreatorForm();
                form.ShowDialog();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running plugin: {ex.Message}", "Plugin Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
