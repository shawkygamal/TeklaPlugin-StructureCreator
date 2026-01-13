using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;
using DrawingPoint = System.Drawing.Point;
using TeklaPoint = Tekla.Structures.Geometry3d.Point;
using TeklaPlugin.Services.Core;
using TeklaPlugin.Services.Core.Models;
using TeklaPlugin.Services.Foundation.Models;
using TeklaPlugin.Services.Mat.Models;
using TeklaPlugin.Services.Piles.Models;
using TeklaPlugin.Services.Elevation.Models;
using TeklaPlugin.Services.Cap.Models;
using TeklaPlugin.TeklaQueries;

namespace TeklaPlugin.Forms.Main
{
    public partial class StructureCreatorForm: Form
    {
        private TabControl tabControl;
        private Button createStructureButton;

        // Global Parameters
        private TextBox posXTextBox, posYTextBox, posZTextBox, rotationTextBox, skewTextBox;

        // Foundation Parameters
        private TextBox foundationWidthTextBox, foundationLengthTextBox, foundationHeightTextBox;

        // Mat Parameters
        private TextBox matCantileverTextBox, matThicknessTextBox;

        // Pile Parameters
        private TextBox pileRowsTextBox, pileColumnsTextBox, pileRowDistanceTextBox,
                       pileColumnDistanceTextBox, pileLengthTextBox, pileDiameterTextBox,
                       pileEmbeddedLengthTextBox;

        // Elevation Parameters
        private RadioButton lamelarRadioButton, circularRadioButton;
        private TextBox lamelarWidthTextBox, lamelarThicknessTextBox, lamelarHeightTextBox;
        private TextBox circularDiameterTextBox, circularHeightTextBox, circularColumnsTextBox,
                       circularDistanceTextBox, circularOffsetXTextBox, circularOffsetYTextBox;

        // Cap Parameters
        private TextBox capHTextBox, capBTextBox, capWTextBox, capPTextBox, capSlopeHeightTextBox;

        // Material Dropdowns
        private ComboBox foundationMaterialComboBox, matMaterialComboBox, pilesMaterialComboBox,
                         lamelarMaterialComboBox, circularMaterialComboBox, capMaterialComboBox;

        // Class Dropdowns
        private ComboBox foundationClassComboBox, matClassComboBox, pilesClassComboBox,
                         lamelarClassComboBox, circularClassComboBox, capClassComboBox;

        // Materials Service
        private MaterialsService _materialsService;

        public StructureCreatorForm()
        {
            InitializeComponent();
            InitializeCustomComponents();

            // Initialize materials service
            var model = new Tekla.Structures.Model.Model();
            _materialsService = new MaterialsService(model);

            // Load materials into dropdowns
            LoadMaterials();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Tekla Structure Creator";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += StructureCreatorForm_KeyDown;

            tabControl = new TabControl();
            tabControl.Size = new Size(750, 550);
            tabControl.Location = new DrawingPoint(20, 20);
            this.Controls.Add(tabControl);

            // Create tabs
            CreateGlobalTab();
            CreateFoundationTab();
            CreateMatTab();
            CreatePilesTab();
            CreateElevationTab();
            CreateCapTab();

            // Create button
            createStructureButton = new Button();
            createStructureButton.Text = "Create Structure";
            createStructureButton.Size = new Size(200, 50);
            createStructureButton.Location = new DrawingPoint(300, 580);
            createStructureButton.Font = new Font("Arial", 12, FontStyle.Bold);
            createStructureButton.Click += CreateStructureButton_Click;
            this.Controls.Add(createStructureButton);
        }

        private void CreateGlobalTab()
        {
            TabPage tab = new TabPage("Global");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Position X
            AddLabelAndTextBox(tab, "Position X:", ref posXTextBox, "0", 20, yPos);
            yPos += 30;

            // Position Y
            AddLabelAndTextBox(tab, "Position Y:", ref posYTextBox, "0", 20, yPos);
            yPos += 30;

            // Position Z
            AddLabelAndTextBox(tab, "Position Z:", ref posZTextBox, "0", 20, yPos);
            yPos += 30;

            // Rotation Angle
            AddLabelAndTextBox(tab, "Rotation Angle:", ref rotationTextBox, "0", 20, yPos);
            yPos += 30;

            // Skew Angle
            AddLabelAndTextBox(tab, "Skew Angle:", ref skewTextBox, "0", 20, yPos);
        }

