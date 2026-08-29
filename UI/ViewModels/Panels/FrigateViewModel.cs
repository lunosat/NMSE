using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using System.Globalization;

namespace NMSE.UI.ViewModels.Panels;

public partial class FrigateViewModel : PanelViewModelBase
{
    private JsonArray? _frigates;
    private JsonArray? _expeditions;
    private bool _loading;

    private static string[] FrigateTypes => FrigateLogic.FrigateTypes;
    private static string[] FrigateGrades => FrigateLogic.FrigateGrades;
    private static string[] FrigateRaces => FrigateLogic.FrigateRaces;

    [ObservableProperty] private ObservableCollection<FrigateListItemViewModel> _frigateList = new();
    [ObservableProperty] private FrigateListItemViewModel? _selectedFrigate;
    [ObservableProperty] private string _countLabel = "";
    [ObservableProperty] private bool _hasSelection;

    [ObservableProperty] private string _frigateName = "";
    [ObservableProperty] private int _typeIndex = -1;
    [ObservableProperty] private int _classIndex = -1;
    [ObservableProperty] private int _raceIndex = -1;
    [ObservableProperty] private List<string> _typeItems = new(FrigateLogic.FrigateTypes);
    [ObservableProperty] private List<string> _classItems = new(FrigateLogic.FrigateGrades);
    [ObservableProperty] private List<string> _raceItems = new(FrigateLogic.FrigateRaces);

    [ObservableProperty] private string _homeSeed = "";
    [ObservableProperty] private string _modelSeed = "";
    [ObservableProperty] private string _damageText = "";

    [ObservableProperty] private int _statCombat;
    [ObservableProperty] private int _statExploration;
    [ObservableProperty] private int _statIndustry;
    [ObservableProperty] private int _statTrading;
    [ObservableProperty] private int _statCostPerWarp;
    [ObservableProperty] private int _statFuelCost;
    [ObservableProperty] private int _statDuration;
    [ObservableProperty] private int _statLoot;
    [ObservableProperty] private int _statRepair;
    [ObservableProperty] private int _statDamageReduction;
    [ObservableProperty] private int _statStealth;

    [ObservableProperty] private int _totalExpeditions;
    [ObservableProperty] private int _totalSuccessful;
    [ObservableProperty] private int _totalFailed;
    [ObservableProperty] private int _timesDamaged;
    [ObservableProperty] private string _levelUpIn = "";
    [ObservableProperty] private string _levelsRemaining = "";
    [ObservableProperty] private string _stateText = "";
    [ObservableProperty] private string _missionType = "";

