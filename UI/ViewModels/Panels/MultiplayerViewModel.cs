using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Core.Utilities;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>
/// A boolean paired with a seed, which is how the save stores several of the
/// multiplayer settings: a two-element array of [active, seed].
/// </summary>
public partial class SeededFlagViewModel : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private string _seed = "";

    public string Label { get; }

    public SeededFlagViewModel(string label) => Label = label;
}

/// <summary>
/// The Swarm co-op settings, which all live under CommonStateData.SeasonData.
/// </summary>
/// <remarks>
/// The game enforces some of these regardless of what the save says, and the shape of
/// the data has changed between updates, so the panel is gated behind an acknowledgement
/// and only writes back fields the user actually changed. Values are compared against
/// what was loaded rather than written unconditionally, so a field left alone keeps
/// whatever the save had — including a shape this editor does not model.
/// </remarks>
public partial class MultiplayerViewModel : PanelViewModelBase
{
    internal static readonly string[] AlienRaces =
        ["None", "Traders", "Warriors", "Explorers", "Robots", "Atlas", "Diplomats", "Exotics", "Builders"];

    internal static readonly string[] CommunityTeams = ["Red", "Green", "Blue"];

    private JsonObject? _seasonData;
    private readonly Dictionary<string, object?> _original = new();

    /// <summary>Nothing is editable until the user acknowledges the warning.</summary>
    [ObservableProperty] private bool _isAcknowledged;

    [ObservableProperty] private ObservableCollection<string> _teamItems = new(CommunityTeams);
    [ObservableProperty] private ObservableCollection<string> _raceItems = new(AlienRaces);

    [ObservableProperty] private int _cachedTeamIndex = -1;
    [ObservableProperty] private bool _useTeamShipSeeds;
    [ObservableProperty] private bool _useTeamShipPalettes;
    [ObservableProperty] private bool _useCommunityTeamPalettes;

    [ObservableProperty] private bool _neverAllowShipPurchases;
    [ObservableProperty] private bool _allowOnlyCorvetteShipPurchases;
    [ObservableProperty] private bool _neverAllowCorvettePurchases;
    [ObservableProperty] private bool _allowSaveContextCorvetteTransfer;
    [ObservableProperty] private bool _allowSaveContextShipTransfer;
    [ObservableProperty] private bool _allowSaveContextMultitoolTransfer;
    [ObservableProperty] private bool _onlyCorvettesSpawnWhenPlayerTeleports;
    [ObservableProperty] private bool _onlyCorvetteLauncherCanBeRepaired;

    [ObservableProperty] private int _forceRaceIndex = -1;
    [ObservableProperty] private string _persistentPoi = "";
    [ObservableProperty] private string _introSequencePoi = "";
    [ObservableProperty] private string _startWithIntroQuizId = "";

    /// <summary>The three per-team ship seeds, in team order.</summary>
    public ObservableCollection<SeededFlagViewModel> TeamShipSeeds { get; } =
    [
        new(UiStrings.Get("multiplayer.team_n_red")),
        new(UiStrings.Get("multiplayer.team_n_green")),
        new(UiStrings.Get("multiplayer.team_n_blue")),
    ];

    public SeededFlagViewModel BlockShipPurchases { get; } =
        new(UiStrings.Get("multiplayer.block_ship_purchases_until_milestone"));

    public SeededFlagViewModel BlockShipRepair { get; } =
        new(UiStrings.Get("multiplayer.block_ship_repair_until_milestone"));

    public SeededFlagViewModel WeaponSeed { get; } = new(UiStrings.Get("multiplayer.weapon_seed"));
    public SeededFlagViewModel ShipSeed { get; } = new(UiStrings.Get("multiplayer.ship_seed"));

    public override void ApplyLocalisation()
    {
        TeamItems = new ObservableCollection<string>(CommunityTeams);
        RaceItems = new ObservableCollection<string>(AlienRaces);
    }

    // =================================== Loading ===================================

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _seasonData = saveData.GetObject("CommonStateData")?.GetObject("SeasonData");
        _original.Clear();

        CachedTeamIndex = LoadEnum("CachedPlayerCommunityTeam", "CommunityTeam", CommunityTeams);
        UseTeamShipSeeds = LoadBool("UseTeamShipSeeds");
        UseTeamShipPalettes = LoadBool("UseTeamShipPalettes");
        UseCommunityTeamPalettes = LoadBool("UseCommunityTeamPalettes");

