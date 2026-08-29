using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.UI.ViewModels.Controls;

namespace NMSE.UI.ViewModels.Panels;

public partial class MultitoolViewModel : PanelViewModelBase
{
    private JsonArray? _multitools;
    private JsonObject? _playerState;
    private GameItemDatabase? _database;
    private IconManager? _iconManager;
    private int _activeToolIndex;

    [ObservableProperty] private ObservableCollection<string> _toolList = new();
    [ObservableProperty] private int _selectedToolIndex = -1;
    [ObservableProperty] private string _primaryToolLabel = "";

    private readonly List<int> _toolDataIndices = new();

    [ObservableProperty] private string _toolName = "";
    [ObservableProperty] private string _toolSeed = "";
    [ObservableProperty] private ObservableCollection<string> _toolTypes = new();
    [ObservableProperty] private int _selectedTypeIndex = -1;
    [ObservableProperty] private ObservableCollection<string> _toolClasses = new(MultitoolLogic.ToolClasses);
    [ObservableProperty] private int _selectedClassIndex = -1;

    [ObservableProperty] private double _damage;
    [ObservableProperty] private double _mining;
    [ObservableProperty] private double _scan;

    [ObservableProperty] private InventoryGridViewModel _storeGrid = new();

    /// <summary>Tool size (small / large), which the game stores as an IsLarge flag.</summary>
    [ObservableProperty] private ObservableCollection<string> _toolSizes = new(MultitoolLogic.GetToolSizeItems());
    [ObservableProperty] private int _selectedSizeIndex = -1;

    // --- Archive --------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<string> _archivedTools = new();
    [ObservableProperty] private int _selectedArchiveIndex = -1;

    private readonly List<int> _archiveDataIndices = new();

    private MultitoolLogic.ToolTypeItem[] _typeItems = [];

    public MultitoolViewModel()
    {
        StoreGrid.SetIsTechInventory(true);
        StoreGrid.SetInventoryOwnerType("Weapon");
        StoreGrid.SetInventoryGroup("Weapon");
    }

    public override IEnumerable<Controls.InventoryGridViewModel> Grids => [StoreGrid];

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _iconManager = iconManager;
        StoreGrid.SetDatabase(database);
        StoreGrid.SetIconManager(iconManager);

        try
        {
            _playerState = saveData.GetObject("PlayerStateData");
            if (_playerState == null) return;

            _multitools = _playerState.GetArray("Multitools");

            RefreshTypeItems();

            if (_multitools != null && _multitools.Length > 0)
            {
                _activeToolIndex = 0;
                try { _activeToolIndex = _playerState.GetInt("ActiveMultioolIndex"); } catch { }
                PrimaryToolLabel = MultitoolLogic.GetPrimaryToolName(_multitools, _activeToolIndex);
                RefreshArchive();

                RefreshToolList();

                if (ToolList.Count > 0)
                {
                    int selectIdx = 0;
                    for (int i = 0; i < _toolDataIndices.Count; i++)
                    {
                        if (_toolDataIndices[i] == _activeToolIndex)
                        {
                            selectIdx = i;
                            break;
                        }
                    }
                    SelectedToolIndex = Math.Clamp(selectIdx, 0, ToolList.Count - 1);
                }
            }
        }
        catch { }
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToToolJsonAsync() => GoToJsonAsync("PlayerStateData", "Multitools", (_toolDataIndices.Count > SelectedToolIndex && SelectedToolIndex >= 0 ? _toolDataIndices[SelectedToolIndex] : 0).ToString(CultureInfo.InvariantCulture));

