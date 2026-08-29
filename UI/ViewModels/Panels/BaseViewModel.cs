using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.UI.ViewModels.Controls;
using System.Globalization;

namespace NMSE.UI.ViewModels.Panels;

public partial class BaseViewModel : PanelViewModelBase
{
    private JsonObject? _playerState;
    private GameItemDatabase? _database;
    private IconManager? _iconManager;

    [ObservableProperty] private int _selectedTabIndex;

    [ObservableProperty] private ObservableCollection<BaseInfoViewModel> _bases = new();
    [ObservableProperty] private BaseInfoViewModel? _selectedBase;
    [ObservableProperty] private string _baseName = "";
    [ObservableProperty] private string _baseItemCount = "";
    [ObservableProperty] private bool _hasBaseSelection;

    [ObservableProperty] private ObservableCollection<NpcWorkerViewModel> _npcWorkers = new();
    [ObservableProperty] private NpcWorkerViewModel? _selectedNpc;
    [ObservableProperty] private string _npcSeed = "";
    [ObservableProperty] private string _npcRace = "";

    [ObservableProperty] private ObservableCollection<InventoryGridViewModel> _chestGrids = new();

    [ObservableProperty] private ObservableCollection<StorageTabViewModel> _storageTabs = new();

    /// <summary>Objects that make up the selected base, with their raw JSON details.</summary>
    [ObservableProperty] private ObservableCollection<BaseObjectViewModel> _baseObjects = new();
    [ObservableProperty] private BaseObjectViewModel? _selectedObject;
    [ObservableProperty] private string _objectFilter = "";
    [ObservableProperty] private string _objectDetails = "";

    /// <summary>Chest currently shown, used by the rename box.</summary>
    [ObservableProperty] private int _selectedChestIndex;
    [ObservableProperty] private string _chestName = "";

    partial void OnSelectedBaseChanged(BaseInfoViewModel? value)
    {
        HasBaseSelection = value != null;
        if (value == null) return;
        BaseName = value.Data?.GetString("Name") ?? "";
        int objectCount = 0;
        try
        {
            var objects = value.Data?.GetArray("Objects");
            if (objects != null) objectCount = objects.Length;
        }
        catch { }
        BaseItemCount = objectCount.ToString();
    }

    public override IEnumerable<Controls.InventoryGridViewModel> Grids =>
        ChestGrids.Concat(StorageTabs.Select(s => s.Grid));

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _iconManager = iconManager;

        Bases.Clear();
        NpcWorkers.Clear();
        ChestGrids.Clear();
        StorageTabs.Clear();

        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;
            _playerState = playerState;

