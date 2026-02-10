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
using TeklaPlugin.Services.Buffer.Models;
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
        private TextBox lamelarWidthTextBox, lamelarThicknessTextBox, lamelarHeightTextBox, lamelarNumberOfColumnsTextBox, lamelarDistanceTextBox, lamelarOffsetXTextBox, lamelarOffsetYTextBox;
        private TextBox circularDiameterTextBox, circularHeightTextBox, circularColumnsTextBox,
                       circularDistanceTextBox, circularOffsetXTextBox, circularOffsetYTextBox;

        // Cap Parameters
        private TextBox capTopLengthTextBox, capBottomLengthTextBox, capWidthTextBox, capDepthTextBox, capHeightDiffTextBox, capPTextBox;
        private TextBox capCutXTextBox, capCutYTextBox;
        private ComboBox capCutSideComboBox;

        // Buffer Parameters
        private TextBox bufferNumberTextBox, bufferSpacingTextBox, bufferLeftOffsetTextBox,
                       bufferRightOffsetTextBox, bufferWidthTextBox, bufferBreadthTextBox, bufferHeightsTextBox;
        private ComboBox bufferMaterialComboBox, bufferClassComboBox;

        // Material Dropdowns
        private ComboBox foundationMaterialComboBox, matMaterialComboBox, pilesMaterialComboBox,
                         lamelarMaterialComboBox, circularMaterialComboBox, capMaterialComboBox;

        // Class Dropdowns
        private ComboBox foundationClassComboBox, matClassComboBox, pilesClassComboBox,
                         lamelarClassComboBox, circularClassComboBox, capClassComboBox;

        // Materials Service
        private MaterialsService _materialsService;

        // UI Enhancement Controls
        private PictureBox structurePreviewBox;
        private PictureBox logoPictureBox;
        private Label titleLabel;
        private Label subtitleLabel;
        private Label copyrightLabel;

        private void StructureCreatorForm_Load(object sender, EventArgs e)
        {

        }

        private Panel headerPanel;

        public StructureCreatorForm()
        {
            InitializeComponent();
            InitializeCustomComponents();

            // Initialize materials service
            var model = new Tekla.Structures.Model.Model();
            _materialsService = new MaterialsService(model);

            // Load materials into dropdowns
            LoadMaterials();

            // Initialize modern UI components
            InitializeModernUI();

            // Set form icon
            SetFormIcon();

            // Apply modern styling
            ApplyModernStyling();

            // Add tooltips
            AddTooltips();

            // Add input validation
            AddInputValidation();

            // Initialize tab visualization
            UpdateTabVisualization();
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
            CreateBufferTab();

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




        private void CreateElevationTab()
        {
            TabPage tab = new TabPage("Elevation");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Type selection
            lamelarRadioButton = new RadioButton();
            lamelarRadioButton.Text = "Lamelar";
            lamelarRadioButton.Location = new System.Drawing.Point(20, yPos);
            lamelarRadioButton.Checked = true;
            lamelarRadioButton.CheckedChanged += ElevationType_Changed;
            tab.Controls.Add(lamelarRadioButton);

            circularRadioButton = new RadioButton();
            circularRadioButton.Text = "Circular";
            circularRadioButton.Location = new System.Drawing.Point(120, yPos);
            circularRadioButton.CheckedChanged += ElevationType_Changed;
            tab.Controls.Add(circularRadioButton);

            yPos += 35;

            // Lamelar properties (visible by default)
            AddLabelAndTextBox(tab, "Width (mm):", ref lamelarWidthTextBox, "400", 20, yPos);
            AddLabelAndTextBox(tab, "Thickness (mm):", ref lamelarThicknessTextBox, "300", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Height (mm):", ref lamelarHeightTextBox, "8000", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Number of Columns:", ref lamelarNumberOfColumnsTextBox, "1", 20, yPos);
            AddLabelAndTextBox(tab, "Distance Between (mm):", ref lamelarDistanceTextBox, "1000", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Offset X (mm):", ref lamelarOffsetXTextBox, "0", 20, yPos);
            AddLabelAndTextBox(tab, "Offset Y (mm):", ref lamelarOffsetYTextBox, "0", 310, yPos);
            yPos += 35;
            AddLabelAndComboBox(tab, "Material:", ref lamelarMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref lamelarClassComboBox, 200, yPos);

            // Circular properties (hidden by default)
            yPos += 40;
            AddLabelAndTextBox(tab, "Diameter (mm):", ref circularDiameterTextBox, "600", 20, yPos);
            AddLabelAndTextBox(tab, "Height (mm):", ref circularHeightTextBox, "8000", 310, yPos);
            circularDiameterTextBox.Visible = false;
            circularHeightTextBox.Visible = false;
            yPos += 35;
            AddLabelAndTextBox(tab, "Number of Columns:", ref circularColumnsTextBox, "4", 20, yPos);
            AddLabelAndTextBox(tab, "Distance (mm):", ref circularDistanceTextBox, "1500", 310, yPos);
            circularColumnsTextBox.Visible = false;
            circularDistanceTextBox.Visible = false;
            yPos += 35;
            AddLabelAndTextBox(tab, "Offset X (mm):", ref circularOffsetXTextBox, "0", 20, yPos);
            AddLabelAndTextBox(tab, "Offset Y (mm):", ref circularOffsetYTextBox, "0", 310, yPos);
            circularOffsetXTextBox.Visible = false;
            circularOffsetYTextBox.Visible = false;
            yPos += 35;
            AddLabelAndComboBox(tab, "Circular Material:", ref circularMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Circular Class:", ref circularClassComboBox, 200, yPos);
            circularMaterialComboBox.Visible = false;
            circularClassComboBox.Visible = false;
        }

        private void CreateCapTab()
        {
            TabPage tab = new TabPage("Cap");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Cap Dimensions
            AddLabelAndTextBox(tab, "Top Length - mm:", ref capTopLengthTextBox, "4000", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Bottom Length - mm:", ref capBottomLengthTextBox, "2000", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Width - mm:", ref capWidthTextBox, "600", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Depth - mm:", ref capDepthTextBox, "500", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Height Diff (slope) - mm:", ref capHeightDiffTextBox, "300", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "P (Offset from Center) - mm:", ref capPTextBox, "0", 20, yPos);
            yPos += 35;

            // Cross-section L-cut (0 = no cut)
            AddLabelAndTextBox(tab, "Cut X (width) - mm:", ref capCutXTextBox, "0", 20, yPos);
            AddLabelAndTextBox(tab, "Cut Y (height) - mm:", ref capCutYTextBox, "0", 310, yPos);
            yPos += 35;

            // Cut side selector
            Label cutSideLabel = new Label();
            cutSideLabel.Text = "Cut Side:";
            cutSideLabel.Location = new System.Drawing.Point(20, yPos);
            cutSideLabel.Size = new Size(150, 20);
            cutSideLabel.Font = new Font("Segoe UI", 9F);
            tab.Controls.Add(cutSideLabel);

            capCutSideComboBox = new ComboBox();
            capCutSideComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            capCutSideComboBox.Items.AddRange(new object[] { "Right", "Left" });
            capCutSideComboBox.SelectedItem = "Right";
            capCutSideComboBox.Location = new System.Drawing.Point(180, yPos);
            capCutSideComboBox.Size = new Size(120, 23);
            capCutSideComboBox.Font = new Font("Segoe UI", 9F);
            tab.Controls.Add(capCutSideComboBox);
            yPos += 35;

            // Material & Class
            AddLabelAndComboBox(tab, "Material:", ref capMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref capClassComboBox, 200, yPos);
        }

        private void CreateBufferTab()
        {
            TabPage tab = new TabPage("Buffer");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            AddLabelAndTextBox(tab, "Number of Buffers:", ref bufferNumberTextBox, "3", 20, yPos);
            AddLabelAndTextBox(tab, "Spacing (S) - mm:", ref bufferSpacingTextBox, "500", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Left Offset - mm:", ref bufferLeftOffsetTextBox, "200", 20, yPos);
            AddLabelAndTextBox(tab, "Right Offset - mm:", ref bufferRightOffsetTextBox, "200", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Width (W) - mm:", ref bufferWidthTextBox, "400", 20, yPos);
            AddLabelAndTextBox(tab, "Breadth (B) - mm:", ref bufferBreadthTextBox, "300", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Heights (comma sep) - mm:", ref bufferHeightsTextBox, "50,50,50", 20, yPos);
            yPos += 35;

            // Material & Class
            AddLabelAndComboBox(tab, "Material:", ref bufferMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref bufferClassComboBox, 200, yPos);
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
                bufferMaterialComboBox.Items.AddRange(concreteMaterials.ToArray());

                // Populate class dropdowns
                var commonClasses = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
                foundationClassComboBox.Items.AddRange(commonClasses);
                matClassComboBox.Items.AddRange(commonClasses);
                pilesClassComboBox.Items.AddRange(commonClasses);
                lamelarClassComboBox.Items.AddRange(commonClasses);
                circularClassComboBox.Items.AddRange(commonClasses);
                capClassComboBox.Items.AddRange(commonClasses);
                bufferClassComboBox.Items.AddRange(commonClasses);

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
                bufferMaterialComboBox.SelectedItem = "C12/15";
                bufferClassComboBox.SelectedItem = "8";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading materials: {ex.Message}", "Material Loading Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyModernStyling()
        {
            // Form styling
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Tab control styling
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            tabControl.BackColor = Color.White;

            // Header styling
            if (headerPanel != null)
            {
                headerPanel.BackColor = Color.LightGray;
                headerPanel.ForeColor = Color.Black;
            }

            if (titleLabel != null)
            {
                titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
                titleLabel.ForeColor = Color.Black;
            }
            
            if (subtitleLabel != null)
            {
                subtitleLabel.ForeColor = Color.FromArgb(80, 80, 80);
            }

            // Button styling
            if (createStructureButton != null)
            {
                createStructureButton.BackColor = Color.FromArgb(0, 120, 212);
                createStructureButton.ForeColor = Color.White;
                createStructureButton.FlatStyle = FlatStyle.Flat;
                createStructureButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                createStructureButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
            }

        }

        private void InitializeModernUI()
        {
            // Header Panel (taller to accommodate logo)
            headerPanel = new Panel();
            headerPanel.Size = new Size(this.ClientSize.Width, 90);
            headerPanel.Location = new System.Drawing.Point(0, 0);
            headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            headerPanel.BackColor = Color.LightGray;
            this.Controls.Add(headerPanel);

            // Logo PictureBox
            logoPictureBox = new PictureBox();
            logoPictureBox.Size = new Size(75, 75);
            logoPictureBox.Location = new System.Drawing.Point(15, 8);
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            
            // Load logo
            try
            {
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "InfraNovaLogo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    logoPictureBox.Image = Image.FromFile(logoPath);
                }
                else
                {
                    logoPictureBox.Image = CreateTextLogo();
                }
            }
            catch
            {
                logoPictureBox.Image = CreateTextLogo();
            }
            
            headerPanel.Controls.Add(logoPictureBox);

            // Title Label
            titleLabel = new Label();
            titleLabel.Text = "Tekla Structure Creator";
            titleLabel.Location = new System.Drawing.Point(100, 22);
            titleLabel.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            headerPanel.Controls.Add(titleLabel);

            // Subtitle
            subtitleLabel = new Label();
            subtitleLabel.Text = "Professional Foundation and Structural Element Design";
            subtitleLabel.Location = new System.Drawing.Point(100, 52);
            subtitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            subtitleLabel.ForeColor = Color.FromArgb(80, 80, 80);
            subtitleLabel.AutoSize = true;
            headerPanel.Controls.Add(subtitleLabel);

            // Company Branding
            var companyLabel = new Label();
            companyLabel.Text = "Powered by InfraNova";
            companyLabel.Location = new System.Drawing.Point(this.ClientSize.Width - 180, 30);
            companyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            companyLabel.ForeColor = Color.FromArgb(60, 60, 60);
            companyLabel.AutoSize = true;
            companyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(companyLabel);

            // Adjust tab control position
            tabControl.Location = new System.Drawing.Point(20, 100);
            tabControl.Size = new Size(this.ClientSize.Width - 240, this.ClientSize.Height - 190);
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;

            // Simple Visualization Panel
            var vizPanel = new Panel();
            vizPanel.Size = new Size(200, this.ClientSize.Height - 190);
            vizPanel.Location = new System.Drawing.Point(this.ClientSize.Width - 220, 100);
            vizPanel.BackColor = Color.White;
            vizPanel.BorderStyle = BorderStyle.FixedSingle;
            vizPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            this.Controls.Add(vizPanel);

            // Visualization PictureBox for per-tab graphs
            structurePreviewBox = new PictureBox();
            structurePreviewBox.Size = new Size(180, vizPanel.Height - 20);
            structurePreviewBox.Location = new System.Drawing.Point(10, 10);
            structurePreviewBox.BackColor = Color.FromArgb(250, 250, 250);
            structurePreviewBox.BorderStyle = BorderStyle.FixedSingle;
            vizPanel.Controls.Add(structurePreviewBox);

            // Copyright footer
            copyrightLabel = new Label();
            copyrightLabel.Text = "© 2026 InfraNova  |  All Rights Reserved  |  Professional Structural Engineering Tools";
            copyrightLabel.Location = new System.Drawing.Point(25, this.ClientSize.Height - 28);
            copyrightLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            copyrightLabel.ForeColor = Color.FromArgb(100, 100, 100);
            copyrightLabel.AutoSize = true;
            copyrightLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(copyrightLabel);

            // Version label
            var versionLabel = new Label();
            versionLabel.Text = "v1.0  |  Tekla Structures Integration";
            versionLabel.Location = new System.Drawing.Point(this.ClientSize.Width - 230, this.ClientSize.Height - 28);
            versionLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            versionLabel.ForeColor = Color.FromArgb(100, 100, 100);
            versionLabel.AutoSize = true;
            versionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.Add(versionLabel);

            // Create button
            createStructureButton.Location = new System.Drawing.Point(this.ClientSize.Width - 180, this.ClientSize.Height - 60);
            createStructureButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            // Form size
            this.MinimumSize = new Size(900, 750);
            this.Size = new Size(1100, 800);
        }

        private Bitmap CreateTextLogo()
        {
            // Create a simple text-based logo as fallback
            Bitmap logoBitmap = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(logoBitmap))
            {
                g.Clear(Color.FromArgb(0, 120, 212)); // InfraNova blue

                // Draw "IN" text
                using (Font logoFont = new Font("Arial", 16, FontStyle.Bold))
                {
                    g.DrawString("IN", logoFont, Brushes.White, 8, 15);
                }

                // Draw small "InfraNova" text
                using (Font smallFont = new Font("Arial", 6, FontStyle.Regular))
                {
                    g.DrawString("InfraNova", smallFont, Brushes.White, 2, 40);
                }
            }
            return logoBitmap;
        }

        private void SetFormIcon()
        {
            try
            {
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "InfraNovaLogo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    using (var logoImage = Image.FromFile(logoPath))
                    using (var iconBitmap = new Bitmap(logoImage, new Size(32, 32)))
                    {
                        IntPtr hIcon = iconBitmap.GetHicon();
                        this.Icon = Icon.FromHandle(hIcon);
                    }
                }
                else
                {
                    using (var fallbackBitmap = CreateTextLogo())
                    using (var iconBitmap = new Bitmap(fallbackBitmap, new Size(32, 32)))
                    {
                        IntPtr hIcon = iconBitmap.GetHicon();
                        this.Icon = Icon.FromHandle(hIcon);
                    }
                }
            }
            catch
            {
                // Use default icon if loading fails
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTabVisualization();
        }

        private void UpdateTabVisualization()
        {
            try
            {
                Bitmap vizImage = new Bitmap(structurePreviewBox.Width, structurePreviewBox.Height);
                using (Graphics g = Graphics.FromImage(vizImage))
                {
                    g.Clear(Color.FromArgb(250, 250, 250));
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    string currentTab = tabControl.SelectedTab?.Text ?? "Global";
                    DrawTabVisualization(g, structurePreviewBox.Width, structurePreviewBox.Height, currentTab);
                }

                structurePreviewBox.Image = vizImage;
                structurePreviewBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (Exception ex)
            {
                // Silently handle visualization errors
                using (Graphics g = Graphics.FromImage(new Bitmap(structurePreviewBox.Width, structurePreviewBox.Height)))
                {
                    g.Clear(Color.LightGray);
                    g.DrawString("Visualization\nunavailable", new Font("Segoe UI", 8), Brushes.Black, 10, 10);
                }
            }
        }

        private void DrawTabVisualization(Graphics g, int width, int height, string tabName)
        {
            Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI", 8);
            Brush titleBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
            Pen linePen = new Pen(Color.Gray, 1);

            // Draw tab title
            g.DrawString($"{tabName} Visualization", titleFont, titleBrush, 10, 10);

            switch (tabName)
            {
                case "Global":
                    DrawGlobalVisualization(g, width, height);
                    break;
                case "Foundation":
                    DrawFoundationVisualization(g, width, height);
                    break;
                case "Mat":
                    DrawMatVisualization(g, width, height);
                    break;
                case "Piles":
                    DrawPilesVisualization(g, width, height);
                    break;
                case "Elevation":
                    DrawElevationVisualization(g, width, height);
                    break;
                case "Cap":
                    DrawCapVisualization(g, width, height);
                    break;
                default:
                    g.DrawString("No visualization\navailable", labelFont, Brushes.Gray, 20, 50);
                    break;
            }
        }

        private void DrawGlobalVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Brush axisBrush = new SolidBrush(Color.Black);
            Pen axisPen = new Pen(Color.Black, 2);
            Pen gridPen = new Pen(Color.LightGray, 1);

            int centerX = width / 2;
            int centerY = height / 2;

            // Draw coordinate system
            g.DrawLine(axisPen, centerX, 40, centerX, height - 40); // Y axis
            g.DrawLine(axisPen, 20, centerY, width - 20, centerY); // X axis

            // Draw arrows
            g.DrawLine(axisPen, centerX, 40, centerX - 5, 50); // Y arrow
            g.DrawLine(axisPen, centerX, 40, centerX + 5, 50);
            g.DrawLine(axisPen, width - 20, centerY, width - 30, centerY - 5); // X arrow
            g.DrawLine(axisPen, width - 20, centerY, width - 30, centerY + 5);

            // Labels
            g.DrawString("Y", new Font("Segoe UI", 9, FontStyle.Bold), axisBrush, centerX + 5, 35);
            g.DrawString("X", new Font("Segoe UI", 9, FontStyle.Bold), axisBrush, width - 25, centerY - 20);

            // Grid lines
            for (int i = 1; i < 5; i++)
            {
                int gridY = 40 + (height - 80) * i / 5;
                g.DrawLine(gridPen, centerX - 10, gridY, centerX + 10, gridY);
            }

            g.DrawString("Global Coordinate System", new Font("Segoe UI", 8), Brushes.Gray, 20, height - 30);
        }

        private void DrawFoundationVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Font paramFont = new Font("Segoe UI", 7, FontStyle.Bold);
            Brush foundationBrush = new SolidBrush(Color.FromArgb(139, 69, 19));
            Pen outlinePen = new Pen(Color.Black, 2);
            Pen dimensionPen = new Pen(Color.Blue, 1);

            int centerX = width / 2;
            int centerY = height / 2;

            // Draw foundation as a 3D-like box
            int boxWidth = 100;
            int boxHeight = 30;
            int boxDepth = 40;

            // Front face
            g.FillRectangle(foundationBrush, centerX - boxWidth/2, centerY - boxHeight/2, boxWidth, boxHeight);
            g.DrawRectangle(outlinePen, centerX - boxWidth/2, centerY - boxHeight/2, boxWidth, boxHeight);

            // Top face
            System.Drawing.Point[] topFace = {
                new System.Drawing.Point(centerX - boxWidth/2, centerY - boxHeight/2),
                new System.Drawing.Point(centerX - boxWidth/2 + boxDepth/2, centerY - boxHeight/2 - boxDepth/2),
                new System.Drawing.Point(centerX + boxWidth/2 + boxDepth/2, centerY - boxHeight/2 - boxDepth/2),
                new System.Drawing.Point(centerX + boxWidth/2, centerY - boxHeight/2)
            };
            g.FillPolygon(foundationBrush, topFace);
            g.DrawPolygon(outlinePen, topFace);

            // Side face
            System.Drawing.Point[] sideFace = {
                new System.Drawing.Point(centerX + boxWidth/2, centerY - boxHeight/2),
                new System.Drawing.Point(centerX + boxWidth/2 + boxDepth/2, centerY - boxHeight/2 - boxDepth/2),
                new System.Drawing.Point(centerX + boxWidth/2 + boxDepth/2, centerY + boxHeight/2 - boxDepth/2),
                new System.Drawing.Point(centerX + boxWidth/2, centerY + boxHeight/2)
            };
            g.FillPolygon(foundationBrush, sideFace);
            g.DrawPolygon(outlinePen, sideFace);

            // Add dimension labels
            // Width dimension
            g.DrawLine(dimensionPen, centerX - boxWidth/2 - 15, centerY + boxHeight/2 + 10,
                     centerX + boxWidth/2 + 15, centerY + boxHeight/2 + 10);
            g.DrawLine(dimensionPen, centerX - boxWidth/2 - 15, centerY + boxHeight/2 + 5,
                     centerX - boxWidth/2 - 15, centerY + boxHeight/2 + 15);
            g.DrawLine(dimensionPen, centerX + boxWidth/2 + 15, centerY + boxHeight/2 + 5,
                     centerX + boxWidth/2 + 15, centerY + boxHeight/2 + 15);
            g.DrawString("Width", paramFont, Brushes.Blue, centerX - 20, centerY + boxHeight/2 + 12);

            // Length dimension (depth)
            g.DrawLine(dimensionPen, centerX + boxWidth/2 + 25, centerY - boxHeight/2,
                     centerX + boxWidth/2 + 25, centerY + boxHeight/2);
            g.DrawLine(dimensionPen, centerX + boxWidth/2 + 20, centerY - boxHeight/2,
                     centerX + boxWidth/2 + 30, centerY - boxHeight/2);
            g.DrawLine(dimensionPen, centerX + boxWidth/2 + 20, centerY + boxHeight/2,
                     centerX + boxWidth/2 + 30, centerY + boxHeight/2);
            g.DrawString("Length", paramFont, Brushes.Blue, centerX + boxWidth/2 + 35, centerY - 5);

            // Height dimension
            g.DrawLine(dimensionPen, centerX - boxWidth/2 - 25, centerY - boxHeight/2,
                     centerX - boxWidth/2 - 25, centerY + boxHeight/2);
            g.DrawLine(dimensionPen, centerX - boxWidth/2 - 30, centerY - boxHeight/2,
                     centerX - boxWidth/2 - 20, centerY - boxHeight/2);
            g.DrawLine(dimensionPen, centerX - boxWidth/2 - 30, centerY + boxHeight/2,
                     centerX - boxWidth/2 - 20, centerY + boxHeight/2);
            // Rotate and draw "Height" text vertically
            System.Drawing.Drawing2D.Matrix matrix = g.Transform;
            g.RotateTransform(-90);
            g.DrawString("Height", paramFont, Brushes.Blue, -(centerY + 10), centerX - boxWidth/2 - 35);
            g.Transform = matrix;

            g.DrawString("Foundation Block", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            g.DrawString("3D View with Dimensions", labelFont, Brushes.Gray, 20, height - 25);
        }

        private void DrawMatVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Font paramFont = new Font("Segoe UI", 7, FontStyle.Bold);
            Brush matBrush = new SolidBrush(Color.FromArgb(105, 105, 105));
            Brush foundationBrush = new SolidBrush(Color.FromArgb(139, 69, 19, 100)); // Semi-transparent foundation
            Pen outlinePen = new Pen(Color.Black, 2);
            Pen dimensionPen = new Pen(Color.Green, 1);

            int centerX = width / 2;
            int centerY = height / 2;

            // First draw foundation outline to show relationship
            int foundationWidth = 80;
            int foundationHeight = 20;
            g.FillRectangle(foundationBrush, centerX - foundationWidth/2, centerY + 30, foundationWidth, foundationHeight);
            g.DrawRectangle(new Pen(Color.Black, 1), centerX - foundationWidth/2, centerY + 30, foundationWidth, foundationHeight);
            g.DrawString("Foundation", new Font("Segoe UI", 7), Brushes.Black, centerX - 30, centerY + 55);

            // Draw mat as a flat slab above foundation with cantilever
            int matWidth = 120;
            int matLength = 80;
            int cantilever = 20;

            // Main mat area (above foundation)
            g.FillRectangle(matBrush, centerX - matWidth/2, centerY - matLength/2, matWidth, matLength);
            g.DrawRectangle(outlinePen, centerX - matWidth/2, centerY - matLength/2, matWidth, matLength);

            // Cantilever extensions
            g.FillRectangle(matBrush, centerX - matWidth/2 - cantilever, centerY - matLength/2, cantilever, matLength);
            g.DrawRectangle(outlinePen, centerX - matWidth/2 - cantilever, centerY - matLength/2, cantilever, matLength);

            g.FillRectangle(matBrush, centerX + matWidth/2, centerY - matLength/2, cantilever, matLength);
            g.DrawRectangle(outlinePen, centerX + matWidth/2, centerY - matLength/2, cantilever, matLength);

            g.FillRectangle(matBrush, centerX - matWidth/2, centerY - matLength/2 - cantilever, matWidth, cantilever);
            g.DrawRectangle(outlinePen, centerX - matWidth/2, centerY - matLength/2 - cantilever, matWidth, cantilever);

            g.FillRectangle(matBrush, centerX - matWidth/2, centerY + matLength/2, matWidth, cantilever);
            g.DrawRectangle(outlinePen, centerX - matWidth/2, centerY + matLength/2, matWidth, cantilever);

            // Add dimension labels for cantilever
            // Horizontal cantilever dimension
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever - 15, centerY + matLength/2 + 10,
                     centerX - matWidth/2 - cantilever + cantilever + 15, centerY + matLength/2 + 10);
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever - 15, centerY + matLength/2 + 5,
                     centerX - matWidth/2 - cantilever - 15, centerY + matLength/2 + 15);
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever + cantilever + 15, centerY + matLength/2 + 5,
                     centerX - matWidth/2 - cantilever + cantilever + 15, centerY + matLength/2 + 15);
            g.DrawString("Cantilever", paramFont, Brushes.Green, centerX - matWidth/2 - cantilever - 10, centerY + matLength/2 + 12);

            // Vertical cantilever dimension
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever - 25, centerY - matLength/2,
                     centerX - matWidth/2 - cantilever - 25, centerY - matLength/2 + cantilever);
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever - 30, centerY - matLength/2,
                     centerX - matWidth/2 - cantilever - 20, centerY - matLength/2);
            g.DrawLine(dimensionPen, centerX - matWidth/2 - cantilever - 30, centerY - matLength/2 + cantilever,
                     centerX - matWidth/2 - cantilever - 20, centerY - matLength/2 + cantilever);
            // Rotate and draw "Cantilever" text vertically
            System.Drawing.Drawing2D.Matrix matrix = g.Transform;
            g.RotateTransform(-90);
            g.DrawString("Cantilever", paramFont, Brushes.Green, -(centerY - matLength/2 + cantilever/2), centerX - matWidth/2 - cantilever - 35);
            g.Transform = matrix;

            // Thickness dimension (between mat and foundation)
            g.DrawLine(dimensionPen, centerX + matWidth/2 + 25, centerY - matLength/2,
                     centerX + matWidth/2 + 25, centerY + 30);
            g.DrawLine(dimensionPen, centerX + matWidth/2 + 20, centerY - matLength/2,
                     centerX + matWidth/2 + 30, centerY - matLength/2);
            g.DrawLine(dimensionPen, centerX + matWidth/2 + 20, centerY + 30,
                     centerX + matWidth/2 + 30, centerY + 30);
            g.DrawString("Thickness", paramFont, Brushes.Green, centerX + matWidth/2 + 35, centerY - 5);

            g.DrawString("Mat Foundation", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            g.DrawString("Above Foundation with Cantilevers", labelFont, Brushes.Gray, 20, height - 25);
        }

        private void DrawPilesVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Font paramFont = new Font("Segoe UI", 7, FontStyle.Bold);
            Brush pileBrush = new SolidBrush(Color.FromArgb(169, 169, 169));
            Brush foundationBrush = new SolidBrush(Color.FromArgb(139, 69, 19, 100)); // Semi-transparent foundation
            Pen outlinePen = new Pen(Color.Black, 2);
            Pen dimensionPen = new Pen(Color.Purple, 1);

            int centerX = width / 2;
            int spacing = 25;

            // Draw foundation outline
            int foundationWidth = 80;
            int foundationHeight = 20;
            g.FillRectangle(foundationBrush, centerX - foundationWidth/2, 80, foundationWidth, foundationHeight);
            g.DrawRectangle(new Pen(Color.Black, 1), centerX - foundationWidth/2, 80, foundationWidth, foundationHeight);
            g.DrawString("Foundation", new Font("Segoe UI", 7), Brushes.Black, centerX - 30, 105);

            // Draw pile grid (3x3) extending downward from foundation
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int pileX = centerX - spacing + col * spacing;
                    int pileY = 120 + row * spacing;

                    // Draw pile as circle
                    g.FillEllipse(pileBrush, pileX - 8, pileY - 8, 16, 16);
                    g.DrawEllipse(outlinePen, pileX - 8, pileY - 8, 16, 16);
                }
            }

            // Add dimension labels
            // Row spacing
            g.DrawLine(dimensionPen, centerX - spacing - 15, 125, centerX - spacing - 15, 125 + spacing);
            g.DrawLine(dimensionPen, centerX - spacing - 20, 125, centerX - spacing - 10, 125);
            g.DrawLine(dimensionPen, centerX - spacing - 20, 125 + spacing, centerX - spacing - 10, 125 + spacing);
            g.DrawString("Row", paramFont, Brushes.Purple, centerX - spacing - 45, 125 + spacing/2 - 5);
            g.DrawString("Spacing", paramFont, Brushes.Purple, centerX - spacing - 45, 125 + spacing/2 + 5);

            // Column spacing
            g.DrawLine(dimensionPen, centerX - spacing, 120 - 15, centerX - spacing + spacing, 120 - 15);
            g.DrawLine(dimensionPen, centerX - spacing, 120 - 20, centerX - spacing, 120 - 10);
            g.DrawLine(dimensionPen, centerX - spacing + spacing, 120 - 20, centerX - spacing + spacing, 120 - 10);
            g.DrawString("Column", paramFont, Brushes.Purple, centerX - spacing + spacing/2 - 20, 120 - 35);
            g.DrawString("Spacing", paramFont, Brushes.Purple, centerX - spacing + spacing/2 - 20, 120 - 25);

            // Pile diameter
            g.DrawEllipse(dimensionPen, centerX + spacing + 10, 120, 16, 16);
            g.DrawString("Diameter", paramFont, Brushes.Purple, centerX + spacing + 30, 128);

            g.DrawString("Pile Layout Grid", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            g.DrawString("3×3 Pile Arrangement with Spacing", labelFont, Brushes.Gray, 20, height - 25);
        }

        private void DrawElevationVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Font paramFont = new Font("Segoe UI", 7, FontStyle.Bold);
            Brush columnBrush = new SolidBrush(Color.FromArgb(70, 130, 180));
            Brush foundationBrush = new SolidBrush(Color.FromArgb(139, 69, 19, 100)); // Semi-transparent foundation
            Pen outlinePen = new Pen(Color.Black, 2);
            Pen dimensionPen = new Pen(Color.Red, 1);

            int centerX = width / 2;
            int columnHeight = 80;

            // Draw foundation outline at bottom
            int foundationWidth = 60;
            int foundationHeight = 15;
            g.FillRectangle(foundationBrush, centerX - foundationWidth/2, height - 40, foundationWidth, foundationHeight);
            g.DrawRectangle(new Pen(Color.Black, 1), centerX - foundationWidth/2, height - 40, foundationWidth, foundationHeight);
            g.DrawString("Foundation", new Font("Segoe UI", 7), Brushes.Black, centerX - 25, height - 20);

            if (lamelarRadioButton.Checked)
            {
                // Draw single lamelar column above foundation
                int columnWidth = 20;
                g.FillRectangle(columnBrush, centerX - columnWidth/2, height/2 - columnHeight, columnWidth, columnHeight);
                g.DrawRectangle(outlinePen, centerX - columnWidth/2, height/2 - columnHeight, columnWidth, columnHeight);

                // Add dimensions
                // Width
                g.DrawLine(dimensionPen, centerX - columnWidth/2 - 15, height/2 + 5,
                         centerX + columnWidth/2 + 15, height/2 + 5);
                g.DrawLine(dimensionPen, centerX - columnWidth/2 - 15, height/2, centerX - columnWidth/2 - 15, height/2 + 10);
                g.DrawLine(dimensionPen, centerX + columnWidth/2 + 15, height/2, centerX + columnWidth/2 + 15, height/2 + 10);
                g.DrawString("Width", paramFont, Brushes.Red, centerX - 18, height/2 + 7);

                // Height
                g.DrawLine(dimensionPen, centerX + columnWidth/2 + 25, height/2 - columnHeight,
                         centerX + columnWidth/2 + 25, height/2);
                g.DrawLine(dimensionPen, centerX + columnWidth/2 + 20, height/2 - columnHeight,
                         centerX + columnWidth/2 + 30, height/2 - columnHeight);
                g.DrawLine(dimensionPen, centerX + columnWidth/2 + 20, height/2,
                         centerX + columnWidth/2 + 30, height/2);
                g.DrawString("Height", paramFont, Brushes.Red, centerX + columnWidth/2 + 35, height/2 - columnHeight/2 - 5);

                g.DrawString("Lamelar Column", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            }
            else
            {
                // Draw multiple circular columns above foundation
                int spacing = 30;
                for (int i = 0; i < 3; i++)
                {
                    int columnX = centerX - spacing + i * spacing;
                    g.FillEllipse(columnBrush, columnX - 8, height/2 - columnHeight, 16, columnHeight);
                    g.DrawEllipse(outlinePen, columnX - 8, height/2 - columnHeight, 16, columnHeight);
                }

                // Add dimensions
                // Diameter
                g.DrawEllipse(dimensionPen, centerX + spacing + 20, height/2 - 10, 16, 16);
                g.DrawString("Diameter", paramFont, Brushes.Red, centerX + spacing + 40, height/2 - 2);

                // Distance between columns
                g.DrawLine(dimensionPen, centerX - spacing + 8, height/2 + 15,
                         centerX - spacing + spacing - 8, height/2 + 15);
                g.DrawLine(dimensionPen, centerX - spacing + 8, height/2 + 10, centerX - spacing + 8, height/2 + 20);
                g.DrawLine(dimensionPen, centerX - spacing + spacing - 8, height/2 + 10,
                         centerX - spacing + spacing - 8, height/2 + 20);
                g.DrawString("Distance", paramFont, Brushes.Red, centerX - 18, height/2 + 17);

                // Height
                g.DrawLine(dimensionPen, centerX + spacing + 25, height/2 - columnHeight,
                         centerX + spacing + 25, height/2);
                g.DrawLine(dimensionPen, centerX + spacing + 20, height/2 - columnHeight,
                         centerX + spacing + 30, height/2 - columnHeight);
                g.DrawLine(dimensionPen, centerX + spacing + 20, height/2,
                         centerX + spacing + 30, height/2);
                g.DrawString("Height", paramFont, Brushes.Red, centerX + spacing + 35, height/2 - columnHeight/2 - 5);

                g.DrawString("Circular Columns", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            }

            g.DrawString("Vertical Structural Elements", labelFont, Brushes.Gray, 20, height - 25);
        }

        private void DrawCapVisualization(Graphics g, int width, int height)
        {
            Font labelFont = new Font("Segoe UI", 8);
            Font paramFont = new Font("Segoe UI", 7, FontStyle.Bold);
            Brush capBrush = new SolidBrush(Color.FromArgb(0, 100, 0));
            Brush columnBrush = new SolidBrush(Color.FromArgb(70, 130, 180, 100)); // Semi-transparent column
            Pen outlinePen = new Pen(Color.Black, 2);
            Pen dimensionPen = new Pen(Color.Orange, 1);

            int centerX = width / 2;
            int centerY = height / 2;

            // Draw column outline below cap
            int columnWidth = 20;
            int columnHeight = 60;
            int capBottom = centerY + 10;
            g.FillRectangle(columnBrush, centerX - columnWidth / 2, capBottom, columnWidth, columnHeight);
            g.DrawRectangle(new Pen(Color.Black, 1), centerX - columnWidth / 2, capBottom, columnWidth, columnHeight);
            g.DrawString("Column", new Font("Segoe UI", 7), Brushes.Black, centerX - 15, capBottom + columnHeight + 5);

            // New shape: rectangular top + sloped bottom
            int topHalf = 60;   // half of top length (drawing scale)
            int botHalf = 30;   // half of bottom length (drawing scale)
            int depthH = 20;    // depth of rectangular portion
            int slopeH = 15;    // height diff (slope portion)

            int capTop = capBottom - depthH - slopeH;

            // Shape: top-left, top-right, depth-right, slope-bottom-right, slope-bottom-left, depth-left
            System.Drawing.Point[] capShape = {
                new System.Drawing.Point(centerX - topHalf, capTop),                      // Top left
                new System.Drawing.Point(centerX + topHalf, capTop),                      // Top right
                new System.Drawing.Point(centerX + topHalf, capTop + depthH),             // Depth right
                new System.Drawing.Point(centerX + botHalf, capTop + depthH + slopeH),    // Bottom right (slope end)
                new System.Drawing.Point(centerX - botHalf, capTop + depthH + slopeH),    // Bottom left (slope end)
                new System.Drawing.Point(centerX - topHalf, capTop + depthH)               // Depth left
            };

            g.FillPolygon(capBrush, capShape);
            g.DrawPolygon(outlinePen, capShape);

            // Add dimensions
            // Top Length
            g.DrawLine(dimensionPen, centerX - topHalf, capTop - 10, centerX + topHalf, capTop - 10);
            g.DrawLine(dimensionPen, centerX - topHalf, capTop - 15, centerX - topHalf, capTop - 5);
            g.DrawLine(dimensionPen, centerX + topHalf, capTop - 15, centerX + topHalf, capTop - 5);
            g.DrawString("Top Length", paramFont, Brushes.Orange, centerX - 25, capTop - 22);

            // Bottom Length
            int botY = capTop + depthH + slopeH;
            g.DrawLine(dimensionPen, centerX - botHalf, botY + 5, centerX + botHalf, botY + 5);
            g.DrawLine(dimensionPen, centerX - botHalf, botY, centerX - botHalf, botY + 10);
            g.DrawLine(dimensionPen, centerX + botHalf, botY, centerX + botHalf, botY + 10);
            g.DrawString("Bottom Length", paramFont, Brushes.Orange, centerX - 30, botY + 8);

            // Depth (right side)
            int dimX = centerX + topHalf + 10;
            g.DrawLine(dimensionPen, dimX, capTop, dimX, capTop + depthH);
            g.DrawLine(dimensionPen, dimX - 5, capTop, dimX + 5, capTop);
            g.DrawLine(dimensionPen, dimX - 5, capTop + depthH, dimX + 5, capTop + depthH);
            g.DrawString("Depth", paramFont, Brushes.Red, dimX + 8, capTop + depthH / 2 - 5);

            // Height Diff (right side, below depth)
            g.DrawLine(dimensionPen, dimX, capTop + depthH, dimX, botY);
            g.DrawLine(dimensionPen, dimX - 5, botY, dimX + 5, botY);
            g.DrawString("Height Diff", paramFont, Brushes.Red, dimX + 8, capTop + depthH + slopeH / 2 - 5);

            // Slope indicator
            g.DrawLine(new Pen(Color.Red, 1), centerX - topHalf, capTop + depthH, centerX - botHalf, botY);
            g.DrawLine(new Pen(Color.Red, 1), centerX + topHalf, capTop + depthH, centerX + botHalf, botY);

            g.DrawString("Cap Beam", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Black, 20, height - 40);
            g.DrawString("Rect top + Sloped bottom", labelFont, Brushes.Gray, 20, height - 25);
        }

        private void DrawDimensionLines(Graphics g, int width, int height, double scale, int centerX, float groundY)
        {
            Pen dimPen = new Pen(Color.Blue, 1);
            Font dimFont = new Font("Segoe UI", 6);

            // Horizontal dimension line for foundation width
            float dimY = groundY + 40;
            g.DrawLine(dimPen, centerX - 100, dimY, centerX + 100, dimY);
            g.DrawLine(dimPen, centerX - 100, dimY - 5, centerX - 100, dimY + 5);
            g.DrawLine(dimPen, centerX + 100, dimY - 5, centerX + 100, dimY + 5);
            g.DrawString("Foundation Width", dimFont, Brushes.Blue, centerX - 45, dimY + 8);
        }

        private double ParseDouble(string text, double defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text)) return defaultValue;
            return double.TryParse(text, out double result) ? result : defaultValue;
        }

        private void AddTooltips()
        {
            var toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            toolTip.ShowAlways = true;

            // Add tooltips to controls
            toolTip.SetToolTip(createStructureButton, "Create the complete structure in Tekla Structures");

            // Tab tooltips
            foreach (TabPage tab in tabControl.TabPages)
            {
                switch (tab.Text)
                {
                    case "Global":
                        toolTip.SetToolTip(tab, "Set global position and orientation for the entire structure");
                        break;
                    case "Foundation":
                        toolTip.SetToolTip(tab, "Configure foundation dimensions and material properties");
                        break;
                    case "Mat":
                        toolTip.SetToolTip(tab, "Set mat foundation parameters and classification");
                        break;
                    case "Piles":
                        toolTip.SetToolTip(tab, "Configure pile layout, spacing, and properties");
                        break;
                    case "Elevation":
                        toolTip.SetToolTip(tab, "Set column/vertical element specifications");
                        break;
                    case "Cap":
                        toolTip.SetToolTip(tab, "Configure cap beam dimensions and properties");
                        break;
                }
            }
        }

        private void AddInputValidation()
        {
            // Setup all textboxes with default values (no validation for now)
            SetupTextBox(foundationWidthTextBox, "2000");
            SetupTextBox(foundationLengthTextBox, "4000");
            SetupTextBox(foundationHeightTextBox, "600");

            SetupTextBox(matCantileverTextBox, "300");
            SetupTextBox(matThicknessTextBox, "200");

            SetupTextBox(pileRowsTextBox, "3");
            SetupTextBox(pileColumnsTextBox, "3");
            SetupTextBox(pileRowDistanceTextBox, "2000");
            SetupTextBox(pileColumnDistanceTextBox, "2000");
            SetupTextBox(pileLengthTextBox, "12000");
            SetupTextBox(pileDiameterTextBox, "600");
            SetupTextBox(pileEmbeddedLengthTextBox, "2000");

            SetupTextBox(lamelarWidthTextBox, "400");
            SetupTextBox(lamelarThicknessTextBox, "300");
            SetupTextBox(lamelarHeightTextBox, "8000");
            SetupTextBox(lamelarNumberOfColumnsTextBox, "1");
            SetupTextBox(lamelarDistanceTextBox, "1000");
            SetupTextBox(lamelarOffsetXTextBox, "0");
            SetupTextBox(lamelarOffsetYTextBox, "0");

            SetupTextBox(circularDiameterTextBox, "600");
            SetupTextBox(circularHeightTextBox, "8000");
            SetupTextBox(circularColumnsTextBox, "4");
            SetupTextBox(circularDistanceTextBox, "1500");
            SetupTextBox(circularOffsetXTextBox, "0");
            SetupTextBox(circularOffsetYTextBox, "0");

            SetupTextBox(capTopLengthTextBox, "4000");
            SetupTextBox(capBottomLengthTextBox, "2000");
            SetupTextBox(capWidthTextBox, "600");
            SetupTextBox(capDepthTextBox, "500");
            SetupTextBox(capHeightDiffTextBox, "300");
            SetupTextBox(capPTextBox, "0");
            SetupTextBox(capCutXTextBox, "0");
            SetupTextBox(capCutYTextBox, "0");

            SetupTextBox(bufferNumberTextBox, "3");
            SetupTextBox(bufferSpacingTextBox, "500");
            SetupTextBox(bufferLeftOffsetTextBox, "200");
            SetupTextBox(bufferRightOffsetTextBox, "200");
            SetupTextBox(bufferWidthTextBox, "400");
            SetupTextBox(bufferBreadthTextBox, "300");
            SetupTextBox(bufferHeightsTextBox, "50,50,50");
        }

        private void SetupTextBox(TextBox textBox, string defaultValue = "")
        {
            // Make sure the textbox is enabled and accessible
            textBox.Enabled = true;
            textBox.ReadOnly = false;
            textBox.Text = defaultValue;
            textBox.BackColor = Color.White;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private void CreateFoundationTab()
        {
            TabPage tab = new TabPage("Foundation");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Foundation Dimensions
            AddLabelAndTextBox(tab, "Width (mm):", ref foundationWidthTextBox, "2000", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Length (mm):", ref foundationLengthTextBox, "4000", 20, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Height (mm):", ref foundationHeightTextBox, "600", 20, yPos);
            yPos += 35;

            // Material & Class
            AddLabelAndComboBox(tab, "Material:", ref foundationMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref foundationClassComboBox, 200, yPos);
        }

        private void CreateMatTab()
        {
            TabPage tab = new TabPage("Mat");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Mat Dimensions
            AddLabelAndTextBox(tab, "Cantilever (mm):", ref matCantileverTextBox, "300", 20, yPos);
            AddLabelAndTextBox(tab, "Thickness (mm):", ref matThicknessTextBox, "200", 310, yPos);
            yPos += 35;

            // Material & Class
            AddLabelAndComboBox(tab, "Material:", ref matMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref matClassComboBox, 200, yPos);
        }

        private void CreatePilesTab()
        {
            TabPage tab = new TabPage("Piles");
            tabControl.TabPages.Add(tab);

            int yPos = 20;

            // Pile Layout
            AddLabelAndTextBox(tab, "Rows:", ref pileRowsTextBox, "3", 20, yPos);
            AddLabelAndTextBox(tab, "Columns:", ref pileColumnsTextBox, "3", 150, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Row Distance (mm):", ref pileRowDistanceTextBox, "2000", 20, yPos);
            AddLabelAndTextBox(tab, "Column Distance (mm):", ref pileColumnDistanceTextBox, "2000", 310, yPos);
            yPos += 35;

            // Pile Properties
            AddLabelAndTextBox(tab, "Length (mm):", ref pileLengthTextBox, "12000", 20, yPos);
            AddLabelAndTextBox(tab, "Diameter (mm):", ref pileDiameterTextBox, "600", 310, yPos);
            yPos += 35;
            AddLabelAndTextBox(tab, "Embedded Length (mm):", ref pileEmbeddedLengthTextBox, "2000", 20, yPos);
            yPos += 35;

            // Material & Class
            AddLabelAndComboBox(tab, "Material:", ref pilesMaterialComboBox, 20, yPos);
            AddLabelAndComboBox(tab, "Class:", ref pilesClassComboBox, 200, yPos);
        }

        private bool ValidateAllInputs()
        {
            try
            {
                // Determine elevation type
                ElevationType elevationType = lamelarRadioButton.Checked
                    ? ElevationType.Lamelar
                    : ElevationType.Circular;

                // Build parameter objects from current UI values
                var lamelarParams = new TeklaPlugin.Services.Elevation.Models.LamelarElevationParameters
                {
                    Width = ParseDouble(lamelarWidthTextBox.Text, 400),
                    Thickness = ParseDouble(lamelarThicknessTextBox.Text, 300),
                    Height = ParseDouble(lamelarHeightTextBox.Text, 8000),
                    NumberOfColumns = (int)ParseDouble(lamelarNumberOfColumnsTextBox.Text, 1),
                    DistanceBetweenColumns = ParseDouble(lamelarDistanceTextBox.Text, 1000),
                    OffsetX = ParseDouble(lamelarOffsetXTextBox.Text, 0),
                    OffsetY = ParseDouble(lamelarOffsetYTextBox.Text, 0)
                };

                var circularParams = new TeklaPlugin.Services.Elevation.Models.CircularElevationParameters
                {
                    Diameter = ParseDouble(circularDiameterTextBox.Text, 600),
                    Height = ParseDouble(circularHeightTextBox.Text, 8000),
                    NumberOfColumns = (int)ParseDouble(circularColumnsTextBox.Text, 4),
                    DistanceBetweenColumns = ParseDouble(circularDistanceTextBox.Text, 1500),
                    OffsetX = ParseDouble(circularOffsetXTextBox.Text, 0),
                    OffsetY = ParseDouble(circularOffsetYTextBox.Text, 0)
                };

                var capParams = new TeklaPlugin.Services.Cap.Models.CapParameters
                {
                    TopLength = ParseDouble(capTopLengthTextBox.Text, 4000),
                    BottomLength = ParseDouble(capBottomLengthTextBox.Text, 2000),
                    Width = ParseDouble(capWidthTextBox.Text, 600),
                    Depth = ParseDouble(capDepthTextBox.Text, 500),
                    HeightDiff = ParseDouble(capHeightDiffTextBox.Text, 300),
                    P = ParseDouble(capPTextBox.Text, 0),
                    CutX = ParseDouble(capCutXTextBox.Text, 0),
                    CutY = ParseDouble(capCutYTextBox.Text, 0),
                    CutSide = capCutSideComboBox.SelectedItem?.ToString() ?? "Right"
                };

                var pileParams = new TeklaPlugin.Services.Piles.Models.PileParameters
                {
                    Rows = (int)ParseDouble(pileRowsTextBox.Text, 3),
                    Columns = (int)ParseDouble(pileColumnsTextBox.Text, 3),
                    RowDistance = ParseDouble(pileRowDistanceTextBox.Text, 2000),
                    ColumnDistance = ParseDouble(pileColumnDistanceTextBox.Text, 2000),
                    Diameter = ParseDouble(pileDiameterTextBox.Text, 600),
                    EmbeddedLength = ParseDouble(pileEmbeddedLengthTextBox.Text, 2000)
                };

                var foundationParams = new TeklaPlugin.Services.Foundation.Models.FoundationParameters
                {
                    Width = ParseDouble(foundationWidthTextBox.Text, 2000),
                    Length = ParseDouble(foundationLengthTextBox.Text, 4000),
                    Height = ParseDouble(foundationHeightTextBox.Text, 600)
                };

                var bufferParams = new TeklaPlugin.Services.Buffer.Models.BufferParameters
                {
                    Number = (int)ParseDouble(bufferNumberTextBox.Text, 3),
                    Spacing = ParseDouble(bufferSpacingTextBox.Text, 500),
                    LeftOffset = ParseDouble(bufferLeftOffsetTextBox.Text, 200),
                    RightOffset = ParseDouble(bufferRightOffsetTextBox.Text, 200),
                    Width = ParseDouble(bufferWidthTextBox.Text, 400),
                    Breadth = ParseDouble(bufferBreadthTextBox.Text, 300),
                    Heights = bufferHeightsTextBox.Text
                };

                // Run cross-object validations
                var validator = new StructureValidator();
                var result = validator.ValidateAll(
                    elevationType, lamelarParams, circularParams,
                    capParams, pileParams, foundationParams, bufferParams);

                if (!result.IsValid)
                {
                    MessageBox.Show(
                        "Geometry validation failed:\n\n" + result.GetSummary(),
                        "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during validation: {ex.Message}",
                    "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void AddLabelAndTextBox(Control parent, string labelText, ref TextBox textBox, string defaultValue, int x, int y)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new System.Drawing.Point(x, y);
            label.Size = new Size(150, 20);
            label.Font = new Font("Segoe UI", 9F);
            parent.Controls.Add(label);

            textBox = new TextBox();
            textBox.Location = new System.Drawing.Point(x + 160, y);
            textBox.Size = new Size(120, 23);
            textBox.Font = new Font("Segoe UI", 9F);
            SetupTextBox(textBox, defaultValue);
            parent.Controls.Add(textBox);
        }

        private void AddLabelAndComboBox(Control parent, string labelText, ref ComboBox comboBox, int x, int y)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new System.Drawing.Point(x, y);
            label.Size = new Size(150, 20);
            label.Font = new Font("Segoe UI", 9F);
            parent.Controls.Add(label);

            comboBox = new ComboBox();
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Location = new System.Drawing.Point(x + 160, y);
            comboBox.Size = new Size(120, 23);
            comboBox.Font = new Font("Segoe UI", 9F);
            parent.Controls.Add(comboBox);
        }

        private void ElevationType_Changed(object sender, EventArgs e)
        {
            bool isLamelar = lamelarRadioButton.Checked;

            // Toggle visibility of lamelar controls
            lamelarWidthTextBox.Visible = isLamelar;
            lamelarThicknessTextBox.Visible = isLamelar;
            lamelarHeightTextBox.Visible = isLamelar;
            lamelarMaterialComboBox.Visible = isLamelar;
            lamelarClassComboBox.Visible = isLamelar;

            // Toggle visibility of circular controls
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
            // Validate all inputs first
            if (!ValidateAllInputs())
            {
                MessageBox.Show("Please correct the input errors before creating the structure.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                    NumberOfColumns = int.Parse(lamelarNumberOfColumnsTextBox.Text),
                    DistanceBetweenColumns = double.Parse(lamelarDistanceTextBox.Text),
                    OffsetX = double.Parse(lamelarOffsetXTextBox.Text),
                    OffsetY = double.Parse(lamelarOffsetYTextBox.Text),
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
                    TopLength = double.Parse(capTopLengthTextBox.Text),
                    BottomLength = double.Parse(capBottomLengthTextBox.Text),
                    Width = double.Parse(capWidthTextBox.Text),
                    Depth = double.Parse(capDepthTextBox.Text),
                    HeightDiff = double.Parse(capHeightDiffTextBox.Text),
                    P = double.Parse(capPTextBox.Text),
                    CutX = double.Parse(capCutXTextBox.Text),
                    CutY = double.Parse(capCutYTextBox.Text),
                    CutSide = capCutSideComboBox.SelectedItem?.ToString() ?? "Right",
                    Material = capMaterialComboBox.SelectedItem?.ToString() ?? "C12/15",
                    Class = capClassComboBox.SelectedItem?.ToString() ?? "8"
                };

                var bufferParams = new TeklaPlugin.Services.Buffer.Models.BufferParameters
                {
                    Number = int.Parse(bufferNumberTextBox.Text),
                    Spacing = double.Parse(bufferSpacingTextBox.Text),
                    LeftOffset = double.Parse(bufferLeftOffsetTextBox.Text),
                    RightOffset = double.Parse(bufferRightOffsetTextBox.Text),
                    Width = double.Parse(bufferWidthTextBox.Text),
                    Breadth = double.Parse(bufferBreadthTextBox.Text),
                    Heights = bufferHeightsTextBox.Text,
                    Material = bufferMaterialComboBox.SelectedItem?.ToString() ?? "C12/15",
                    Class = bufferClassComboBox.SelectedItem?.ToString() ?? "8"
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
                    capParams,
                    bufferParams);

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