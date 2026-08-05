using System.Globalization;
using NMSE.Data;

namespace NMSE.UI;

/// <summary>
/// Dialog for selecting a backup ZIP file to restore from.
/// Lists backups newest first with creation time, size, and location.
/// </summary>
public sealed class BackupPickerDialog : Form
{
    /// <summary>Gets the selected backup ZIP path, or <c>null</c> if the dialog was cancelled.</summary>
    public string? SelectedZipPath { get; private set; }

    private readonly ListView _list;
    private readonly Button _okButton;

    /// <summary>
    /// Creates the picker dialog from the given backup ZIP paths.
    /// Paths should be ordered newest first; the first item is preselected.
    /// </summary>
    /// <param name="zipPaths">The backup ZIP paths to offer.</param>
    public BackupPickerDialog(IReadOnlyList<string> zipPaths)
    {
        Text = UiStrings.Get("dialog.restore_picker_title");
        Size = new Size(700, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var hint = new Label
        {
            Text = UiStrings.Get("dialog.restore_picker_hint"),
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10, 10, 10, 2)
        };

        _list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            BorderStyle = BorderStyle.FixedSingle
        };
        _list.Columns.Add(new ColumnHeader { Text = UiStrings.Get("dialog.restore_col_backup"), Width = 250 });
        _list.Columns.Add(new ColumnHeader { Text = UiStrings.Get("dialog.restore_col_created"), Width = 140 });
        _list.Columns.Add(new ColumnHeader { Text = UiStrings.Get("dialog.restore_col_size"), Width = 90 });
        _list.Columns.Add(new ColumnHeader { Text = UiStrings.Get("dialog.restore_col_location"), Width = 190 });
        _list.DoubleClick += (_, _) =>
        {
            if (_list.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        };

        foreach (string zipPath in zipPaths)
        {
            var item = new ListViewItem(Path.GetFileName(zipPath));
            item.SubItems.Add(GetCreationTimeDisplay(zipPath));
            item.SubItems.Add(GetSizeDisplay(zipPath));
            item.SubItems.Add(Path.GetDirectoryName(zipPath) ?? "");
            item.Tag = zipPath;
            _list.Items.Add(item);
        }

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8)
        };
        _okButton = new Button
        {
            Text = UiStrings.Get("common.ok"),
            DialogResult = DialogResult.OK,
            Width = 110,
            Enabled = false,
            Margin = new Padding(6, 0, 0, 0)
        };
        var cancelButton = new Button
        {
            Text = UiStrings.Get("common.cancel"),
            DialogResult = DialogResult.Cancel,
            Width = 110
        };
        _okButton.Click += (_, _) =>
        {
            if (_list.SelectedItems.Count > 0)
                SelectedZipPath = _list.SelectedItems[0].Tag as string;
        };
        buttonPanel.Controls.Add(_okButton);
        buttonPanel.Controls.Add(cancelButton);

        Controls.Add(_list);
        Controls.Add(hint);
        Controls.Add(buttonPanel);
        AcceptButton = _okButton;
        CancelButton = cancelButton;

        _list.SelectedIndexChanged += (_, _) => _okButton.Enabled = _list.SelectedItems.Count > 0;

        if (_list.Items.Count > 0)
            _list.Items[0].Selected = true;
    }

    /// <summary>
    /// Returns the local creation time of the given ZIP as a culture-formatted
    /// string, or an empty string if the file cannot be stat-ed.
    /// </summary>
    private static string GetCreationTimeDisplay(string zipPath)
    {
        try
        {
            return File.GetCreationTime(zipPath).ToString("g", CultureInfo.CurrentCulture);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Formats the file size of the given ZIP in a human-readable unit.
    /// </summary>
    private static string GetSizeDisplay(string zipPath)
    {
        try
        {
            long bytes = new FileInfo(zipPath).Length;
            if (bytes >= 1024L * 1024 * 1024)
                return $"{(bytes / (1024.0 * 1024 * 1024)):0.0} GB";
            if (bytes >= 1024L * 1024)
                return $"{(bytes / (1024.0 * 1024)):0.0} MB";
            if (bytes >= 1024L)
                return $"{(bytes / 1024.0):0.0} KB";
            return $"{bytes} B";
        }
        catch
        {
            return "";
        }
    }
}