        for (int i = 0; i < TeamShipSeeds.Count; i++) LoadTeamSeed(i, TeamShipSeeds[i]);

        NeverAllowShipPurchases = LoadBool("NeverAllowShipPurchases");
        AllowOnlyCorvetteShipPurchases = LoadBool("AllowOnlyCorvetteShipPurchases");
        NeverAllowCorvettePurchases = LoadBool("NeverAllowCorvettePurchases");
        AllowSaveContextCorvetteTransfer = LoadBool("AllowSaveContextCorvetteTransfer");
        AllowSaveContextShipTransfer = LoadBool("AllowSaveContextShipTransfer");
        AllowSaveContextMultitoolTransfer = LoadBool("AllowSaveContextMultitoolTransfer");
        OnlyCorvettesSpawnWhenPlayerTeleports = LoadBool("OnlyCorvettesSpawnWhenPlayerTeleports");
        OnlyCorvetteLauncherCanBeRepaired = LoadBool("OnlyCorvetteLauncherCanBeRepaired");

        LoadSeededFlag("BlockShipPurchasesUntilMilestoneWithSeedComplete", BlockShipPurchases);
        LoadSeededFlag("BlockShipRepairUntilMilestoneWithSeedComplete", BlockShipRepair);

        ForceRaceIndex = LoadEnum("ForcePlayerRace", "AlienRace", AlienRaces);
        LoadSeededFlag("WeaponSeed", WeaponSeed);
        LoadSeededFlag("ShipSeed", ShipSeed);

