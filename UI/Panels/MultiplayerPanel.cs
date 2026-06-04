using System.Globalization;
using NMSE.Core.Utilities;
using NMSE.Models;
using NMSE.UI.Controls;
using NMSE.Data;

namespace NMSE.UI.Panels;

/// <summary>
/// Experimental panel for editing Swarm Update multiplayer co-op save keys.
/// All keys reside under CommonStateData.SeasonData.
/// </summary>
public partial class MultiplayerPanel : UserControl
{
    private JsonObject? _saveData;
    private JsonObject? _seasonData;
    private bool _loading;
    private readonly Random _rng = new();
    private readonly Dictionary<string, object?> _originalValues = new();

    internal static readonly string[] AlienRaces =
        ["None", "Traders", "Warriors", "Explorers", "Robots", "Atlas", "Diplomats", "Exotics", "Builders"];

    internal static readonly string[] CommunityTeams = ["Red", "Green", "Blue"];

    /// <summary>Raised whenever the user modifies a field.</summary>
    public event EventHandler? DataModified;

    private void RaiseDataModified()
    {
        if (!_loading)
            DataModified?.Invoke(this, EventArgs.Empty);
    }

    public MultiplayerPanel()
    {
        InitializeComponent();
        SetupLayout();
    }

    private JsonObject? GetSeasonData()
    {
        if (_saveData == null) return null;
        return _saveData.GetObject("CommonStateData")?.GetObject("SeasonData");
    }

    // --- Load helpers ---

    private void LoadString(string key, TextBox field)
    {
        var val = _seasonData?.GetString(key);
        _originalValues[key] = val;
        field.Text = val ?? "";
    }

    private void LoadBool(string key, CheckBox field)
    {
        bool val = false;
        if (_seasonData != null && _seasonData.Contains(key))
        {
            try { val = _seasonData.GetBool(key); } catch { }
        }
        _originalValues[key] = val;
        field.Checked = val;
    }

    private void LoadBoolSeed(string key, CheckBox activeCheck, TextBox seedField)
    {
        bool active = false;
        string seed = "";
        var arr = _seasonData?.GetArray(key);
        if (arr != null && arr.Length >= 2)
        {
            try { active = arr.GetBool(0); } catch { }
            seed = arr.Get(1)?.ToString() ?? "";
        }
        _originalValues[key + ".active"] = active;
        _originalValues[key + ".seed"] = seed;
        activeCheck.Checked = active;
        seedField.Text = seed;
    }

    private void LoadTeamSeed(int index, CheckBox activeCheck, TextBox seedField)
    {
        bool active = false;
        string seed = "";
        var arr = _seasonData?.GetArray("TeamShipSeeds");
        if (arr != null && index < arr.Length)
        {
            var entry = arr.GetArray(index);
            if (entry != null && entry.Length >= 2)
            {
                try { active = entry.GetBool(0); } catch { }
                seed = entry.Get(1)?.ToString() ?? "";
            }
        }
        var dk = $"TeamShipSeeds.{index}";
        _originalValues[dk + ".active"] = active;
        _originalValues[dk + ".seed"] = seed;
        activeCheck.Checked = active;
        seedField.Text = seed;
    }

    private void LoadEnumObject(string key, string subKey, ComboBox combo, string[] values)
    {
        string currentVal = "";
        var obj = _seasonData?.GetObject(key);
        if (obj != null)
        {
            try { currentVal = obj.GetString(subKey) ?? ""; } catch { }
        }
        _originalValues[key] = currentVal;

        int idx = -1;
        for (int i = 0; i < values.Length; i++)
        {
            if (string.Equals(values[i], currentVal, StringComparison.OrdinalIgnoreCase))
            {
                idx = i;
                break;
            }
        }
        combo.SelectedIndex = idx;
    }

    // --- Save helpers ---

    private static void SaveString(JsonObject season, string key, string newVal, string? orig)
    {
        if (newVal == orig) return;
        if (string.IsNullOrEmpty(newVal) && string.IsNullOrEmpty(orig)) return;
        season.Set(key, newVal);
    }

    private static void SaveBool(JsonObject season, string key, bool newVal, bool? orig)
    {
        if (orig.HasValue && newVal == orig.Value) return;
        season.Set(key, newVal);
    }

    private static void SaveBoolSeed(JsonObject season, string key, bool newActive, string newSeed, object? origActive, object? origSeed)
    {
        bool oldActive = origActive as bool? ?? false;
        string? oldSeed = origSeed as string;
        if (newActive == oldActive && newSeed == oldSeed) return;

        var arr = season.GetArray(key);
        if (arr != null && arr.Length >= 2)
        {
            if (newActive != oldActive) arr.Set(0, newActive);
            var normalized = SeedHelper.NormalizeSeed(newSeed);
            if (normalized != null && newSeed != oldSeed) arr.Set(1, normalized);
        }
    }

