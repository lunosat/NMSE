#nullable enable
using NMSE.Data;
using NMSE.UI.Controls;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class MultiplayerPanel
{
    private const int SeedFieldWidth = 160;
    private const int ComboWidth = 160;

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
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.DoubleBuffered = true;
        this.ResumeLayout(false);
    }
    #endregion

    private static Label AddRow(TableLayoutPanel layout, string labelText, Control field, int row)
    {
        var lbl = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 5, 10, 0) };
        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(field, 1, row);
        return lbl;
    }

    private static Label AddCenteredRow(TableLayoutPanel layout, string labelText, Control field, int row)
    {
        var lbl = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 3, 10, 0) };
        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(field, 1, row);
        return lbl;
    }

    private static Label AddHeading(TableLayoutPanel layout, int row, string text)
    {
        var lbl = new Label
        {
            Text = text.Replace("&", "&&"),
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 4),
            Margin = new Padding(0, 8, 0, 0)
        };
        FontManager.ApplyHeadingFont(lbl, 10);
        layout.Controls.Add(lbl, 0, row);
        layout.SetColumnSpan(lbl, 2);
        return lbl;
    }

    private CheckBox AddCheckBoxRow(TableLayoutPanel layout, int row, string text)
    {
        var check = new CheckBox { Text = text.Replace("&", "&&"), AutoSize = true };
        check.CheckedChanged += (s, e) => RaiseDataModified();
        layout.Controls.Add(check, 0, row);
        layout.SetColumnSpan(check, 2);
        return check;
    }

    private (TableLayoutPanel panel, CheckBox activeCheck, TextBox hexField, Button generateBtn) CreateBoolHexPanel(int hexWidth)
    {
        var panel = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, hexWidth));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var activeCheck = new CheckBox { Text = UiStrings.Get("multiplayer.active").Replace("&", "&&"), AutoSize = true };
        activeCheck.CheckedChanged += (s, e) => RaiseDataModified();

        var hexField = new TextBox { Dock = DockStyle.Fill };
        hexField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; hexField.Parent?.Focus(); }
        };
        hexField.TextChanged += (s, e) => RaiseDataModified();

        var generateBtn = new Button
        {
            Text = "Generate",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(70, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
        };
        generateBtn.Click += (s, e) =>
        {
            byte[] bytes = new byte[8];
            _rng.NextBytes(bytes);
            hexField.Text = "0x" + BitConverter.ToString(bytes).Replace("-", "");
        };

        panel.Controls.Add(activeCheck, 0, 0);
        panel.Controls.Add(hexField, 1, 0);
        panel.Controls.Add(generateBtn, 2, 0);

        return (panel, activeCheck, hexField, generateBtn);
    }

    private void OnConfirmChanged()
    {
        _sectionsLayout.Enabled = _confirmCheck.Checked;
        if (!_confirmCheck.Checked)
            return;
        foreach (Control c in _sectionsLayout.Controls)
            c.Enabled = true;
    }

    private void SetupLayout()
    {
        SuspendLayout();

        _scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

        _mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(10, 0, 10, 10)
        };
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int mainRow = 0;

        // -- Warning --
        _experimentalWarningLabel = new Label
        {
            Text = UiStrings.Get("multiplayer.experimental_warning"),
            ForeColor = Color.DarkOrange,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 8)
        };
        _mainLayout.Controls.Add(_experimentalWarningLabel, 0, mainRow);
        _mainLayout.SetColumnSpan(_experimentalWarningLabel, 2);
        mainRow++;

        // -- Enforced warning --
        _enforcedWarningLabel = new Label
        {
            Text = UiStrings.Get("multiplayer.enforced_warning"),
            ForeColor = Color.DarkOrange,
            Font = new Font(Font.FontFamily, 9, FontStyle.Italic),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        };
        _mainLayout.Controls.Add(_enforcedWarningLabel, 0, mainRow);
        _mainLayout.SetColumnSpan(_enforcedWarningLabel, 2);
        mainRow++;

        // -- Confirm --
        _confirmCheck = new CheckBox
        {
            Text = UiStrings.Get("multiplayer.confirm_understanding").Replace("&", "&&"),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 9, FontStyle.Regular),
            Padding = new Padding(0, 0, 0, 8)
        };
        _confirmCheck.CheckedChanged += (s, e) => OnConfirmChanged();
        _mainLayout.Controls.Add(_confirmCheck, 0, mainRow);
        _mainLayout.SetColumnSpan(_confirmCheck, 2);
        mainRow++;

        // -- Sections (conditionally enabled) --
        _sectionsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Enabled = false,
            Padding = new Padding(0, 4, 0, 0)
        };
        _sectionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _sectionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _mainLayout.Controls.Add(_sectionsLayout, 0, mainRow);
        _mainLayout.SetColumnSpan(_sectionsLayout, 2);
        mainRow++;

        int row = 0;

        // ====== TEAM CONFIGURATION ======
        _teamConfigHeader = AddHeading(_sectionsLayout, row++, UiStrings.Get("multiplayer.section_team_configuration"));

        _cachedTeamCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = ComboWidth };
        foreach (var t in CommunityTeams) _cachedTeamCombo.Items.Add(t);
        _cachedTeamCombo.SelectedIndexChanged += (s, e) => RaiseDataModified();
        _cachedTeamLabel = AddRow(_sectionsLayout, UiStrings.Get("multiplayer.cached_player_community_team"), _cachedTeamCombo, row++);

        _useTeamShipSeedsCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.use_team_ship_seeds"));
        _useTeamShipPalettesCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.use_team_ship_palettes"));
        _useCommunityTeamPalettesCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.use_community_team_palettes"));

        // Team Ship Seeds
        var ts1 = CreateBoolHexPanel(SeedFieldWidth);
        _teamSeed1ActiveCheck = ts1.activeCheck; _teamSeed1Field = ts1.hexField; _generateTeamSeed1Btn = ts1.generateBtn;
        _teamSeed1Label = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.team_ship_seed_1"), ts1.panel, row++);

        var ts2 = CreateBoolHexPanel(SeedFieldWidth);
        _teamSeed2ActiveCheck = ts2.activeCheck; _teamSeed2Field = ts2.hexField; _generateTeamSeed2Btn = ts2.generateBtn;
        _teamSeed2Label = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.team_ship_seed_2"), ts2.panel, row++);

        var ts3 = CreateBoolHexPanel(SeedFieldWidth);
        _teamSeed3ActiveCheck = ts3.activeCheck; _teamSeed3Field = ts3.hexField; _generateTeamSeed3Btn = ts3.generateBtn;
        _teamSeed3Label = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.team_ship_seed_3"), ts3.panel, row++);

        // ====== PURCHASE & TRANSFER RESTRICTIONS ======
        _purchaseHeader = AddHeading(_sectionsLayout, row++, UiStrings.Get("multiplayer.section_purchase_restrictions"));

        _neverAllowShipPurchasesCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.never_allow_ship_purchases"));
        _allowOnlyCorvetteCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.allow_only_corvette_ship_purchases"));

        var bsp = CreateBoolHexPanel(SeedFieldWidth);
        _blockShipPurchasesActiveCheck = bsp.activeCheck; _blockShipPurchasesHexField = bsp.hexField; _generateBlockShipPurchasesHexBtn = bsp.generateBtn;
        _blockShipPurchasesLabel = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.block_ship_purchases_until_milestone"), bsp.panel, row++);

        var bsr = CreateBoolHexPanel(SeedFieldWidth);
        _blockShipRepairActiveCheck = bsr.activeCheck; _blockShipRepairHexField = bsr.hexField; _generateBlockShipRepairHexBtn = bsr.generateBtn;
        _blockShipRepairLabel = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.block_ship_repair_until_milestone"), bsr.panel, row++);

        _neverAllowCorvetteCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.never_allow_corvette_purchases"));
        _allowCorvetteTransferCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.allow_save_context_corvette_transfer"));
        _allowShipTransferCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.allow_save_context_ship_transfer"));
        _allowMultitoolTransferCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.allow_save_context_multitool_transfer"));
        _onlyCorvettesSpawnCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.only_corvettes_spawn_when_player_teleports"));
        _onlyCorvetteLauncherCheck = AddCheckBoxRow(_sectionsLayout, row++, UiStrings.Get("multiplayer.only_corvette_launcher_can_be_repaired"));

        // ====== PLAYER SETUP ======
        _playerSetupHeader = AddHeading(_sectionsLayout, row++, UiStrings.Get("multiplayer.section_player_setup"));

        _forceRaceCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = ComboWidth };
        foreach (var r in AlienRaces) _forceRaceCombo.Items.Add(r);
        _forceRaceCombo.SelectedIndexChanged += (s, e) => RaiseDataModified();
        _forceRaceLabel = AddRow(_sectionsLayout, UiStrings.Get("multiplayer.force_player_race"), _forceRaceCombo, row++);

        var ws = CreateBoolHexPanel(SeedFieldWidth);
        _weaponSeedActiveCheck = ws.activeCheck; _weaponSeedField = ws.hexField; _generateWeaponSeedBtn = ws.generateBtn;
        _weaponSeedLabel = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.weapon_seed"), ws.panel, row++);

        var ss = CreateBoolHexPanel(SeedFieldWidth);
        _shipSeedActiveCheck = ss.activeCheck; _shipSeedField = ss.hexField; _generateShipSeedBtn = ss.generateBtn;
        _shipSeedLabel = AddCenteredRow(_sectionsLayout, UiStrings.Get("multiplayer.ship_seed"), ss.panel, row++);

        _persistentPoiField = new TextBox { Width = 250 };
        _persistentPoiField.TextChanged += (s, e) => RaiseDataModified();
        _persistentPoiLabel = AddRow(_sectionsLayout, UiStrings.Get("multiplayer.persistent_poi"), _persistentPoiField, row++);

        _introSequencePoiField = new TextBox { Width = 250 };
        _introSequencePoiField.TextChanged += (s, e) => RaiseDataModified();
        _introSequencePoiLabel = AddRow(_sectionsLayout, UiStrings.Get("multiplayer.intro_sequence_poi"), _introSequencePoiField, row++);

        _startQuizIdField = new TextBox { Width = 250 };
        _startQuizIdField.TextChanged += (s, e) => RaiseDataModified();
        _startQuizIdLabel = AddRow(_sectionsLayout, UiStrings.Get("multiplayer.start_with_intro_quiz_id"), _startQuizIdField, row++);

        _sectionsLayout.RowCount = row;
        for (int i = 0; i < row; i++)
            _sectionsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _mainLayout.RowCount = mainRow;
        for (int i = 0; i < mainRow; i++)
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _scrollPanel.Controls.Add(_mainLayout);
        Controls.Add(_scrollPanel);

        ResumeLayout(false);
        PerformLayout();
    }

    // Scrollable container
    private Panel _scrollPanel = null!;
    private TableLayoutPanel _mainLayout = null!;

    // Confirm
    private CheckBox _confirmCheck = null!;
    private TableLayoutPanel _sectionsLayout = null!;

    // Warning
    private Label _experimentalWarningLabel = null!;
    private Label _enforcedWarningLabel = null!;

    // Team Configuration
    private Label _teamConfigHeader = null!;
    private Label _cachedTeamLabel = null!;
    private ComboBox _cachedTeamCombo = null!;
    private CheckBox _useTeamShipSeedsCheck = null!;
    private CheckBox _useTeamShipPalettesCheck = null!;
    private CheckBox _useCommunityTeamPalettesCheck = null!;

    private Label _teamSeed1Label = null!;
    private CheckBox _teamSeed1ActiveCheck = null!;
    private TextBox _teamSeed1Field = null!;
    private Button _generateTeamSeed1Btn = null!;

    private Label _teamSeed2Label = null!;
    private CheckBox _teamSeed2ActiveCheck = null!;
    private TextBox _teamSeed2Field = null!;
    private Button _generateTeamSeed2Btn = null!;

    private Label _teamSeed3Label = null!;
    private CheckBox _teamSeed3ActiveCheck = null!;
    private TextBox _teamSeed3Field = null!;
    private Button _generateTeamSeed3Btn = null!;

    // Purchase & Transfer Restrictions
    private Label _purchaseHeader = null!;
    private CheckBox _neverAllowShipPurchasesCheck = null!;
    private CheckBox _allowOnlyCorvetteCheck = null!;
    private Label _blockShipPurchasesLabel = null!;
    private CheckBox _blockShipPurchasesActiveCheck = null!;
    private TextBox _blockShipPurchasesHexField = null!;
    private Button _generateBlockShipPurchasesHexBtn = null!;
    private Label _blockShipRepairLabel = null!;
    private CheckBox _blockShipRepairActiveCheck = null!;
    private TextBox _blockShipRepairHexField = null!;
    private Button _generateBlockShipRepairHexBtn = null!;
    private CheckBox _neverAllowCorvetteCheck = null!;
    private CheckBox _allowCorvetteTransferCheck = null!;
    private CheckBox _allowShipTransferCheck = null!;
    private CheckBox _allowMultitoolTransferCheck = null!;
    private CheckBox _onlyCorvettesSpawnCheck = null!;
    private CheckBox _onlyCorvetteLauncherCheck = null!;

    // Player Setup
    private Label _playerSetupHeader = null!;
    private Label _forceRaceLabel = null!;
    private ComboBox _forceRaceCombo = null!;
    private Label _weaponSeedLabel = null!;
    private CheckBox _weaponSeedActiveCheck = null!;
    private TextBox _weaponSeedField = null!;
    private Button _generateWeaponSeedBtn = null!;
    private Label _shipSeedLabel = null!;
    private CheckBox _shipSeedActiveCheck = null!;
    private TextBox _shipSeedField = null!;
    private Button _generateShipSeedBtn = null!;
    private Label _persistentPoiLabel = null!;
    private TextBox _persistentPoiField = null!;
    private Label _introSequencePoiLabel = null!;
    private TextBox _introSequencePoiField = null!;
    private Label _startQuizIdLabel = null!;
    private TextBox _startQuizIdField = null!;
}
