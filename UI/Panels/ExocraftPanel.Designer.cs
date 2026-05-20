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
        _buttonPanel = new FlowLayoutPanel();
        _invPage = new TabPage();
        _techPage = new TabPage();

        _invTabs.SuspendLayout();
        _invPage.SuspendLayout();
        _techPage.SuspendLayout();
        _layout.SuspendLayout();
        SuspendLayout();

        // 
        // _layout
        // 
        _layout.Dock = DockStyle.Fill;
        _layout.ColumnCount = 2;
        _layout.RowCount = 7;
        _layout.Padding = new Padding(10);
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        _layout.SetColumnSpan(_titleLabel, 2);

        // 
        // _vehicleLabel
        // 
        _vehicleLabel.Text = "Vehicle:";
        _vehicleLabel.AutoSize = true;
        _vehicleLabel.Anchor = AnchorStyles.Left;
        _vehicleLabel.Padding = new Padding(0, 5, 10, 0);

        // 
        // _vehicleSelector
        // 
        _vehicleSelector.Dock = DockStyle.Fill;
        _vehicleSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _vehicleSelector.SelectedIndexChanged += OnVehicleSelected;
        _layout.Controls.Add(_vehicleLabel, 0, 1);
        _layout.Controls.Add(_vehicleSelector, 1, 1);

        // 
        // _nameLabel
        // 
        _nameLabel.Text = "Name:";
        _nameLabel.AutoSize = true;
        _nameLabel.Anchor = AnchorStyles.Left;
        _nameLabel.Padding = new Padding(0, 5, 10, 0);

        // 
        // _nameField
        // 
        _nameField.Dock = DockStyle.Fill;
        _nameField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _nameField.Parent?.Focus(); }
        };
        _layout.Controls.Add(_nameLabel, 0, 2);
        _layout.Controls.Add(_nameField, 1, 2);

        //
        // Row 3 - Primary + Deployed + Undeploy
        //
        var statusPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };

        _primaryVehicleCheck = new CheckBox { Text = "Primary Vehicle", AutoSize = true };
        _primaryVehicleCheck.CheckedChanged += OnPrimaryVehicleChanged;

        _deployedLabel = new Label { Text = "", AutoSize = true, Padding = new Padding(10, 5, 10, 0) };

        _undeployBtn = new Button { Text = "Undeploy", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(80, 0) };
        _undeployBtn.Click += OnUndeploy;

        statusPanel.Controls.Add(_primaryVehicleCheck);
        statusPanel.Controls.Add(_deployedLabel);
        statusPanel.Controls.Add(_undeployBtn);
        _layout.Controls.Add(statusPanel, 0, 3);
        _layout.SetColumnSpan(statusPanel, 2);

        // 
        // _buttonPanel
        // 
        _buttonPanel.Dock = DockStyle.Fill;
        _buttonPanel.AutoSize = true;
        _buttonPanel.FlowDirection = FlowDirection.LeftToRight;

        // 
        // _exportBtn
        // 
        _exportBtn.Text = "Export Exocraft";
        _exportBtn.AutoSize = true;
        _exportBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _exportBtn.MinimumSize = new Size(75, 0);
        _exportBtn.Click += OnExportVehicle;

        // 
        // _importBtn
        // 
        _importBtn.Text = "Import Exocraft";
        _importBtn.AutoSize = true;
        _importBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _importBtn.MinimumSize = new Size(75, 0);
        _importBtn.Click += OnImportVehicle;

        // 
        // _thirdPersonCam
        // 
        _thirdPersonCam.Text = "Third Person Camera";
        _thirdPersonCam.AutoSize = true;

        // 
        // _minotaurAI
        // 
        _minotaurAI.Text = "Minotaur AI Pilot";
        _minotaurAI.AutoSize = true;

        _buttonPanel.Controls.Add(_exportBtn);
        _buttonPanel.Controls.Add(_importBtn);
        _buttonPanel.Controls.Add(_thirdPersonCam);
        _buttonPanel.Controls.Add(_minotaurAI);
        _layout.Controls.Add(_buttonPanel, 0, 4);
        _layout.SetColumnSpan(_buttonPanel, 2);

        // 
        // _inventoryGrid
        // 
        _inventoryGrid.Dock = DockStyle.Fill;

        // 
        // _techGrid
        // 
        _techGrid.Dock = DockStyle.Fill;

        // 
        // _techNoteLabel
        // 
        _techNoteLabel.Text = "Note: Supercharged slots are fixed for this inventory type in game and can't be modified.";
        _techNoteLabel.AutoSize = true;
        _techNoteLabel.ForeColor = System.Drawing.Color.FromArgb(200, 160, 0);
        _techNoteLabel.Padding = new Padding(0, 0, 0, 5);
        _techNoteLabel.Visible = false;
        _layout.Controls.Add(_techNoteLabel, 0, 5);
        _layout.SetColumnSpan(_techNoteLabel, 2);

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
        _layout.Controls.Add(_invTabs, 0, 6);
        _layout.SetColumnSpan(_invTabs, 2);

        // 
        // ExocraftPanel
        // 
        DoubleBuffered = true;
        Controls.Add(_layout);

        _invTabs.ResumeLayout(false);
        _invPage.ResumeLayout(false);
        _techPage.ResumeLayout(false);
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
    private FlowLayoutPanel _buttonPanel;
    private TabPage _invPage;
    private TabPage _techPage;
    private CheckBox _primaryVehicleCheck;
    private Label _deployedLabel;
    private Button _undeployBtn;
}
