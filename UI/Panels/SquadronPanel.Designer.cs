#nullable enable
using NMSE.Core;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class SquadronPanel
{
    private System.ComponentModel.IContainer? components = null;

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
        // SquadronPanel
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.DoubleBuffered = true;
        this.ResumeLayout(false);
    }
    #endregion

    private void SetupLayout()
    {

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // -- Left column: list + buttons --
        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _titleLabel = new Label
        {
            Text = "Squadron Pilots",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 5),
            Anchor = AnchorStyles.Left
        };
        FontManager.ApplyHeadingFont(_titleLabel, 14);

        var headerStrip = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0),
        };
        headerStrip.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        headerStrip.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        headerStrip.Controls.Add(_titleLabel, 0, 0);

		var gotoButtonPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Anchor = AnchorStyles.Left,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
		};
        headerStrip.Controls.Add(gotoButtonPanel, 2, 0);
        headerStrip.Padding = new Padding(0, 0, 10, 0);

        _pilotList = new ListBox { Dock = DockStyle.Fill };
        _pilotList.SelectedIndexChanged += OnPilotSelected;
        leftLayout.Controls.Add(_pilotList, 0, 1);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        _countLabel = new Label { Text = "No pilots loaded.", AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 5, 0, 0) };
        _deleteBtn = new Button { Text = "Delete", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 0) };
        _deleteBtn.Click += OnDelete;
        _exportBtn = new Button { Text = "Export", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 0) };
        _exportBtn.Click += OnExport;
        _importBtn = new Button { Text = "Import", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(75, 0) };
        _importBtn.Click += OnImport;

        _gotoListBtn = new Button();
		_gotoListBtn.FlatStyle = FlatStyle.Flat;
		_gotoListBtn.FlatAppearance.BorderSize = 1;
		_gotoListBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoListBtn.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        _gotoListBtn.Size = new System.Drawing.Size(28, 24);
        _gotoListBtn.Text = "\U0001F4D1";
        _gotoListBtn.Margin = new Padding(1, 3, 1, 1);
        _gotoListBtn.Cursor = Cursors.Hand;
        _gotoListBtn.Click += OnGoToJsonListClicked;
        gotoButtonPanel.Controls.Add(_gotoListBtn);

        _gotoSelectedBtn = new Button();
		_gotoSelectedBtn.FlatStyle = FlatStyle.Flat;
		_gotoSelectedBtn.FlatAppearance.BorderSize = 1;
		_gotoSelectedBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        _gotoSelectedBtn.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        _gotoSelectedBtn.Size = new System.Drawing.Size(28, 24);
        _gotoSelectedBtn.Text = "\U0001F4D1";
        _gotoSelectedBtn.Margin = new Padding(1, 3, 1, 1);
        _gotoSelectedBtn.Cursor = Cursors.Hand;
        _gotoSelectedBtn.Click += OnGoToJsonSelectedClicked;
        gotoButtonPanel.Controls.Add(_gotoSelectedBtn);

        btnPanel.Controls.Add(_deleteBtn);
        btnPanel.Controls.Add(_exportBtn);
        btnPanel.Controls.Add(_importBtn);
        btnPanel.Controls.Add(_countLabel);
        leftLayout.Controls.Add(btnPanel, 0, 2);

        mainLayout.Controls.Add(headerStrip, 0, 0);
        mainLayout.SetColumnSpan(headerStrip, 2);
        mainLayout.Controls.Add(leftLayout, 0, 1);

        // -- Right column: detail panel --
        _detailPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Visible = false };
        var detailLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        int row = 0;
        int totalRows = 1 + 9;  // section header + 9 fields
        detailLayout.RowCount = totalRows;
        for (int i = 0; i < totalRows; i++)
            detailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _pilotInfoLabel = AddSectionHeader(detailLayout, "Pilot Info", row++);

        _raceField = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var r in SquadronLogic.PilotRaces)
            _raceField.Items.Add(SquadronLogic.GetLocalisedPilotRaceName(r));
        _raceField.SelectedIndexChanged += (s, e) =>
        {
            if (_loading) return;
            var pilot = SelectedPilot();
            if (pilot == null || _raceField.SelectedIndex < 0) return;
            SquadronLogic.SetPilotRace(pilot, SquadronLogic.PilotRaces[_raceField.SelectedIndex]);
            RefreshListEntry();
            RaiseDataModified();
        };
        _raceLabel = AddRow(detailLayout, "Race:", _raceField, row++);

        _rankField = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _rankField.Items.AddRange(SquadronLogic.PilotRanks);
        _rankField.SelectedIndexChanged += (s, e) =>
        {
            if (_loading) return;
            var pilot = SelectedPilot();
            if (pilot == null || _rankField.SelectedIndex < 0) return;
            try { pilot.Set("PilotRank", _rankField.SelectedIndex); RaiseDataModified(); } catch { }
            RefreshListEntry();
        };
        _rankLabel = AddRow(detailLayout, "Rank:", _rankField, row++);

        _shipTypeCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _shipTypeCombo.Items.AddRange(SquadronLogic.GetShipTypeItems());
        _shipTypeCombo.SelectedIndexChanged += (s, e) =>
        {
            if (_loading) return;
            var pilot = SelectedPilot();
            if (pilot == null || _shipTypeCombo.SelectedIndex < 0) return;
            var typeItem = _shipTypeCombo.SelectedItem as StarshipLogic.ShipTypeItem;
            if (typeItem != null && SquadronLogic.ShipTypeToResource.TryGetValue(typeItem.InternalName, out string? resource))
            {
                string res = resource ?? "";
                try { pilot.GetObject("ShipResource")?.Set("Filename", res); } catch { }
                try { if (_shipResourceField != null) _shipResourceField.Text = res; } catch { }
                RaiseDataModified();
            }
            RefreshListEntry();
        };
        _shipTypeLabel = AddRow(detailLayout, "Ship Type:", _shipTypeCombo, row++);

        _npcSeedField = new TextBox { Dock = DockStyle.Fill };
        _npcSeedField.Leave += (s, e) => { if (!_loading) WriteNpcSeed(); };
        _npcSeedField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _npcSeedField.Parent?.Focus(); }
        };
        _npcSeedLabel = AddSeedRow(detailLayout, "NPC Seed:", _npcSeedField, WriteNpcSeed, row++);

        _shipSeedField = new TextBox { Dock = DockStyle.Fill };
        _shipSeedField.Leave += (s, e) => { if (!_loading) WriteShipSeed(); };
        _shipSeedField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _shipSeedField.Parent?.Focus(); }
        };
        _shipSeedLabel = AddSeedRow(detailLayout, "Ship Seed:", _shipSeedField, WriteShipSeed, row++);

        _traitsSeedField = new TextBox { Dock = DockStyle.Fill };
        _traitsSeedField.Leave += (s, e) => { if (!_loading) WriteTraitsSeed(); };
        _traitsSeedField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _traitsSeedField.Parent?.Focus(); }
        };
        _traitsSeedLabel = AddSeedRow(detailLayout, "Traits Seed:", _traitsSeedField, WriteTraitsSeed, row++);

        _npcResourceField = new TextBox { Dock = DockStyle.Fill, ReadOnly = true};
        _npcResourceField.Leave += (s, e) =>
        {
            if (_loading) return;
            var pilot = SelectedPilot();
            if (pilot == null) return;
            try { pilot.GetObject("NPCResource")?.Set("Filename", _npcResourceField.Text); RaiseDataModified(); } catch { }
            RefreshListEntry();
        };
        _npcResourceField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _npcResourceField.Parent?.Focus(); }
        };
        _npcResourceLabel = AddRow(detailLayout, "NPC Resource:", _npcResourceField, row++);

        _shipResourceField = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
        _shipResourceField.Leave += (s, e) =>
        {
            if (_loading) return;
            var pilot = SelectedPilot();
            if (pilot == null) return;
            try { pilot.GetObject("ShipResource")?.Set("Filename", _shipResourceField.Text); RaiseDataModified(); } catch { }
            RefreshListEntry();
        };
        _shipResourceField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _shipResourceField.Parent?.Focus(); }
        };
        _shipResourceLabel = AddRow(detailLayout, "Ship Resource:", _shipResourceField, row++);

        _unlockedCheck = new CheckBox { Text = "", AutoSize = true };
        _unlockedCheck.CheckedChanged += (s, e) =>
        {
            if (_loading) return;
            int selectedIndex = _pilotList.SelectedIndex;
            if (_unlockedSlots != null && selectedIndex >= 0 && selectedIndex < _unlockedSlots.Length)
            {
                _unlockedSlots.Set(selectedIndex, _unlockedCheck.Checked);
                RaiseDataModified();
            }
        };
        _slotUnlockedLabel = AddRow(detailLayout, "Slot Unlocked:", _unlockedCheck, row++);

        _detailPanel.Controls.Add(detailLayout);
        mainLayout.Controls.Add(_detailPanel, 1, 1);

        Controls.Add(mainLayout);
        ResumeLayout(false);
        PerformLayout();
    }

    private ListBox _pilotList = null!;
    private Label _countLabel = null!;
    private Panel _detailPanel = null!;
    private Button _deleteBtn = null!;
    private Button _exportBtn = null!;
    private Button _importBtn = null!;
    private ComboBox _raceField = null!;
    private ComboBox _rankField = null!;
    private ComboBox _shipTypeCombo = null!;
    private TextBox _npcSeedField = null!;
    private TextBox _shipSeedField = null!;
    private TextBox _traitsSeedField = null!;
    private TextBox _npcResourceField = null!;
    private TextBox _shipResourceField = null!;
    private CheckBox _unlockedCheck = null!;
    private Label _titleLabel = null!;
    private Label _pilotInfoLabel = null!;
    private Label _raceLabel = null!;
    private Label _rankLabel = null!;
    private Label _shipTypeLabel = null!;
    private Label _npcSeedLabel = null!;
    private Label _shipSeedLabel = null!;
    private Label _traitsSeedLabel = null!;
    private Label _npcResourceLabel = null!;
    private Label _shipResourceLabel = null!;
    private Label _slotUnlockedLabel = null!;
    private Button _gotoListBtn = null!;
    private Button _gotoSelectedBtn = null!;
}
