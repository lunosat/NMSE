using NMSE.Core;
using NMSE.Data;
using NMSE.UI.Controls;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class MultitoolPanel
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
        this.SuspendLayout();
        // 
        // MultitoolPanel
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.DoubleBuffered = true;
        this.ResumeLayout(false);
    }
    #endregion

    private void SetupLayout()
    {
        SuspendLayout();

        // Main layout: 1 column, stack everything vertically
        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10),
            AutoSize = true
        };
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // title row
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // selector row
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // main two-column panel
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // buttons

        // Title
        _titleLabel = new Label
        {
            Text = "Multi-tools",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 1),
            Anchor = AnchorStyles.Left
        };
        FontManager.ApplyHeadingFont(_titleLabel, 14);

        _primaryToolLabel = new Label
        {
            Text = "Primary Multi-tool: (none)",
            AutoSize = true,
            Padding = new Padding(8, 6, 0, 0),
            ForeColor = SystemColors.GrayText
        };

        // Header strip: title | spacer | goto buttons
        var headerStrip = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1
        };
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var titlePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        titlePanel.Controls.Add(_titleLabel);
        titlePanel.Controls.Add(_primaryToolLabel);
        headerStrip.Controls.Add(titlePanel, 0, 0);

		var gotoButtonPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
		};

        _gotoListBtn = new Button
        {
            FlatStyle = FlatStyle.Flat,
            Size = new Size(28, 24),
            Text = "\U0001F4D1",
            Margin = new Padding(1, 3, 1, 1),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point)
        };
        _gotoListBtn.Click += OnGoToJsonListClicked;
        gotoButtonPanel.Controls.Add(_gotoListBtn);

        _gotoSelectedBtn = new Button
        {
            FlatStyle = FlatStyle.Flat,
            Size = new Size(28, 24),
            Text = "\U0001F4D1",
            Margin = new Padding(1, 3, 1, 1),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point)
        };
        _gotoSelectedBtn.Click += OnGoToJsonSelectedClicked;
        gotoButtonPanel.Controls.Add(_gotoSelectedBtn);

        _gotoStoreBtn = new Button
        {
            FlatStyle = FlatStyle.Flat,
            Size = new Size(28, 24),
            Text = "\U0001F4D1",
            Margin = new Padding(1, 3, 1, 1),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Emoji", 9F, FontStyle.Regular, GraphicsUnit.Point)
        };
        _gotoStoreBtn.Click += OnGoToJsonStoreClicked;
        gotoButtonPanel.Controls.Add(_gotoStoreBtn);

        headerStrip.Controls.Add(gotoButtonPanel, 2, 0);

        rootLayout.Controls.Add(headerStrip, 0, 0);

        // Selector row (label + dropdown + archive buttons), similar to the Starship panel
        var selectorRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 4),
        };
        selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selectorRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _selectLabel = new Label
        {
            Text = "Select Multi-tool:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 5, 10, 0),
        };
        _toolSelector = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _toolSelector.SelectedIndexChanged += OnToolSelected;
        selectorRow.Controls.Add(_selectLabel, 0, 0);
        selectorRow.Controls.Add(_toolSelector, 1, 0);

        _archiveMoveBtn = new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(3, 0, 0, 0),
        };
        _archiveMoveBtn.Click += OnMoveToolToArchive;
        selectorRow.Controls.Add(_archiveMoveBtn, 2, 0);

        _archiveImportBtn = new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(3, 0, 0, 0),
        };
        _archiveImportBtn.Click += OnImportToolFromArchive;
        selectorRow.Controls.Add(_archiveImportBtn, 3, 0);

        rootLayout.Controls.Add(selectorRow, 0, 1);

        // Main two-column panel
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent) { Width = 50 });
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent) { Width = 50 });

        // Left panel: name, type+size, class, seed
        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = true
        };
        leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (int i = 0; i < 5; i++)
            leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        int leftRow = 0;

        _detailsLabel = new Label
        {
            Text = "Multitool Details",
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 1)
        };
        FontManager.ApplyHeadingFont(_detailsLabel, 10);
        leftPanel.Controls.Add(_detailsLabel, 0, leftRow);
        leftPanel.SetColumnSpan(_detailsLabel, 2);
        leftRow++;

        _toolName = new TextBox { Dock = DockStyle.Fill };
        _toolName.Leave += OnToolNameChanged;
        _toolName.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _toolName.Parent?.Focus(); }
        };
        _nameLabel = AddRow(leftPanel, "Name:", _toolName, leftRow++);

        _toolType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _toolType.Items.AddRange(MultitoolLogic.GetToolTypeItems());
        _toolType.SelectedIndexChanged += OnToolTypeChanged;

        _toolSize = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _toolSize.Items.AddRange(MultitoolLogic.GetToolSizeItems());

        // Type and Size share one row, following the same pattern as the Seed row:
        // "Type:" label goes into leftPanel col 0 (via AddRow), and a flat inner panel
        // containing [TypeCombo | SizeLabel | SizeCombo] goes into leftPanel col 1.
        var typeSizePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };
        typeSizePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        typeSizePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        typeSizePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        typeSizePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _sizeLabel = new Label { Text = "Size:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(6, 0, 10, 0) };
        typeSizePanel.Controls.Add(_toolType, 0, 0);
        typeSizePanel.Controls.Add(_sizeLabel, 1, 0);
        typeSizePanel.Controls.Add(_toolSize, 2, 0);
        _typeLabel = AddRow(leftPanel, "Type:", typeSizePanel, leftRow++);

        _toolClass = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _toolClass.Items.AddRange(MultitoolLogic.ToolClasses);
        _classLabel = AddRow(leftPanel, "Class:", _toolClass, leftRow++);

        var seedPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        seedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        seedPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        seedPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _toolSeed = new TextBox { Dock = DockStyle.Fill };
        _toolSeed.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _toolSeed.Parent?.Focus(); }
        };
        _generateSeedBtn = new Button
        {
            Text = "Generate",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(70, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
        };
        _generateSeedBtn.Click += (s, e) =>
        {
            byte[] bytes = new byte[8];
            _rng.NextBytes(bytes);
            _toolSeed.Text = "0x" + BitConverter.ToString(bytes).Replace("-", "");
        };
        seedPanel.Controls.Add(_toolSeed, 0, 0);
        seedPanel.Controls.Add(_generateSeedBtn, 1, 0);
        _seedLabel = AddRow(leftPanel, "Seed:", seedPanel, leftRow++);

        // Right panel: Base Stats
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            AutoSize = true
        };
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        int rightRow = 0;

        _statsLabel = new Label
        {
            Text = "Base Stats",
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 1)
        };
        FontManager.ApplyHeadingFont(_statsLabel, 10);
        rightPanel.Controls.Add(_statsLabel, 0, rightRow);
        rightPanel.SetColumnSpan(_statsLabel, 2);
        rightRow++;

        _damageField = new InvariantNumericTextBox { Dock = DockStyle.Fill };
        _damageLabel = AddRow(rightPanel, "Damage:", _damageField, rightRow++);

        _miningField = new InvariantNumericTextBox { Dock = DockStyle.Fill };
        _miningLabel = AddRow(rightPanel, "Mining:", _miningField, rightRow++);

        _scanField = new InvariantNumericTextBox { Dock = DockStyle.Fill };
        _scanLabel = AddRow(rightPanel, "Scan:", _scanField, rightRow++);

        mainPanel.Controls.Add(leftPanel, 0, 0);
        mainPanel.Controls.Add(rightPanel, 1, 0);

        rootLayout.Controls.Add(mainPanel, 0, 2);

        // Buttons panel
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        _deleteBtn = new Button { Text = "Delete Multitool", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(110, 0) };
        _deleteBtn.Click += OnDeleteTool;
        _exportBtn = new Button { Text = "Export Multitool", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 0) };
        _exportBtn.Click += OnExportTool;
        _importBtn = new Button { Text = "Import Multitool", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 0) };
        _importBtn.Click += OnImportTool;
        _makePrimaryBtn = new Button { Text = "Make Primary", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(88, 0) };
        _makePrimaryBtn.Click += OnMakePrimary;
        buttonPanel.Controls.Add(_deleteBtn);
        buttonPanel.Controls.Add(_exportBtn);
        buttonPanel.Controls.Add(_importBtn);
        buttonPanel.Controls.Add(_makePrimaryBtn);

        rootLayout.Controls.Add(buttonPanel, 0, 3);

        // Inventory grid (fills remaining space)
        _storeGrid = new InventoryGridPanel { Dock = DockStyle.Fill };
        _storeGrid.SetIsTechInventory(true);
        _storeGrid.SetInventoryOwnerType("Weapon");
        _storeGrid.SetInventoryGroup("Personal");
        _storeGrid.DataModified += (s, e) => DataModified?.Invoke(this, e);
        rootLayout.Controls.Add(_storeGrid);

        Controls.Add(rootLayout);

        // Set Max Supported label for multitool technology
        _storeGrid.SetMaxSupportedLabel(UiStrings.Format("common.max_supported", "10x6"));

        _gotoListBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoSelectedBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoStoreBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;

        ResumeLayout(false);
        PerformLayout();
    }

    private ComboBox _toolSelector;
    private TextBox _toolName;
    private ComboBox _toolClass;
    private TextBox _toolSeed;
    private Button _generateSeedBtn;
    private Button _deleteBtn;
    private Button _exportBtn;
    private Button _importBtn;
    private Button _makePrimaryBtn;
    private Button _archiveMoveBtn;
    private Button _archiveImportBtn;
    private Button _gotoListBtn;
    private Button _gotoSelectedBtn;
    private Button _gotoStoreBtn;
    private Label _primaryToolLabel;
    private Label _titleLabel;
    private Label _detailsLabel;
    private Label _statsLabel;
    private Label _selectLabel;
    private Label _nameLabel;
    private Label _typeLabel;
    private Label _classLabel;
    private Label _seedLabel;
    private Label _damageLabel;
    private Label _miningLabel;
    private Label _scanLabel;
    private InvariantNumericTextBox _damageField;
    private InvariantNumericTextBox _miningField;
    private InvariantNumericTextBox _scanField;
    private InventoryGridPanel _storeGrid;
    private ComboBox _toolType;
    private ComboBox _toolSize;
    private Label _sizeLabel;
}
