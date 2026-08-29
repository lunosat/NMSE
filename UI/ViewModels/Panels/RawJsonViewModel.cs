using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>What a tree node stands for, which drives how it is coloured.</summary>
public enum JsonNodeKind { Object, Array, String, Number, Boolean, Null, Binary }

/// <summary>
/// Raw JSON editor: a lazily built tree over the save document, plus a text view
/// of the same data.
/// </summary>
public partial class RawJsonViewModel : PanelViewModelBase
{
    /// <summary>
    /// Depth built eagerly. A save has tens of thousands of nodes, so anything below
    /// this level is materialised only when its parent is expanded.
    /// </summary>
    private const int EagerDepth = 2;

    private JsonObject? _saveData;
    private JsonObject? _accountData;
    private string? _saveFilePath;

    /// <summary>Nodes in visit order, used to step through search matches.</summary>
    private readonly List<JsonTreeNodeViewModel> _searchMatches = new();
    private int _searchIndex = -1;

    [ObservableProperty] private bool _isTreeView = true;
    [ObservableProperty] private string _jsonText = "";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private bool _isShowingAccountData;
    [ObservableProperty] private JsonTreeNodeViewModel? _selectedNode;
    [ObservableProperty] private bool _hasAccountData;

    [ObservableProperty] private ObservableCollection<JsonTreeNodeViewModel> _treeNodes = new();

    /// <summary>Raised when a node should be scrolled into view and selected.</summary>
    public event Action<JsonTreeNodeViewModel>? NodeRevealRequested;

    /// <summary>Raised when the panel needs a value edited; the view supplies the dialog.</summary>
    public event Func<string, string, Task<string?>>? EditValueRequested;

    /// <summary>Raised when the panel needs a file path; the view supplies the picker.</summary>
    public event Func<bool, string, Task<string?>>? FilePathRequested;

