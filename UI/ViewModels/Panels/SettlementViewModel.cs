using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.Core.Utilities;

namespace NMSE.UI.ViewModels.Panels;

public partial class SettlementViewModel : PanelViewModelBase
{
    private JsonArray? _settlements;
    private readonly List<int> _filteredIndices = new();
    private GameItemDatabase? _database;

    [ObservableProperty] private ObservableCollection<string> _settlementNames = new();
    [ObservableProperty] private int _selectedSettlementIndex = -1;
    [ObservableProperty] private string _infoLabel = "";

    [ObservableProperty] private string _settlementName = "";
    [ObservableProperty] private string _seedValue = "";
    [ObservableProperty] private bool _hasSelection;

    [ObservableProperty] private int _population;
    [ObservableProperty] private int _happiness;
    [ObservableProperty] private int _productivity;
    [ObservableProperty] private int _upkeep;
    [ObservableProperty] private int _sentinels;
    [ObservableProperty] private int _debt;
    [ObservableProperty] private int _alert;
    [ObservableProperty] private int _bugAttack;

    [ObservableProperty] private int _decisionTypeIndex = -1;
    [ObservableProperty] private List<string> _decisionTypes = new(SettlementLogic.DecisionTypes);

    [ObservableProperty] private ObservableCollection<ProductionItemViewModel> _productionItems = new();

    // --- Identity and timestamps ---------------------------------------------
    [ObservableProperty] private ObservableCollection<string> _alienRaces = new(
        SettlementLogic.AlienRaces.Select(r =>
            SettlementLogic.AlienRaceLocKeys.TryGetValue(r, out var k) ? UiStrings.Get(k) : r));
    [ObservableProperty] private int _alienRaceIndex = -1;

    [ObservableProperty] private int _maxPopulation;
    [ObservableProperty] private string _missionSeed = "";
    [ObservableProperty] private string _lastDecisionTime = "";
    [ObservableProperty] private string _lastAlertTime = "";
    [ObservableProperty] private string _lastBugAttackTime = "";
    [ObservableProperty] private string _lastDebtTime = "";
    [ObservableProperty] private string _lastUpkeepTime = "";
    [ObservableProperty] private string _lastPopulationTime = "";
    [ObservableProperty] private string _miniMissionStartTime = "";

    // --- Perks ---------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<SettlementPerkViewModel> _perks = new();

    // --- Building states -----------------------------------------------------
    [ObservableProperty] private ObservableCollection<BuildingStateViewModel> _buildingStates = new();
    [ObservableProperty] private BuildingStateViewModel? _selectedBuildingState;

    partial void OnSelectedSettlementIndexChanged(int value)
    {
        if (value < 0 || value >= _filteredIndices.Count || _settlements == null)
        {
            HasSelection = false;
            return;
        }

        HasSelection = true;
        int dataIdx = _filteredIndices[value];
        if (dataIdx >= _settlements.Length) return;

        var settlement = _settlements.GetObject(dataIdx);
        var sdata = SettlementLogic.LoadSettlementData(settlement);

        SettlementName = sdata.Name;
        SeedValue = sdata.SeedValue;
        Population = sdata.Stats[0];
        Happiness = sdata.Stats[1];
        Productivity = sdata.Stats[2];
        Upkeep = sdata.Stats[3];
        Sentinels = sdata.Stats[4];
        Debt = sdata.Stats[5];
        Alert = sdata.Stats[6];
        BugAttack = sdata.Stats[7];
        DecisionTypeIndex = sdata.DecisionTypeIndex;

        MaxPopulation = sdata.Population;
        MissionSeed = sdata.MiniMissionSeed.ToString(CultureInfo.InvariantCulture);
        AlienRaceIndex = Array.FindIndex(SettlementLogic.AlienRaces,
            r => string.Equals(r, sdata.AlienRace, StringComparison.OrdinalIgnoreCase));

        // Timestamps are Unix seconds; showing them raw keeps them editable through the
        // raw editor without the panel silently reinterpreting a value.
        LastDecisionTime = sdata.LastDecisionTime?.ToString("u", CultureInfo.InvariantCulture) ?? "";
        LastAlertTime = sdata.LastAlertChangeTime.ToString(CultureInfo.InvariantCulture);
        LastBugAttackTime = sdata.LastBugAttackChangeTime.ToString(CultureInfo.InvariantCulture);
        LastDebtTime = sdata.LastDebtChangeTime.ToString(CultureInfo.InvariantCulture);
        LastUpkeepTime = sdata.LastUpkeepDebtCheckTime.ToString(CultureInfo.InvariantCulture);
        LastPopulationTime = sdata.LastPopulationChangeTime.ToString(CultureInfo.InvariantCulture);
        MiniMissionStartTime = sdata.MiniMissionStartTime.ToString(CultureInfo.InvariantCulture);

        LoadBuildingStates(sdata.BuildingStates);
        LoadPerks(settlement);
        LoadProductionState(settlement);
    }