    partial void OnSelectedFrigateChanged(FrigateListItemViewModel? value)
    {
        HasSelection = value != null;
        if (value != null) LoadFrigateDetails(value);
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        FrigateList.Clear();
        HasSelection = false;
        _frigates = null;
        _expeditions = null;

        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            _frigates = playerState.GetArray("FleetFrigates");
            try { _expeditions = playerState.GetArray("FleetExpeditions"); } catch { }
            if (_frigates == null || _frigates.Length == 0)
            {
                CountLabel = "No frigates found.";
                return;
            }

            RefreshList();
        }
        catch { CountLabel = "Failed to load frigates."; }
    }

    private void RefreshList()
    {
        if (_frigates == null) return;
        var sel = SelectedFrigate;
        FrigateList.Clear();

        for (int i = 0; i < _frigates.Length; i++)
        {
            try
            {
                var f = _frigates.GetObject(i);
                string name = FrigateLogic.GetFrigateName(f, i);
                string type = FrigateLogic.GetFrigateType(f);
                string cls = FrigateLogic.ComputeClassFromTraits(f);
                FrigateList.Add(new FrigateListItemViewModel
                {
                    DisplayText = $"{name}  [{type}] ({cls})",
                    Index = i,
                    Data = f
                });
            }
            catch
            {
                FrigateList.Add(new FrigateListItemViewModel
                {
                    DisplayText = $"Frigate {i + 1}",
                    Index = i
                });
            }
        }

        CountLabel = $"Total frigates: {_frigates.Length}";
    }

    private void LoadFrigateDetails(FrigateListItemViewModel item)
    {
        _loading = true;
        try
        {
            var frigate = item.Data;
            if (frigate == null) return;

            FrigateName = frigate.GetString("CustomName") ?? "";

            string type = FrigateLogic.GetFrigateType(frigate);
            TypeIndex = Array.IndexOf(FrigateTypes, type);

            string computedClass = FrigateLogic.ComputeClassFromTraits(frigate);
            ClassIndex = Array.IndexOf(FrigateGrades, computedClass);

            string race = "";
            try { race = frigate.GetObject("Race")?.GetString("AlienRace") ?? ""; } catch { }
            RaceIndex = Array.IndexOf(FrigateRaces, race);

            HomeSeed = ReadSeed(frigate, "HomeSystemSeed");
            ModelSeed = ReadSeed(frigate, "ResourceSeed");

            int dmg = 0;
            try { dmg = frigate.GetInt("DamageTaken"); } catch { }
            DamageText = dmg > 0 ? $"Damage: {dmg}" : "No damage";

            var stats = frigate.GetArray("Stats");
            int[] statValues = new int[11];
            for (int i = 0; i < 11; i++)
            {
                try { if (stats != null && i < stats.Length) statValues[i] = stats.GetInt(i); } catch { }
            }
            StatCombat = statValues[0];
            StatExploration = statValues[1];
            StatIndustry = statValues[2];
            StatTrading = statValues[3];
            StatCostPerWarp = statValues[4];
            StatFuelCost = statValues[5];
            StatDuration = statValues[6];
            StatLoot = statValues[7];
            StatRepair = statValues[8];
            StatDamageReduction = statValues[9];
            StatStealth = statValues[10];

            try { TotalExpeditions = frigate.GetInt("TotalNumberOfExpeditions"); } catch { TotalExpeditions = 0; }
            try { TotalSuccessful = frigate.GetInt("TotalNumberOfSuccessfulEvents"); } catch { TotalSuccessful = 0; }
            try { TotalFailed = frigate.GetInt("TotalNumberOfFailedEvents"); } catch { TotalFailed = 0; }
            try { TimesDamaged = frigate.GetInt("NumberOfTimesDamaged"); } catch { TimesDamaged = 0; }

            int levelUp = FrigateLogic.GetLevelUpIn(TotalExpeditions);
            LevelUpIn = levelUp >= 0 ? levelUp.ToString(CultureInfo.InvariantCulture) : "MAX";
            LevelsRemaining = FrigateLogic.GetLevelUpsRemaining(TotalExpeditions).ToString(CultureInfo.InvariantCulture);

            int state = FrigateLogic.GetFrigateState(frigate, item.Index, _expeditions);
            StateText = state >= 0 && state < FrigateLogic.FrigateStateKeys.Length
                ? FrigateLogic.FrigateStateKeys[state] : "Unknown";

            if (state == 1 || state == 3)
            {
                int expIdx = _expeditions != null ? FrigateLogic.FindExpeditionIndex(item.Index, _expeditions) : -1;
                MissionType = expIdx >= 0 && _expeditions != null ? FrigateLogic.GetExpeditionCategory(_expeditions, expIdx) : "";
            }
            else
            {
                MissionType = "";
            }
        }
        catch { }
        finally { _loading = false; }
    }

    private static string ReadSeed(JsonObject frigate, string key)
    {
        try
        {
            var arr = frigate.GetArray(key);
            if (arr != null && arr.Length >= 2)
                return arr.Get(1)?.ToString() ?? "";
        }
        catch { }
        return "";
    }

    [RelayCommand]
    private void SaveFrigateChanges()
    {
        if (_loading || SelectedFrigate?.Data == null) return;
        var frigate = SelectedFrigate.Data;

        frigate.Set("CustomName", FrigateName);

        if (TypeIndex >= 0 && TypeIndex < FrigateTypes.Length)
        {
            try { frigate.GetObject("FrigateClass")?.Set("FrigateClass", FrigateTypes[TypeIndex]); } catch { }
        }

        if (RaceIndex >= 0 && RaceIndex < FrigateRaces.Length)
        {
            try { frigate.GetObject("Race")?.Set("AlienRace", FrigateRaces[RaceIndex]); } catch { }
        }

        var stats = frigate.GetArray("Stats");
        if (stats != null)
        {
            int[] vals = { StatCombat, StatExploration, StatIndustry, StatTrading,
                StatCostPerWarp, StatFuelCost, StatDuration, StatLoot, StatRepair,
                StatDamageReduction, StatStealth };
            for (int i = 0; i < 11 && i < stats.Length; i++)
                stats.Set(i, vals[i]);
        }

        frigate.Set("TotalNumberOfExpeditions", TotalExpeditions);
        frigate.Set("TotalNumberOfSuccessfulEvents", TotalSuccessful);
        frigate.Set("TotalNumberOfFailedEvents", TotalFailed);
        frigate.Set("NumberOfTimesDamaged", TimesDamaged);
    }

    [RelayCommand]
    private void RepairFrigate()
    {
        if (SelectedFrigate?.Data == null) return;
        SelectedFrigate.Data.Set("DamageTaken", 0);
        SelectedFrigate.Data.Set("RepairsMade", 0);
        DamageText = "No damage";
    }

    [RelayCommand]
    private async Task DeleteFrigateAsync()
    {
        if (Dialogs is not null)
        {
            // A frigate away on an expedition leaves the expedition pointing at a slot
            // that no longer holds it.
            int state = SelectedFrigate?.Data is { } f && _expeditions is not null
                ? FrigateLogic.GetFrigateState(f, SelectedFrigate.Index, _expeditions) : 0;

            string message = state == 1
                ? UiStrings.Get("frigate.delete_on_mission")
                : UiStrings.Get("frigate.delete_confirm");

            if (!await Dialogs.ConfirmAsync(UiStrings.Get("frigate.delete_title"), message,
                    Services.DialogIcon.Warning))
                return;
        }

        if (_frigates == null || SelectedFrigate == null) return;
        int idx = SelectedFrigate.Index;

        if (_expeditions != null && FrigateLogic.FindExpeditionIndex(idx, _expeditions) >= 0)
            return;

        _frigates.RemoveAt(idx);
        if (_expeditions != null)
            FrigateLogic.AdjustExpeditionIndicesAfterRemoval(idx, _expeditions);

        RefreshList();
        if (FrigateList.Count > 0)
            SelectedFrigate = FrigateList[Math.Min(idx, FrigateList.Count - 1)];
    }

    [RelayCommand]
    private void CopyFrigate()
    {
        if (_frigates == null || SelectedFrigate?.Data == null) return;
        if (_frigates.Length >= 30) return;

        var clone = SelectedFrigate.Data.DeepClone();
        _frigates.Add(clone);
        RefreshList();
        SelectedFrigate = FrigateList[^1];
    }

    // ================================= Expeditions =================================

    /// <summary>
    /// Sets the frigate one expedition short of its next level, so the level ticks over
    /// on the next successful run rather than jumping straight there.
    /// </summary>
    [RelayCommand]
    private void FastForwardLevel()
    {
        var frigate = SelectedFrigate?.Data;
        if (frigate is null) return;

        try
        {
            int completed = frigate.GetInt("TotalNumberOfExpeditions");
            foreach (int threshold in FrigateLogic.LevelVictoriesRequired)
            {
                if (completed >= threshold) continue;

                frigate.Set("TotalNumberOfExpeditions", threshold - 1);
                TotalExpeditions = threshold - 1;
                LevelUpIn = FrigateLogic.GetLevelUpIn(threshold - 1)
                    .ToString(CultureInfo.CurrentCulture);
                LevelsRemaining = FrigateLogic.GetLevelUpsRemaining(threshold - 1)
                    .ToString(CultureInfo.CurrentCulture);
                return;
            }

            LevelUpIn = UiStrings.Get("frigate.max_reached");
        }
        catch { }
    }

    /// <summary>
    /// Completes the frigate's current expedition: every event succeeds, no frigate is
    /// damaged or destroyed, and the fleet is repaired.
    /// </summary>
    [RelayCommand]
    private void FinishExpedition()
    {
        var expeditions = _expeditions;
        int frigateIndex = SelectedFrigate?.Index ?? -1;
        if (expeditions is null || frigateIndex < 0) return;

        int expIndex = FrigateLogic.FindExpeditionIndex(frigateIndex, expeditions);
        if (expIndex < 0)
        {
            MissionType = UiStrings.Get("frigate.no_frigates_found");
            return;
        }

        try
        {
            var expedition = expeditions.GetObject(expIndex);

            Clear(expedition.GetArray("DamagedFrigateIndices"));
            Clear(expedition.GetArray("DestroyedFrigateIndices"));

            // Everyone who set out is still active.
            var all = expedition.GetArray("AllFrigateIndices");
            var active = expedition.GetArray("ActiveFrigateIndices");
            if (all is not null && active is not null)
            {
                Clear(active);
                for (int i = 0; i < all.Length; i++) active.Add(all.GetInt(i));
            }

            var events = expedition.GetArray("Events");
            if (events is not null)
            {
                int total = events.Length;
                expedition.Set("NextEventToTrigger", total);
                try { expedition.Set("NumberOfSuccessfulEventsThisExpedition", total); } catch { }
                try { expedition.Set("NumberOfFailedEventsThisExpedition", 0); } catch { }
                for (int i = 0; i < total; i++)
                {
                    try { events.GetObject(i).Set("Success", true); } catch { }
                }
            }

            try { expedition.Set("PauseTime", 0); } catch { }

            // Repair every frigate that took part.
            if (all is not null && _frigates is not null)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    int index = all.GetInt(i);
                    if (index < 0 || index >= _frigates.Length) continue;
                    try { _frigates.GetObject(index).Set("Damage", 0); } catch { }
                }
            }

            RefreshList();
            DamageText = UiStrings.Get("frigate.no_damage");
        }
        catch (Exception ex)
        {
            MissionType = UiStrings.Format("common.error", ex.Message);
        }

        static void Clear(JsonArray? array)
        {
            if (array is null) return;
            for (int i = array.Length - 1; i >= 0; i--) array.RemoveAt(i);
        }
    }

    [RelayCommand]
    private void GenerateHomeSeed()
    {
        byte[] bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        HomeSeed = "0x" + BitConverter.ToString(bytes).Replace("-", "");
    }

    [RelayCommand]
    private void GenerateModelSeed()
    {
        byte[] bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        ModelSeed = "0x" + BitConverter.ToString(bytes).Replace("-", "");
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToFrigatesJsonAsync() => GoToJsonAsync("PlayerStateData", "FleetFrigates");

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToExpeditionsJsonAsync() => GoToJsonAsync("PlayerStateData", "FleetExpeditions");

    public override void SaveData(JsonObject saveData)
    {
        SaveFrigateChanges();
    }

    // Go to JSON. The tooltips name the section they open, so they are formatted here
    // rather than bound straight to the string table.
    public override void ApplyLocalisation()
    {
        OnPropertyChanged(nameof(GoToListTooltip));
        OnPropertyChanged(nameof(GoToSelectedTooltip));
        OnPropertyChanged(nameof(GoToExpeditionsTooltip));
    }

    public string GoToListTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("frigate.title"));

    public string GoToSelectedTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("frigate.info_header"));

    public string GoToExpeditionsTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("goto_json.nav_expeditions"));

    [RelayCommand] private Task GoToListJsonAsync() => GoToJsonAsync("PlayerStateData", "FleetFrigates");

    [RelayCommand]
    private Task GoToSelectedJsonAsync()
    {
        int idx = SelectedFrigate?.Index ?? -1;
        return idx < 0
            ? Task.CompletedTask
            : GoToJsonAsync("PlayerStateData", "FleetFrigates", $"[{idx}]");
    }
}

public partial class FrigateListItemViewModel : ObservableObject
{
    [ObservableProperty] private string _displayText = "";
    public int Index { get; set; }
    public JsonObject? Data { get; set; }
    public override string ToString() => DisplayText;
}