    public override void SaveData(JsonObject saveData)
    {
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            var multitools = playerState.GetArray("Multitools");
            if (multitools != null && SelectedToolIndex >= 0 && SelectedToolIndex < _toolDataIndices.Count)
            {
                int idx = _toolDataIndices[SelectedToolIndex];
                if (idx >= multitools.Length) return;

                try { playerState.Set("ActiveMultioolIndex", _activeToolIndex); } catch { }

                var tool = multitools.GetObject(idx);
                bool isPrimary = (idx == _activeToolIndex);

                var values = new MultitoolLogic.ToolSaveValues
                {
                    Name = ToolName,
                    ClassIndex = SelectedClassIndex,
                    TypeIndex = GetSelectedTypeDataIndex(),
                    Seed = ToolSeed,
                    Damage = Damage,
                    Mining = Mining,
                    Scan = Scan,
                    IsLargeIndex = SelectedSizeIndex,
                };

                MultitoolLogic.SaveToolData(tool, playerState, values, isPrimary);
            }
        }
        catch { }
    }

    partial void OnSelectedToolIndexChanged(int value)
    {
        if (value < 0 || value >= _toolDataIndices.Count) return;
        if (_multitools == null) return;

        try
        {
            int idx = _toolDataIndices[value];
            if (idx >= _multitools.Length) return;

            var tool = _multitools.GetObject(idx);
            var data = MultitoolLogic.LoadToolData(tool);

            ToolName = data.Name;
            SelectTypeByDataIndex(data.TypeIndex);
            SelectedClassIndex = data.ClassIndex;
            ToolSeed = data.Seed;
            Damage = data.Damage;
            Mining = data.Mining;
            Scan = data.Scan;
            // Index 0 is the large body, 1 the small one. A save with no IsLarge flag
            // leaves the selector empty rather than guessing, so saving does not write
            // a value the game never had.
            SelectedSizeIndex = data.IsLarge switch { true => 0, false => 1, null => -1 };

            StoreGrid.LoadInventory(data.Store);
        }
        catch { }
    }

    [RelayCommand]
    private void GenerateSeed()
    {
        ToolSeed = $"0x{Random.Shared.NextInt64():X16}";
    }

    [RelayCommand]
    private void MakePrimary()
    {
        if (_multitools == null || _playerState == null || SelectedToolIndex < 0 || SelectedToolIndex >= _toolDataIndices.Count) return;
        int idx = _toolDataIndices[SelectedToolIndex];
        if (idx >= _multitools.Length) return;

        _activeToolIndex = idx;
        try { _playerState.Set("ActiveMultioolIndex", _activeToolIndex); } catch { }
        PrimaryToolLabel = MultitoolLogic.GetPrimaryToolName(_multitools, _activeToolIndex);
    }

    // =================================== Archive ===================================

    private void RefreshArchive()
    {
        ArchivedTools = new ObservableCollection<string>();
        _archiveDataIndices.Clear();

        var archived = _playerState?.GetArray("ArchivedMultiTools");
        if (archived is null) return;

        foreach (var item in MultitoolLogic.BuildArchivedToolList(archived))
        {
            ArchivedTools.Add(item.DisplayName);
            _archiveDataIndices.Add(item.ArchiveIndex);
        }
    }

    /// <summary>Moves the selected tool into an empty archived slot.</summary>
    [RelayCommand]
    private async Task ArchiveToolAsync()
    {
        if (Dialogs is null || _multitools is null || SelectedToolIndex < 0) return;
        int idx = _toolDataIndices[SelectedToolIndex];

        if (idx == _activeToolIndex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("multitool.archive_primary_blocked"), Services.DialogIcon.Warning);
            return;
        }

        var archived = _playerState?.GetArray("ArchivedMultiTools");
        int slot = archived is null ? -1 : MultitoolLogic.FindEmptyArchivedToolSlot(archived);
        if (archived is null || slot < 0)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("multitool.archive_no_slots"), Services.DialogIcon.Warning);
            return;
        }

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("multitool.archive_move_title"),
                UiStrings.Get("multitool.archive_move_confirm")))
            return;

        MultitoolLogic.MoveToolToArchive(_multitools.GetObject(idx), archived.GetObject(slot));
        RefreshToolList();
        RefreshArchive();
    }

    /// <summary>Brings an archived tool back into a free owned slot.</summary>
    [RelayCommand]
    private async Task ImportFromArchiveAsync()
    {
        if (Dialogs is null || _multitools is null || SelectedArchiveIndex < 0) return;

        var archived = _playerState?.GetArray("ArchivedMultiTools");
        if (archived is null) return;

        int target = MultitoolLogic.FindEmptySlot(_multitools);
        if (target < 0)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("multitool.archive_no_list_slots"), Services.DialogIcon.Warning);
            return;
        }

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("multitool.archive_import_title"),
                UiStrings.Get("multitool.archive_import_confirm")))
            return;

        MultitoolLogic.ImportToolFromArchive(archived.GetObject(_archiveDataIndices[SelectedArchiveIndex]),
            _multitools.GetObject(target));

        RefreshToolList();
        RefreshArchive();
    }

    private void RefreshToolList()
    {
        ToolList.Clear();
        _toolDataIndices.Clear();
        if (_multitools == null) return;
        foreach (var item in MultitoolLogic.BuildToolList(_multitools))
        {
            ToolList.Add(item.DisplayName);
            _toolDataIndices.Add(item.DataIndex);
        }
    }

    private void RefreshTypeItems()
    {
        _typeItems = MultitoolLogic.GetToolTypeItems();
        ToolTypes.Clear();
        foreach (var item in _typeItems)
            ToolTypes.Add(item.DisplayName);
    }

    private int GetSelectedTypeDataIndex()
    {
        if (SelectedTypeIndex < 0 || SelectedTypeIndex >= _typeItems.Length) return -1;
        string internalName = _typeItems[SelectedTypeIndex].InternalName;
        return Array.FindIndex(MultitoolLogic.ToolTypes, t => t.Name.Equals(internalName, StringComparison.OrdinalIgnoreCase));
    }

    private void SelectTypeByDataIndex(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= MultitoolLogic.ToolTypes.Length) { SelectedTypeIndex = -1; return; }
        string targetName = MultitoolLogic.ToolTypes[typeIndex].Name;
        for (int i = 0; i < _typeItems.Length; i++)
        {
            if (_typeItems[i].InternalName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedTypeIndex = i;
                return;
            }
        }
        SelectedTypeIndex = -1;
    }

    /// <summary>File dialog funcs set by view code-behind.</summary>
    public Func<string, string, Task<string?>>? SaveFileFunc { get; set; }
    public Func<string, Task<string?>>? OpenFileFunc { get; set; }

    [RelayCommand]
    private async Task ExportTool()
    {
        if (_multitools == null || SelectedToolIndex < 0 || SaveFileFunc == null) return;
        int idx = _toolDataIndices[SelectedToolIndex];
        if (idx >= _multitools.Length) return;

        var tool = _multitools.GetObject(idx);
        if (tool == null) return;

        var cfg = ExportConfig.Instance;
        var vars = new Dictionary<string, string>
        {
            ["multitool_name"] = ToolName,
            ["type"] = ToolTypes.Count > 0 && SelectedTypeIndex >= 0 ? ToolTypes[SelectedTypeIndex] : "",
            ["class"] = ToolClasses.Count > 0 && SelectedClassIndex >= 0 ? ToolClasses[SelectedClassIndex] : ""
        };
        string fileName = ExportConfig.BuildFileName(cfg.MultitoolTemplate, cfg.MultitoolExt, vars);
        var path = await SaveFileFunc(fileName, cfg.MultitoolExt);
        if (path != null)
            tool.ExportToFile(path);
    }

    [RelayCommand]
    private async Task ImportTool()
    {
        if (_multitools == null || SelectedToolIndex < 0 || OpenFileFunc == null) return;
        int idx = _toolDataIndices[SelectedToolIndex];
        if (idx >= _multitools.Length) return;

        var path = await OpenFileFunc(ExportConfig.Instance.MultitoolExt);
        if (path == null) return;

        var imported = JsonObject.ImportFromFile(path);
        if (imported == null) return;

        _multitools.Set(idx, imported);
        OnSelectedToolIndexChanged(SelectedToolIndex);
    }

    [RelayCommand]
    private async Task DeleteToolAsync()
    {
        if (_multitools == null || SelectedToolIndex < 0) return;
        int idx = _toolDataIndices[SelectedToolIndex];
        if (idx >= _multitools.Length) return;

        if (MultitoolLogic.CountValidTools(_multitools) <= 1)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("multitool.delete_title"),
                    UiStrings.Get("multitool.cannot_delete_only"), Services.DialogIcon.Warning);
            return;
        }

        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("multitool.delete_title"),
                UiStrings.Get("multitool.delete_confirm"), Services.DialogIcon.Warning))
            return;

        // Clear the slot in place. Removing the element renumbers every tool after it,
        // which silently repoints PrimaryWeapon at the wrong one.
        MultitoolLogic.DeleteToolData(_multitools.GetObject(idx));

        RefreshToolList();
        if (ToolList.Count > 0)
            SelectedToolIndex = 0;
    }
}