    private void LoadProductionState(JsonObject settlement)
    {
        ProductionItems.Clear();
        var prodArr = settlement.GetArray("ProductionState");
        if (prodArr == null) return;

        for (int i = 0; i < prodArr.Length; i++)
        {
            try
            {
                var prodObj = prodArr.GetObject(i);
                string elementId = prodObj.GetString("ElementId") ?? prodObj.Get("ElementId")?.ToString() ?? "";
                string lookupId = elementId.StartsWith('^') ? elementId[1..] : elementId;
                var dbItem = string.IsNullOrEmpty(lookupId) ? null : _database?.GetItem(lookupId);
                string itemName = dbItem?.Name ?? lookupId;
                int amount = 0;
                try { amount = prodObj.GetInt("Amount"); } catch { }

                ProductionItems.Add(new ProductionItemViewModel
                {
                    ElementId = elementId,
                    ItemName = itemName,
                    Amount = amount
                });
            }
            catch { }
        }
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        SettlementNames.Clear();
        _filteredIndices.Clear();
        HasSelection = false;

        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            _settlements = playerState.GetArray("SettlementStatesV2");
            if (_settlements == null || _settlements.Length == 0)
            {
                InfoLabel = "No settlements found.";
                return;
            }

            var filtered = SettlementLogic.FilterSettlements(saveData, playerState, _settlements);
            foreach (int i in filtered)
            {
                try
                {
                    _filteredIndices.Add(i);
                    var settlement = _settlements.GetObject(i);
                    string name = settlement.GetString("Name") ?? $"Settlement {i + 1}";
                    SettlementNames.Add(name);
                }
                catch
                {
                    _filteredIndices.Add(i);
                    SettlementNames.Add($"Settlement {i + 1}");
                }
            }

            InfoLabel = $"Found {_filteredIndices.Count} settlement(s).";
            if (SettlementNames.Count > 0)
                SelectedSettlementIndex = 0;
        }
        catch { InfoLabel = "Failed to load settlements."; }
    }

    // ============================== Building states ================================

    /// <summary>
    /// Builds the per-building state rows. The game packs construction progress, tier
    /// progression and arrival flags into one integer per building, so each row decodes
    /// the fields it owns.
    /// </summary>
    private void LoadBuildingStates(int[] states)
    {
        var rows = new ObservableCollection<BuildingStateViewModel>();
        for (int i = 0; i < states.Length; i++)
            rows.Add(new BuildingStateViewModel(i, states[i]));

        BuildingStates = rows;
        SelectedBuildingState = rows.FirstOrDefault();
    }

    private int[] CollectBuildingStates() =>
        BuildingStates.Select(b => b.RawValue).ToArray();

    // ==================================== Perks ====================================

    private void LoadPerks(JsonObject settlement)
    {
        var rows = new ObservableCollection<SettlementPerkViewModel>();

        var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var array = settlement.GetArray("Perks");
        if (array is not null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                string id = (array.GetString(i) ?? "").TrimStart('^');
                if (id.Length > 0) owned.Add(id);
            }
        }

        foreach (var perk in SettlementDatabase.Perks)
            rows.Add(new SettlementPerkViewModel(perk, owned.Contains(perk.Id)));

        Perks = rows;
    }

    private void SavePerks(JsonObject settlement)
    {
        var array = settlement.GetArray("Perks");
        if (array is null) return;

        for (int i = array.Length - 1; i >= 0; i--) array.RemoveAt(i);
        foreach (var perk in Perks.Where(p => p.IsOwned))
            array.Add("^" + perk.Id);
    }

    // =================================== Deletion ==================================

    [RelayCommand]
    private void GenerateSeed()
    {
        byte[] bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        SeedValue = "0x" + BitConverter.ToString(bytes).Replace("-", "");
    }

    [RelayCommand]
    private async Task DeleteSettlementAsync()
    {
        if (_settlements == null || SelectedSettlementIndex < 0 || _filteredIndices.Count == 0) return;

        // Removing a settlement cannot be undone from the editor.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("settlement.delete_title"),
                UiStrings.Get("settlement.delete_confirm"), Services.DialogIcon.Warning))
            return;

        int selIdx = SelectedSettlementIndex;
        int dataIdx = _filteredIndices[selIdx];
        if (dataIdx >= _settlements.Length) return;

        SettlementLogic.RemoveSettlement(_settlements, dataIdx);

        _filteredIndices.RemoveAt(selIdx);
        for (int i = 0; i < _filteredIndices.Count; i++)
        {
            if (_filteredIndices[i] > dataIdx)
                _filteredIndices[i]--;
        }

        SettlementNames.RemoveAt(selIdx);
        if (SettlementNames.Count > 0)
            SelectedSettlementIndex = Math.Min(selIdx, SettlementNames.Count - 1);
        else
            HasSelection = false;
    }

    [RelayCommand]
    private async Task ExportSettlement()
    {
        if (_settlements == null || SelectedSettlementIndex < 0 || _filteredIndices.Count == 0 || SaveFilePickerFunc == null) return;
        int dataIdx = _filteredIndices[SelectedSettlementIndex];
        if (dataIdx >= _settlements.Length) return;

        var settlement = _settlements.GetObject(dataIdx);
        var cfg = ExportConfig.Instance;
        var vars = new Dictionary<string, string>
        {
            ["settlement_name"] = SettlementName,
            ["seed"] = SeedValue
        };
        string defaultName = ExportConfig.BuildFileName(cfg.SettlementTemplate, "", vars);
        string? path = await SaveFilePickerFunc("Export Settlement", cfg.SettlementExt.TrimStart('.'),
            ExportConfig.BuildDialogFilter(cfg.SettlementExt, "Settlement files"));
        if (string.IsNullOrEmpty(path)) return;
        try { settlement.ExportToFile(path); } catch { }
    }

    [RelayCommand]
    private async Task ImportSettlement()
    {
        if (_settlements == null || OpenFilePickerFunc == null) return;
        string? path = await OpenFilePickerFunc("Import Settlement",
            ExportConfig.BuildImportFilter(ExportConfig.Instance.SettlementExt, "Settlement files", ".stl"));
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var imported = JsonObject.ImportFromFile(path);
            imported = InventoryImportHelper.UnwrapNomNom(imported, "Settlement");

            int selectedDataIdx = (SelectedSettlementIndex >= 0 && _filteredIndices.Count > 0)
                ? _filteredIndices[SelectedSettlementIndex] : -1;
            int target = SettlementLogic.FindImportTargetIndex(_settlements, selectedDataIdx);

            if (target == -2)
                target = selectedDataIdx >= 0 ? selectedDataIdx : 0;

            if (target == -1)
            {
                _settlements.Add(imported);
            }
            else if (target >= 0 && target < _settlements.Length)
            {
                var existing = _settlements.GetObject(target);
                foreach (var propName in imported.Names())
                    existing.Set(propName, imported.Get(propName));
            }

            ReloadSettlementList();
        }
        catch { }
    }

    private void ReloadSettlementList()
    {
        SettlementNames.Clear();
        _filteredIndices.Clear();
        HasSelection = false;
        if (_settlements == null) return;

        for (int i = 0; i < _settlements.Length; i++)
        {
            try
            {
                _filteredIndices.Add(i);
                var settlement = _settlements.GetObject(i);
                string name = settlement.GetString("Name") ?? $"Settlement {i + 1}";
                SettlementNames.Add(name);
            }
            catch
            {
                _filteredIndices.Add(i);
                SettlementNames.Add($"Settlement {i + 1}");
            }
        }

        InfoLabel = $"Found {_filteredIndices.Count} settlement(s).";
        if (SettlementNames.Count > 0)
            SelectedSettlementIndex = 0;
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToSettlementsJsonAsync() => GoToJsonAsync("PlayerStateData", "SettlementStatesV2");

    public override void SaveData(JsonObject saveData)
    {
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            var settlements = playerState.GetArray("SettlementStatesV2");
            if (settlements == null || SelectedSettlementIndex < 0 || _filteredIndices.Count == 0) return;

            int dataIdx = _filteredIndices[SelectedSettlementIndex];
            if (dataIdx >= settlements.Length) return;

            var settlement = settlements.GetObject(dataIdx);

            var saveValues = new SettlementLogic.SettlementSaveValues
            {
                Name = SettlementName,
                SeedValue = SeedValue,
                DecisionTypeIndex = DecisionTypeIndex,
            };
            saveValues.Stats[0] = Population;
            saveValues.Stats[1] = Happiness;
            saveValues.Stats[2] = Productivity;
            saveValues.Stats[3] = Upkeep;
            saveValues.Stats[4] = Sentinels;
            saveValues.Stats[5] = Debt;
            saveValues.Stats[6] = Alert;
            saveValues.Stats[7] = BugAttack;

            saveValues.Population = MaxPopulation;
            saveValues.AlienRace = AlienRaceIndex >= 0 && AlienRaceIndex < SettlementLogic.AlienRaces.Length
                ? SettlementLogic.AlienRaces[AlienRaceIndex] : "None";
            saveValues.BuildingStates = CollectBuildingStates();

            SettlementLogic.SaveSettlementData(settlement, saveValues);
            SavePerks(settlement);

            var prodArr = settlement.GetArray("ProductionState");
            if (prodArr != null)
            {
                for (int i = 0; i < ProductionItems.Count && i < prodArr.Length; i++)
                {
                    var prodObj = prodArr.GetObject(i);
                    prodObj.Set("ElementId", ProductionItems[i].ElementId);
                    prodObj.Set("Amount", Math.Clamp(ProductionItems[i].Amount, 0, SettlementLogic.ProductionMaxAmount));
                }
            }
        }
        catch { }
    }
}