        private void CreateFoundationTab()
        {
            TabPage tab = new TabPage("Foundation");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Width
            AddLabelAndTextBox(tab, "Width:", ref foundationWidthTextBox, "2000", 20, yPos);
            yPos += 30;

            // Length
            AddLabelAndTextBox(tab, "Length:", ref foundationLengthTextBox, "4000", 20, yPos);
            yPos += 30;

            // Height
            AddLabelAndTextBox(tab, "Height:", ref foundationHeightTextBox, "600", 20, yPos);
            yPos += 30;

            // Material
            AddLabelAndComboBox(tab, "Material:", ref foundationMaterialComboBox, 20, yPos);
            yPos += 30;

            // Class
            AddLabelAndComboBox(tab, "Class:", ref foundationClassComboBox, 20, yPos);
        }

        private void CreateMatTab()
        {
            TabPage tab = new TabPage("Mat");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Cantilever
            AddLabelAndTextBox(tab, "Cantilever:", ref matCantileverTextBox, "300", 20, yPos);
            yPos += 30;

            // Thickness
            AddLabelAndTextBox(tab, "Thickness:", ref matThicknessTextBox, "200", 20, yPos);
            yPos += 30;

            // Material
            AddLabelAndComboBox(tab, "Material:", ref matMaterialComboBox, 20, yPos);
            yPos += 30;

            // Class
            AddLabelAndComboBox(tab, "Class:", ref matClassComboBox, 20, yPos);
        }

        private void CreatePilesTab()
        {
            TabPage tab = new TabPage("Piles");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Rows
            AddLabelAndTextBox(tab, "Rows:", ref pileRowsTextBox, "3", 20, yPos);
            yPos += 30;

            // Columns
            AddLabelAndTextBox(tab, "Columns:", ref pileColumnsTextBox, "3", 20, yPos);
            yPos += 30;

            // Row Distance
            AddLabelAndTextBox(tab, "Row Distance:", ref pileRowDistanceTextBox, "2000", 20, yPos);
            yPos += 30;

            // Column Distance
            AddLabelAndTextBox(tab, "Column Distance:", ref pileColumnDistanceTextBox, "2000", 20, yPos);
            yPos += 30;

            // Length
            AddLabelAndTextBox(tab, "Length:", ref pileLengthTextBox, "12000", 20, yPos);
            yPos += 30;

            // Diameter
            AddLabelAndTextBox(tab, "Diameter:", ref pileDiameterTextBox, "600", 20, yPos);
            yPos += 30;

            // Embedded Length
            AddLabelAndTextBox(tab, "Embedded Length:", ref pileEmbeddedLengthTextBox, "2000", 20, yPos);
            yPos += 30;

            // Material
            AddLabelAndComboBox(tab, "Material:", ref pilesMaterialComboBox, 20, yPos);
            yPos += 30;

            // Class
            AddLabelAndComboBox(tab, "Class:", ref pilesClassComboBox, 20, yPos);
        }

