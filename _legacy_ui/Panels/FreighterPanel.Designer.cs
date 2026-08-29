using NMSE.Data;
using NMSE.Core;
using NMSE.UI.Controls;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class FreighterPanel
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        _mainLayout = new TableLayoutPanel();
        _headerStrip = new TableLayoutPanel();
        _gotoButtonPanelFreighter = new FlowLayoutPanel();
        _titleLabel = new Label();
        _subTabs = new DoubleBufferedTabControl();
        _freighterSubPage = new TabPage();
        _freighterContentLayout = new TableLayoutPanel();
        _detailsStatsLayout = new TableLayoutPanel();
        _detailsPanel = new TableLayoutPanel();
        _nameLabel = new Label();
        _freighterName = new TextBox();
        _typeLabel = new Label();
        _freighterType = new ComboBox();
        _classLabel = new Label();
        _freighterClass = new ComboBox();
        _homeSeedLabel = new Label();
        _homeSeedPanel = new TableLayoutPanel();
        _homeSeed = new TextBox();
        _generateHomeSeedBtn = new Button();
        _crewRaceLabel = new Label();
        _crewRaceCombo = new ComboBox();
        _statsPanel = new TableLayoutPanel();
        _hyperdriveLabel = new Label();
        _hyperdriveField = new InvariantNumericTextBox();
        _fleetLabel = new Label();
        _fleetField = new InvariantNumericTextBox();
        _baseItemsLabel = new Label();
        _baseItemsField = new TextBox();
        _modelSeedLabel = new Label();
        _modelSeedPanel = new TableLayoutPanel();
        _modelSeed = new TextBox();
        _generateModelSeedBtn = new Button();
        _crewSeedLabel = new Label();
        _crewSeedPanel = new TableLayoutPanel();
        _crewSeedField = new TextBox();
        _generateCrewSeedBtn = new Button();
        _buttonPanel = new FlowLayoutPanel();
        _exportBtn = new Button();
        _importBtn = new Button();
        _gotoJsonBtn = new Button();
        _gotoInvBtn = new Button();
        _gotoTechBtn = new Button();
        _invTabs = new DoubleBufferedTabControl();
        _generalPage = new TabPage();
        _generalGrid = new InventoryGridPanel();
        _techPage = new TabPage();
        _techGrid = new InventoryGridPanel();
        _roomsSubPage = new TabPage();
        _roomList = new ListBox();
        _mainLayout.SuspendLayout();
        _headerStrip.SuspendLayout();
        _gotoButtonPanelFreighter.SuspendLayout();
        _subTabs.SuspendLayout();
        _freighterSubPage.SuspendLayout();
        _freighterContentLayout.SuspendLayout();
        _detailsStatsLayout.SuspendLayout();
        _detailsPanel.SuspendLayout();
        _homeSeedPanel.SuspendLayout();
        _statsPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_hyperdriveField).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_fleetField).BeginInit();
        _modelSeedPanel.SuspendLayout();
        _crewSeedPanel.SuspendLayout();
        _buttonPanel.SuspendLayout();
        _invTabs.SuspendLayout();
        _generalPage.SuspendLayout();
        _techPage.SuspendLayout();
        _roomsSubPage.SuspendLayout();
        SuspendLayout();
        // 
        // _mainLayout
        // 
        _mainLayout.ColumnCount = 1;
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        _mainLayout.Controls.Add(_headerStrip, 0, 0);
        _mainLayout.Controls.Add(_subTabs, 0, 1);
        _mainLayout.Dock = DockStyle.Fill;
        _mainLayout.Location = new Point(0, 0);
        _mainLayout.Name = "_mainLayout";
        _mainLayout.Padding = new Padding(10);
        _mainLayout.RowCount = 2;
        _mainLayout.RowStyles.Add(new RowStyle());
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _mainLayout.Size = new Size(1384, 995);
        _mainLayout.TabIndex = 0;
        // 
        // _headerStrip
        // 
        _headerStrip.AutoSize = true;
        _headerStrip.ColumnCount = 3;
        _headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _headerStrip.Controls.Add(_titleLabel, 0, 0);
        _headerStrip.Controls.Add(_gotoButtonPanelFreighter, 2, 0);
        _headerStrip.Dock = DockStyle.Top;
        _headerStrip.RowCount = 1;
        _headerStrip.RowStyles.Add(new RowStyle());
        // 
        // _gotoButtonPanelFreighter
        // 
        _gotoButtonPanelFreighter.AutoSize = true;
        _gotoButtonPanelFreighter.Dock = DockStyle.Fill;
		_gotoButtonPanelFreighter.FlowDirection = FlowDirection.LeftToRight;
		_gotoButtonPanelFreighter.WrapContents = false;
        // 
        // _titleLabel
        // 
        _titleLabel.AutoSize = true;
        _titleLabel.Dock = DockStyle.Fill;
        _titleLabel.Anchor = AnchorStyles.Left;
        FontManager.ApplyHeadingFont(_titleLabel, 14F);
        _titleLabel.Name = "_titleLabel";
        _titleLabel.Padding = new Padding(0, 0, 0, 5);
        _titleLabel.TabIndex = 0;
        _titleLabel.Text = "Freighter";
        // 
        // _subTabs
        // 
        _subTabs.Controls.Add(_freighterSubPage);
        _subTabs.Controls.Add(_roomsSubPage);
        _subTabs.Dock = DockStyle.Fill;
        _subTabs.Location = new Point(13, 42);
        _subTabs.Name = "_subTabs";
        _subTabs.SelectedIndex = 0;
        _subTabs.Size = new Size(1358, 940);
        _subTabs.TabIndex = 1;
        // 
        // _freighterSubPage
        // 
        _freighterSubPage.Controls.Add(_freighterContentLayout);
        _freighterSubPage.Location = new Point(4, 24);
        _freighterSubPage.Name = "_freighterSubPage";
        _freighterSubPage.Size = new Size(1350, 912);
        _freighterSubPage.TabIndex = 0;
        _freighterSubPage.Text = "Freighter";
        // 
        // _freighterContentLayout
        // 
        _freighterContentLayout.ColumnCount = 1;
        _freighterContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        _freighterContentLayout.Controls.Add(_detailsStatsLayout, 0, 0);
        _freighterContentLayout.Controls.Add(_invTabs, 0, 1);
        _freighterContentLayout.Dock = DockStyle.Fill;
        _freighterContentLayout.Location = new Point(0, 0);
        _freighterContentLayout.Name = "_freighterContentLayout";
        _freighterContentLayout.RowCount = 2;
        _freighterContentLayout.RowStyles.Add(new RowStyle());
        _freighterContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _freighterContentLayout.Size = new Size(1350, 912);
        _freighterContentLayout.TabIndex = 0;
        // 
        // _detailsStatsLayout
        // 
        _detailsStatsLayout.AutoSize = true;
        _detailsStatsLayout.ColumnCount = 2;
        _detailsStatsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _detailsStatsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _detailsStatsLayout.Controls.Add(_detailsPanel, 0, 0);
        _detailsStatsLayout.Controls.Add(_statsPanel, 1, 0);
        _detailsStatsLayout.Controls.Add(_buttonPanel, 0, 1);
        _detailsStatsLayout.Dock = DockStyle.Top;
        _detailsStatsLayout.Location = new Point(3, 3);
        _detailsStatsLayout.Name = "_detailsStatsLayout";
        _detailsStatsLayout.RowCount = 2;
        _detailsStatsLayout.RowStyles.Add(new RowStyle());
        _detailsStatsLayout.RowStyles.Add(new RowStyle());
        _detailsStatsLayout.Size = new Size(1344, 186);
        _detailsStatsLayout.TabIndex = 0;
        // 
        // _detailsPanel
        // 
        _detailsPanel.AutoSize = true;
        _detailsPanel.ColumnCount = 2;
        _detailsPanel.ColumnStyles.Add(new ColumnStyle());
        _detailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _detailsPanel.Controls.Add(_nameLabel, 0, 0);
        _detailsPanel.Controls.Add(_freighterName, 1, 0);
        _detailsPanel.Controls.Add(_typeLabel, 0, 1);
        _detailsPanel.Controls.Add(_freighterType, 1, 1);
        _detailsPanel.Controls.Add(_classLabel, 0, 2);
        _detailsPanel.Controls.Add(_freighterClass, 1, 2);
        _detailsPanel.Controls.Add(_homeSeedLabel, 0, 3);
        _detailsPanel.Controls.Add(_homeSeedPanel, 1, 3);
        _detailsPanel.Controls.Add(_crewRaceLabel, 0, 4);
        _detailsPanel.Controls.Add(_crewRaceCombo, 1, 4);
        _detailsPanel.Dock = DockStyle.Fill;
        _detailsPanel.Location = new Point(3, 3);
        _detailsPanel.Name = "_detailsPanel";
        _detailsPanel.RowCount = 5;
        _detailsPanel.RowStyles.Add(new RowStyle());
        _detailsPanel.RowStyles.Add(new RowStyle());
        _detailsPanel.RowStyles.Add(new RowStyle());
        _detailsPanel.RowStyles.Add(new RowStyle());
        _detailsPanel.RowStyles.Add(new RowStyle());
        _detailsPanel.Size = new Size(666, 145);
        _detailsPanel.TabIndex = 0;
        // 
        // _nameLabel
        // 
        _nameLabel.Anchor = AnchorStyles.Left;
        _nameLabel.AutoSize = true;
        _nameLabel.Location = new Point(3, 5);
        _nameLabel.Name = "_nameLabel";
        _nameLabel.Padding = new Padding(0, 4, 10, 0);
        _nameLabel.Size = new Size(52, 19);
        _nameLabel.TabIndex = 0;
        _nameLabel.Text = "Name:";
        // 
        // _freighterName
        // 
        _freighterName.Dock = DockStyle.Fill;
        _freighterName.Location = new Point(90, 3);
        _freighterName.Name = "_freighterName";
        _freighterName.Size = new Size(573, 23);
        _freighterName.TabIndex = 1;
        // 
        // _typeLabel
        // 
        _typeLabel.Anchor = AnchorStyles.Left;
        _typeLabel.AutoSize = true;
        _typeLabel.Location = new Point(3, 34);
        _typeLabel.Name = "_typeLabel";
        _typeLabel.Padding = new Padding(0, 4, 10, 0);
        _typeLabel.Size = new Size(45, 19);
        _typeLabel.TabIndex = 2;
        _typeLabel.Text = "Type:";
        // 
        // _freighterType
        // 
        _freighterType.Dock = DockStyle.Fill;
        _freighterType.DropDownStyle = ComboBoxStyle.DropDownList;
        _freighterType.Location = new Point(90, 32);
        _freighterType.Name = "_freighterType";
        _freighterType.Size = new Size(573, 23);
        _freighterType.TabIndex = 3;
        // 
        // _classLabel
        // 
        _classLabel.Anchor = AnchorStyles.Left;
        _classLabel.AutoSize = true;
        _classLabel.Location = new Point(3, 63);
        _classLabel.Name = "_classLabel";
        _classLabel.Padding = new Padding(0, 4, 10, 0);
        _classLabel.Size = new Size(47, 19);
        _classLabel.TabIndex = 4;
        _classLabel.Text = "Class:";
        // 
        // _freighterClass
        // 
        _freighterClass.Dock = DockStyle.Fill;
        _freighterClass.DropDownStyle = ComboBoxStyle.DropDownList;
        _freighterClass.Location = new Point(90, 61);
        _freighterClass.Name = "_freighterClass";
        _freighterClass.Size = new Size(573, 23);
        _freighterClass.TabIndex = 5;
        // 
        // _homeSeedLabel
        // 
        _homeSeedLabel.Anchor = AnchorStyles.Left;
        _homeSeedLabel.AutoSize = true;
        _homeSeedLabel.Location = new Point(3, 92);
        _homeSeedLabel.Name = "_homeSeedLabel";
        _homeSeedLabel.Padding = new Padding(0, 4, 10, 0);
        _homeSeedLabel.Size = new Size(81, 19);
        _homeSeedLabel.TabIndex = 6;
        _homeSeedLabel.Text = "Home Seed:";
        // 
        // _homeSeedPanel
        // 
        _homeSeedPanel.ColumnCount = 2;
        _homeSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _homeSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _homeSeedPanel.Controls.Add(_homeSeed, 0, 0);
        _homeSeedPanel.Controls.Add(_generateHomeSeedBtn, 1, 0);
        _homeSeedPanel.Dock = DockStyle.Fill;
        _homeSeedPanel.RowCount = 1;
        _homeSeedPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _homeSeedPanel.AutoSize = true;
        _homeSeedPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _homeSeedPanel.Padding = Padding.Empty;
        _homeSeedPanel.Margin = Padding.Empty;
        _homeSeedPanel.Name = "_homeSeedPanel";
        _homeSeedPanel.TabIndex = 7;
        // 
        // _homeSeed
        // 
        _homeSeed.Dock = DockStyle.Fill;
        _homeSeed.Name = "_homeSeed";
        _homeSeed.TabIndex = 0;
        // 
        // _generateHomeSeedBtn
        // 
        _generateHomeSeedBtn.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        _generateHomeSeedBtn.Name = "_generateHomeSeedBtn";
        _generateHomeSeedBtn.AutoSize = true;
        _generateHomeSeedBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _generateHomeSeedBtn.MinimumSize = new Size(70, 0);
        _generateHomeSeedBtn.TabIndex = 1;
        _generateHomeSeedBtn.Text = "Generate";
        // 
        // _crewRaceLabel
        // 
        _crewRaceLabel.Anchor = AnchorStyles.Left;
        _crewRaceLabel.AutoSize = true;
        _crewRaceLabel.Location = new Point(3, 121);
        _crewRaceLabel.Name = "_crewRaceLabel";
        _crewRaceLabel.Padding = new Padding(0, 4, 10, 0);
        _crewRaceLabel.Size = new Size(75, 19);
        _crewRaceLabel.TabIndex = 8;
        _crewRaceLabel.Text = "Crew Race:";
        // 
        // _crewRaceCombo
        // 
        _crewRaceCombo.Dock = DockStyle.Fill;
        _crewRaceCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _crewRaceCombo.Location = new Point(90, 119);
        _crewRaceCombo.Name = "_crewRaceCombo";
        _crewRaceCombo.Size = new Size(573, 23);
        _crewRaceCombo.TabIndex = 9;
        // 
        // _statsPanel
        // 
        _statsPanel.AutoSize = true;
        _statsPanel.ColumnCount = 2;
        _statsPanel.ColumnStyles.Add(new ColumnStyle());
        _statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _statsPanel.Controls.Add(_hyperdriveLabel, 0, 0);
        _statsPanel.Controls.Add(_hyperdriveField, 1, 0);
        _statsPanel.Controls.Add(_fleetLabel, 0, 1);
        _statsPanel.Controls.Add(_fleetField, 1, 1);
        _statsPanel.Controls.Add(_baseItemsLabel, 0, 2);
        _statsPanel.Controls.Add(_baseItemsField, 1, 2);
        _statsPanel.Controls.Add(_modelSeedLabel, 0, 3);
        _statsPanel.Controls.Add(_modelSeedPanel, 1, 3);
        _statsPanel.Controls.Add(_crewSeedLabel, 0, 4);
        _statsPanel.Controls.Add(_crewSeedPanel, 1, 4);
        _statsPanel.Dock = DockStyle.Fill;
        _statsPanel.Location = new Point(675, 3);
        _statsPanel.Name = "_statsPanel";
        _statsPanel.RowCount = 5;
        _statsPanel.RowStyles.Add(new RowStyle());
        _statsPanel.RowStyles.Add(new RowStyle());
        _statsPanel.RowStyles.Add(new RowStyle());
        _statsPanel.RowStyles.Add(new RowStyle());
        _statsPanel.RowStyles.Add(new RowStyle());
        _statsPanel.Size = new Size(666, 145);
        _statsPanel.TabIndex = 1;
        // 
        // _hyperdriveLabel
        // 
        _hyperdriveLabel.Anchor = AnchorStyles.Left;
        _hyperdriveLabel.AutoSize = true;
        _hyperdriveLabel.Location = new Point(3, 5);
        _hyperdriveLabel.Name = "_hyperdriveLabel";
        _hyperdriveLabel.Padding = new Padding(0, 4, 10, 0);
        _hyperdriveLabel.Size = new Size(78, 19);
        _hyperdriveLabel.TabIndex = 0;
        _hyperdriveLabel.Text = "Hyperdrive:";
        // 
        // _hyperdriveField
        // 
        _hyperdriveField.Dock = DockStyle.Fill;
        _hyperdriveField.Location = new Point(127, 3);
        _hyperdriveField.Name = "_hyperdriveField";
        _hyperdriveField.Size = new Size(536, 23);
        _hyperdriveField.TabIndex = 1;
        // 
        // _fleetLabel
        // 
        _fleetLabel.Anchor = AnchorStyles.Left;
        _fleetLabel.AutoSize = true;
        _fleetLabel.Location = new Point(3, 34);
        _fleetLabel.Name = "_fleetLabel";
        _fleetLabel.Padding = new Padding(0, 4, 10, 0);
        _fleetLabel.Size = new Size(118, 19);
        _fleetLabel.TabIndex = 2;
        _fleetLabel.Text = "Fleet Coordination:";
        // 
        // _fleetField
        // 
        _fleetField.Dock = DockStyle.Fill;
        _fleetField.Location = new Point(127, 32);
        _fleetField.Name = "_fleetField";
        _fleetField.Size = new Size(536, 23);
        _fleetField.TabIndex = 3;
        // 
        // _baseItemsLabel
        // 
        _baseItemsLabel.Anchor = AnchorStyles.Left;
        _baseItemsLabel.AutoSize = true;
        _baseItemsLabel.Location = new Point(3, 63);
        _baseItemsLabel.Name = "_baseItemsLabel";
        _baseItemsLabel.Padding = new Padding(0, 4, 10, 0);
        _baseItemsLabel.Size = new Size(49, 19);
        _baseItemsLabel.TabIndex = 4;
        _baseItemsLabel.Text = "Items:";
        // 
        // _baseItemsField
        // 
        _baseItemsField.Dock = DockStyle.Fill;
        _baseItemsField.Location = new Point(127, 61);
        _baseItemsField.Name = "_baseItemsField";
        _baseItemsField.ReadOnly = true;
        _baseItemsField.Size = new Size(536, 23);
        _baseItemsField.TabIndex = 5;
        // 
        // _modelSeedLabel
        // 
        _modelSeedLabel.Anchor = AnchorStyles.Left;
        _modelSeedLabel.AutoSize = true;
        _modelSeedLabel.Location = new Point(3, 92);
        _modelSeedLabel.Name = "_modelSeedLabel";
        _modelSeedLabel.Padding = new Padding(0, 4, 10, 0);
        _modelSeedLabel.Size = new Size(82, 19);
        _modelSeedLabel.TabIndex = 6;
        _modelSeedLabel.Text = "Model Seed:";
        // 
        // _modelSeedPanel
        // 
        _modelSeedPanel.ColumnCount = 2;
        _modelSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _modelSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _modelSeedPanel.Controls.Add(_modelSeed, 0, 0);
        _modelSeedPanel.Controls.Add(_generateModelSeedBtn, 1, 0);
        _modelSeedPanel.Dock = DockStyle.Fill;
        _modelSeedPanel.RowCount = 1;
        _modelSeedPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _modelSeedPanel.AutoSize = true;
        _modelSeedPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _modelSeedPanel.Padding = Padding.Empty;
        _modelSeedPanel.Margin = Padding.Empty;
        _modelSeedPanel.Name = "_modelSeedPanel";
        _modelSeedPanel.TabIndex = 7;
        // 
        // _modelSeed
        // 
        _modelSeed.Dock = DockStyle.Fill;
        _modelSeed.Name = "_modelSeed";
        _modelSeed.TabIndex = 0;
        // 
        // _generateModelSeedBtn
        // 
        _generateModelSeedBtn.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        _generateModelSeedBtn.Name = "_generateModelSeedBtn";
        _generateModelSeedBtn.AutoSize = true;
        _generateModelSeedBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _generateModelSeedBtn.MinimumSize = new Size(70, 0);
        _generateModelSeedBtn.TabIndex = 1;
        _generateModelSeedBtn.Text = "Generate";
        // 
        // _crewSeedLabel
        // 
        _crewSeedLabel.Anchor = AnchorStyles.Left;
        _crewSeedLabel.AutoSize = true;
        _crewSeedLabel.Location = new Point(3, 121);
        _crewSeedLabel.Name = "_crewSeedLabel";
        _crewSeedLabel.Padding = new Padding(0, 4, 10, 0);
        _crewSeedLabel.Size = new Size(75, 19);
        _crewSeedLabel.TabIndex = 8;
        _crewSeedLabel.Text = "Crew Seed:";
        // 
        // _crewSeedPanel
        // 
        _crewSeedPanel.ColumnCount = 2;
        _crewSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _crewSeedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _crewSeedPanel.Controls.Add(_crewSeedField, 0, 0);
        _crewSeedPanel.Controls.Add(_generateCrewSeedBtn, 1, 0);
        _crewSeedPanel.Dock = DockStyle.Fill;
        _crewSeedPanel.RowCount = 1;
        _crewSeedPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _crewSeedPanel.AutoSize = true;
        _crewSeedPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _crewSeedPanel.Padding = Padding.Empty;
        _crewSeedPanel.Margin = Padding.Empty;
        _crewSeedPanel.Name = "_crewSeedPanel";
        _crewSeedPanel.TabIndex = 9;
        // 
        // _crewSeedField
        // 
        _crewSeedField.Dock = DockStyle.Fill;
        _crewSeedField.Name = "_crewSeedField";
        _crewSeedField.TabIndex = 0;
        // 
        // _generateCrewSeedBtn
        // 
        _generateCrewSeedBtn.Anchor = AnchorStyles.Top;
        _generateCrewSeedBtn.Name = "_generateCrewSeedBtn";
        _generateCrewSeedBtn.AutoSize = true;
        _generateCrewSeedBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _generateCrewSeedBtn.MinimumSize = new Size(70, 0);
        _generateCrewSeedBtn.TabIndex = 1;
        _generateCrewSeedBtn.Text = "Generate";
        // 
        // _buttonPanel
        // 
        _buttonPanel.AutoSize = true;
        _detailsStatsLayout.SetColumnSpan(_buttonPanel, 2);
        _buttonPanel.Controls.Add(_exportBtn);
        _buttonPanel.Controls.Add(_importBtn);
        _buttonPanel.Dock = DockStyle.Fill;
        _buttonPanel.Location = new Point(3, 154);
        _buttonPanel.Name = "_buttonPanel";
        _buttonPanel.Size = new Size(1338, 29);
        _buttonPanel.TabIndex = 2;
        // 
        // _exportBtn
        // 
        _exportBtn.Location = new Point(3, 3);
        _exportBtn.Name = "_exportBtn";
        _exportBtn.AutoSize = true;
        _exportBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _exportBtn.MinimumSize = new Size(70, 0);
        _exportBtn.TabIndex = 0;
        _exportBtn.Text = "Export Freighter";
        // 
        // _importBtn
        // 
        _importBtn.Location = new Point(89, 3);
        _importBtn.Name = "_importBtn";
        _importBtn.AutoSize = true;
        _importBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _importBtn.MinimumSize = new Size(70, 0);
        _importBtn.TabIndex = 1;
        _importBtn.Text = "Import Freighter";
        //
        // _gotoJsonBtn
        // 
		_gotoJsonBtn.FlatStyle = FlatStyle.Flat;
		_gotoJsonBtn.FlatAppearance.BorderSize = 1;
		_gotoJsonBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoJsonBtn.Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point);
        _gotoJsonBtn.Size = new Size(28, 24);
        _gotoJsonBtn.Text = "\U0001F4D1";
        _gotoJsonBtn.Margin = new Padding(1, 3, 1, 1);
        _gotoJsonBtn.Cursor = Cursors.Hand;
        _gotoJsonBtn.Click += OnGoToJsonClicked;
        _gotoButtonPanelFreighter.Controls.Add(_gotoJsonBtn);
        //
        // _gotoInvBtn
        //
		_gotoInvBtn.FlatStyle = FlatStyle.Flat;
		_gotoInvBtn.FlatAppearance.BorderSize = 1;
		_gotoInvBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoInvBtn.Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point);
        _gotoInvBtn.Size = new Size(28, 24);
        _gotoInvBtn.Text = "\U0001F4D1";
        _gotoInvBtn.Margin = new Padding(1, 3, 1, 1);
        _gotoInvBtn.Cursor = Cursors.Hand;
        _gotoInvBtn.Click += OnGoToJsonInventoryClicked;
        _gotoButtonPanelFreighter.Controls.Add(_gotoInvBtn);
        //
        // _gotoTechBtn
        //
		_gotoTechBtn.FlatStyle = FlatStyle.Flat;
		_gotoTechBtn.FlatAppearance.BorderSize = 1;
		_gotoTechBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoTechBtn.Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point);
        _gotoTechBtn.Size = new Size(28, 24);
        _gotoTechBtn.Text = "\U0001F4D1";
        _gotoTechBtn.Margin = new Padding(1, 3, 1, 1);
        _gotoTechBtn.Cursor = Cursors.Hand;
        _gotoTechBtn.Click += OnGoToJsonTechClicked;
        _gotoButtonPanelFreighter.Controls.Add(_gotoTechBtn);
        // 
        // _invTabs
        // 
        _invTabs.Controls.Add(_generalPage);
        _invTabs.Controls.Add(_techPage);
        _invTabs.Dock = DockStyle.Fill;
        _invTabs.Location = new Point(3, 195);
        _invTabs.Name = "_invTabs";
        _invTabs.SelectedIndex = 0;
        _invTabs.Size = new Size(1344, 714);
        _invTabs.TabIndex = 1;
        // 
        // _generalPage
        // 
        _generalPage.Controls.Add(_generalGrid);
        _generalPage.Location = new Point(4, 24);
        _generalPage.Name = "_generalPage";
        _generalPage.Size = new Size(1336, 686);
        _generalPage.TabIndex = 0;
        _generalPage.Text = "Cargo";
        // 
        // _generalGrid
        // 
        _generalGrid.Dock = DockStyle.Fill;
        _generalGrid.Location = new Point(0, 0);
        _generalGrid.Name = "_generalGrid";
        _generalGrid.Size = new Size(1336, 686);
        _generalGrid.TabIndex = 0;
        // 
        // _techPage
        // 
        _techPage.Controls.Add(_techGrid);
        _techPage.Location = new Point(4, 24);
        _techPage.Name = "_techPage";
        _techPage.Size = new Size(192, 72);
        _techPage.TabIndex = 1;
        _techPage.Text = "Technology";
        // 
        // _techGrid
        // 
        _techGrid.Dock = DockStyle.Fill;
        _techGrid.Location = new Point(0, 0);
        _techGrid.Name = "_techGrid";
        _techGrid.Size = new Size(192, 72);
        _techGrid.TabIndex = 0;
        // 
        // _roomsSubPage
        // 
        _roomsSubPage.Controls.Add(_roomList);
        _roomsSubPage.Location = new Point(4, 24);
        _roomsSubPage.Name = "_roomsSubPage";
        _roomsSubPage.Size = new Size(1350, 912);
        _roomsSubPage.TabIndex = 1;
        _roomsSubPage.Text = "Freighter Rooms";
        // 
        // _roomList
        // 
        _roomList.Dock = DockStyle.Fill;
        _roomList.IntegralHeight = false;
        _roomList.Location = new Point(0, 0);
        _roomList.Name = "_roomList";
        _roomList.SelectionMode = SelectionMode.None;
        _roomList.Size = new Size(1350, 912);
        _roomList.TabIndex = 1;
        // 
        // FreighterPanel
        // 
        Controls.Add(_mainLayout);
        DoubleBuffered = true;
        Name = "FreighterPanel";
        Size = new Size(1384, 995);
        _mainLayout.ResumeLayout(false);
        _mainLayout.PerformLayout();
        _headerStrip.ResumeLayout(false);
        _headerStrip.PerformLayout();
        _gotoButtonPanelFreighter.ResumeLayout(false);
        _gotoButtonPanelFreighter.PerformLayout();
        _subTabs.ResumeLayout(false);
        _freighterSubPage.ResumeLayout(false);
        _freighterContentLayout.ResumeLayout(false);
        _freighterContentLayout.PerformLayout();
        _detailsStatsLayout.ResumeLayout(false);
        _detailsStatsLayout.PerformLayout();
        _detailsPanel.ResumeLayout(false);
        _detailsPanel.PerformLayout();
        _homeSeedPanel.ResumeLayout(false);
        _homeSeedPanel.PerformLayout();
        _statsPanel.ResumeLayout(false);
        _statsPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_hyperdriveField).EndInit();
        ((System.ComponentModel.ISupportInitialize)_fleetField).EndInit();
        _modelSeedPanel.ResumeLayout(false);
        _modelSeedPanel.PerformLayout();
        _crewSeedPanel.ResumeLayout(false);
        _crewSeedPanel.PerformLayout();
        _buttonPanel.ResumeLayout(false);
        _invTabs.ResumeLayout(false);
        _generalPage.ResumeLayout(false);
        _techPage.ResumeLayout(false);
        _roomsSubPage.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private void SetupLayout()
    {
        _freighterType.Items.AddRange(FreighterLogic.GetFreighterTypeItems());
        _freighterClass.Items.AddRange(FreighterClasses);
        _crewRaceCombo.Items.AddRange(FreighterLogic.GetCrewRaceItems());

        _generateHomeSeedBtn.Click += (s, e) =>
        {
            byte[] bytes = new byte[8];
            _rng.NextBytes(bytes);
            _homeSeed.Text = "0x" + BitConverter.ToString(bytes).Replace("-", "");
            RaiseDataModified();
        };
        _generateModelSeedBtn.Click += (s, e) =>
        {
            byte[] bytes = new byte[8];
            _rng.NextBytes(bytes);
            _modelSeed.Text = "0x" + BitConverter.ToString(bytes).Replace("-", "");
            RaiseDataModified();
        };
        _generateCrewSeedBtn.Click += (s, e) =>
        {
            byte[] bytes = new byte[8];
            _rng.NextBytes(bytes);
            _crewSeedField.Text = "0x" + BitConverter.ToString(bytes).Replace("-", "");
            RaiseDataModified();
        };
        _exportBtn.Click += OnBackup;
        _importBtn.Click += OnRestore;

        _generalGrid.DataModified += (s, e) => DataModified?.Invoke(this, e);
        _techGrid.DataModified += (s, e) => DataModified?.Invoke(this, e);

        // Wire non-inventory field changes to raise DataModified
        _freighterName.Leave += (s, e) => RaiseDataModified();
        _freighterName.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _freighterName.Parent?.Focus(); }
        };
        _freighterType.SelectedIndexChanged += (s, e) => RaiseDataModified();
        _freighterClass.SelectedIndexChanged += (s, e) => RaiseDataModified();
        _homeSeed.Leave += (s, e) => RaiseDataModified();
        _homeSeed.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _homeSeed.Parent?.Focus(); }
        };
        _modelSeed.Leave += (s, e) => RaiseDataModified();
        _modelSeed.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _modelSeed.Parent?.Focus(); }
        };
        _crewSeedField.Leave += (s, e) => RaiseDataModified();
        _crewSeedField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _crewSeedField.Parent?.Focus(); }
        };
        _crewRaceCombo.SelectedIndexChanged += (s, e) => RaiseDataModified();
        _hyperdriveField.NumericValueChanged += (s, e) => RaiseDataModified();
        _fleetField.NumericValueChanged += (s, e) => RaiseDataModified();

        _techGrid.SetIsTechInventory(true);
        _generalGrid.SetIsCargoInventory(true);
        _generalGrid.SetSortingEnabled(true);
        _techGrid.SetInventoryOwnerType("Freighter");
        _generalGrid.SetInventoryOwnerType("Freighter");
        _generalGrid.SetInventoryGroup("FreighterCargo");
        _techGrid.SetInventoryGroup("Freighter");
        _generalGrid.SetSuperchargeDisabled(true);
        _generalGrid.SetMaxSupportedLabel("");
        _techGrid.SetMaxSupportedLabel("");
    }

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.TableLayoutPanel _headerStrip;
    private System.Windows.Forms.FlowLayoutPanel _gotoButtonPanelFreighter;
    private System.Windows.Forms.Label _titleLabel;
    private NMSE.UI.Panels.DoubleBufferedTabControl _subTabs;
    private System.Windows.Forms.TabPage _freighterSubPage;
    private System.Windows.Forms.TabPage _roomsSubPage;
    private System.Windows.Forms.TableLayoutPanel _freighterContentLayout;
    private System.Windows.Forms.TableLayoutPanel _detailsStatsLayout;
    private System.Windows.Forms.TableLayoutPanel _detailsPanel;
    private System.Windows.Forms.TableLayoutPanel _statsPanel;
    private System.Windows.Forms.FlowLayoutPanel _buttonPanel;
    private System.Windows.Forms.TableLayoutPanel _homeSeedPanel;
    private System.Windows.Forms.TableLayoutPanel _modelSeedPanel;
    private System.Windows.Forms.TableLayoutPanel _crewSeedPanel;
    private System.Windows.Forms.Label _nameLabel;
    private System.Windows.Forms.Label _typeLabel;
    private System.Windows.Forms.Label _classLabel;
    private System.Windows.Forms.Label _homeSeedLabel;
    private System.Windows.Forms.Label _crewRaceLabel;
    private System.Windows.Forms.Label _hyperdriveLabel;
    private System.Windows.Forms.Label _fleetLabel;
    private System.Windows.Forms.Label _baseItemsLabel;
    private System.Windows.Forms.Label _modelSeedLabel;
    private System.Windows.Forms.Label _crewSeedLabel;
    private System.Windows.Forms.TextBox _freighterName;
    private System.Windows.Forms.ComboBox _freighterType;
    private System.Windows.Forms.ComboBox _freighterClass;
    private System.Windows.Forms.TextBox _homeSeed;
    private System.Windows.Forms.Button _generateHomeSeedBtn;
    private System.Windows.Forms.ComboBox _crewRaceCombo;
    private NMSE.UI.Controls.InvariantNumericTextBox _hyperdriveField;
    private NMSE.UI.Controls.InvariantNumericTextBox _fleetField;
    private System.Windows.Forms.TextBox _baseItemsField;
    private System.Windows.Forms.TextBox _modelSeed;
    private System.Windows.Forms.Button _generateModelSeedBtn;
    private System.Windows.Forms.TextBox _crewSeedField;
    private System.Windows.Forms.Button _generateCrewSeedBtn;
    private System.Windows.Forms.Button _exportBtn;
    private System.Windows.Forms.Button _importBtn;
    private NMSE.UI.Panels.DoubleBufferedTabControl _invTabs;
    private System.Windows.Forms.TabPage _generalPage;
    private System.Windows.Forms.TabPage _techPage;
    private NMSE.UI.Panels.InventoryGridPanel _generalGrid;
    private NMSE.UI.Panels.InventoryGridPanel _techGrid;
    private System.Windows.Forms.ListBox _roomList;
    private System.Windows.Forms.Button _gotoJsonBtn;
    private System.Windows.Forms.Button _gotoInvBtn;
    private System.Windows.Forms.Button _gotoTechBtn;
}