public partial class ProductionItemViewModel : ObservableObject
{
    [ObservableProperty] private string _elementId = "";
    [ObservableProperty] private string _itemName = "";
    [ObservableProperty] private int _amount;
}

/// <summary>A settlement perk and whether this settlement has it.</summary>
public partial class SettlementPerkViewModel : ObservableObject
{
    [ObservableProperty] private bool _isOwned;

    public string Id { get; }
    public string Name { get; }

    public SettlementPerkViewModel(SettlementPerk perk, bool owned)
    {
        Id = perk.Id;
        Name = string.IsNullOrEmpty(perk.Name) ? perk.Id : perk.Name;
        IsOwned = owned;
    }
}

/// <summary>
/// One building's packed state. The game stores construction progress, tier progression
/// and arrival flags in a single integer, so the row exposes the fields separately and
/// writes them back into the same value.
/// </summary>
public partial class BuildingStateViewModel : ObservableObject
{
    private bool _updating;

    [ObservableProperty] private int _initPhases;
    [ObservableProperty] private int _upgradePhases;
    [ObservableProperty] private int _tierProgress;
    [ObservableProperty] private bool _classSystemActive;
    [ObservableProperty] private bool _bArrived;
    [ObservableProperty] private bool _aArrived;
    [ObservableProperty] private bool _sArrived;
    [ObservableProperty] private int _rawValue;