        private void CreateElevationTab()
        {
            TabPage tab = new TabPage("Elevation");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Radio buttons for type selection
            lamelarRadioButton = new RadioButton();
            lamelarRadioButton.Text = "Lamelar";
            lamelarRadioButton.Location = new DrawingPoint(20, yPos);
            lamelarRadioButton.Checked = true;
            lamelarRadioButton.CheckedChanged += ElevationType_Changed;
            tab.Controls.Add(lamelarRadioButton);

            circularRadioButton = new RadioButton();
            circularRadioButton.Text = "Circular";
            circularRadioButton.Location = new DrawingPoint(120, yPos);
            circularRadioButton.CheckedChanged += ElevationType_Changed;
            tab.Controls.Add(circularRadioButton);

            yPos += 30;

            // Lamelar controls
            AddLabelAndTextBox(tab, "Width:", ref lamelarWidthTextBox, "400", 20, yPos);
            yPos += 30;
            AddLabelAndTextBox(tab, "Thickness:", ref lamelarThicknessTextBox, "300", 20, yPos);
            yPos += 30;
            AddLabelAndTextBox(tab, "Height:", ref lamelarHeightTextBox, "8000", 20, yPos);
            yPos += 30;
            AddLabelAndComboBox(tab, "Material:", ref lamelarMaterialComboBox, 20, yPos);
            yPos += 30;

            // Class
            AddLabelAndComboBox(tab, "Class:", ref lamelarClassComboBox, 20, yPos);
            yPos += 30;

            // Circular controls (initially hidden)
            yPos += 20; // Gap
            AddLabelAndTextBox(tab, "Diameter:", ref circularDiameterTextBox, "600", 20, yPos);
            circularDiameterTextBox.Visible = false;
            yPos += 30;
            AddLabelAndTextBox(tab, "Height:", ref circularHeightTextBox, "8000", 20, yPos);
            circularHeightTextBox.Visible = false;
            yPos += 30;
            AddLabelAndTextBox(tab, "Number of Columns:", ref circularColumnsTextBox, "4", 20, yPos);
            circularColumnsTextBox.Visible = false;
            yPos += 30;
            AddLabelAndTextBox(tab, "Distance Between Columns:", ref circularDistanceTextBox, "1500", 20, yPos);
            circularDistanceTextBox.Visible = false;
            yPos += 30;
            AddLabelAndTextBox(tab, "Offset X:", ref circularOffsetXTextBox, "0", 20, yPos);
            circularOffsetXTextBox.Visible = false;
            yPos += 30;
            AddLabelAndTextBox(tab, "Offset Y:", ref circularOffsetYTextBox, "0", 20, yPos);
            circularOffsetYTextBox.Visible = false;
            yPos += 30;
            AddLabelAndComboBox(tab, "Material:", ref circularMaterialComboBox, 20, yPos);
            circularMaterialComboBox.Visible = false;
            yPos += 30;
            AddLabelAndComboBox(tab, "Class:", ref circularClassComboBox, 20, yPos);
            circularClassComboBox.Visible = false;
        }

        private void CreateCapTab()
        {
            TabPage tab = new TabPage("Cap");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // H - Height (vertical dimension)
            AddLabelAndTextBox(tab, "H (Height):", ref capHTextBox, "500", 20, yPos);
            yPos += 30;

            // B - Top width (transverse dimension, visible in 2D view)
            AddLabelAndTextBox(tab, "B (Top Width):", ref capBTextBox, "2000", 20, yPos);
            yPos += 30;

            // W - Depth/width (longitudinal dimension, not visible in 2D front view)
            AddLabelAndTextBox(tab, "W (Depth):", ref capWTextBox, "4000", 20, yPos);
            yPos += 30;

            // P - Offset from column center
            AddLabelAndTextBox(tab, "P (Offset from Center):", ref capPTextBox, "0", 20, yPos);
            yPos += 30;

            // Slope Height - Where the trapezoidal slope starts
            AddLabelAndTextBox(tab, "Slope Height:", ref capSlopeHeightTextBox, "250", 20, yPos);
            yPos += 30;

            // Material
            AddLabelAndComboBox(tab, "Material:", ref capMaterialComboBox, 20, yPos);
            yPos += 30;

            // Class
            AddLabelAndComboBox(tab, "Class:", ref capClassComboBox, 20, yPos);
        }