        PersistentPoi = LoadString("PersistentPOI");
        IntroSequencePoi = LoadString("IntroSequencePOI");
        StartWithIntroQuizId = LoadString("StartWithIntroQuizID");
    }

    private string LoadString(string key)
    {
        string? value = _seasonData?.GetString(key);
        _original[key] = value;
        return value ?? "";
    }

    private bool LoadBool(string key)
    {
        bool value = false;
        if (_seasonData is not null && _seasonData.Contains(key))
        {
            try { value = _seasonData.GetBool(key); } catch { }
        }
        _original[key] = value;
        return value;
    }

    private void LoadSeededFlag(string key, SeededFlagViewModel target)
    {
        bool active = false;
        string seed = "";

        var arr = _seasonData?.GetArray(key);
        if (arr is not null && arr.Length >= 2)
        {
            try { active = arr.GetBool(0); } catch { }
            seed = arr.Get(1)?.ToString() ?? "";
        }

        _original[key + ".active"] = active;
        _original[key + ".seed"] = seed;
        target.IsActive = active;
        target.Seed = seed;
    }

    private void LoadTeamSeed(int index, SeededFlagViewModel target)
    {
        bool active = false;
        string seed = "";

        var entry = _seasonData?.GetArray("TeamShipSeeds") is { } arr && index < arr.Length
            ? arr.GetArray(index)
            : null;

        if (entry is not null && entry.Length >= 2)
        {
            try { active = entry.GetBool(0); } catch { }
            seed = entry.Get(1)?.ToString() ?? "";
        }

        _original[$"TeamShipSeeds.{index}.active"] = active;
        _original[$"TeamShipSeeds.{index}.seed"] = seed;
        target.IsActive = active;
        target.Seed = seed;
    }

    private int LoadEnum(string key, string subKey, string[] values)
    {
        string current = "";
        if (_seasonData?.GetObject(key) is { } obj)
        {
            try { current = obj.GetString(subKey) ?? ""; } catch { }
        }
        _original[key] = current;

        return Array.FindIndex(values, v => string.Equals(v, current, StringComparison.OrdinalIgnoreCase));
    }

    // =================================== Saving ====================================

    public override void SaveData(JsonObject saveData)
    {
        var season = saveData.GetObject("CommonStateData")?.GetObject("SeasonData");
        if (season is null) return;

        SaveEnum(season, "CachedPlayerCommunityTeam", "CommunityTeam",
            Pick(CommunityTeams, CachedTeamIndex, ""));
        SaveBool(season, "UseTeamShipSeeds", UseTeamShipSeeds);
        SaveBool(season, "UseTeamShipPalettes", UseTeamShipPalettes);
        SaveBool(season, "UseCommunityTeamPalettes", UseCommunityTeamPalettes);

        for (int i = 0; i < TeamShipSeeds.Count; i++) SaveTeamSeed(season, i, TeamShipSeeds[i]);

        SaveBool(season, "NeverAllowShipPurchases", NeverAllowShipPurchases);
        SaveBool(season, "AllowOnlyCorvetteShipPurchases", AllowOnlyCorvetteShipPurchases);
        SaveBool(season, "NeverAllowCorvettePurchases", NeverAllowCorvettePurchases);
        SaveBool(season, "AllowSaveContextCorvetteTransfer", AllowSaveContextCorvetteTransfer);
        SaveBool(season, "AllowSaveContextShipTransfer", AllowSaveContextShipTransfer);
        SaveBool(season, "AllowSaveContextMultitoolTransfer", AllowSaveContextMultitoolTransfer);
        SaveBool(season, "OnlyCorvettesSpawnWhenPlayerTeleports", OnlyCorvettesSpawnWhenPlayerTeleports);
        SaveBool(season, "OnlyCorvetteLauncherCanBeRepaired", OnlyCorvetteLauncherCanBeRepaired);

        SaveSeededFlag(season, "BlockShipPurchasesUntilMilestoneWithSeedComplete", BlockShipPurchases);
        SaveSeededFlag(season, "BlockShipRepairUntilMilestoneWithSeedComplete", BlockShipRepair);

        SaveEnum(season, "ForcePlayerRace", "AlienRace", Pick(AlienRaces, ForceRaceIndex, "None"));
        SaveSeededFlag(season, "WeaponSeed", WeaponSeed);
        SaveSeededFlag(season, "ShipSeed", ShipSeed);

        SaveString(season, "PersistentPOI", PersistentPoi);
        SaveString(season, "IntroSequencePOI", IntroSequencePoi);
        SaveString(season, "StartWithIntroQuizID", StartWithIntroQuizId);
    }

    private static string Pick(string[] values, int index, string fallback) =>
        index >= 0 && index < values.Length ? values[index] : fallback;

    private void SaveString(JsonObject season, string key, string value)
    {
        string? original = _original.GetValueOrDefault(key) as string;
        if (value == original) return;
        if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(original)) return;

        season.Set(key, value);
    }

    private void SaveBool(JsonObject season, string key, bool value)
    {
        if (_original.GetValueOrDefault(key) is bool original && value == original) return;
        season.Set(key, value);
    }

    private void SaveSeededFlag(JsonObject season, string key, SeededFlagViewModel source)
    {
        bool wasActive = _original.GetValueOrDefault(key + ".active") as bool? ?? false;
        string? wasSeed = _original.GetValueOrDefault(key + ".seed") as string;
        if (source.IsActive == wasActive && source.Seed == wasSeed) return;

        var arr = season.GetArray(key);
        if (arr is null || arr.Length < 2) return;

        if (source.IsActive != wasActive) arr.Set(0, source.IsActive);
        if (source.Seed != wasSeed && SeedHelper.NormalizeSeed(source.Seed) is { } normalised)
            arr.Set(1, normalised);
    }

    private void SaveTeamSeed(JsonObject season, int index, SeededFlagViewModel source)
    {
        bool wasActive = _original.GetValueOrDefault($"TeamShipSeeds.{index}.active") as bool? ?? false;
        string? wasSeed = _original.GetValueOrDefault($"TeamShipSeeds.{index}.seed") as string;
        if (source.IsActive == wasActive && source.Seed == wasSeed) return;

        var entry = season.GetArray("TeamShipSeeds") is { } arr && index < arr.Length
            ? arr.GetArray(index)
            : null;
        if (entry is null || entry.Length < 2) return;

        if (source.IsActive != wasActive) entry.Set(0, source.IsActive);
        if (source.Seed != wasSeed && SeedHelper.NormalizeSeed(source.Seed) is { } normalised)
            entry.Set(1, normalised);
    }

    private void SaveEnum(JsonObject season, string key, string subKey, string value)
    {
        if (_original.GetValueOrDefault(key) as string == value) return;
        season.GetObject(key)?.Set(subKey, value);
    }
}
