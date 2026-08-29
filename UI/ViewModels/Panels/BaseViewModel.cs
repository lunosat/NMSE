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

        string[] workerNames = { "Armorer", "Farmer", "Overseer", "Technician", "Scientist" };

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
                        Name = workerNames[i],
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
            grid.SetInventoryGroup($"Chest {i + 1}");
            grid.SetSuperchargeDisabled(true);
            if (_database != null) grid.SetDatabase(_database);
            grid.SetIconManager(_iconManager);
            grid.LoadInventory(inv);

            ChestGrids.Add(grid);
        }
    }

    private void LoadStorage(JsonObject playerState)
    {
        (string Label, string Key)[] storageKeys =
        {
            ("Ingredient Storage", "CookingIngredientsInventory"),
            ("Corvette Parts", "CorvetteStorageInventory"),
            ("Salvage Capsule", "ChestMagicInventory"),
            ("Rocket Locker", "RocketLockerInventory"),
            ("Fishing Platform", "FishPlatformInventory"),
            ("Fish Bait Box", "FishBaitBoxInventory"),
            ("Food Unit", "FoodUnitInventory"),
            ("Freighter Refund", "ChestMagic2Inventory"),
        };

        foreach (var (label, key) in storageKeys)
        {
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

            if (baseFlag == null) return;

            BaseLogic.SwapPositions(baseFlag, target.data);

            int objectCount = 0;
            try
            {
                var objs = SelectedBase.Data.GetArray("Objects");
                if (objs != null) objectCount = objs.Length;
            }
            catch { }
            BaseItemCount = objectCount.ToString(CultureInfo.InvariantCulture);
        }
        catch { }
    }

    [RelayCommand]
    private async Task ExportBase()
    {
        if (SelectedBase?.Data == null || SaveFilePickerFunc == null) return;
        var cfg = ExportConfig.Instance;
        string? path = await SaveFilePickerFunc("Backup Base", "json",
            "NMS Base Backup (*.json)|*.json|All Files (*.*)|*.*");
        if (string.IsNullOrEmpty(path)) return;
        try { SelectedBase.Data.ExportToFile(path); } catch { }
    }

    [RelayCommand]
    private async Task ImportBase()
    {
        if (SelectedBase?.Data == null || OpenFilePickerFunc == null) return;
        string? path = await OpenFilePickerFunc("Restore Base",
            "NMS Base Backup (*.json)|*.json|All Files (*.*)|*.*");
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
        }
        catch { }
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
    [ObservableProperty] private InventoryGridViewModel _grid = new();
}

/// <summary>One building object inside a base, with its raw JSON shown on selection.</summary>
public partial class BaseObjectViewModel : ObservableObject
{
    [ObservableProperty] private string _label = "";

    public JsonObject? Data { get; set; }
    public int Index { get; set; }

    /// <summary>Formatted JSON of the object, for the details pane.</summary>
    public string Details => Data?.ToFormattedString() ?? "";

    public override string ToString() => Label;
}