    /// <summary>The document currently on screen: the save, or the account data.</summary>
    private JsonObject? Current => IsShowingAccountData ? _accountData : _saveData;

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _saveData = saveData;
        RefreshTree();
        StatusText = UiStrings.Format("raw_json.loaded_keys",
            saveData.Size().ToString("N0", CultureInfo.CurrentCulture));
    }

    /// <summary>Supplies the account data document so it can be edited alongside the save.</summary>
    public void SetAccountData(JsonObject? accountData)
    {
        _accountData = accountData;
        HasAccountData = accountData is not null;
    }

    /// <summary>Records the path of the loaded save, used as the export dialog's starting point.</summary>
    public void SetSaveFilePath(string? path) => _saveFilePath = path;

    partial void OnIsShowingAccountDataChanged(bool value) => RefreshTree();

    // ---------------------------------------------------------------- tree

    /// <summary>Rebuilds the tree (or the text) from the current document.</summary>
    public void RefreshTree()
    {
        var data = Current;
        if (data is null) return;

        _searchMatches.Clear();
        _searchIndex = -1;

        if (IsTreeView)
        {
            var root = new JsonTreeNodeViewModel("Root", data, null, null, JsonNodeKind.Object);
            root.Populate(EagerDepth, 0);
            root.IsExpanded = true;
            TreeNodes = new ObservableCollection<JsonTreeNodeViewModel> { root };
        }
        else
        {
            JsonText = RawJsonLogic.ToDisplayString(data);
        }
    }

    [RelayCommand]
    private void SwitchToTreeView()
    {
        if (IsTreeView) return;

        // Text edits are the source of truth while the text view is showing, so they
        // have to be parsed back into the document before the tree is rebuilt from it.
        if (!TryCommitTextEdits()) return;

        IsTreeView = true;
        RefreshTree();
    }

    [RelayCommand]
    private void SwitchToTextView()
    {
        if (!IsTreeView) return;
        IsTreeView = false;
        RefreshTree();
    }

    /// <summary>
    /// Parses the text view back into the live document. Returns false and reports the
    /// error when the text is not valid JSON, so the caller can leave the user in the
    /// text view rather than discarding their edits.
    /// </summary>
    private bool TryCommitTextEdits()
    {
        var data = Current;
        if (data is null || string.IsNullOrWhiteSpace(JsonText)) return true;

        try
        {
            var parsed = RawJsonLogic.ParseJson(JsonText);
            ReplaceContents(data, parsed);
            return true;
        }
        catch (JsonException ex)
        {
            StatusText = UiStrings.Format("raw_json.parse_error", ex.Message);
            return false;
        }
    }

    [RelayCommand]
    private void ExpandAll()
    {
        int count = 0;
        foreach (var node in TreeNodes) count += node.ExpandRecursive(maxNodes: 20000);
        StatusText = UiStrings.Format("raw_json.expanded_nodes",
            count.ToString("N0", CultureInfo.CurrentCulture));
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var node in TreeNodes) node.CollapseRecursive();
        foreach (var node in TreeNodes) node.IsExpanded = true; // keep Root open
    }

    // -------------------------------------------------------------- editing

    /// <summary>Whether the selected node holds a scalar that can be edited in place.</summary>
    public bool CanEditSelected => SelectedNode is { IsContainer: false, Parent: not null };

    partial void OnSelectedNodeChanged(JsonTreeNodeViewModel? value)
    {
        OnPropertyChanged(nameof(CanEditSelected));
        EditValueCommand.NotifyCanExecuteChanged();
        DeleteNodeCommand.NotifyCanExecuteChanged();
        CopyValueCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task EditValueAsync()
    {
        var node = SelectedNode;
        if (node is null || EditValueRequested is null) return;

        string current = RawJsonLogic.FormatValueForEdit(node.Value);
        string? input = await EditValueRequested(node.Key ?? "", current);
        if (input is null) return; // cancelled

        try
        {
            object? parsed = RawJsonLogic.ParseInputValue(input, node.Value);
            node.WriteBack(parsed);
            StatusText = UiStrings.Get("raw_json.value_modified");
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("raw_json.parse_error", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private void CopyValue()
    {
        var node = SelectedNode;
        if (node is null) return;
        ClipboardText = RawJsonLogic.SerializeValue(node.Value);
        StatusText = UiStrings.Get("raw_json.value_copied");
    }

    /// <summary>Set by the view after a clipboard write; kept here so the VM stays toolkit-free.</summary>
    [ObservableProperty] private string? _clipboardText;

    private bool CanDeleteSelected => SelectedNode is { Parent: not null };

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private void DeleteNode()
    {
        var node = SelectedNode;
        if (node?.Parent is null) return;

        node.RemoveFromParent();
        SelectedNode = null;
        StatusText = UiStrings.Get("raw_json.node_deleted");
    }

    // --------------------------------------------------------------- search

    [RelayCommand]
    private void Search() => StepSearch(forward: true);

    [RelayCommand]
    private void SearchBack() => StepSearch(forward: false);

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = "";
        _searchMatches.Clear();
        _searchIndex = -1;
        StatusText = "";
    }

    /// <summary>
    /// Moves to the next or previous node whose label contains the query, collecting
    /// matches on the first step and cycling afterwards.
    /// </summary>
    private void StepSearch(bool forward)
    {
        string query = SearchQuery?.Trim() ?? "";
        if (query.Length == 0) return;

        if (_searchMatches.Count == 0)
        {
            foreach (var root in TreeNodes)
                root.CollectMatches(query, _searchMatches, maxMatches: 5000);
            _searchIndex = -1;

            if (_searchMatches.Count == 0)
            {
                StatusText = UiStrings.Format("raw_json.no_matches", query);
                return;
            }
        }

        _searchIndex = forward
            ? (_searchIndex + 1) % _searchMatches.Count
            : (_searchIndex - 1 + _searchMatches.Count) % _searchMatches.Count;

        var match = _searchMatches[_searchIndex];
        match.ExpandAncestors();
        SelectedNode = match;
        NodeRevealRequested?.Invoke(match);

        StatusText = UiStrings.Format("raw_json.match_position",
            (_searchIndex + 1).ToString("N0", CultureInfo.CurrentCulture),
            _searchMatches.Count.ToString("N0", CultureInfo.CurrentCulture));
    }

    partial void OnSearchQueryChanged(string value)
    {
        // A new query invalidates the collected matches.
        _searchMatches.Clear();
        _searchIndex = -1;
    }

    /// <summary>
    /// Selects the node at a slash-separated path, expanding ancestors on the way.
    /// This is what the other panels' "go to JSON" action calls.
    /// </summary>
    public bool NavigateToPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || TreeNodes.Count == 0) return false;

        if (!IsTreeView)
        {
            IsTreeView = true;
            RefreshTree();
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var node = TreeNodes[0];

        foreach (string segment in segments)
        {
            node.Populate(1, 0);
            var next = node.Children.FirstOrDefault(c =>
                string.Equals(c.Key, segment, StringComparison.Ordinal));
            if (next is null) return false;
            node = next;
        }

        node.ExpandAncestors();
        SelectedNode = node;
        NodeRevealRequested?.Invoke(node);
        return true;
    }

    // -------------------------------------------------------- text commands

    [RelayCommand]
    private void FormatJson()
    {
        try
        {
            JsonText = RawJsonLogic.FormatJson(JsonText);
            StatusText = UiStrings.Get("raw_json.formatted");
        }
        catch (JsonException ex)
        {
            StatusText = UiStrings.Format("raw_json.parse_error", ex.Message);
        }
    }

    [RelayCommand]
    private void ValidateJson()
    {
        try
        {
            RawJsonLogic.ParseJson(JsonText);
            StatusText = UiStrings.Get("raw_json.valid_json");
        }
        catch (JsonException ex)
        {
            StatusText = UiStrings.Format("raw_json.invalid_json", ex.Message);
        }
    }

    // ------------------------------------------------------- import/export

    [RelayCommand]
    private async Task ExportAsync()
    {
        var data = Current;
        if (data is null || FilePathRequested is null) return;

        string suggested = Path.GetFileNameWithoutExtension(_saveFilePath ?? "save") + ".json";
        string? path = await FilePathRequested(true, suggested);
        if (path is null) return;

        try
        {
            data.ExportToFile(path);
            StatusText = UiStrings.Format("raw_json.exported", Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("raw_json.export_failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        var data = Current;
        if (data is null || FilePathRequested is null) return;

        string? path = await FilePathRequested(false, "");
        if (path is null) return;

        try
        {
            var imported = JsonObject.ImportFromFile(path);
            ReplaceContents(data, imported);
            RefreshTree();
            StatusText = UiStrings.Format("raw_json.imported", Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("raw_json.import_failed", ex.Message);
        }
    }

    /// <summary>
    /// Overwrites <paramref name="target"/> with the properties of <paramref name="source"/>,
    /// keeping the same instance.
    /// </summary>
    /// <remarks>
    /// The WinForms panel handed a freshly parsed object back to the form, which then
    /// re-synced every panel. Here the other panels hold the same JsonObject reference
    /// they were loaded with, so the contents are swapped in place instead - replacing
    /// the reference would leave them editing an orphaned document.
    /// </remarks>
    private static void ReplaceContents(JsonObject target, JsonObject source)
    {
        foreach (string name in target.Names())
            target.Remove(name);
        foreach (string name in source.Names())
            target.Add(name, source.Get(name));
    }

    public override void SaveData(JsonObject saveData)
    {
        // The tree edits the live document in place, so only the text view has
        // anything pending to commit.
        if (!IsTreeView) TryCommitTextEdits();
    }
}

/// <summary>
/// One node of the JSON tree. Children below <see cref="RawJsonViewModel"/>'s eager
/// depth are created on first expansion, so opening a save does not walk the whole
/// document.
/// </summary>
public partial class JsonTreeNodeViewModel : ObservableObject
{
    [ObservableProperty] private string _displayText = "";
    [ObservableProperty] private ObservableCollection<JsonTreeNodeViewModel> _children = new();

    private bool _isExpanded;
    private bool _needsPopulate;

    /// <summary>The key under an object parent, or the index under an array parent.</summary>
    public string? Key { get; private set; }

    /// <summary>The value this node stands for.</summary>
    public object? Value { get; private set; }

    /// <summary>The containing JsonObject or JsonArray, or null at the root.</summary>
    public object? Container { get; }

    public JsonTreeNodeViewModel? Parent { get; private set; }

    public JsonNodeKind Kind { get; private set; }

    public bool IsContainer => Kind is JsonNodeKind.Object or JsonNodeKind.Array;

    public JsonTreeNodeViewModel(string key, object? value, object? container,
        JsonTreeNodeViewModel? parent, JsonNodeKind kind)
    {
        Key = key;
        Value = value;
        Container = container;
        Parent = parent;
        Kind = kind;
        DisplayText = BuildLabel(key, value, kind);
    }

    /// <summary>
    /// Expanding a node materialises its children. Binding this to the TreeViewItem's
    /// IsExpanded is what makes the lazy build happen at the moment the user opens it.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value) && value && _needsPopulate)
                Populate(1, 0);
        }
    }

    // ------------------------------------------------------------- labelling

    private static string BuildLabel(string key, object? value, JsonNodeKind kind) => kind switch
    {
        JsonNodeKind.Object => $"{key}  {{...}}  ({((JsonObject)value!).Size()} properties)",
        JsonNodeKind.Array  => $"{key}  [...]  ({((JsonArray)value!).Length} items)",
        _                   => $"{key} : {FormatValue(value)}",
    };

    /// <summary>Formats a scalar the way the WinForms tree did.</summary>
    internal static string FormatValue(object? value) => value switch
    {
        null => "null",
        string s => $"\"{EscapeString(s)}\"",
        bool b => b ? "true" : "false",
        BinaryData bd => $"<binary:{bd.ToHexString()}>",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture) ?? "null",
        _ => value.ToString() ?? "null",
    };

    /// <summary>
    /// Escapes a string for display. Long values are truncated: a save holds base64
    /// blobs thousands of characters long, and rendering them whole stalls the tree.
    /// </summary>
    private static string EscapeString(string s)
    {
        if (s.Length > 200) s = s[..200] + "...";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }

    internal static JsonNodeKind KindOf(object? value) => value switch
    {
        JsonObject => JsonNodeKind.Object,
        JsonArray => JsonNodeKind.Array,
        string => JsonNodeKind.String,
        bool => JsonNodeKind.Boolean,
        null => JsonNodeKind.Null,
        BinaryData => JsonNodeKind.Binary,
        _ => JsonNodeKind.Number,
    };

    // ------------------------------------------------------------ populating

    /// <summary>
    /// Builds children down to <paramref name="maxDepth"/> levels. Deeper containers get
    /// a placeholder child so the expander arrow appears, and fill in when opened.
    /// </summary>
    public void Populate(int maxDepth, int currentDepth)
    {
        if (!IsContainer) return;
        if (!_needsPopulate && Children.Count > 0 && currentDepth == 0 && maxDepth <= 1) return;

        var built = new ObservableCollection<JsonTreeNodeViewModel>();

        if (Value is JsonObject obj)
        {
            foreach (string name in obj.Names())
                built.Add(CreateChild(name, obj.Get(name), obj, maxDepth, currentDepth));
        }
        else if (Value is JsonArray arr)
        {
            for (int i = 0; i < arr.Length; i++)
                built.Add(CreateChild(i.ToString(CultureInfo.InvariantCulture),
                    arr.Get(i), arr, maxDepth, currentDepth));
        }

        Children = built;
        _needsPopulate = false;
    }

    private JsonTreeNodeViewModel CreateChild(string key, object? value, object container,
        int maxDepth, int currentDepth)
    {
        var kind = KindOf(value);
        var node = new JsonTreeNodeViewModel(key, value, container, this, kind);

        if (node.IsContainer)
        {
            int count = value is JsonObject o ? o.Size() : ((JsonArray)value!).Length;
            if (currentDepth + 1 < maxDepth)
                node.Populate(maxDepth, currentDepth + 1);
            else if (count > 0)
                node._needsPopulate = true;   // filled in on expansion
        }
        return node;
    }

    /// <summary>Expands this node and its descendants, stopping after a node budget.</summary>
    public int ExpandRecursive(int maxNodes)
    {
        int count = 0;
        Walk(this);
        return count;

        void Walk(JsonTreeNodeViewModel n)
        {
            if (count >= maxNodes || !n.IsContainer) return;
            n.IsExpanded = true;
            count++;
            foreach (var c in n.Children) Walk(c);
        }
    }

    public void CollapseRecursive()
    {
        IsExpanded = false;
        foreach (var c in Children) c.CollapseRecursive();
    }

    /// <summary>Opens every ancestor so this node is reachable in the tree.</summary>
    public void ExpandAncestors()
    {
        for (var p = Parent; p is not null; p = p.Parent) p.IsExpanded = true;
    }

    /// <summary>
    /// Adds every descendant whose label contains <paramref name="query"/>. Containers
    /// are populated as they are visited, so this searches the whole document rather
    /// than only what is currently on screen.
    /// </summary>
    public void CollectMatches(string query, List<JsonTreeNodeViewModel> into, int maxMatches)
    {
        if (into.Count >= maxMatches) return;

        if (DisplayText.Contains(query, StringComparison.OrdinalIgnoreCase))
            into.Add(this);

        if (!IsContainer) return;
        if (_needsPopulate) Populate(1, 0);
        foreach (var c in Children) c.CollectMatches(query, into, maxMatches);
    }

    // --------------------------------------------------------------- mutation

    /// <summary>Writes a new scalar into the underlying container and relabels the node.</summary>
    public void WriteBack(object? newValue)
    {
        switch (Container)
        {
            case JsonObject obj when Key is not null:
                obj.Set(Key, newValue);
                break;
            case JsonArray arr when int.TryParse(Key, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int index):
                arr.Set(index, newValue);
                break;
            default:
                return;
        }

        Value = newValue;
        Kind = KindOf(newValue);
        DisplayText = BuildLabel(Key ?? "", newValue, Kind);
    }

    /// <summary>Removes this node from its container and from the parent's children.</summary>
    public void RemoveFromParent()
    {
        switch (Container)
        {
            case JsonObject obj when Key is not null:
                obj.Remove(Key);
                break;
            case JsonArray arr when int.TryParse(Key, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int index):
                arr.RemoveAt(index);
                break;
            default:
                return;
        }

        Parent?.Children.Remove(this);
        // Array indices shift after a removal, so the siblings' keys and labels
        // no longer match their positions.
        if (Container is JsonArray) Parent?.Populate(1, 0);
    }
}
