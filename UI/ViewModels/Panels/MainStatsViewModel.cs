using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.IO;
using NMSE.Models;
using NMSE.Core.Utilities;
using System.Globalization;

namespace NMSE.UI.ViewModels.Panels;

public partial class MainStatsViewModel : PanelViewModelBase
{
    private JsonObject? _saveData;
    private JsonObject? _playerState;
    private JsonObject? _accountData;
    private string? _saveFilePath;
    private IconManager? _iconManager;

    public event EventHandler? ReloadRequested;

    private static readonly string[] DifficultyPresets =
        { "Invalid", "Custom", "Normal", "Creative", "Relaxed", "Survival", "Permadeath" };

    /// <summary>The hyperdrive upgrade that unlocks purple-system warping.</summary>
    private const string PurpleWarpTechId = "^HDRIVEBOOST4";

    /// <summary>
    /// The expedition this save belongs to. The number lives in the player state for a
    /// save started as one, and in the season data for one converted later.
    /// </summary>
    private static string ReadExpeditionNumber(JsonObject playerState, JsonObject saveData)
    {
        try
        {
            int starting = playerState.GetInt("StartingSeasonNumber");
            if (starting > 0) return UiStrings.Format("player.expedition_format", starting);
        }
        catch { }

        var seasonData = saveData.GetObject("CommonStateData")?.GetObject("SeasonData");
        if (seasonData?.GetString("DisplayNumber") is { Length: > 0 } display) return display;

        try
        {
            int fallback = seasonData?.GetInt("SeasonNumber") ?? 0;
            if (fallback > 0) return UiStrings.Format("player.expedition_format", fallback);
        }
        catch { }

        return "";
    }

    private static bool HasDriveBoost(JsonObject playerState)
    {
        try
        {
            var knownTech = playerState.GetArray("KnownTech");
            return knownTech is not null && knownTech.IndexOf(PurpleWarpTechId) >= 0;
        }
        catch { return false; }
    }

    private static readonly string[] GuideCategories =
        { "Survival Basics", "Getting Around", "Making Discoveries", "Upgrades & Crafting",
          "Construction", "Making Money", "Alien Lifeforms", "Combat" };

    [ObservableProperty] private decimal _health;
    [ObservableProperty] private decimal _shield;
    [ObservableProperty] private decimal _energy;
    [ObservableProperty] private decimal _units;
    [ObservableProperty] private decimal _nanites;
    [ObservableProperty] private decimal _quicksilver;

    [ObservableProperty] private string _saveName = "";
    [ObservableProperty] private string _saveSummary = "";
    [ObservableProperty] private string _playTime = "";
    [ObservableProperty] private string _lastSaveDate = "";
    [ObservableProperty] private string _accountName = "";
    [ObservableProperty] private bool _thirdPersonCamera;

    [ObservableProperty] private int _currentPresetIndex = -1;
    [ObservableProperty] private int _easiestPresetIndex = -1;
    [ObservableProperty] private int _hardestPresetIndex = -1;
    /// <summary>
    /// Localisation keys for <see cref="DifficultyPresets"/>, in the same order. The save
    /// stores the English name, so only the display side is translated.
    /// </summary>
    private static readonly string[] DifficultyPresetLocKeys =
        { "player.preset_invalid", "player.preset_custom", "player.preset_normal",
          "player.preset_creative", "player.preset_relaxed", "player.preset_survival",
          "player.preset_permadeath" };

    [ObservableProperty] private List<string> _presetItems =
        new(DifficultyPresetLocKeys.Select(UiStrings.Get));

    [ObservableProperty] private string _galaxyDisplay = "";
    [ObservableProperty] private string _portalCode = "";
    [ObservableProperty] private string _portalCodeDec = "";
    [ObservableProperty] private string _signalBooster = "";
    [ObservableProperty] private string _distanceToCenter = "";
    [ObservableProperty] private string _jumpsToCenter = "";
    [ObservableProperty] private string _freighterInSystem = "";
    [ObservableProperty] private string _nexusInSystem = "";
    [ObservableProperty] private string _planetsInSystem = "";

    [ObservableProperty] private int _playerStateIndex = -1;
    [ObservableProperty] private List<string> _playerStateItems = new(CoordinateHelper.PlayerStates);
    [ObservableProperty] private bool _portalInterference;
    [ObservableProperty] private bool _purpleWarpEnabled;


    /// <summary>
    /// Set only for an expedition save, where the difficulty presets are fixed by the
    /// expedition and so are shown but not editable.
    /// </summary>
    [ObservableProperty] private string _expeditionNumber = "";
    [ObservableProperty] private bool _isExpeditionSave;

    /// <summary>The galaxy's core colour, shown as a dot beside its name.</summary>
    [ObservableProperty] private Avalonia.Media.IBrush _galaxyCoreBrush =
        Avalonia.Media.Brushes.Transparent;

    [ObservableProperty] private int _galaxyIndex;
    [ObservableProperty] private int _voxelX;
    [ObservableProperty] private int _voxelY;
    [ObservableProperty] private int _voxelZ;
    [ObservableProperty] private int _solarSystemIndex;
    [ObservableProperty] private int _planetIndex;
    [ObservableProperty] private string _portalHexInput = "";