            LoadBases(playerState);
            LoadNpcWorkers(playerState);
            LoadChests(playerState);
            LoadStorage(playerState);
        }
        catch { }
    }

    private void LoadBases(JsonObject playerState)
    {
        var bases = playerState.GetArray("PersistentPlayerBases");
        if (bases == null) return;

        for (int i = 0; i < bases.Length; i++)
        {
            try
            {
                var baseObj = bases.GetObject(i);
                string? baseType = null;
                try { baseType = baseObj.GetString("BaseType.PersistentBaseTypes") ?? baseObj.GetString("BaseType"); }
                catch { try { baseType = baseObj.GetString("BaseType"); } catch { } }

                int baseVersion = 0;
                try { baseVersion = baseObj.GetInt("BaseVersion"); } catch { }

                if ("HomePlanetBase".Equals(baseType, StringComparison.OrdinalIgnoreCase) && baseVersion >= 3)
                {
                    string name = baseObj.GetString("Name") ?? $"Base {i + 1}";
                    int objectCount = 0;
                    try
                    {
                        var objects = baseObj.GetArray("Objects");
                        if (objects != null) objectCount = objects.Length;
                    }
                    catch { }

                    Bases.Add(new BaseInfoViewModel
                    {
                        DisplayName = name,
                        Data = baseObj,
                        DataIndex = i,
                        ObjectCount = objectCount
                    });
                }
            }
            catch { }
        }

        if (Bases.Count > 0)
            SelectedBase = Bases[0];
    }

    private void LoadNpcWorkers(JsonObject playerState)
    {
        var npcWorkers = playerState.GetArray("NPCWorkers");
        if (npcWorkers == null) return;

        // The five roles are positional; anything beyond them falls back to a number.
        string[] workerKeys =
        {
            "base.worker_armorer", "base.worker_farmer", "base.worker_overseer",
            "base.worker_technician", "base.worker_scientist",
        };

        for (int i = 0; i < npcWorkers.Length && i < 5; i++)
        {
            try
            {
                var npc = npcWorkers.GetObject(i);
                bool hired = false;
                try { hired = npc.GetBool("HiredWorker"); } catch { }
                if (hired)
                {
                    NpcWorkers.Add(new NpcWorkerViewModel
                    {
                        Name = i < workerKeys.Length
                            ? UiStrings.Get(workerKeys[i])
                            : UiStrings.Format("base.worker_n", i.ToString(CultureInfo.CurrentCulture)),
                        Data = npc,
                        Index = i
                    });
                }
            }
            catch { }
        }

        if (NpcWorkers.Count > 0)
            SelectedNpc = NpcWorkers[0];
    }

    private void LoadChests(JsonObject playerState)
    {
        for (int i = 0; i < 10; i++)
        {
            string key = $"Chest{i + 1}Inventory";
            var inv = playerState.GetObject(key);

            var grid = new InventoryGridViewModel();
            grid.SetIsCargoInventory(true);
            grid.SetInventoryOwnerType("Chest");
            // A chest can be renamed in game; the tab shows that name when set.
            string chestName = BaseLogic.GetChestName(inv);
            grid.SetInventoryGroup(BaseLogic.FormatChestTabTitle(
                UiStrings.Format("base.chest_tab", (i + 1).ToString(CultureInfo.CurrentCulture)), chestName));
            grid.SetSuperchargeDisabled(true);
            if (_database != null) grid.SetDatabase(_database);
            grid.SetIconManager(_iconManager);
            grid.LoadInventory(inv);

            ChestGrids.Add(grid);
        }
    }

    private void LoadStorage(JsonObject playerState)
    {
        (string LabelKey, string Key, string WarningKey)[] storageKeys =
        {
            ("base.storage_ingredient",        "CookingIngredientsInventory", ""),
            ("base.storage_corvette_parts",    "CorvetteStorageInventory",    ""),
            ("base.storage_salvage_capsule",   "ChestMagicInventory",         ""),
            ("base.storage_rocket",            "RocketLockerInventory",       ""),
            ("base.storage_fishing_platform",  "FishPlatformInventory",       ""),
            ("base.storage_fish_bait",         "FishBaitBoxInventory",        ""),
            ("base.storage_food_unit",         "FoodUnitInventory",           ""),
            // Present in the save but unused by the game, so it carries a warning.
            ("base.storage_freighter_refund",  "ChestMagic2Inventory",
                "base.storage_freighter_refund_warning"),
        };

        foreach (var (labelKey, key, warningKey) in storageKeys)
        {
            string label = UiStrings.Get(labelKey);
            var inv = playerState.GetObject(key);
            var grid = new InventoryGridViewModel();
            grid.SetIsCargoInventory(true);
            grid.SetInventoryOwnerType("Storage");
            grid.SetInventoryGroup(label);
            grid.SetSuperchargeDisabled(true);
            if (_database != null) grid.SetDatabase(_database);
            grid.SetIconManager(_iconManager);
            grid.LoadInventory(inv);

            StorageTabs.Add(new StorageTabViewModel
            {
                Label = label,
                Warning = warningKey.Length > 0 ? UiStrings.Get(warningKey) : "",
                Grid = grid
            });
        }
    }

    public Func<List<string>, Task<int>>? ShowObjectPickerFunc { get; set; }

    [RelayCommand]
    private async Task MoveBaseComputer()
    {
        if (SelectedBase?.Data == null) return;
        try
        {
            var objects = SelectedBase.Data.GetArray("Objects");
            if (objects == null || objects.Length == 0)
            {
                if (Dialogs is not null)
                    await Dialogs.ShowMessageAsync(UiStrings.Get("base.move_basecomp_title"),
                        UiStrings.Get("base.move_basecomp_no_objects"));
                return;
            }

            var candidates = new List<(string id, JsonObject data, int index)>();
            for (int i = 0; i < objects.Length; i++)
            {
                try
                {
                    var obj = objects.GetObject(i);
                    string objectId = obj.GetString("ObjectID") ?? "";
                    if (!string.IsNullOrEmpty(objectId) && objectId != "^BASE_FLAG")
                        candidates.Add((objectId, obj, i));
                }
                catch { }
            }

            if (candidates.Count == 0 || ShowObjectPickerFunc == null) return;

            var displayNames = candidates.Select(c => c.id).ToList();
            int selectedIdx = await ShowObjectPickerFunc(displayNames);
            if (selectedIdx < 0 || selectedIdx >= candidates.Count) return;

            var target = candidates[selectedIdx];

            JsonObject? baseFlag = null;
            for (int i = 0; i < objects.Length; i++)
            {
                try
                {
                    var obj = objects.GetObject(i);
                    if (obj.GetString("ObjectID") == "^BASE_FLAG")
                    {
                        baseFlag = obj;
                        break;
                    }
                }
                catch { }
            }

            if (baseFlag == null)
            {
                if (Dialogs is not null)
                    await Dialogs.ShowMessageAsync(UiStrings.Get("base.move_basecomp_title"),
                        UiStrings.Get("base.move_basecomp_not_found"), Services.DialogIcon.Warning);
                return;
            }

            // Swapping the two objects' positions is what moves the computer, since the
            // base is anchored to wherever the BASE_FLAG object sits.
            BaseLogic.SwapPositions(baseFlag, target.data);
            RefreshObjects();

            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("base.move_basecomp_success_title"),
                    UiStrings.Get("base.move_basecomp_success"));
        }
        catch (Exception ex)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                    UiStrings.Format("base.move_basecomp_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task ExportBase()
    {
        if (SelectedBase?.Data == null || SaveFilePickerFunc == null) return;

        var cfg = ExportConfig.Instance;
        var vars = new Dictionary<string, string>
        {
            ["base_name"] = string.IsNullOrWhiteSpace(BaseName)
                ? UiStrings.Get("base.fallback_base_name") : BaseName,
        };

        string? path = await SaveFilePickerFunc(UiStrings.Get("base.export_title"),
            cfg.BaseExt.TrimStart('.'),
            ExportConfig.BuildFileName(cfg.BaseTemplate, cfg.BaseExt, vars));
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            SelectedBase.Data.ExportToFile(path);
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("base.export_title"),
                    UiStrings.Format("base.export_success", Path.GetFileName(path)));
        }
        catch (Exception ex)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                    UiStrings.Format("base.export_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task ImportBase()
    {
        if (SelectedBase?.Data == null || OpenFilePickerFunc == null) return;

        // Importing replaces the base's objects wholesale, so it asks first.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("base.confirm_import_title"),
                UiStrings.Get("base.import_confirm"), Services.DialogIcon.Warning))
            return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("base.import_title"),
            ExportConfig.Instance.BaseExt);
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var imported = JsonObject.ImportFromFile(path);
            if (imported.Contains("Objects"))
                SelectedBase.Data.Set("Objects", imported.Get("Objects"));
            if (imported.Contains("BaseVersion"))
                SelectedBase.Data.Set("BaseVersion", imported.Get("BaseVersion"));
            if (imported.Contains("Name"))
            {
                SelectedBase.Data.Set("Name", imported.Get("Name"));
                BaseName = imported.GetString("Name") ?? BaseName;
                SelectedBase.DisplayName = BaseName;
            }

            int objectCount = 0;
            try
            {
                var objects = SelectedBase.Data.GetArray("Objects");
                if (objects != null) objectCount = objects.Length;
            }
            catch { }
            BaseItemCount = objectCount.ToString(CultureInfo.InvariantCulture);
            RefreshObjects();

            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("base.import_title"),
                    UiStrings.Format("base.import_success", Path.GetFileName(path)));
        }
        catch (Exception ex)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                    UiStrings.Format("base.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    [RelayCommand]
    private void SaveBaseName()
    {
        if (SelectedBase?.Data == null) return;
        SelectedBase.Data.Set("Name", BaseName);
        SelectedBase.DisplayName = BaseName;
    }

    [RelayCommand]
    private void GenerateNpcSeed()
    {
        byte[] bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        NpcSeed = "0x" + BitConverter.ToString(bytes).Replace("-", "");
    }

    // ================================ Base ordering ================================

    private JsonArray? Bases_ => _playerState?.GetArray("PersistentPlayerBases");

    /// <summary>
    /// Rebuilds the base list after the underlying array changed, restoring the selection
    /// by array index rather than by list position, which reordering invalidates.
    /// </summary>
    private void RefreshBases(int selectDataIndex)
    {
        if (_playerState is null) return;

        Bases.Clear();
        LoadBases(_playerState);

        SelectedBase = Bases.FirstOrDefault(b => b.DataIndex == selectDataIndex) ?? Bases.FirstOrDefault();
        RefreshObjects();
    }

    /// <summary>
    /// Moves the selected base within PersistentPlayerBases. The order decides which
    /// base the game treats as primary, so it is meaningful rather than cosmetic.
    /// </summary>
    private void MoveSelectedBase(Func<int, int> destination)
    {
        var bases = Bases_;
        if (bases is null || SelectedBase is null) return;

        int from = SelectedBase.DataIndex;
        int to = Math.Clamp(destination(from), 0, bases.Length - 1);
        if (to == from) return;

        // Walk one step at a time: SwapPlayerBases exchanges a pair, so a longer move is
        // a sequence of adjacent swaps that keeps every other base's relative order.
        int step = to > from ? 1 : -1;
        for (int i = from; i != to; i += step)
            BaseLogic.SwapPlayerBases(bases, i, i + step);

        RefreshBases(selectDataIndex: to);
    }

    [RelayCommand] private void MoveBaseUp() => MoveSelectedBase(i => i - 1);
    [RelayCommand] private void MoveBaseDown() => MoveSelectedBase(i => i + 1);
    [RelayCommand] private void MoveBaseToTop() => MoveSelectedBase(_ => 0);
    [RelayCommand] private void MoveBaseToBottom() => MoveSelectedBase(_ => (Bases_?.Length ?? 1) - 1);

    [RelayCommand] private void SortBasesAscending() => SortBases(ascending: true);
    [RelayCommand] private void SortBasesDescending() => SortBases(ascending: false);

    /// <summary>Reorders the bases by name, by repeatedly swapping into place.</summary>
    private void SortBases(bool ascending)
    {
        var bases = Bases_;
        if (bases is null || Bases.Count < 2) return;

        var order = Bases.Select(b => (b.DataIndex, b.DisplayName)).ToList();
        order.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName,
            StringComparison.CurrentCultureIgnoreCase) * (ascending ? 1 : -1));

        // Selection sort against the live array so each step is a legal swap.
        var current = Enumerable.Range(0, bases.Length).ToList();
        for (int target = 0; target < order.Count; target++)
        {
            int want = order[target].DataIndex;
            int at = current.IndexOf(want);
            if (at == target) continue;
            BaseLogic.SwapPlayerBases(bases, target, at);
            (current[target], current[at]) = (current[at], current[target]);
        }

        RefreshBases(selectDataIndex: 0);
    }

    // ================================= Terrain =====================================

    /// <summary>
    /// Terrain edits are stored per save and grow without bound; clearing them is the
    /// supported way to shrink a save and to undo terrain damage around a base.
    /// </summary>
    [RelayCommand]
    private Task ClearTerrainAsync() => RunTerrainClearAsync(
        "base.clear_terrain_title", "base.clear_terrain_confirm",
        "base.clear_terrain_success", "base.clear_terrain_none", "base.clear_terrain_failed",
        ps => SelectedBase?.Data is { } data ? BaseLogic.ClearTerrainEdits(ps, data) : 0);

    [RelayCommand]
    private Task ClearAllTerrainExceptBasesAsync() => RunTerrainClearAsync(
        "base.clear_all_terrain_except_bases_title", "base.clear_all_terrain_except_bases_confirm",
        "base.clear_all_terrain_except_bases_success", "base.clear_all_terrain_except_bases_none",
        "base.clear_all_terrain_except_bases_failed",
        BaseLogic.ClearAllTerrainEditsExceptBases);

    [RelayCommand]
    private Task ClearAllTerrainAsync() => RunTerrainClearAsync(
        "base.clear_all_terrain_title", "base.clear_all_terrain_confirm",
        "base.clear_all_terrain_success", "base.clear_all_terrain_none", "base.clear_all_terrain_failed",
        BaseLogic.ClearAllTerrainEdits);

    private async Task RunTerrainClearAsync(string titleKey, string confirmKey,
        string successKey, string noneKey, string failedKey, Func<JsonObject, int> clear)
    {
        if (Dialogs is null || _playerState is null) return;

        string title = UiStrings.Get(titleKey);
        if (!await Dialogs.ConfirmAsync(title, UiStrings.Get(confirmKey), Services.DialogIcon.Warning))
            return;

        try
        {
            int removed = clear(_playerState);
            await Dialogs.ShowMessageAsync(title, removed > 0
                ? UiStrings.Format(successKey, removed.ToString("N0", CultureInfo.CurrentCulture))
                : UiStrings.Get(noneKey));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format(failedKey, ex.Message), Services.DialogIcon.Error);
        }
    }

    // ================================== Deletion ===================================

    [RelayCommand]
    private async Task DeleteBaseAsync()
    {
        var bases = Bases_;
        if (Dialogs is null || bases is null || SelectedBase is null) return;

        bool isFreighter = string.Equals(
            SelectedBase.Data?.GetString("BaseType.PersistentBaseTypes")
                ?? SelectedBase.Data?.GetString("BaseType"),
            "FreighterBase", StringComparison.OrdinalIgnoreCase);

        // A freighter base cannot be rebuilt in game, so it warns separately.
        string message = isFreighter
            ? UiStrings.Get("base.delete_freighter_base_confirm")
            : UiStrings.Format("base.delete_base_confirm", SelectedBase.DisplayName);

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("base.delete_base_title"), message,
                Services.DialogIcon.Warning))
            return;

        try
        {
            bases.RemoveAt(SelectedBase.DataIndex);
            RefreshBases(selectDataIndex: 0);
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("base.delete_base_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    // ============================== NPC worker summon ==============================

    /// <summary>
    /// Points a base worker at the selected base by copying its address and position,
    /// which is how the game decides where the NPC stands.
    /// </summary>
    [RelayCommand]
    private async Task SummonWorkerAsync()
    {
        if (Dialogs is null || SelectedNpc is null || SelectedBase?.Data is null) return;

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("base.summon_worker_title"),
                UiStrings.Format("base.summon_worker_confirm", SelectedNpc.Name, SelectedBase.DisplayName)))
            return;

        try
        {
            var workers = _playerState?.GetArray("NPCWorkers");
            if (workers is null || SelectedNpc.Index >= workers.Length) return;

            var worker = workers.GetObject(SelectedNpc.Index);
            var baseData = SelectedBase.Data;

            if (baseData.Get("GalacticAddress") is { } address)
                worker.Set("BaseUA", address);

            // BaseOffset carries a trailing 1.0 that Position does not.
            if (baseData.Get("Position") is JsonArray position)
            {
                var offset = new JsonArray();
                offset.Add(position.Get(0));
                offset.Add(position.Get(1));
                offset.Add(position.Get(2));
                offset.Add(1.0);
                worker.Set("BaseOffset", offset);
            }

            bool isFreighter = string.Equals(
                baseData.GetString("BaseType.PersistentBaseTypes") ?? baseData.GetString("BaseType"),
                "FreighterBase", StringComparison.OrdinalIgnoreCase);
            worker.Set("FreighterBase", isFreighter);

            await Dialogs.ShowMessageAsync(UiStrings.Get("base.summon_complete_title"),
                UiStrings.Format("base.summon_complete_msg", SelectedNpc.Name, SelectedBase.DisplayName));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("base.summon_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    // =============================== Objects listing ===============================

    partial void OnObjectFilterChanged(string value) => RefreshObjects();
    partial void OnSelectedObjectChanged(BaseObjectViewModel? value) => ObjectDetails = value?.Details ?? "";

    /// <summary>Lists the selected base's building objects, filtered by the search box.</summary>
    private void RefreshObjects()
    {
        var list = new ObservableCollection<BaseObjectViewModel>();
        var objects = SelectedBase?.Data?.GetArray("Objects");

        if (objects is not null)
        {
            string filter = ObjectFilter?.Trim() ?? "";
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects.GetObject(i);
                if (obj is null) continue;

                string id = ShipCustomisationIo.Strip(obj.GetString("ObjectID"));
                string display = StarshipDatabase.GetDisplayName(id);
                string label = $"[{i}] {(string.IsNullOrEmpty(display) ? id : display)}";

                if (filter.Length > 0 &&
                    !label.Contains(filter, StringComparison.OrdinalIgnoreCase) &&
                    !id.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                list.Add(new BaseObjectViewModel { Label = label, Data = obj, Index = i });
            }
        }

        BaseObjects = list;
        SelectedObject = null;
    }

    // ================================ Chest naming =================================

    partial void OnSelectedChestIndexChanged(int value) => ChestName = ReadChestName(value);

    private string ReadChestName(int index)
    {
        if (_playerState is null || index < 0 || index >= BaseLogic.ChestInventoryKeys.Length) return "";
        return BaseLogic.GetChestName(_playerState.GetObject(BaseLogic.ChestInventoryKeys[index]));
    }

    /// <summary>Renames the visible chest, which the game shows on the container itself.</summary>
    [RelayCommand]
    private void RenameChest() => WriteChestName(ChestName);

    [RelayCommand]
    private void ClearChestName()
    {
        ChestName = "";
        WriteChestName(null);
    }

    private void WriteChestName(string? name)
    {
        if (_playerState is null) return;
        int index = SelectedChestIndex;
        if (index < 0 || index >= BaseLogic.ChestInventoryKeys.Length) return;

        var inventory = _playerState.GetObject(BaseLogic.ChestInventoryKeys[index]);
        BaseLogic.SetChestName(inventory, name);

        // The tab title carries the name, so it has to be rebuilt.
        if (index < ChestGrids.Count)
        {
            ChestGrids[index].SetInventoryGroup(BaseLogic.FormatChestTabTitle(
                UiStrings.Format("base.chest_tab", (index + 1).ToString(CultureInfo.CurrentCulture)),
                BaseLogic.GetChestName(inventory)));
        }
    }

    public override void SaveData(JsonObject saveData)
    {
        if (SelectedBase?.Data != null && !string.IsNullOrEmpty(BaseName))
            SelectedBase.Data.Set("Name", BaseName);
    }
}

public partial class BaseInfoViewModel : ObservableObject
{
    [ObservableProperty] private string _displayName = "";
    public JsonObject? Data { get; set; }
    public int DataIndex { get; set; }
    public int ObjectCount { get; set; }
    public override string ToString() => DisplayName;
}

public partial class NpcWorkerViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    public JsonObject? Data { get; set; }
    public int Index { get; set; }
    public override string ToString() => Name;
}

public partial class StorageTabViewModel : ObservableObject
{
    [ObservableProperty] private string _label = "";

    /// <summary>Shown above the grid when the inventory needs one; empty otherwise.</summary>
    [ObservableProperty] private string _warning = "";
    [ObservableProperty] private InventoryGridViewModel _grid = new();
}

/// <summary>One building object inside a base, with its raw JSON shown on selection.</summary>
public partial class BaseObjectViewModel : ObservableObject
{
    [ObservableProperty] private string _label = "";

    public JsonObject? Data { get; set; }
    public int Index { get; set; }

    /// <summary>
    /// The object's fields, labelled. The raw JSON is still reachable through the raw
    /// editor; this pane names the ones that mean something.
    /// </summary>
    public string Details
    {
        get
        {
            if (Data is null) return "";

            var sb = new System.Text.StringBuilder();
            void Row(string key, string? value)
            {
                if (!string.IsNullOrEmpty(value))
                    sb.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get(key)} {value}");
            }
            string? Str(string name) { try { return Data.GetString(name); } catch { return null; } }
            string? Num(string name)
            {
                try { return Data.GetInt(name).ToString(CultureInfo.CurrentCulture); } catch { return null; }
            }
            string? Arr(string name)
            {
                try
                {
                    var a = Data.GetArray(name);
                    if (a is null) return null;
                    var parts = new List<string>();
                    for (int i = 0; i < a.Length; i++)
                        parts.Add(a.GetDouble(i).ToString("F3", CultureInfo.InvariantCulture));
                    return string.Join(", ", parts);
                }
                catch { return null; }
            }

            Row("base.obj_detail_object_id", Str("ObjectID"));
            Row("base.obj_detail_timestamp", Num("Timestamp"));
            Row("base.obj_detail_position", Arr("Position"));
            Row("base.obj_detail_up", Arr("Up"));
            Row("base.obj_forward", Arr("At"));
            Row("base.obj_detail_user_data", Num("UserData"));
            Row("base.obj_auto_power", Str("AutoPower"));
            Row("base.obj_region_seed", Str("RegionSeed"));

            return sb.ToString();
        }
    }

    public override string ToString() => Label;
}