    private static void SaveTeamSeed(JsonObject season, int index, bool newActive, string newSeed, object? origActive, object? origSeed)
    {
        bool oldActive = origActive as bool? ?? false;
        string? oldSeed = origSeed as string;
        if (newActive == oldActive && newSeed == oldSeed) return;

        var arr = season.GetArray("TeamShipSeeds");
        if (arr != null && index < arr.Length)
        {
            var entry = arr.GetArray(index);
            if (entry != null && entry.Length >= 2)
            {
                if (newActive != oldActive) entry.Set(0, newActive);
                var normalized = SeedHelper.NormalizeSeed(newSeed);
                if (normalized != null && newSeed != oldSeed) entry.Set(1, normalized);
            }
        }
    }

    private static void SaveEnumObject(JsonObject season, string key, string subKey, string newVal, object? orig)
    {
        string? oldVal = orig as string;
        if (newVal == oldVal) return;

        var obj = season.GetObject(key);
        obj?.Set(subKey, newVal);
    }

    // --- Public API ---

    public void LoadData(JsonObject saveData)
    {
        _saveData = saveData;
        _seasonData = GetSeasonData();
        _loading = true;
        SuspendLayout();
        try
        {
            _originalValues.Clear();

            // Team Configuration
            LoadEnumObject("CachedPlayerCommunityTeam", "CommunityTeam", _cachedTeamCombo, CommunityTeams);
            LoadBool("UseTeamShipSeeds", _useTeamShipSeedsCheck);
            LoadBool("UseTeamShipPalettes", _useTeamShipPalettesCheck);
            LoadBool("UseCommunityTeamPalettes", _useCommunityTeamPalettesCheck);
            LoadTeamSeed(0, _teamSeed1ActiveCheck, _teamSeed1Field);
            LoadTeamSeed(1, _teamSeed2ActiveCheck, _teamSeed2Field);
            LoadTeamSeed(2, _teamSeed3ActiveCheck, _teamSeed3Field);

            // Purchase & Transfer Restrictions
            LoadBool("NeverAllowShipPurchases", _neverAllowShipPurchasesCheck);
            LoadBool("AllowOnlyCorvetteShipPurchases", _allowOnlyCorvetteCheck);
            LoadBoolSeed("BlockShipPurchasesUntilMilestoneWithSeedComplete", _blockShipPurchasesActiveCheck, _blockShipPurchasesHexField);
            LoadBoolSeed("BlockShipRepairUntilMilestoneWithSeedComplete", _blockShipRepairActiveCheck, _blockShipRepairHexField);
            LoadBool("NeverAllowCorvettePurchases", _neverAllowCorvetteCheck);
            LoadBool("AllowSaveContextCorvetteTransfer", _allowCorvetteTransferCheck);
            LoadBool("AllowSaveContextShipTransfer", _allowShipTransferCheck);
            LoadBool("AllowSaveContextMultitoolTransfer", _allowMultitoolTransferCheck);
            LoadBool("OnlyCorvettesSpawnWhenPlayerTeleports", _onlyCorvettesSpawnCheck);
            LoadBool("OnlyCorvetteLauncherCanBeRepaired", _onlyCorvetteLauncherCheck);

            // Player Setup
            LoadEnumObject("ForcePlayerRace", "AlienRace", _forceRaceCombo, AlienRaces);
            LoadBoolSeed("WeaponSeed", _weaponSeedActiveCheck, _weaponSeedField);
            LoadBoolSeed("ShipSeed", _shipSeedActiveCheck, _shipSeedField);
            LoadString("PersistentPOI", _persistentPoiField);
            LoadString("IntroSequencePOI", _introSequencePoiField);
            LoadString("StartWithIntroQuizID", _startQuizIdField);
        }
        finally
        {
            _loading = false;
            ResumeLayout(true);
        }
    }