    [ObservableProperty] private string _timeToNextBattle = "";
    [ObservableProperty] private int _warpsToNextBattle;

    [ObservableProperty] private string _statusText = "";

    // Save Utilities
    [ObservableProperty] private int _sourceSlotIndex;
    [ObservableProperty] private int _destSlotIndex = 1;
    [ObservableProperty] private int _transferPlatformIndex;
    public List<string> SlotItems { get; } = Enumerable.Range(1, 15).Select(i => $"Slot {i}").ToList();
    [ObservableProperty] private List<string> _platformItems = new(
    [
        UiStrings.Get("player.platform_steam"),
        UiStrings.Get("player.platform_gog"),
        UiStrings.Get("player.platform_xbox"),
        UiStrings.Get("player.platform_ps4"),
        UiStrings.Get("player.platform_switch"),
    ]);

    // Guides
    [ObservableProperty] private ObservableCollection<GuideTopicViewModel> _guideTopics = new();
    [ObservableProperty] private string _guideFilter = "";

    // Titles
    [ObservableProperty] private ObservableCollection<TitleRowViewModel> _titleRows = new();

    public string PlayerName { get; private set; } = "Explorer";

    public void SetSaveFilePath(string? path)
    {
        _saveFilePath = path;

        // The save carries no timestamp of its own, so this comes from the file.
        try
        {
            LastSaveDate = path is not null && File.Exists(path)
                ? File.GetLastWriteTime(path).ToString("g", CultureInfo.CurrentCulture)
                : "";
        }
        catch { LastSaveDate = ""; }
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        Multiplayer.Dialogs ??= Dialogs;
        Multiplayer.LoadData(saveData, database, iconManager);

        _saveData = saveData;
        _iconManager = iconManager;
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;
            _playerState = playerState;
            LoadOutfits(playerState);

            Health = MainStatsLogic.ReadStatValue(playerState, "Health", 0, 999999);
            Shield = MainStatsLogic.ReadStatValue(playerState, "Shield", 0, 999999);
            Energy = MainStatsLogic.ReadStatValue(playerState, "Energy", 0, 999999);
            Units = MainStatsLogic.ReadStatValue(playerState, "Units", 0, uint.MaxValue);
            Nanites = MainStatsLogic.ReadStatValue(playerState, "Nanites", 0, uint.MaxValue);
            Quicksilver = MainStatsLogic.ReadStatValue(playerState, "Specials", 0, uint.MaxValue);

            try { SaveName = saveData.GetObject("CommonStateData")?.GetString("SaveName") ?? ""; } catch { }
            try { SaveSummary = playerState.GetString("SaveSummary") ?? ""; } catch { }
            try
            {
                int totalSeconds = saveData.GetObject("CommonStateData")?.GetInt("TotalPlayTime") ?? 0;
                var ts = TimeSpan.FromSeconds(totalSeconds);
                PlayTime = UiStrings.Format("player.time_format", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
            }
            catch { PlayTime = ""; }

            try { ThirdPersonCamera = saveData.GetObject("CommonStateData")?.GetBool("UsesThirdPersonCharacterCam") ?? false; } catch { }

            try
            {
                string usn = "";
                var commonState = saveData.GetObject("CommonStateData");
                var owners = commonState?.GetArray("UsedDiscoveryOwnersV2");
                if (owners != null && owners.Length > 0)
                    usn = owners.GetObject(0)?.GetString("USN") ?? "";
                string displayName = string.IsNullOrEmpty(usn)
                    ? UiStrings.Get("player.name_fallback")
                    : usn;
                AccountName = displayName;
                PlayerName = displayName;
            }
            catch
            {
                AccountName = UiStrings.Get("player.name_fallback");
                PlayerName = AccountName;
            }

            try
            {
                var diffState = playerState.GetObject("DifficultyState");
                if (diffState != null)
                {
                    CurrentPresetIndex = FindPresetIndex(diffState.GetObject("Preset")?.GetString("DifficultyPresetType"));
                    EasiestPresetIndex = FindPresetIndex(diffState.GetObject("EasiestUsedPreset")?.GetString("DifficultyPresetType"));
                    HardestPresetIndex = FindPresetIndex(diffState.GetObject("HardestUsedPreset")?.GetString("DifficultyPresetType"));
                }
            }
            catch { }

            // An expedition fixes its own difficulty, so the presets are shown but not
            // editable and the expedition number is named instead.
            IsExpeditionSave = SaveContext.IsExpeditionSave;
            ExpeditionNumber = IsExpeditionSave ? ReadExpeditionNumber(playerState, saveData) : "";

            LoadCoordinates(playerState, saveData);
            LoadSpaceBattle(playerState, saveData);
        }
        catch { }
    }

    private static int FindPresetIndex(string? value)
    {
        if (string.IsNullOrEmpty(value)) return -1;
        int idx = Array.IndexOf(DifficultyPresets, value);
        return idx >= 0 ? idx : -1;
    }

