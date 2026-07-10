using NMSE.Core;
using NMSE.UI.Util;

namespace NMSE.UI.Panels;

partial class ByteBeatPanel
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
        this._mainLayout = new System.Windows.Forms.TableLayoutPanel();
        this._leftLayout = new System.Windows.Forms.TableLayoutPanel();
        this._titleLabel = new System.Windows.Forms.Label();
        this._songList = new System.Windows.Forms.ListBox();
        this._btnPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._exportBtn = new System.Windows.Forms.Button();
        this._importBtn = new System.Windows.Forms.Button();
        this._deleteBtn = new System.Windows.Forms.Button();
        this._gotoJsonBtn = new System.Windows.Forms.Button();
        this._infoLabel = new System.Windows.Forms.Label();
        this._detailPanel = new System.Windows.Forms.Panel();
        this._detailLayout = new System.Windows.Forms.TableLayoutPanel();
        this._nameField = new System.Windows.Forms.TextBox();
        this._authorUsernameField = new System.Windows.Forms.TextBox();
        this._authorOnlineIdField = new System.Windows.Forms.TextBox();
        this._authorPlatformField = new System.Windows.Forms.TextBox();
        this._dataField0 = new System.Windows.Forms.TextBox();
        this._dataField1 = new System.Windows.Forms.TextBox();
        this._dataField2 = new System.Windows.Forms.TextBox();
        this._dataField3 = new System.Windows.Forms.TextBox();
        this._dataField4 = new System.Windows.Forms.TextBox();
        this._dataField5 = new System.Windows.Forms.TextBox();
        this._dataField6 = new System.Windows.Forms.TextBox();
        this._dataField7 = new System.Windows.Forms.TextBox();
        this._shuffleField = new System.Windows.Forms.CheckBox();
        this._autoplayOnFootField = new System.Windows.Forms.CheckBox();
        this._autoplayInShipField = new System.Windows.Forms.CheckBox();
        this._autoplayInVehicleField = new System.Windows.Forms.CheckBox();
        this._mainLayout.SuspendLayout();
        this._leftLayout.SuspendLayout();
        this._btnPanel.SuspendLayout();
        this._detailPanel.SuspendLayout();
        this._detailLayout.SuspendLayout();
        this.SuspendLayout();
        //
        // _mainLayout
        //
        this._mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainLayout.ColumnCount = 2;
        this._mainLayout.RowCount = 2;
        this._mainLayout.Padding = new System.Windows.Forms.Padding(10);
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
        this._mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._mainLayout.Controls.Add(this._leftLayout, 0, 1);
        this._mainLayout.Controls.Add(this._detailPanel, 1, 1);
        //
        // _leftLayout
        //
        this._leftLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        this._leftLayout.ColumnCount = 1;
        this._leftLayout.RowCount = 3;
        this._leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        //
        // Header strip
        //
        this._byteBeatHeaderStrip = new System.Windows.Forms.TableLayoutPanel();
        this._byteBeatHeaderStrip.AutoSize = true;
        this._byteBeatHeaderStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this._byteBeatHeaderStrip.ColumnCount = 3;
        this._byteBeatHeaderStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._byteBeatHeaderStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this._byteBeatHeaderStrip.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._byteBeatHeaderStrip.Controls.Add(this._titleLabel, 0, 0);
        this._byteBeatHeaderStrip.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this._byteBeatHeaderStrip.Margin = new System.Windows.Forms.Padding(0);
        this._byteBeatHeaderStrip.RowCount = 1;
        this._byteBeatHeaderStrip.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this._byteBeatGotoPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._byteBeatGotoPanel.AutoSize = true;
        this._byteBeatGotoPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._byteBeatGotoPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this._byteBeatGotoPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
		this._byteBeatGotoPanel.WrapContents = false;
        this._byteBeatHeaderStrip.Controls.Add(this._byteBeatGotoPanel, 2, 0);
        this._leftLayout.Controls.Add(this._songList, 0, 1);
        this._leftLayout.Controls.Add(this._btnPanel, 0, 2);
        //
        // _titleLabel
        //
        this._titleLabel.Text = "ByteBeats";
        FontManager.ApplyHeadingFont(_titleLabel, 14);
        this._titleLabel.AutoSize = true;
        this._titleLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
        this._titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        //
        // _songList
        //
        this._songList.Dock = System.Windows.Forms.DockStyle.Fill;
        this._songList.SelectedIndexChanged += OnSongSelected;
        //
        // _btnPanel
        //
        this._btnPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._btnPanel.AutoSize = true;
        this._btnPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this._btnPanel.Controls.Add(this._exportBtn);
        this._btnPanel.Controls.Add(this._importBtn);
        this._btnPanel.Controls.Add(this._deleteBtn);
        this._btnPanel.Controls.Add(this._infoLabel);
        //
        // _exportBtn
        //
        this._exportBtn.Text = "Export";
        this._exportBtn.AutoSize = true;
        this._exportBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._exportBtn.MinimumSize = new System.Drawing.Size(75, 0);
        this._exportBtn.Click += OnExport;
        //
        // _importBtn
        //
        this._importBtn.Text = "Import";
        this._importBtn.AutoSize = true;
        this._importBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._importBtn.MinimumSize = new System.Drawing.Size(75, 0);
        this._importBtn.Click += OnImport;
        //
        // _deleteBtn
        //
        this._deleteBtn.Text = "Delete";
        this._deleteBtn.AutoSize = true;
        this._deleteBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this._deleteBtn.MinimumSize = new System.Drawing.Size(75, 0);
        this._deleteBtn.Click += OnDeleteSong;
        //
        // _gotoJsonBtn
        //
		this._gotoJsonBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this._gotoJsonBtn.FlatAppearance.BorderSize = 1;
		this._gotoJsonBtn.FlatAppearance.BorderColor = ThemeManager.Effective == AppTheme.Dark ? Color.FromArgb(100, 100, 100) : SystemColors.ControlDark;
        this._gotoJsonBtn.Font = new System.Drawing.Font("Segoe UI Emoji", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this._gotoJsonBtn.Size = new System.Drawing.Size(28, 24);
        this._gotoJsonBtn.Text = "\U0001F4D1";
        this._gotoJsonBtn.Margin = new System.Windows.Forms.Padding(1, 3, 1, 1);
        this._gotoJsonBtn.Cursor = System.Windows.Forms.Cursors.Hand;
        this._gotoJsonBtn.Click += OnGoToJsonClicked;
        this._byteBeatGotoPanel.Controls.Add(this._gotoJsonBtn);
        //
        // _infoLabel
        //
        this._infoLabel.Text = "No songs loaded.";
        this._infoLabel.AutoSize = true;
        this._infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this._infoLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
        //
        // _detailPanel
        //
        this._detailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._detailPanel.AutoScroll = true;
        this._detailPanel.Visible = false;
        this._detailPanel.Controls.Add(this._detailLayout);
        //
        // _detailLayout
        //
        this._detailLayout.Dock = System.Windows.Forms.DockStyle.Top;
        this._detailLayout.ColumnCount = 2;
        this._detailLayout.AutoSize = true;
        this._detailLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        this._detailLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        //
        // _nameField
        //
        this._nameField.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _authorUsernameField
        //
        this._authorUsernameField.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _authorOnlineIdField
        //
        this._authorOnlineIdField.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _authorPlatformField
        //
        this._authorPlatformField.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField0
        //
        this._dataField0.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField1
        //
        this._dataField1.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField2
        //
        this._dataField2.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField3
        //
        this._dataField3.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField4
        //
        this._dataField4.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField5
        //
        this._dataField5.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField6
        //
        this._dataField6.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _dataField7
        //
        this._dataField7.Dock = System.Windows.Forms.DockStyle.Fill;
        //
        // _shuffleField
        //
        this._shuffleField.Text = "Shuffle";
        this._shuffleField.AutoSize = true;
        //
        // _autoplayOnFootField
        //
        this._autoplayOnFootField.Text = "Autoplay On Foot";
        this._autoplayOnFootField.AutoSize = true;
        //
        // _autoplayInShipField
        //
        this._autoplayInShipField.Text = "Autoplay In Ship";
        this._autoplayInShipField.AutoSize = true;
        //
        // _autoplayInVehicleField
        //
        this._autoplayInVehicleField.Text = "Autoplay In Vehicle";
        this._autoplayInVehicleField.AutoSize = true;
        //
        // ByteBeatPanel
        //
        this.DoubleBuffered = true;
        this._mainLayout.Controls.Add(this._byteBeatHeaderStrip, 0, 0);
        this._mainLayout.SetColumnSpan(this._byteBeatHeaderStrip, 2);
        this.Controls.Add(this._mainLayout);
        this._mainLayout.ResumeLayout(false);
        this._leftLayout.ResumeLayout(false);
        this._byteBeatGotoPanel.ResumeLayout(false);
        this._byteBeatGotoPanel.PerformLayout();
        this._btnPanel.ResumeLayout(false);
        this._detailPanel.ResumeLayout(false);
        this._detailLayout.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private void SetupLayout()
    {
        _dataFields = new TextBox[]
        {
            _dataField0, _dataField1, _dataField2, _dataField3,
            _dataField4, _dataField5, _dataField6, _dataField7
        };

        int row = 0;
        _dataLabels = new Label[8];

        _sectionDetailsLabel = AddSectionHeader(_detailLayout, "Song Details", row++);
        _nameField.Leave += (s, e) => RaiseDataModified();
        _nameField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _nameField.Parent?.Focus(); }
        };
        _authorUsernameField.Leave += (s, e) => RaiseDataModified();
        _authorUsernameField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _authorUsernameField.Parent?.Focus(); }
        };
        _authorOnlineIdField.Leave += (s, e) => RaiseDataModified();
        _authorOnlineIdField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _authorOnlineIdField.Parent?.Focus(); }
        };
        _authorPlatformField.Leave += (s, e) => RaiseDataModified();
        _authorPlatformField.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _authorPlatformField.Parent?.Focus(); }
        };
        _nameLabel = AddRow(_detailLayout, "Name:", _nameField, row++);
        _authorUsernameLabel = AddRow(_detailLayout, "Author Username:", _authorUsernameField, row++);
        _authorOnlineIdLabel = AddRow(_detailLayout, "Author Online ID:", _authorOnlineIdField, row++);
        _authorPlatformLabel = AddRow(_detailLayout, "Author Platform:", _authorPlatformField, row++);

        _sectionDataLabel = AddSectionHeader(_detailLayout, "Data (8 channels)", row++);
        for (int i = 0; i < 8; i++)
        {
            int idx = i;
            _dataFields[i].Leave += (s, e) => RaiseDataModified();
            _dataFields[i].KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _dataFields[idx].Parent?.Focus(); }
            };
            _dataLabels[i] = AddRow(_detailLayout, $"Data [{i}]:", _dataFields[i], row++);
        }

        _sectionLibraryLabel = AddSectionHeader(_detailLayout, "Library Settings", row++);
        _shuffleField.CheckedChanged += (s, e) => RaiseDataModified();
        _autoplayOnFootField.CheckedChanged += (s, e) => RaiseDataModified();
        _autoplayInShipField.CheckedChanged += (s, e) => RaiseDataModified();
        _autoplayInVehicleField.CheckedChanged += (s, e) => RaiseDataModified();
        _detailLayout.Controls.Add(_shuffleField, 1, row++);
        _detailLayout.Controls.Add(_autoplayOnFootField, 1, row++);
        _detailLayout.Controls.Add(_autoplayInShipField, 1, row++);
        _detailLayout.Controls.Add(_autoplayInVehicleField, 1, row++);

        _detailLayout.RowCount = row;
        for (int i = 0; i < row; i++)
            _detailLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    private System.Windows.Forms.TableLayoutPanel _mainLayout;
    private System.Windows.Forms.TableLayoutPanel _leftLayout;
    private System.Windows.Forms.TableLayoutPanel _byteBeatHeaderStrip;
    private System.Windows.Forms.FlowLayoutPanel _byteBeatGotoPanel;
    private System.Windows.Forms.Label _titleLabel;
    private System.Windows.Forms.ListBox _songList;
    private System.Windows.Forms.FlowLayoutPanel _btnPanel;
    private System.Windows.Forms.Button _exportBtn;
    private System.Windows.Forms.Button _importBtn;
    private System.Windows.Forms.Button _deleteBtn;
    private System.Windows.Forms.Button _gotoJsonBtn;
    private System.Windows.Forms.Label _infoLabel;
    private System.Windows.Forms.Panel _detailPanel;
    private System.Windows.Forms.TableLayoutPanel _detailLayout;
    private System.Windows.Forms.TextBox _nameField;
    private System.Windows.Forms.TextBox _authorUsernameField;
    private System.Windows.Forms.TextBox _authorOnlineIdField;
    private System.Windows.Forms.TextBox _authorPlatformField;
    private System.Windows.Forms.TextBox _dataField0;
    private System.Windows.Forms.TextBox _dataField1;
    private System.Windows.Forms.TextBox _dataField2;
    private System.Windows.Forms.TextBox _dataField3;
    private System.Windows.Forms.TextBox _dataField4;
    private System.Windows.Forms.TextBox _dataField5;
    private System.Windows.Forms.TextBox _dataField6;
    private System.Windows.Forms.TextBox _dataField7;
    private System.Windows.Forms.TextBox[] _dataFields;
    private System.Windows.Forms.CheckBox _shuffleField;
    private System.Windows.Forms.CheckBox _autoplayOnFootField;
    private System.Windows.Forms.CheckBox _autoplayInShipField;
    private System.Windows.Forms.CheckBox _autoplayInVehicleField;
    private System.Windows.Forms.Label _sectionDetailsLabel;
    private System.Windows.Forms.Label _nameLabel;
    private System.Windows.Forms.Label _authorUsernameLabel;
    private System.Windows.Forms.Label _authorOnlineIdLabel;
    private System.Windows.Forms.Label _authorPlatformLabel;
    private System.Windows.Forms.Label _sectionDataLabel;
    private System.Windows.Forms.Label[] _dataLabels;
    private System.Windows.Forms.Label _sectionLibraryLabel;
}