    public void SaveData(JsonObject saveData)
    {
        var season = saveData.GetObject("CommonStateData")?.GetObject("SeasonData");
        if (season == null) return;

        // Team Configuration
        SaveEnumObject(season, "CachedPlayerCommunityTeam", "CommunityTeam", GetSelectedCommunityTeam(), _originalValues["CachedPlayerCommunityTeam"]);
        SaveBool(season, "UseTeamShipSeeds", _useTeamShipSeedsCheck.Checked, _originalValues["UseTeamShipSeeds"] as bool?);
        SaveBool(season, "UseTeamShipPalettes", _useTeamShipPalettesCheck.Checked, _originalValues["UseTeamShipPalettes"] as bool?);
        SaveBool(season, "UseCommunityTeamPalettes", _useCommunityTeamPalettesCheck.Checked, _originalValues["UseCommunityTeamPalettes"] as bool?);
        SaveTeamSeed(season, 0, _teamSeed1ActiveCheck.Checked, _teamSeed1Field.Text, _originalValues["TeamShipSeeds.0.active"], _originalValues["TeamShipSeeds.0.seed"]);
        SaveTeamSeed(season, 1, _teamSeed2ActiveCheck.Checked, _teamSeed2Field.Text, _originalValues["TeamShipSeeds.1.active"], _originalValues["TeamShipSeeds.1.seed"]);
        SaveTeamSeed(season, 2, _teamSeed3ActiveCheck.Checked, _teamSeed3Field.Text, _originalValues["TeamShipSeeds.2.active"], _originalValues["TeamShipSeeds.2.seed"]);

        // Purchase & Transfer Restrictions
        SaveBool(season, "NeverAllowShipPurchases", _neverAllowShipPurchasesCheck.Checked, _originalValues["NeverAllowShipPurchases"] as bool?);
        SaveBool(season, "AllowOnlyCorvetteShipPurchases", _allowOnlyCorvetteCheck.Checked, _originalValues["AllowOnlyCorvetteShipPurchases"] as bool?);
        SaveBoolSeed(season, "BlockShipPurchasesUntilMilestoneWithSeedComplete", _blockShipPurchasesActiveCheck.Checked, _blockShipPurchasesHexField.Text, _originalValues["BlockShipPurchasesUntilMilestoneWithSeedComplete.active"], _originalValues["BlockShipPurchasesUntilMilestoneWithSeedComplete.seed"]);
        SaveBoolSeed(season, "BlockShipRepairUntilMilestoneWithSeedComplete", _blockShipRepairActiveCheck.Checked, _blockShipRepairHexField.Text, _originalValues["BlockShipRepairUntilMilestoneWithSeedComplete.active"], _originalValues["BlockShipRepairUntilMilestoneWithSeedComplete.seed"]);
        SaveBool(season, "NeverAllowCorvettePurchases", _neverAllowCorvetteCheck.Checked, _originalValues["NeverAllowCorvettePurchases"] as bool?);
        SaveBool(season, "AllowSaveContextCorvetteTransfer", _allowCorvetteTransferCheck.Checked, _originalValues["AllowSaveContextCorvetteTransfer"] as bool?);
        SaveBool(season, "AllowSaveContextShipTransfer", _allowShipTransferCheck.Checked, _originalValues["AllowSaveContextShipTransfer"] as bool?);
        SaveBool(season, "AllowSaveContextMultitoolTransfer", _allowMultitoolTransferCheck.Checked, _originalValues["AllowSaveContextMultitoolTransfer"] as bool?);
        SaveBool(season, "OnlyCorvettesSpawnWhenPlayerTeleports", _onlyCorvettesSpawnCheck.Checked, _originalValues["OnlyCorvettesSpawnWhenPlayerTeleports"] as bool?);
        SaveBool(season, "OnlyCorvetteLauncherCanBeRepaired", _onlyCorvetteLauncherCheck.Checked, _originalValues["OnlyCorvetteLauncherCanBeRepaired"] as bool?);

        // Player Setup
        SaveEnumObject(season, "ForcePlayerRace", "AlienRace", GetSelectedAlienRace(), _originalValues["ForcePlayerRace"]);
        SaveBoolSeed(season, "WeaponSeed", _weaponSeedActiveCheck.Checked, _weaponSeedField.Text, _originalValues["WeaponSeed.active"], _originalValues["WeaponSeed.seed"]);
        SaveBoolSeed(season, "ShipSeed", _shipSeedActiveCheck.Checked, _shipSeedField.Text, _originalValues["ShipSeed.active"], _originalValues["ShipSeed.seed"]);
        SaveString(season, "PersistentPOI", _persistentPoiField.Text, _originalValues["PersistentPOI"] as string);
        SaveString(season, "IntroSequencePOI", _introSequencePoiField.Text, _originalValues["IntroSequencePOI"] as string);
        SaveString(season, "StartWithIntroQuizID", _startQuizIdField.Text, _originalValues["StartWithIntroQuizID"] as string);
    }

    private string GetSelectedAlienRace()
    {
        int idx = _forceRaceCombo.SelectedIndex;
        return idx >= 0 && idx < AlienRaces.Length ? AlienRaces[idx] : "None";
    }

    private string GetSelectedCommunityTeam()
    {
        int idx = _cachedTeamCombo.SelectedIndex;
        return idx >= 0 && idx < CommunityTeams.Length ? CommunityTeams[idx] : "";
    }