    private void LoadCoordinates(JsonObject playerState, JsonObject saveData)
    {
        try
        {
            var addr = playerState.GetObject("UniverseAddress");
            if (addr == null) return;

            int realityIndex = addr.GetInt("RealityIndex");
            string galaxyType = GalaxyDatabase.GetGalaxyType(realityIndex);
            GalaxyDisplay = $"{GalaxyDatabase.GetGalaxyDisplayName(realityIndex)} ({galaxyType})";

            var galactic = addr.GetObject("GalacticAddress");
            if (galactic == null) return;

            int vx = galactic.GetInt("VoxelX");
            int vy = galactic.GetInt("VoxelY");
            int vz = galactic.GetInt("VoxelZ");
            int si = galactic.GetInt("SolarSystemIndex");
            int pi = 0;
            try { pi = galactic.GetInt("PlanetIndex"); } catch { }

            PortalCode = CoordinateHelper.VoxelToPortalCode(vx, vy, vz, si, pi);
            PortalCodeDec = CoordinateHelper.PortalHexToDec(PortalCode);
            SignalBooster = CoordinateHelper.VoxelToSignalBooster(vx, vy, vz, si);

            GalaxyIndex = realityIndex;
            VoxelX = vx;
            VoxelY = vy;
            VoxelZ = vz;
            SolarSystemIndex = si;
            PlanetIndex = pi;

            try
            {
                var spawnState = saveData.GetObject("SpawnStateData");
                string lastState = spawnState?.GetString("LastKnownPlayerState") ?? "";
                int stateIdx = Array.IndexOf(CoordinateHelper.PlayerStates, lastState);
                PlayerStateIndex = stateIdx >= 0 ? stateIdx : -1;
            }
            catch { PlayerStateIndex = -1; }

            double dist = CoordinateHelper.GetDistanceToCenter(vx, vy, vz);
            DistanceToCenter = $"{dist:F0} ly";
            JumpsToCenter = CoordinateHelper.GetJumpsToCenter(dist, CoordinateHelper.DefaultHyperdriveRange).ToString(CultureInfo.InvariantCulture);

            try
            {
                var freighterAddr = playerState.GetObject("FreighterUniverseAddress");
                bool freighterHere = false;
                if (freighterAddr != null)
                {
                    int fRealIdx = freighterAddr.GetInt("RealityIndex");
                    var fGal = freighterAddr.GetObject("GalacticAddress");
                    if (fGal != null && fRealIdx == realityIndex)
                        freighterHere = fGal.GetInt("VoxelX") == vx && fGal.GetInt("VoxelY") == vy
                            && fGal.GetInt("VoxelZ") == vz && fGal.GetInt("SolarSystemIndex") == si;
                }
                FreighterInSystem = UiStrings.Get(freighterHere ? "common.yes" : "common.no");
            }
            catch { FreighterInSystem = UiStrings.Get("common.unknown"); }

            try
            {
                var nexusAddr = playerState.GetObject("NexusUniverseAddress");
                bool nexusHere = false;
                if (nexusAddr != null)
                {
                    int nRealIdx = nexusAddr.GetInt("RealityIndex");
                    var nGal = nexusAddr.GetObject("GalacticAddress");
                    if (nGal != null && nRealIdx == realityIndex)
                        nexusHere = nGal.GetInt("VoxelX") == vx && nGal.GetInt("VoxelY") == vy
                            && nGal.GetInt("VoxelZ") == vz && nGal.GetInt("SolarSystemIndex") == si;
                }
                NexusInSystem = UiStrings.Get(nexusHere ? "common.yes" : "common.no");
            }
            catch { NexusInSystem = UiStrings.Get("common.unknown"); }

            try
            {
                var planetSeeds = playerState.GetArray("PlanetSeeds");
                int count = 0;
                if (planetSeeds != null)
                {
                    for (int i = 0; i < planetSeeds.Length; i++)
                    {
                        try
                        {
                            var seed = planetSeeds.GetArray(i);
                            if (seed != null && seed.Length >= 2 && seed.Get(1)?.ToString() != "0x0")
                                count++;
                        }
                        catch { }
                    }
                }
                PlanetsInSystem = count.ToString(CultureInfo.InvariantCulture);
            }
            catch { PlanetsInSystem = "0"; }

            try { PortalInterference = playerState.GetBool("OnOtherSideOfPortal"); } catch { }

            // The game only lets a player warp to purple systems when it has both
            // recorded the discovery and learned the drive upgrade, so the box reflects
            // the two together rather than either alone.
            try
            {
                bool discovered = false;
                try { discovered = playerState.GetBool("HasDiscoveredPurpleSystems"); } catch { }
                PurpleWarpEnabled = discovered && HasDriveBoost(playerState);
            }
            catch { PurpleWarpEnabled = false; }
        }
        catch { }
    }