    public int Index { get; }
    public string Label { get; }

    public BuildingStateViewModel(int index, int rawValue)
    {
        Index = index;
        Label = UiStrings.Format("settlement.slot_label",
            (index + 1).ToString(System.Globalization.CultureInfo.CurrentCulture));

        _updating = true;
        RawValue = rawValue;
        Decode(rawValue);
        _updating = false;
    }

    private void Decode(int value)
    {
        InitPhases = value & SettlementLogic.SettlementBuildingState.InitConstructionMask;
        UpgradePhases = (value & SettlementLogic.SettlementBuildingState.UpgradeProgressMask) >> 10;
        TierProgress = (value & SettlementLogic.SettlementBuildingState.TierProgressionMask) >> 20;
        ClassSystemActive = Bit(value, SettlementLogic.SettlementBuildingState.Bit_ClassSystemActive);
        BArrived = Bit(value, SettlementLogic.SettlementBuildingState.Bit_B_Arrived);
        AArrived = Bit(value, SettlementLogic.SettlementBuildingState.Bit_A_Arrived);
        SArrived = Bit(value, SettlementLogic.SettlementBuildingState.Bit_S_Arrived);
    }

    private static bool Bit(int value, int bit) => (value & (1 << bit)) != 0;

    /// <summary>Rebuilds the packed value, leaving bits this row does not own untouched.</summary>
    private void Encode()
    {
        if (_updating) return;

        int value = RawValue;
        value &= ~SettlementLogic.SettlementBuildingState.InitConstructionMask;
        value |= InitPhases & SettlementLogic.SettlementBuildingState.InitConstructionMask;

        value &= ~SettlementLogic.SettlementBuildingState.UpgradeProgressMask;
        value |= (UpgradePhases << 10) & SettlementLogic.SettlementBuildingState.UpgradeProgressMask;

        value &= ~SettlementLogic.SettlementBuildingState.TierProgressionMask;
        value |= (TierProgress << 20) & SettlementLogic.SettlementBuildingState.TierProgressionMask;

        void SetBit(int bit, bool on)
        {
            if (on) value |= 1 << bit;
            else value &= ~(1 << bit);
        }

        SetBit(SettlementLogic.SettlementBuildingState.Bit_ClassSystemActive, ClassSystemActive);
        SetBit(SettlementLogic.SettlementBuildingState.Bit_B_Arrived, BArrived);
        SetBit(SettlementLogic.SettlementBuildingState.Bit_A_Arrived, AArrived);
        SetBit(SettlementLogic.SettlementBuildingState.Bit_S_Arrived, SArrived);

        _updating = true;
        RawValue = value;
        _updating = false;
    }

    partial void OnInitPhasesChanged(int value) => Encode();
    partial void OnUpgradePhasesChanged(int value) => Encode();
    partial void OnTierProgressChanged(int value) => Encode();
    partial void OnClassSystemActiveChanged(bool value) => Encode();
    partial void OnBArrivedChanged(bool value) => Encode();
    partial void OnAArrivedChanged(bool value) => Encode();
    partial void OnSArrivedChanged(bool value) => Encode();

    /// <summary>Editing the packed value directly re-derives the fields.</summary>
    partial void OnRawValueChanged(int value)
    {
        if (_updating) return;
        _updating = true;
        Decode(value);
        _updating = false;
    }
}