    public void ApplyUiLocalisation()
    {
        _experimentalWarningLabel.Text = UiStrings.Get("multiplayer.experimental_warning");
        _enforcedWarningLabel.Text = UiStrings.Get("multiplayer.enforced_warning");
        _confirmCheck.Text = UiStrings.Get("multiplayer.confirm_understanding");

        // Team Configuration
        _teamConfigHeader.Text = UiStrings.Get("multiplayer.section_team_configuration");
        _cachedTeamLabel.Text = UiStrings.Get("multiplayer.cached_player_community_team");
        _useTeamShipSeedsCheck.Text = UiStrings.Get("multiplayer.use_team_ship_seeds");
        _useTeamShipPalettesCheck.Text = UiStrings.Get("multiplayer.use_team_ship_palettes");
        _useCommunityTeamPalettesCheck.Text = UiStrings.Get("multiplayer.use_community_team_palettes");
        _teamSeed1Label.Text = UiStrings.Get("multiplayer.team_ship_seed_1");
        _teamSeed2Label.Text = UiStrings.Get("multiplayer.team_ship_seed_2");
        _teamSeed3Label.Text = UiStrings.Get("multiplayer.team_ship_seed_3");
        _teamSeed1ActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _teamSeed2ActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _teamSeed3ActiveCheck.Text = UiStrings.Get("multiplayer.active");

        // Purchase & Transfer Restrictions
        _purchaseHeader.Text = UiStrings.Get("multiplayer.section_purchase_restrictions");
        _neverAllowShipPurchasesCheck.Text = UiStrings.Get("multiplayer.never_allow_ship_purchases");
        _allowOnlyCorvetteCheck.Text = UiStrings.Get("multiplayer.allow_only_corvette_ship_purchases");
        _blockShipPurchasesLabel.Text = UiStrings.Get("multiplayer.block_ship_purchases_until_milestone");
        _blockShipPurchasesActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _blockShipRepairLabel.Text = UiStrings.Get("multiplayer.block_ship_repair_until_milestone");
        _blockShipRepairActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _neverAllowCorvetteCheck.Text = UiStrings.Get("multiplayer.never_allow_corvette_purchases");
        _allowCorvetteTransferCheck.Text = UiStrings.Get("multiplayer.allow_save_context_corvette_transfer");
        _allowShipTransferCheck.Text = UiStrings.Get("multiplayer.allow_save_context_ship_transfer");
        _allowMultitoolTransferCheck.Text = UiStrings.Get("multiplayer.allow_save_context_multitool_transfer");
        _onlyCorvettesSpawnCheck.Text = UiStrings.Get("multiplayer.only_corvettes_spawn_when_player_teleports");
        _onlyCorvetteLauncherCheck.Text = UiStrings.Get("multiplayer.only_corvette_launcher_can_be_repaired");

        // Player Setup
        _playerSetupHeader.Text = UiStrings.Get("multiplayer.section_player_setup");
        _forceRaceLabel.Text = UiStrings.Get("multiplayer.force_player_race");
        _weaponSeedLabel.Text = UiStrings.Get("multiplayer.weapon_seed");
        _weaponSeedActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _shipSeedLabel.Text = UiStrings.Get("multiplayer.ship_seed");
        _shipSeedActiveCheck.Text = UiStrings.Get("multiplayer.active");
        _persistentPoiLabel.Text = UiStrings.Get("multiplayer.persistent_poi");
        _introSequencePoiLabel.Text = UiStrings.Get("multiplayer.intro_sequence_poi");
        _startQuizIdLabel.Text = UiStrings.Get("multiplayer.start_with_intro_quiz_id");

        RefreshAlienRaceCombo();
        RefreshCommunityTeamCombo();
    }

    private void RefreshAlienRaceCombo()
    {
        string current = GetSelectedAlienRace();
        _forceRaceCombo.BeginUpdate();
        _forceRaceCombo.Items.Clear();
        foreach (var race in AlienRaces)
            _forceRaceCombo.Items.Add(race);
        int idx = IndexOf(AlienRaces, current, StringComparer.OrdinalIgnoreCase);
        _forceRaceCombo.SelectedIndex = idx >= 0 ? idx : -1;
        _forceRaceCombo.EndUpdate();
    }

    private void RefreshCommunityTeamCombo()
    {
        string current = GetSelectedCommunityTeam();
        _cachedTeamCombo.BeginUpdate();
        _cachedTeamCombo.Items.Clear();
        foreach (var team in CommunityTeams)
            _cachedTeamCombo.Items.Add(team);
        int idx = IndexOf(CommunityTeams, current, StringComparer.OrdinalIgnoreCase);
        _cachedTeamCombo.SelectedIndex = idx >= 0 ? idx : -1;
        _cachedTeamCombo.EndUpdate();
    }

    private static int IndexOf(string[] arr, string value, StringComparer comparer)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (comparer.Equals(arr[i], value))
                return i;
        }
        return -1;
    }
}