    private void LoadSpaceBattle(JsonObject playerState, JsonObject saveData)
    {
        try
        {
            int totalPlayTime = 0;
            try { totalPlayTime = saveData.GetObject("CommonStateData")?.GetInt("TotalPlayTime") ?? 0; } catch { }
            int timeLastBattle = 0;
            try { timeLastBattle = playerState.GetInt("TimeLastSpaceBattle"); } catch { }

            int timeRemaining = Math.Max(0, Math.Min(
                CoordinateHelper.SpaceBattleIntervalSeconds - (totalPlayTime - timeLastBattle),
                CoordinateHelper.SpaceBattleIntervalSeconds));
            var ts = TimeSpan.FromSeconds(timeRemaining);
            TimeToNextBattle = UiStrings.Format("player.time_format", (int)ts.TotalHours, ts.Minutes, ts.Seconds);

            int warpsLastBattle = 0;
            try { warpsLastBattle = playerState.GetInt("WarpsLastSpaceBattle"); } catch { }
            int totalWarps = 0;
            try
            {
                var statsGroups = playerState.GetArray("Stats");
                if (statsGroups != null)
                {
                    for (int i = 0; i < statsGroups.Length; i++)
                    {
                        var group = statsGroups.GetObject(i);
                        if (group.GetString("GroupId") == "^GLOBAL_STATS")
                        {
                            var stats = group.GetArray("Stats");
                            if (stats != null)
                            {
                                for (int j = 0; j < stats.Length; j++)
                                {
                                    var stat = stats.GetObject(j);
                                    if (stat.GetString("Id") == "^DIST_WARP")
                                    {
                                        totalWarps = stat.GetObject("Value")?.GetInt("IntValue") ?? 0;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch { }

            WarpsToNextBattle = Math.Max(0, CoordinateHelper.SpaceBattleIntervalWarps - (totalWarps - warpsLastBattle));
        }
        catch { }
    }

    /// <summary>
    /// The galaxy as the game numbers it: Euclid is 1, while the save stores 0. The
    /// field edits this so the number matches what a player reads elsewhere.
    /// </summary>
    public int GalaxyNumber
    {
        get => GalaxyIndex + 1;
        set => GalaxyIndex = value - 1;
    }

    partial void OnGalaxyIndexChanged(int value) => OnPropertyChanged(nameof(GalaxyNumber));

    [RelayCommand]
    private void ApplyCoordinates()
    {
        if (_playerState == null) return;
        try
        {
            var addr = _playerState.GetObject("UniverseAddress");
            if (addr == null) return;

            addr.Set("RealityIndex", GalaxyIndex);
            var galactic = addr.GetObject("GalacticAddress");
            if (galactic == null) return;

            galactic.Set("VoxelX", VoxelX);
            galactic.Set("VoxelY", VoxelY);
            galactic.Set("VoxelZ", VoxelZ);
            galactic.Set("SolarSystemIndex", SolarSystemIndex);
            galactic.Set("PlanetIndex", PlanetIndex);
        }
        catch { }
        RefreshCoordinateDisplay();
    }

    [RelayCommand]
    private async Task ConvertPortalCodeAsync()
    {
        string portalCode = PortalHexInput.Trim().ToUpperInvariant();

        if (portalCode.Length != 12
            || !CoordinateHelper.PortalCodeToVoxel(portalCode, out int vx, out int vy, out int vz, out int si, out int pi))
        {
            // Silently doing nothing reads as a broken button, so say what is wrong.
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("player.invalid_portal_title"),
                    UiStrings.Get("player.invalid_portal_format"), Services.DialogIcon.Warning);
            return;
        }

        VoxelX = vx;
        VoxelY = vy;
        VoxelZ = vz;
        SolarSystemIndex = si;
        PlanetIndex = pi;
    }

    [RelayCommand]
    private async Task CoordinateRouletteAsync()
    {
        const string hexChars = "0123456789ABCDEF";
        var portalChars = new char[12];
        for (int i = 0; i < 12; i++)
            portalChars[i] = hexChars[Random.Shared.Next(16)];
        string portalCode = new string(portalChars);
        int galaxy = Random.Shared.Next(256);

        if (!CoordinateHelper.PortalCodeToVoxel(portalCode, out int vx, out int vy, out int vz, out int si, out int pi))
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("player.roulette_title"),
                    UiStrings.Get("player.roulette_failed"), Services.DialogIcon.Warning);
            return;
        }

        // This moves the player somewhere at random, so it says where before it does it.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.roulette_title"),
                UiStrings.Format("player.roulette_confirm", portalCode,
                    GalaxyDatabase.GetGalaxyDisplayName(galaxy), galaxy)))
            return;

        GalaxyIndex = galaxy;
        VoxelX = vx;
        VoxelY = vy;
        VoxelZ = vz;
        SolarSystemIndex = si;
        PlanetIndex = pi;

        ApplyCoordinates();
    }

    [RelayCommand]
    private async Task TriggerSpaceBattleAsync()
    {
        if (_playerState == null) return;
        try
        {
            _playerState.Set("TimeLastSpaceBattle", 0);
            _playerState.Set("WarpsLastSpaceBattle", 0);
            WarpsToNextBattle = 0;
            TimeToNextBattle = UiStrings.Format("player.time_format", 0, 0, 0);
            StatusText = UiStrings.Get("player.space_battle_triggered");

            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("player.space_battle_title"),
                    UiStrings.Get("player.space_battle_triggered"));
        }
        catch { }
    }

    private void RefreshCoordinateDisplay()
    {
        string galaxyType = GalaxyDatabase.GetGalaxyType(GalaxyIndex);
        GalaxyDisplay = $"{GalaxyDatabase.GetGalaxyDisplayName(GalaxyIndex)} ({galaxyType})";

        // Galaxies cycle through core colours; the dot is how the panel showed which.
        GalaxyCoreBrush = new Avalonia.Media.SolidColorBrush(
            GalaxyDatabase.GetGalaxyCoreColorValue(GalaxyIndex));
        PortalCode = CoordinateHelper.VoxelToPortalCode(VoxelX, VoxelY, VoxelZ, SolarSystemIndex, PlanetIndex);
        PortalCodeDec = CoordinateHelper.PortalHexToDec(PortalCode);
        SignalBooster = CoordinateHelper.VoxelToSignalBooster(VoxelX, VoxelY, VoxelZ, SolarSystemIndex);

        double dist = CoordinateHelper.GetDistanceToCenter(VoxelX, VoxelY, VoxelZ);
        DistanceToCenter = $"{dist:F0} ly";
        JumpsToCenter = CoordinateHelper.GetJumpsToCenter(dist, CoordinateHelper.DefaultHyperdriveRange).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToPlayerJsonAsync() => GoToJsonAsync("PlayerStateData");

    /// <summary>
    /// The Swarm co-op settings, shown as a tab here as the panel did. It is not in the
    /// shell's panel list, so this one forwards what it is given.
    /// </summary>
    public MultiplayerViewModel Multiplayer { get; } = new();

    public override void ApplyLocalisation()
    {
        PresetItems = new List<string>(DifficultyPresetLocKeys.Select(UiStrings.Get));
        PlatformItems = new List<string>(
        [
            UiStrings.Get("player.platform_steam"),
            UiStrings.Get("player.platform_gog"),
            UiStrings.Get("player.platform_xbox"),
            UiStrings.Get("player.platform_ps4"),
            UiStrings.Get("player.platform_switch"),
        ]);
    }

    public override void SaveData(JsonObject saveData)
    {
        Multiplayer.SaveData(saveData);

        var playerState = saveData.GetObject("PlayerStateData");
        if (playerState == null) return;

        MainStatsLogic.WriteStatValues(playerState, Health, Shield, Energy, Units, Nanites, Quicksilver);

        try { saveData.GetObject("CommonStateData")?.Set("SaveName", SaveName); } catch { }
        try { playerState.Set("SaveSummary", SaveSummary); } catch { }
        try { saveData.GetObject("CommonStateData")?.Set("UsesThirdPersonCharacterCam", ThirdPersonCamera); } catch { }

        try
        {
            var diffState = playerState.GetObject("DifficultyState");
            if (diffState != null)
            {
                if (CurrentPresetIndex >= 0 && CurrentPresetIndex < DifficultyPresets.Length)
                    diffState.GetObject("Preset")?.Set("DifficultyPresetType", DifficultyPresets[CurrentPresetIndex]);
                if (EasiestPresetIndex >= 0 && EasiestPresetIndex < DifficultyPresets.Length)
                    diffState.GetObject("EasiestUsedPreset")?.Set("DifficultyPresetType", DifficultyPresets[EasiestPresetIndex]);
                if (HardestPresetIndex >= 0 && HardestPresetIndex < DifficultyPresets.Length)
                    diffState.GetObject("HardestUsedPreset")?.Set("DifficultyPresetType", DifficultyPresets[HardestPresetIndex]);
            }
        }
        catch { }

        if (PlayerStateIndex >= 0)
        {
            try
            {
                var spawnState = saveData.GetObject("SpawnStateData");
                spawnState?.Set("LastKnownPlayerState", CoordinateHelper.PlayerStates[PlayerStateIndex]);
            }
            catch { }
        }

        try { playerState.Set("OnOtherSideOfPortal", PortalInterference); } catch { }

        // Both halves have to agree, or the game ignores the setting.
        try
        {
            playerState.Set("HasDiscoveredPurpleSystems", PurpleWarpEnabled);

            var knownTech = playerState.GetArray("KnownTech");
            if (knownTech is not null)
            {
                int idx = knownTech.IndexOf(PurpleWarpTechId);
                if (PurpleWarpEnabled && idx < 0) knownTech.Add(PurpleWarpTechId);
                else if (!PurpleWarpEnabled && idx >= 0) knownTech.RemoveAt(idx);
            }
        }
        catch { }

        try
        {
            var addr = playerState.GetObject("UniverseAddress");
            if (addr != null)
            {
                addr.Set("RealityIndex", GalaxyIndex);
                var galactic = addr.GetObject("GalacticAddress");
                if (galactic != null)
                {
                    galactic.Set("VoxelX", VoxelX);
                    galactic.Set("VoxelY", VoxelY);
                    galactic.Set("VoxelZ", VoxelZ);
                    galactic.Set("SolarSystemIndex", SolarSystemIndex);
                    galactic.Set("PlanetIndex", PlanetIndex);
                }
            }
        }
        catch { }
    }

    // --- Save Utilities ---

    private string? GetSaveDirectory() =>
        _saveFilePath != null ? Path.GetDirectoryName(_saveFilePath) : null;

    private SaveFileManager.Platform GetDetectedPlatform()
    {
        string? dir = GetSaveDirectory();
        return dir != null ? SaveFileManager.DetectPlatform(dir) : SaveFileManager.Platform.Unknown;
    }

    private static SaveFileManager.Platform TransferPlatformFromIndex(int index) => index switch
    {
        0 => SaveFileManager.Platform.Steam,
        1 => SaveFileManager.Platform.GOG,
        2 => SaveFileManager.Platform.XboxGamePass,
        3 => SaveFileManager.Platform.PS4,
        4 => SaveFileManager.Platform.Switch,
        _ => SaveFileManager.Platform.Unknown,
    };

    [RelayCommand]
    private async Task CopySlotAsync()
    {
        string? dir = GetSaveDirectory();
        if (dir == null) { StatusText = UiStrings.Get("player.no_save_loaded"); return; }

        // These rewrite save files on disk, so none of them happens on a single click.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.save_utils_title"),
                UiStrings.Get("player.copy_slot_confirm"), Services.DialogIcon.Warning))
            return;
        if (SourceSlotIndex == DestSlotIndex) { StatusText = UiStrings.Get("player.slots_must_differ"); return; }
        try
        {
            SaveSlotManager.CopySlot(dir, SourceSlotIndex, DestSlotIndex, GetDetectedPlatform());
            StatusText = UiStrings.Format("player.copy_slot_success", SourceSlotIndex + 1, DestSlotIndex + 1);
        }
        catch (Exception ex) { StatusText = UiStrings.Format("player.copy_slot_failed", ex.Message); }
    }

