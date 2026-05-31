using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class ExocraftPanel
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        // Existing controls
        _vehicleSelector = new ComboBox();
        _invTabs = new DoubleBufferedTabControl();
        _inventoryGrid = new InventoryGridPanel();
        _techGrid = new InventoryGridPanel();
        _exportBtn = new Button();
        _importBtn = new Button();
        _thirdPersonCam = new CheckBox();
        _minotaurAI = new CheckBox();
        _nameField = new TextBox();
        _techNoteLabel = new Label();

        _layout = new TableLayoutPanel();
        _titleLabel = new Label();
        _vehicleLabel = new Label();
        _nameLabel = new Label();
        _invPage = new TabPage();
        _techPage = new TabPage();

        // New tab controls
        _mainTabs = new DoubleBufferedTabControl();
        _exocraftsTabPage = new TabPage();
        _stationsTabPage = new TabPage();
        _exocraftLayout = new TableLayoutPanel();

        // Station controls
        _stationsSplitContainer = new SplitContainer();
        _stationsLeftPanel = new TableLayoutPanel();
        _individualStationsLabel = new Label();
        _individualStationsList = new ListBox();
        _baseStationsLabel = new Label();
        _baseStationsList = new ListBox();
        _stationsDetailPanel = new TableLayoutPanel();
        _stationNameLabel = new Label();
        _stationNameValue = new Label();
        _stationTimestampLabel = new Label();
        _stationTimestampValue = new Label();
        _stationBaseNameLabel = new Label();
        _stationBaseNameValue = new Label();
        _stationGalacticAddrLabel = new Label();
        _stationGalacticAddrValue = new Label();
        _stationRegionSeedLabel = new Label();
        _stationRegionSeedValue = new Label();
        _stationPositionLabel = new Label();
        _stationPositionValue = new Label();
        _coordSectionLabel = new Label();
        _stationGalaxyLabel = new Label();
        _stationGalaxyValue = new Label();
        _stationGalaxyDotLabel = new Label();
        _stationPortalCodeLabel = new Label();
        _stationPortalCodeValue = new Label();
        _stationPortalCodeDecLabel = new Label();
        _stationPortalCodeDecValue = new Label();
        _stationSignalBoosterLabel = new Label();
        _stationSignalBoosterValue = new Label();
        _stationGlyphPanel = new FlowLayoutPanel();
        _stationVoxelXLabel = new Label();
        _stationVoxelXValue = new Label();
        _stationVoxelYLabel = new Label();
        _stationVoxelYValue = new Label();
        _stationVoxelZLabel = new Label();
        _stationVoxelZValue = new Label();
        _stationSolarSystemLabel = new Label();
        _stationSolarSystemValue = new Label();
        _stationPlanetLabel = new Label();
        _stationPlanetValue = new Label();
        _deleteStationBtn = new Button();

        _invTabs.SuspendLayout();
        _invPage.SuspendLayout();
        _techPage.SuspendLayout();
        _layout.SuspendLayout();
        _mainTabs.SuspendLayout();
        _exocraftsTabPage.SuspendLayout();
        _stationsTabPage.SuspendLayout();
        _exocraftLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_stationsSplitContainer).BeginInit();
        _stationsSplitContainer.Panel1.SuspendLayout();
        _stationsSplitContainer.Panel2.SuspendLayout();
        _stationsSplitContainer.SuspendLayout();
        _stationsLeftPanel.SuspendLayout();
        _stationsDetailPanel.SuspendLayout();
        _stationGlyphPanel.SuspendLayout();
        SuspendLayout();

        // 
        // _layout
        // 
        _layout.Dock = DockStyle.Fill;
        _layout.ColumnCount = 1;
        _layout.RowCount = 2;
        _layout.Padding = new Padding(10);
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 
        // _titleLabel
        // 
        _titleLabel.Text = "Exocraft";
        FontManager.ApplyHeadingFont(_titleLabel, 14);
        _titleLabel.AutoSize = true;
        _titleLabel.Padding = new Padding(0, 0, 0, 5);
        _layout.Controls.Add(_titleLabel, 0, 0);

        // ===================== EXOCRAFTS TAB (existing content) =====================

        // 
        // _exocraftLayout — inner layout for the Exocrafts tab page
        // 
        _exocraftLayout.Dock = DockStyle.Fill;
        _exocraftLayout.ColumnCount = 2;
        _exocraftLayout.RowCount = 5;
        _exocraftLayout.Padding = new Padding(0);
        _exocraftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _exocraftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        _exocraftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _exocraftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _exocraftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _exocraftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _exocraftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 
        // _vehicleLabel (row 0)
        // 
        _vehicleLabel.Text = "Vehicle:";
        _vehicleLabel.AutoSize = true;
        _vehicleLabel.Anchor = AnchorStyles.Left;
        _vehicleLabel.Padding = new Padding(0, 5, 10, 0);

        // 
        // _vehicleSelector (row 0, col 1)
        // 
        _vehicleSelector.Dock = DockStyle.Fill;
        _vehicleSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _vehicleSelector.SelectedIndexChanged += OnVehicleSelected;
        _exocraftLayout.Controls.Add(_vehicleLabel, 0, 0);
        _exocraftLayout.Controls.Add(_vehicleSelector, 1, 0);

        // 
        // _nameLabel (row 1)
        // 
        _nameLabel.Text = "Name:";
        _nameLabel.AutoSize = true;
        _nameLabel.Anchor = AnchorStyles.Left;
        _nameLabel.Padding = new Padding(0, 5, 10, 0);

        // 
        // _nameField (row 1, col 1)
        // 
        _nameField.Dock = DockStyle.Fill;
        _nameField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _nameField.Parent?.Focus(); }
        };
        _exocraftLayout.Controls.Add(_nameLabel, 0, 1);
        _exocraftLayout.Controls.Add(_nameField, 1, 1);

        //
        // Row 2 - All action controls: Export, Import, Primary, Deployed, Undeploy, Third Person, AI Pilot
        //
        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        _exportBtn.Text = "Export Exocraft";
        _exportBtn.AutoSize = true;
        _exportBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _exportBtn.MinimumSize = new Size(75, 0);
        _exportBtn.Click += OnExportVehicle;

        _importBtn.Text = "Import Exocraft";
        _importBtn.AutoSize = true;
        _importBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _importBtn.MinimumSize = new Size(75, 0);
        _importBtn.Click += OnImportVehicle;

        _primaryVehicleCheck = new CheckBox { Text = "Primary Vehicle", AutoSize = true };
        _primaryVehicleCheck.CheckedChanged += OnPrimaryVehicleChanged;

        _deployedLabel = new Label { Text = "", AutoSize = true, Padding = new Padding(10, 5, 10, 0) };

        _undeployBtn = new Button { Text = "Undeploy", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(80, 0) };
        _undeployBtn.Click += OnUndeploy;

        _thirdPersonCam.Text = "Third Person Camera";
        _thirdPersonCam.AutoSize = true;

        _minotaurAI.Text = "Minotaur AI Pilot";
        _minotaurAI.AutoSize = true;

        actionPanel.Controls.Add(_exportBtn);
        actionPanel.Controls.Add(_importBtn);
        actionPanel.Controls.Add(_primaryVehicleCheck);
        actionPanel.Controls.Add(_deployedLabel);
        actionPanel.Controls.Add(_undeployBtn);
        actionPanel.Controls.Add(_thirdPersonCam);
        actionPanel.Controls.Add(_minotaurAI);
        _exocraftLayout.Controls.Add(actionPanel, 0, 2);
        _exocraftLayout.SetColumnSpan(actionPanel, 2);

        // 
        // _inventoryGrid
        // 
        _inventoryGrid.Dock = DockStyle.Fill;

        // 
        // _techGrid
        // 
        _techGrid.Dock = DockStyle.Fill;

        // 
        // _techNoteLabel (row 3)
        // 
        _techNoteLabel.Text = "Note: Supercharged slots are fixed for this inventory type in game and can't be modified.";
        _techNoteLabel.AutoSize = true;
        _techNoteLabel.ForeColor = System.Drawing.Color.FromArgb(200, 160, 0);
        _techNoteLabel.Padding = new Padding(0, 0, 0, 5);
        _techNoteLabel.Visible = false;
        _exocraftLayout.Controls.Add(_techNoteLabel, 0, 3);
        _exocraftLayout.SetColumnSpan(_techNoteLabel, 2);

        // 
        // _invPage
        // 
        _invPage.Text = "Cargo";
        _invPage.Controls.Add(_inventoryGrid);

        // 
        // _techPage
        // 
        _techPage.Text = "Technology";
        _techPage.Controls.Add(_techGrid);

        // 
        // _invTabs
        // 
        _invTabs.Dock = DockStyle.Fill;
        _invTabs.TabPages.Add(_invPage);
        _invTabs.TabPages.Add(_techPage);
        _exocraftLayout.Controls.Add(_invTabs, 0, 4);
        _exocraftLayout.SetColumnSpan(_invTabs, 2);

        // 
        // _exocraftsTabPage
        // 
        _exocraftsTabPage.Text = "Exocrafts";
        _exocraftsTabPage.Controls.Add(_exocraftLayout);

        // ===================== SUMMONING STATIONS TAB =====================

        //
        // _stationsSplitContainer
        //
        _stationsSplitContainer.Dock = DockStyle.Fill;
        _stationsSplitContainer.SplitterDistance = 330;
        _stationsSplitContainer.IsSplitterFixed = false;
        _stationsSplitContainer.FixedPanel = FixedPanel.Panel1;

        //
        // _stationsLeftPanel
        //
        _stationsLeftPanel.Dock = DockStyle.Fill;
        _stationsLeftPanel.ColumnCount = 1;
        _stationsLeftPanel.RowCount = 4;
        _stationsLeftPanel.Padding = new Padding(0);
        _stationsLeftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _stationsLeftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stationsLeftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        _stationsLeftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stationsLeftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        //
        // _individualStationsLabel
        //
        _individualStationsLabel.Text = "Individual Stations";
        _individualStationsLabel.AutoSize = true;
        FontManager.ApplyHeadingFont(_individualStationsLabel, 12);
        _individualStationsLabel.Padding = new Padding(0, 5, 0, 2);
        _stationsLeftPanel.Controls.Add(_individualStationsLabel, 0, 0);

        //
        // _individualStationsList
        //
        _individualStationsList.Dock = DockStyle.Fill;
        _individualStationsList.SelectedIndexChanged += OnIndividualStationSelected;
        _stationsLeftPanel.Controls.Add(_individualStationsList, 0, 1);

        //
        // _baseStationsLabel
        //
        _baseStationsLabel.Text = "Part of Bases";
        _baseStationsLabel.AutoSize = true;
        FontManager.ApplyHeadingFont(_baseStationsLabel, 12);
        _baseStationsLabel.Padding = new Padding(0, 8, 0, 2);
        _stationsLeftPanel.Controls.Add(_baseStationsLabel, 0, 2);

        //
        // _baseStationsList
        //
        _baseStationsList.Dock = DockStyle.Fill;
        _baseStationsList.SelectedIndexChanged += OnBaseStationSelected;
        _stationsLeftPanel.Controls.Add(_baseStationsList, 0, 3);

        _stationsSplitContainer.Panel1.Controls.Add(_stationsLeftPanel);

        //
        // _stationsDetailPanel
        //
        _stationsDetailPanel.Dock = DockStyle.Fill;
        _stationsDetailPanel.ColumnCount = 2;
        _stationsDetailPanel.RowCount = 23;
        _stationsDetailPanel.Padding = new Padding(8);
        _stationsDetailPanel.AutoScroll = true;
        _stationsDetailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _stationsDetailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Add spacer rows for all row styles
        for (int i = 0; i < 23; i++)
            _stationsDetailPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        int row = 0;

        // Name
        _stationNameLabel.Text = "Name:";
        _stationNameLabel.AutoSize = true;
        _stationNameLabel.Padding = new Padding(0, 3, 10, 0);
        _stationNameLabel.Anchor = AnchorStyles.Left;
        _stationNameValue.Text = "";
        _stationNameValue.AutoSize = true;
        _stationNameValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationNameLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationNameValue, 1, row);
        row++;

        // Timestamp
        _stationTimestampLabel.Text = "Timestamp:";
        _stationTimestampLabel.AutoSize = true;
        _stationTimestampLabel.Padding = new Padding(0, 3, 10, 0);
        _stationTimestampLabel.Anchor = AnchorStyles.Left;
        _stationTimestampValue.Text = "";
        _stationTimestampValue.AutoSize = true;
        _stationTimestampValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationTimestampLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationTimestampValue, 1, row);
        row++;

        // Base Name
        _stationBaseNameLabel.Text = "Base:";
        _stationBaseNameLabel.AutoSize = true;
        _stationBaseNameLabel.Padding = new Padding(0, 3, 10, 0);
        _stationBaseNameLabel.Anchor = AnchorStyles.Left;
        _stationBaseNameValue.Text = "";
        _stationBaseNameValue.AutoSize = true;
        _stationBaseNameValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationBaseNameLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationBaseNameValue, 1, row);
        row++;

        // Galactic Address
        _stationGalacticAddrLabel.Text = "Galactic Address:";
        _stationGalacticAddrLabel.AutoSize = true;
        _stationGalacticAddrLabel.Padding = new Padding(0, 3, 10, 0);
        _stationGalacticAddrLabel.Anchor = AnchorStyles.Left;
        _stationGalacticAddrValue.Text = "";
        _stationGalacticAddrValue.AutoSize = true;
        _stationGalacticAddrValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationGalacticAddrLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationGalacticAddrValue, 1, row);
        row++;

        // Region Seed
        _stationRegionSeedLabel.Text = "Region Seed:";
        _stationRegionSeedLabel.AutoSize = true;
        _stationRegionSeedLabel.Padding = new Padding(0, 3, 10, 0);
        _stationRegionSeedLabel.Anchor = AnchorStyles.Left;
        _stationRegionSeedValue.Text = "";
        _stationRegionSeedValue.AutoSize = true;
        _stationRegionSeedValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationRegionSeedLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationRegionSeedValue, 1, row);
        row++;

        // Position
        _stationPositionLabel.Text = "Position:";
        _stationPositionLabel.AutoSize = true;
        _stationPositionLabel.Padding = new Padding(0, 3, 10, 0);
        _stationPositionLabel.Anchor = AnchorStyles.Left;
        _stationPositionValue.Text = "";
        _stationPositionValue.AutoSize = true;
        _stationPositionValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationPositionLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationPositionValue, 1, row);
        row++;

        // Coordinates section heading
        _coordSectionLabel.Text = "Coordinates";
        _coordSectionLabel.AutoSize = true;
        _coordSectionLabel.Padding = new Padding(0, 10, 0, 2);
        FontManager.ApplyHeadingFont(_coordSectionLabel, 10);
        _stationsDetailPanel.Controls.Add(_coordSectionLabel, 0, row);
        _stationsDetailPanel.SetColumnSpan(_coordSectionLabel, 2);
        row++;

        // Galaxy
        _stationGalaxyLabel.Text = "Galaxy:";
        _stationGalaxyLabel.AutoSize = true;
        _stationGalaxyLabel.Padding = new Padding(0, 3, 10, 0);
        _stationGalaxyLabel.Anchor = AnchorStyles.Left;
        _stationGalaxyValue.Text = "";
        _stationGalaxyValue.AutoSize = true;
        _stationGalaxyValue.Padding = new Padding(0, 3, 0, 0);
        _stationGalaxyDotLabel.AutoSize = false;
        _stationGalaxyDotLabel.Size = new Size(12, 12);
        _stationGalaxyDotLabel.ImageAlign = ContentAlignment.MiddleCenter;
        _stationGalaxyDotLabel.Margin = new Padding(0, 6, 0, 0);
        _stationGalaxyDotLabel.Text = "";
        var galaxyRowPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        galaxyRowPanel.Controls.Add(_stationGalaxyValue);
        galaxyRowPanel.Controls.Add(_stationGalaxyDotLabel);
        _stationsDetailPanel.Controls.Add(_stationGalaxyLabel, 0, row);
        _stationsDetailPanel.Controls.Add(galaxyRowPanel, 1, row);
        row++;

        // Portal Code
        _stationPortalCodeLabel.Text = "Portal Code:";
        _stationPortalCodeLabel.AutoSize = true;
        _stationPortalCodeLabel.Padding = new Padding(0, 3, 10, 0);
        _stationPortalCodeLabel.Anchor = AnchorStyles.Left;
        _stationPortalCodeValue.Text = "";
        _stationPortalCodeValue.AutoSize = true;
        _stationPortalCodeValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationPortalCodeLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationPortalCodeValue, 1, row);
        row++;

        // Portal Code (decimal)
        _stationPortalCodeDecLabel.Text = "Portal Code (decimal):";
        _stationPortalCodeDecLabel.AutoSize = true;
        _stationPortalCodeDecLabel.Padding = new Padding(0, 3, 10, 0);
        _stationPortalCodeDecLabel.Anchor = AnchorStyles.Left;
        _stationPortalCodeDecValue.Text = "";
        _stationPortalCodeDecValue.AutoSize = true;
        _stationPortalCodeDecValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationPortalCodeDecLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationPortalCodeDecValue, 1, row);
        row++;

        // Signal Booster
        _stationSignalBoosterLabel.Text = "Signal Booster:";
        _stationSignalBoosterLabel.AutoSize = true;
        _stationSignalBoosterLabel.Padding = new Padding(0, 3, 10, 0);
        _stationSignalBoosterLabel.Anchor = AnchorStyles.Left;
        _stationSignalBoosterValue.Text = "";
        _stationSignalBoosterValue.AutoSize = true;
        _stationSignalBoosterValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationSignalBoosterLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationSignalBoosterValue, 1, row);
        row++;

        // Glyph panel (spans both columns)
        _stationGlyphPanel.AutoSize = true;
        _stationGlyphPanel.WrapContents = false;
        _stationGlyphPanel.FlowDirection = FlowDirection.LeftToRight;
        _stationGlyphPanel.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationGlyphPanel, 0, row);
        _stationsDetailPanel.SetColumnSpan(_stationGlyphPanel, 2);
        row++;

        // Voxel X
        _stationVoxelXLabel.Text = "Voxel X:";
        _stationVoxelXLabel.AutoSize = true;
        _stationVoxelXLabel.Padding = new Padding(0, 3, 10, 0);
        _stationVoxelXLabel.Anchor = AnchorStyles.Left;
        _stationVoxelXValue.Text = "";
        _stationVoxelXValue.AutoSize = true;
        _stationVoxelXValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationVoxelXLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationVoxelXValue, 1, row);
        row++;

        // Voxel Y
        _stationVoxelYLabel.Text = "Voxel Y:";
        _stationVoxelYLabel.AutoSize = true;
        _stationVoxelYLabel.Padding = new Padding(0, 3, 10, 0);
        _stationVoxelYLabel.Anchor = AnchorStyles.Left;
        _stationVoxelYValue.Text = "";
        _stationVoxelYValue.AutoSize = true;
        _stationVoxelYValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationVoxelYLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationVoxelYValue, 1, row);
        row++;

        // Voxel Z
        _stationVoxelZLabel.Text = "Voxel Z:";
        _stationVoxelZLabel.AutoSize = true;
        _stationVoxelZLabel.Padding = new Padding(0, 3, 10, 0);
        _stationVoxelZLabel.Anchor = AnchorStyles.Left;
        _stationVoxelZValue.Text = "";
        _stationVoxelZValue.AutoSize = true;
        _stationVoxelZValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationVoxelZLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationVoxelZValue, 1, row);
        row++;

        // Solar System
        _stationSolarSystemLabel.Text = "Solar System:";
        _stationSolarSystemLabel.AutoSize = true;
        _stationSolarSystemLabel.Padding = new Padding(0, 3, 10, 0);
        _stationSolarSystemLabel.Anchor = AnchorStyles.Left;
        _stationSolarSystemValue.Text = "";
        _stationSolarSystemValue.AutoSize = true;
        _stationSolarSystemValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationSolarSystemLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationSolarSystemValue, 1, row);
        row++;

        // Planet
        _stationPlanetLabel.Text = "Planet:";
        _stationPlanetLabel.AutoSize = true;
        _stationPlanetLabel.Padding = new Padding(0, 3, 10, 0);
        _stationPlanetLabel.Anchor = AnchorStyles.Left;
        _stationPlanetValue.Text = "";
        _stationPlanetValue.AutoSize = true;
        _stationPlanetValue.Padding = new Padding(0, 3, 0, 0);
        _stationsDetailPanel.Controls.Add(_stationPlanetLabel, 0, row);
        _stationsDetailPanel.Controls.Add(_stationPlanetValue, 1, row);
        row++;

        // Delete button (spans both columns, below all detail)
        _deleteStationBtn.Text = "Delete Station";
        _deleteStationBtn.AutoSize = true;
        _deleteStationBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _deleteStationBtn.MinimumSize = new Size(120, 0);
        _deleteStationBtn.Click += OnDeleteStation;
        _deleteStationBtn.Margin = new Padding(0, 10, 0, 0);
        _stationsDetailPanel.Controls.Add(_deleteStationBtn, 0, row);
        _stationsDetailPanel.SetColumnSpan(_deleteStationBtn, 2);
        row++;

        _stationsSplitContainer.Panel2.Controls.Add(_stationsDetailPanel);

        //
        // _stationsTabPage
        //
        _stationsTabPage.Text = "Summoning Stations";
        _stationsTabPage.Controls.Add(_stationsSplitContainer);

        // 
        // _mainTabs
        // 
        _mainTabs.Dock = DockStyle.Fill;
        _mainTabs.TabPages.Add(_exocraftsTabPage);
        _mainTabs.TabPages.Add(_stationsTabPage);
        _layout.Controls.Add(_mainTabs, 0, 1);

        // 
        // ExocraftPanel
        // 
        DoubleBuffered = true;
        Controls.Add(_layout);

        _invTabs.ResumeLayout(false);
        _invPage.ResumeLayout(false);
        _techPage.ResumeLayout(false);
        _exocraftLayout.ResumeLayout(false);
        _exocraftLayout.PerformLayout();
        _stationsLeftPanel.ResumeLayout(false);
        _stationsLeftPanel.PerformLayout();
        _stationsDetailPanel.ResumeLayout(false);
        _stationsDetailPanel.PerformLayout();
        _stationGlyphPanel.ResumeLayout(false);
        _stationGlyphPanel.PerformLayout();
        _stationsSplitContainer.Panel1.ResumeLayout(false);
        _stationsSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_stationsSplitContainer).EndInit();
        _stationsSplitContainer.ResumeLayout(false);
        _exocraftsTabPage.ResumeLayout(false);
        _stationsTabPage.ResumeLayout(false);
        _mainTabs.ResumeLayout(false);
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private ComboBox _vehicleSelector;
    private DoubleBufferedTabControl _invTabs;
    private InventoryGridPanel _inventoryGrid;
    private InventoryGridPanel _techGrid;
    private Button _exportBtn;
    private Button _importBtn;
    private CheckBox _thirdPersonCam;
    private CheckBox _minotaurAI;
    private TextBox _nameField;
    private Label _techNoteLabel;
    private TableLayoutPanel _layout;
    private Label _titleLabel;
    private Label _vehicleLabel;
    private Label _nameLabel;
    private TabPage _invPage;
    private TabPage _techPage;
    private CheckBox _primaryVehicleCheck;
    private Label _deployedLabel;
    private Button _undeployBtn;

    // New tab-related controls
    private DoubleBufferedTabControl _mainTabs;
    private TabPage _exocraftsTabPage;
    private TabPage _stationsTabPage;
    private TableLayoutPanel _exocraftLayout;

    // Station list controls
    private SplitContainer _stationsSplitContainer;
    private TableLayoutPanel _stationsLeftPanel;
    private Label _individualStationsLabel;
    private ListBox _individualStationsList;
    private Label _baseStationsLabel;
    private ListBox _baseStationsList;

    // Station detail controls
    private TableLayoutPanel _stationsDetailPanel;
    private Label _stationNameLabel;
    private Label _stationNameValue;
    private Label _stationTimestampLabel;
    private Label _stationTimestampValue;
    private Label _stationBaseNameLabel;
    private Label _stationBaseNameValue;
    private Label _stationGalacticAddrLabel;
    private Label _stationGalacticAddrValue;
    private Label _stationRegionSeedLabel;
    private Label _stationRegionSeedValue;
    private Label _stationPositionLabel;
    private Label _stationPositionValue;
    private Label _coordSectionLabel;
    private Label _stationGalaxyLabel;
    private Label _stationGalaxyValue;
    private Label _stationGalaxyDotLabel;
    private Label _stationPortalCodeLabel;
    private Label _stationPortalCodeValue;
    private Label _stationPortalCodeDecLabel;
    private Label _stationPortalCodeDecValue;
    private Label _stationSignalBoosterLabel;
    private Label _stationSignalBoosterValue;
    private FlowLayoutPanel _stationGlyphPanel;
    private Label _stationVoxelXLabel;
    private Label _stationVoxelXValue;
    private Label _stationVoxelYLabel;
    private Label _stationVoxelYValue;
    private Label _stationVoxelZLabel;
    private Label _stationVoxelZValue;
    private Label _stationSolarSystemLabel;
    private Label _stationSolarSystemValue;
    private Label _stationPlanetLabel;
    private Label _stationPlanetValue;
    private Button _deleteStationBtn;
}
