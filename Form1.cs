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

namespace TeklaPlugin
{
    public partial class Form1: Form
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

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Tekla Structure Creator";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

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

        private void ElevationType_Changed(object sender, EventArgs e)
        {
            bool isLamelar = lamelarRadioButton.Checked;

            lamelarWidthTextBox.Visible = isLamelar;
            lamelarThicknessTextBox.Visible = isLamelar;
            lamelarHeightTextBox.Visible = isLamelar;

            circularDiameterTextBox.Visible = !isLamelar;
            circularHeightTextBox.Visible = !isLamelar;
            circularColumnsTextBox.Visible = !isLamelar;
            circularDistanceTextBox.Visible = !isLamelar;
            circularOffsetXTextBox.Visible = !isLamelar;
            circularOffsetYTextBox.Visible = !isLamelar;
        }

        private void CreateStructureButton_Click(object sender, EventArgs e)
        {
            CreateStructure();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
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

                // Create StructureService
                var structureService = new StructureService(model);

                // Collect parameters
                var globalParams = new GlobalParameters
                {
                    PositionX = double.Parse(posXTextBox.Text),
                    PositionY = double.Parse(posYTextBox.Text),
                    PositionZ = double.Parse(posZTextBox.Text),
                    RotationAngle = double.Parse(rotationTextBox.Text),
                    SkewAngle = double.Parse(skewTextBox.Text)
                };

                var foundationParams = new FoundationParameters
                {
                    Width = double.Parse(foundationWidthTextBox.Text),
                    Length = double.Parse(foundationLengthTextBox.Text),
                    Height = double.Parse(foundationHeightTextBox.Text)
                };

                var matParams = new MatParameters
                {
                    Cantilever = double.Parse(matCantileverTextBox.Text),
                    Thickness = double.Parse(matThicknessTextBox.Text)
                };

                var pileParams = new PileParameters
                {
                    Rows = int.Parse(pileRowsTextBox.Text),
                    Columns = int.Parse(pileColumnsTextBox.Text),
                    RowDistance = double.Parse(pileRowDistanceTextBox.Text),
                    ColumnDistance = double.Parse(pileColumnDistanceTextBox.Text),
                    Length = double.Parse(pileLengthTextBox.Text),
                    Diameter = double.Parse(pileDiameterTextBox.Text),
                    EmbeddedLength = double.Parse(pileEmbeddedLengthTextBox.Text)
                };

                var capParams = new CapParameters
                {
                    H = double.Parse(capHTextBox.Text),
                    B = double.Parse(capBTextBox.Text),
                    W = double.Parse(capWTextBox.Text),
                    P = double.Parse(capPTextBox.Text),
                    SlopeHeight = double.Parse(capSlopeHeightTextBox.Text)
                };

                // Create foundation
                structureService.CreateFoundation(globalParams, foundationParams);

                // Create mat
                structureService.CreateMat(globalParams, foundationParams, matParams);

                // Create piles
                structureService.CreatePiles(globalParams, foundationParams, pileParams);

                // Create elevation
                if (lamelarRadioButton.Checked)
                {
                    var lamelarParams = new LamelarElevationParameters
                    {
                        Width = double.Parse(lamelarWidthTextBox.Text),
                        Thickness = double.Parse(lamelarThicknessTextBox.Text),
                        Height = double.Parse(lamelarHeightTextBox.Text)
                    };
                    structureService.CreateElevationLamelar(globalParams, lamelarParams);
                }
                else
                {
                    var circularParams = new CircularElevationParameters
                    {
                        Diameter = double.Parse(circularDiameterTextBox.Text),
                        Height = double.Parse(circularHeightTextBox.Text),
                        NumberOfColumns = int.Parse(circularColumnsTextBox.Text),
                        DistanceBetweenColumns = double.Parse(circularDistanceTextBox.Text),
                        OffsetX = double.Parse(circularOffsetXTextBox.Text),
                        OffsetY = double.Parse(circularOffsetYTextBox.Text)
                    };
                    structureService.CreateElevationCircular(globalParams, circularParams);
                }

                // Create cap
                double elevationHeight = lamelarRadioButton.Checked ?
                    double.Parse(lamelarHeightTextBox.Text) :
                    double.Parse(circularHeightTextBox.Text);
                structureService.CreateCap(globalParams, capParams, elevationHeight);

                model.CommitChanges();
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