        private void LoadMaterials()
        {
            try
            {
                var concreteMaterials = _materialsService.GetConcreteMaterials();

                if (concreteMaterials.Count == 0)
                {
                    // Fallback materials if Tekla catalog is not accessible
                    concreteMaterials.AddRange(new[] { "C12/15", "C16/20", "C20/25", "C25/30", "C30/37", "C35/45", "C40/50", "C45/55", "C50/60", "C55/67", "C60/75" });
                }

                // Populate all material dropdowns
                foundationMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());
                matMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());
                pilesMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());
                lamelarMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());
                circularMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());
                capMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());

                // Populate class dropdowns
                var commonClasses = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
                foundationClassComboBox.Items.AddRange(commonClasses);
                matClassComboBox.Items.AddRange(commonClasses);
                pilesClassComboBox.Items.AddRange(commonClasses);
                lamelarClassComboBox.Items.AddRange(commonClasses);
                circularClassComboBox.Items.AddRange(commonClasses);
                capClassComboBox.Items.AddRange(commonClasses);

                // Set default selections
                foundationMaterialComboBox.SelectedItem = "C50/60";
                foundationClassComboBox.SelectedItem = "8";
                matMaterialComboBox.SelectedItem = "C12/15";
                matClassComboBox.SelectedItem = "1";
                pilesMaterialComboBox.SelectedItem = "C50/60";
                pilesClassComboBox.SelectedItem = "8";
                lamelarMaterialComboBox.SelectedItem = "C50/60";
                lamelarClassComboBox.SelectedItem = "8";
                circularMaterialComboBox.SelectedItem = "C50/60";
                circularClassComboBox.SelectedItem = "8";
                capMaterialComboBox.SelectedItem = "C12/15";
                capClassComboBox.SelectedItem = "8";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading materials: {ex.Message}", "Material Loading Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddLabelAndTextBox(Control parent, string labelText, ref TextBox textBox, string defaultValue, int x, int y)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new DrawingPoint(x, y);
            label.Size = new Size(150, 20);
            parent.Controls.Add(label);

            textBox = new TextBox();
            textBox.Text = defaultValue;
            textBox.Location = new DrawingPoint(x + 160, y);
            textBox.Size = new Size(100, 20);
            parent.Controls.Add(textBox);
        }

        private void AddLabelAndComboBox(Control parent, string labelText, ref ComboBox comboBox, int x, int y)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new DrawingPoint(x, y);
            label.Size = new Size(150, 20);
            parent.Controls.Add(label);

            comboBox = new ComboBox();
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Location = new DrawingPoint(x + 160, y);
            comboBox.Size = new Size(120, 20);
            parent.Controls.Add(comboBox);
        }

        private void ElevationType_Changed(object sender, EventArgs e)
        {
            bool isLamelar = lamelarRadioButton.Checked;

            lamelarWidthTextBox.Visible = isLamelar;
            lamelarThicknessTextBox.Visible = isLamelar;
            lamelarHeightTextBox.Visible = isLamelar;
            lamelarMaterialComboBox.Visible = isLamelar;
            lamelarClassComboBox.Visible = isLamelar;

            circularDiameterTextBox.Visible = !isLamelar;
            circularHeightTextBox.Visible = !isLamelar;
            circularColumnsTextBox.Visible = !isLamelar;
            circularDistanceTextBox.Visible = !isLamelar;
            circularOffsetXTextBox.Visible = !isLamelar;
            circularOffsetYTextBox.Visible = !isLamelar;
            circularMaterialComboBox.Visible = !isLamelar;
            circularClassComboBox.Visible = !isLamelar;
        }

        private void CreateStructureButton_Click(object sender, EventArgs e)
        {
            CreateStructure();
        }

        private void StructureCreatorForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Create structure when 'S' key is pressed
            if (e.KeyCode == Keys.S)
            {
                CreateStructure();
            }
        }

        private void CreateStructure()
        {
            Model model = null;
            try
            {
                // Connect to Tekla model
                model = new Model();

                if (!model.GetConnectionStatus())
                {
                    MessageBox.Show("Unable to connect to Tekla Structures model. Please ensure Tekla Structures is running and a model is open.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create StructureCreatorService
                var structureCreatorService = new StructureCreatorService(model);

                // Collect parameters
                var globalParams = new GlobalParameters
                {
                    PositionX = double.Parse(posXTextBox.Text),
                    PositionY = double.Parse(posYTextBox.Text),
                    PositionZ = double.Parse(posZTextBox.Text),
                    RotationAngle = double.Parse(rotationTextBox.Text),
                    SkewAngle = double.Parse(skewTextBox.Text)
                };

                var foundationParams = new TeklaPlugin.Services.Foundation.Models.FoundationParameters
                {
                    Width = double.Parse(foundationWidthTextBox.Text),
                    Length = double.Parse(foundationLengthTextBox.Text),
                    Height = double.Parse(foundationHeightTextBox.Text),
                    Material = foundationMaterialComboBox.SelectedItem?.ToString() ?? "C50/60",
                    Class = foundationClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                var matParams = new TeklaPlugin.Services.Mat.Models.MatParameters
                {
                    Cantilever = double.Parse(matCantileverTextBox.Text),
                    Thickness = double.Parse(matThicknessTextBox.Text),
                    Material = matMaterialComboBox.SelectedItem?.ToString() ?? "C12/15",
                    Class = matClassComboBox.SelectedItem?.ToString() ?? "1"
                };

                var pileParams = new TeklaPlugin.Services.Piles.Models.PileParameters
                {
                    Rows = int.Parse(pileRowsTextBox.Text),
                    Columns = int.Parse(pileColumnsTextBox.Text),
                    RowDistance = double.Parse(pileRowDistanceTextBox.Text),
                    ColumnDistance = double.Parse(pileColumnDistanceTextBox.Text),
                    Length = double.Parse(pileLengthTextBox.Text),
                    Diameter = double.Parse(pileDiameterTextBox.Text),
                    EmbeddedLength = double.Parse(pileEmbeddedLengthTextBox.Text),
                    Material = pilesMaterialComboBox.SelectedItem?.ToString() ?? "C50/60",
                    Class = pilesClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                // Determine elevation type and collect parameters
                ElevationType elevationType = lamelarRadioButton.Checked ? ElevationType.Lamelar : ElevationType.Circular;

                var lamelarParams = new TeklaPlugin.Services.Elevation.Models.LamelarElevationParameters
                {
                    Width = double.Parse(lamelarWidthTextBox.Text),
                    Thickness = double.Parse(lamelarThicknessTextBox.Text),
                    Height = double.Parse(lamelarHeightTextBox.Text),
                    Material = lamelarMaterialComboBox.SelectedItem?.ToString() ?? "C50/60",
                    Class = lamelarClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                var circularParams = new TeklaPlugin.Services.Elevation.Models.CircularElevationParameters
                {
                    Diameter = double.Parse(circularDiameterTextBox.Text),
                    Height = double.Parse(circularHeightTextBox.Text),
                    NumberOfColumns = int.Parse(circularColumnsTextBox.Text),
                    DistanceBetweenColumns = double.Parse(circularDistanceTextBox.Text),
                    OffsetX = double.Parse(circularOffsetXTextBox.Text),
                    OffsetY = double.Parse(circularOffsetYTextBox.Text),
                    Material = circularMaterialComboBox.SelectedItem?.ToString() ?? "C50/60",
                    Class = circularClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                var capParams = new TeklaPlugin.Services.Cap.Models.CapParameters
                {
                    H = double.Parse(capHTextBox.Text),
                    B = double.Parse(capBTextBox.Text),
                    W = double.Parse(capWTextBox.Text),
                    P = double.Parse(capPTextBox.Text),
                    SlopeHeight = double.Parse(capSlopeHeightTextBox.Text),
                    Material = capMaterialComboBox.SelectedItem?.ToString() ?? "C12/15",
                    Class = capClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                // Create the complete structure using the unified service
                structureCreatorService.CreateStructure(
                    globalParams,
                    foundationParams,
                    matParams,
                    pileParams,
                    elevationType,
                    lamelarParams,
                    circularParams,
                    capParams);

                MessageBox.Show("Structure created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values for all parameters.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string errorType = "Error";
                string message = ex.Message;

                if (message.ToLower().Contains("connection") || message.ToLower().Contains("connect"))
                {
                    errorType = "Connection Error";
                    message += "\n\nPlease ensure Tekla Structures is running and try again.";
                }
                else if (message.ToLower().Contains("invalid") || message.ToLower().Contains("operation"))
                {
                    errorType = "Operation Error";
                    message += "\n\nPlease check that the model is properly opened and not in read-only mode.";
                }
                else
                {
                    message += "\n\nPlease check the Tekla Structures model and try again.";
                }

                MessageBox.Show($"Error creating structure: {message}", errorType, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}