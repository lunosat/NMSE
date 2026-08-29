using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using NMSE.Data;

/// <summary>Dialog for selecting a game item from a filterable list.</summary>
public class ItemPickerDialog : Form
{
    public string? SelectedId { get; private set; }
    public List<string> SelectedIds { get; } = new();
    private readonly DataGridView _grid;
    private readonly Button _addButton;
    private readonly TextBox _manualIdBox;
    private readonly Button _addManualButton;
    private readonly TextBox _filterBox;
    private readonly Button _filterClearButton;
    private bool _adjustingSelection;

    public ItemPickerDialog(string title, List<(Image? icon, string name, string id, string category)> items)
    {
        Text = title;
        Size = new Size(760, 460);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
        };

        _grid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "Icon",
            HeaderText = UiStrings.Get("item_picker.col_icon"),
            FillWeight = 16,
            ImageLayout = DataGridViewImageCellLayout.Zoom
        });
        _grid.Columns.Add("Name", UiStrings.Get("item_picker.col_name"));
        _grid.Columns.Add("Category", UiStrings.Get("item_picker.col_category"));
        _grid.Columns.Add("ID", UiStrings.Get("item_picker.col_id"));
        _grid.RowTemplate.Height = 28;

        foreach (var (icon, name, id, category) in items)
        {
            if (icon == null || name == null || id == null || category == null)
                continue;
            _grid.Rows.Add(icon, name, category, id);
        }

        // Filter controls
        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4),
        };
        _filterBox = new TextBox { Width = 200, PlaceholderText = UiStrings.Get("item_picker.filter_placeholder") };
        _filterBox.TextChanged += (s, e) => ApplyFilter();
        _filterClearButton = new Button { Text = UiStrings.Get("item_picker.clear"), Width = 28, Height = 23 };
        _filterClearButton.Click += (s, e) => { _filterBox.Text = ""; };
        filterPanel.Controls.Add(_filterBox);
        filterPanel.Controls.Add(_filterClearButton);

        _addButton = new Button { Text = UiStrings.Get("item_picker.add"), Dock = DockStyle.Right, DialogResult = DialogResult.OK, Enabled = false };
        _addButton.Click += (s, e) =>
        {
            SelectedIds.Clear();
            foreach (DataGridViewRow row in _grid.SelectedRows)
            {
                if (!row.Visible) continue;
                var id = row.Cells["ID"].Value as string;
                if (!string.IsNullOrEmpty(id))
                    SelectedIds.Add(id!);
            }
            if (SelectedIds.Count > 0)
                SelectedId = SelectedIds[0];
        };

        _grid.SelectionChanged += OnGridSelectionChanged;

        // Manual entry controls with padding and vertical layout
        _manualIdBox = new TextBox
        {
            Width = 200,
            Margin = new Padding(12, 12, 12, 4),
            PlaceholderText = UiStrings.Get("item_picker.manual_placeholder")
        };
        _addManualButton = new Button
        {
            Text = UiStrings.Get("item_picker.add_manual"),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(12, 0, 12, 4)
        };
        _addManualButton.Click += (s, e) =>
        {
            var id = _manualIdBox.Text.Trim();
            if (!string.IsNullOrEmpty(id))
            {
                SelectedId = id;
                DialogResult = DialogResult.OK;
                Close();
            }
        };

        var warningLabel = new Label
        {
            Text = UiStrings.Get("item_picker.manual_warning"),
            ForeColor = Color.Red,
            AutoSize = true,
            Margin = new Padding(12, 0, 12, 8),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var manualPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 8)
        };
        manualPanel.Controls.Add(_manualIdBox);
        manualPanel.Controls.Add(_addManualButton);
        manualPanel.Controls.Add(warningLabel);

        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 36 };
        _addButton.Width = 120;
        _addButton.Location = new Point(buttonPanel.Width - _addButton.Width - 8, 4);
        buttonPanel.Controls.Add(_addButton);

        Controls.Add(_grid);
        Controls.Add(filterPanel);
        Controls.Add(manualPanel);
        Controls.Add(buttonPanel);
        AcceptButton = _addButton;
    }

    private void OnGridSelectionChanged(object? sender, EventArgs e)
    {
        if (_adjustingSelection) return;
        _adjustingSelection = true;
        try
        {
            var toDeselect = _grid.SelectedRows.Cast<DataGridViewRow>()
                .Where(r => !r.Visible).ToList();
            foreach (var row in toDeselect)
                row.Selected = false;
        }
        finally
        {
            _adjustingSelection = false;
        }
        _addButton.Enabled = _grid.SelectedRows.Cast<DataGridViewRow>().Any(r => r.Visible);
    }

    private void ApplyFilter()
    {
        var filter = _filterBox.Text.Trim();
        _adjustingSelection = true;
        try
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                bool visible;
                if (string.IsNullOrEmpty(filter))
                {
                    visible = true;
                }
                else
                {
                    string name = row.Cells["Name"].Value as string ?? "";
                    string category = row.Cells["Category"].Value as string ?? "";
                    string id = row.Cells["ID"].Value as string ?? "";
                    visible = name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                           || category.Contains(filter, StringComparison.OrdinalIgnoreCase)
                           || id.Contains(filter, StringComparison.OrdinalIgnoreCase);
                }
                if (!visible && row.Selected)
                    row.Selected = false;
                row.Visible = visible;
            }
        }
        finally
        {
            _adjustingSelection = false;
        }
    }
}