    [RelayCommand]
    private async Task MoveSlotAsync()
    {
        string? dir = GetSaveDirectory();
        if (dir == null) { StatusText = UiStrings.Get("player.no_save_loaded"); return; }

        // These rewrite save files on disk, so none of them happens on a single click.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.save_utils_title"),
                UiStrings.Get("player.move_slot_confirm"), Services.DialogIcon.Warning))
            return;
        if (SourceSlotIndex == DestSlotIndex) { StatusText = UiStrings.Get("player.slots_must_differ"); return; }
        try
        {
            SaveSlotManager.MoveSlot(dir, SourceSlotIndex, DestSlotIndex, GetDetectedPlatform());
            StatusText = UiStrings.Format("player.move_slot_success", SourceSlotIndex + 1, DestSlotIndex + 1);
            ReloadRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { StatusText = UiStrings.Format("player.move_slot_failed", ex.Message); }
    }

    [RelayCommand]
    private async Task SwapSlotsAsync()
    {
        string? dir = GetSaveDirectory();
        if (dir == null) { StatusText = UiStrings.Get("player.no_save_loaded"); return; }

        // These rewrite save files on disk, so none of them happens on a single click.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.save_utils_title"),
                UiStrings.Get("player.swap_slot_confirm"), Services.DialogIcon.Warning))
            return;
        if (SourceSlotIndex == DestSlotIndex) { StatusText = UiStrings.Get("player.slots_must_differ"); return; }
        try
        {
            SaveSlotManager.SwapSlots(dir, SourceSlotIndex, DestSlotIndex, GetDetectedPlatform());
            StatusText = UiStrings.Format("player.swap_slot_success", SourceSlotIndex + 1, DestSlotIndex + 1);
            ReloadRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { StatusText = UiStrings.Format("player.swap_slot_failed", ex.Message); }
    }

    [RelayCommand]
    private async Task DeleteSlotAsync()
    {
        string? dir = GetSaveDirectory();
        if (dir == null) { StatusText = UiStrings.Get("player.no_save_loaded"); return; }

        // These rewrite save files on disk, so none of them happens on a single click.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.save_utils_title"),
                UiStrings.Get("player.delete_slot_confirm"), Services.DialogIcon.Warning))
            return;
        try
        {
            SaveSlotManager.DeleteSlot(dir, SourceSlotIndex, GetDetectedPlatform());
            StatusText = UiStrings.Format("player.delete_slot_success", SourceSlotIndex + 1);
            ReloadRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { StatusText = UiStrings.Format("player.delete_slot_failed", ex.Message); }
    }

    public Func<string, Task<string?>>? PickFolderFunc { get; set; }

    [RelayCommand]
    private async Task TransferPlatform()
    {
        if (_saveFilePath == null) { StatusText = UiStrings.Get("player.no_save_loaded"); return; }
        if (PickFolderFunc == null || TransferPlatformIndex < 0) return;

        if (DestSlotIndex < 0)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("player.transfer_label"),
                    UiStrings.Get("player.transfer_select_dest"), Services.DialogIcon.Warning);
            return;
        }

        string? destDir = await PickFolderFunc(UiStrings.Get("player.transfer_dest_folder"));
        if (string.IsNullOrEmpty(destDir)) return;

        // This writes into another platform's save directory, so it names where before
        // it goes ahead.
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("player.transfer_cross_title"),
                UiStrings.Format("player.transfer_cross_confirm",
                    PlatformItems[TransferPlatformIndex], DestSlotIndex + 1, destDir)))
            return;

        var destPlatform = TransferPlatformFromIndex(TransferPlatformIndex);
        try
        {
            SaveSlotManager.TransferCrossPlatform(_saveFilePath, destDir, DestSlotIndex, destPlatform);
            StatusText = UiStrings.Get("player.transfer_cross_complete");
        }
        catch (Exception ex) { StatusText = UiStrings.Format("player.transfer_cross_failed", ex.Message); }
    }

    // --- Guides ---

    public void LoadAccountData(JsonObject accountData)
    {
        _accountData = accountData;
        LoadGuides(accountData);
        LoadTitles(accountData);
    }

    private void LoadGuides(JsonObject accountData)
    {
        GuideTopics.Clear();
        try
        {
            var userData = accountData.GetObject("UserSettingsData");
            if (userData == null) return;

            var seenSet = new HashSet<string>(StringComparer.Ordinal);
            var unlockedSet = new HashSet<string>(StringComparer.Ordinal);

            var seenTopics = userData.GetArray("SeenWikiTopics");
            var unlockedTopics = userData.GetArray("UnlockedWikiTopics");

            if (seenTopics != null)
                for (int i = 0; i < seenTopics.Length; i++)
                    try { seenSet.Add(seenTopics.GetString(i)); } catch { }
            if (unlockedTopics != null)
                for (int i = 0; i < unlockedTopics.Length; i++)
                    try { unlockedSet.Add(unlockedTopics.GetString(i)); } catch { }

            var shown = new HashSet<string>(StringComparer.Ordinal);
            foreach (var topic in WikiGuideDatabase.Topics)
            {
                shown.Add(topic.Id);
                string category = WikiGuideDatabase.GetEnglishCategory(topic.Id);
                GuideTopics.Add(new GuideTopicViewModel
                {
                    TopicId = topic.Id,
                    Name = topic.Name,
                    Category = category,
                    IsSeen = seenSet.Contains(topic.Id),
                    IsUnlocked = unlockedSet.Contains(topic.Id)
                });
            }

            foreach (string topicId in seenSet.Union(unlockedSet))
            {
                if (!shown.Contains(topicId) && !string.IsNullOrEmpty(topicId))
                {
                    shown.Add(topicId);
                    GuideTopics.Add(new GuideTopicViewModel
                    {
                        TopicId = topicId,
                        Name = WikiGuideDatabase.GetTopicName(topicId),
                        Category = WikiGuideDatabase.GetEnglishCategory(topicId),
                        IsSeen = seenSet.Contains(topicId),
                        IsUnlocked = unlockedSet.Contains(topicId)
                    });
                }
            }
        }
        catch { }
    }

    // =================================== Outfits ===================================

    [ObservableProperty] private ObservableCollection<string> _outfits = new();
    [ObservableProperty] private int _selectedOutfitIndex = -1;

    private JsonArray? _outfitArray;

    /// <summary>Lists the player's saved appearances.</summary>
    private void LoadOutfits(JsonObject playerState)
    {
        _outfitArray = playerState.GetArray("Outfits");
        var names = playerState.GetArray("OutfitNames");

        var list = new ObservableCollection<string>();
        if (_outfitArray is not null)
        {
            for (int i = 0; i < _outfitArray.Length; i++)
            {
                var outfit = _outfitArray.GetObject(i);
                if (outfit is null) continue;
                list.Add(OutfitLogic.GetOutfitDisplayName(names, outfit, i));
            }
        }

        Outfits = list;
        if (list.Count > 0 && SelectedOutfitIndex < 0) SelectedOutfitIndex = 0;
    }

    private JsonObject? SelectedOutfit =>
        _outfitArray is not null && SelectedOutfitIndex >= 0 && SelectedOutfitIndex < _outfitArray.Length
            ? _outfitArray.GetObject(SelectedOutfitIndex) : null;

    [RelayCommand]
    private async Task ExportOutfitAsync()
    {
        if (SelectedOutfit is not { } outfit || SaveFilePickerFunc is null) return;

        var config = ExportConfig.Instance;
        string? path = await SaveFilePickerFunc(UiStrings.Get("outfits.export"),
            config.OutfitExt.TrimStart('.'),
            ExportConfig.BuildFileName(config.OutfitTemplate, config.OutfitExt,
                new Dictionary<string, string> { ["player_name"] = SaveName }));
        if (path is null) return;

        try
        {
            OutfitLogic.ExportOutfit(outfit, path);
            StatusText = UiStrings.Get("outfits.export_success");
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("common.export_failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ImportOutfitAsync()
    {
        if (_outfitArray is null || SelectedOutfitIndex < 0 || OpenFilePickerFunc is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("outfits.import"),
            ExportConfig.Instance.OutfitExt);
        if (path is null) return;

        try
        {
            OutfitLogic.ImportOutfit(_outfitArray, SelectedOutfitIndex, path);
            StatusText = UiStrings.Get("outfits.import_success");
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("common.import_failed", ex.Message);
        }
    }

    /// <summary>
    /// Makes the selected outfit the character's current appearance, which is what the
    /// game reads rather than the saved list.
    /// </summary>
    [RelayCommand]
    private async Task CopyOutfitToCurrentAsync()
    {
        if (SelectedOutfit is not { } outfit || _playerState is null) return;

        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("outfits.copy_confirm_title"),
                UiStrings.Get("outfits.copy_confirm_msg")))
            return;

        try
        {
            OutfitLogic.CopyToCustomData(outfit, _playerState);
            StatusText = UiStrings.Get("outfits.copy_success");
        }
        catch (Exception ex)
        {
            StatusText = UiStrings.Format("common.error", ex.Message);
        }
    }

    [RelayCommand]
    private void UnlockAllGuides()
    {
        foreach (var t in GuideTopics) { t.IsSeen = true; t.IsUnlocked = true; }
        SyncGuidesToAccount();
    }

    [RelayCommand]
    private void LockAllGuides()
    {
        foreach (var t in GuideTopics) { t.IsSeen = false; t.IsUnlocked = false; }
        SyncGuidesToAccount();
    }

    public void SyncGuidesToAccount()
    {
        if (_accountData == null) return;
        var userData = _accountData.GetObject("UserSettingsData");
        if (userData == null) return;

        var seenArr = userData.GetArray("SeenWikiTopics");
        var unlockedArr = userData.GetArray("UnlockedWikiTopics");
        if (seenArr == null || unlockedArr == null) return;

        while (seenArr.Length > 0) seenArr.RemoveAt(seenArr.Length - 1);
        while (unlockedArr.Length > 0) unlockedArr.RemoveAt(unlockedArr.Length - 1);

        foreach (var topic in GuideTopics)
        {
            if (topic.IsSeen) seenArr.Add(topic.TopicId);
            if (topic.IsUnlocked) unlockedArr.Add(topic.TopicId);
        }
    }

    // --- Titles ---

    private void LoadTitles(JsonObject accountData)
    {
        TitleRows.Clear();
        if (!TitleDatabase.IsLoaded) return;

        try
        {
            var userData = accountData.GetObject("UserSettingsData") ?? accountData;
            var unlockedTitles = userData.GetArray("UnlockedTitles");
            var unlockedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (unlockedTitles != null)
            {
                for (int i = 0; i < unlockedTitles.Length; i++)
                {
                    string? titleId = ExtractStringValue(unlockedTitles.Get(i));
                    if (!string.IsNullOrEmpty(titleId))
                    {
                        if (titleId.StartsWith('^'))
                            titleId = titleId[1..];
                        unlockedSet.Add(titleId);
                    }
                }
            }

            foreach (var title in TitleDatabase.Titles)
            {
                TitleRows.Add(new TitleRowViewModel
                {
                    TitleId = title.Id,
                    TitleName = string.Format(CultureInfo.InvariantCulture, title.Name, PlayerName),
                    Description = title.UnlockDescription,
                    IsUnlocked = unlockedSet.Contains(title.Id)
                });
            }
        }
        catch { }
    }

    private static string? ExtractStringValue(object? value)
    {
        if (value is string s) return s;
        if (value is BinaryData bin) return Encoding.Latin1.GetString(bin.ToByteArray());
        return value?.ToString();
    }

    [RelayCommand]
    private void UnlockAllTitles()
    {
        foreach (var t in TitleRows) t.IsUnlocked = true;
        SyncTitlesToAccount();
    }

    [RelayCommand]
    private void LockAllTitles()
    {
        foreach (var t in TitleRows) t.IsUnlocked = false;
        SyncTitlesToAccount();
    }

    public void SyncTitlesToAccount()
    {
        if (_accountData == null) return;
        var userData = _accountData.GetObject("UserSettingsData") ?? _accountData;
        var unlockedTitles = userData.GetArray("UnlockedTitles");
        if (unlockedTitles == null) return;

        while (unlockedTitles.Length > 0) unlockedTitles.RemoveAt(unlockedTitles.Length - 1);

        foreach (var row in TitleRows)
        {
            if (row.IsUnlocked)
                unlockedTitles.Add("^" + row.TitleId);
        }
    }
}

public partial class GuideTopicViewModel : ObservableObject
{
    [ObservableProperty] private string _topicId = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _category = "";
    [ObservableProperty] private bool _isSeen;
    [ObservableProperty] private bool _isUnlocked;
}

public partial class TitleRowViewModel : ObservableObject
{
    [ObservableProperty] private string _titleId = "";
    [ObservableProperty] private string _titleName = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private bool _isUnlocked;
}
