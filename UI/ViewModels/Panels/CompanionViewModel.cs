using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.Core.Utilities;
using System.Globalization;

namespace NMSE.UI.ViewModels.Panels;

public partial class CompanionViewModel : PanelViewModelBase
{
    private JsonObject? _playerState;
    private JsonObject? _saveData;
    private GameItemDatabase? _database;
    private IconManager? _iconManager;

    [ObservableProperty] private ObservableCollection<CompanionEntryViewModel> _companions = new();
    [ObservableProperty] private CompanionEntryViewModel? _selectedCompanion;
    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private string _countLabel = "";

    [ObservableProperty] private string _companionName = "";
    [ObservableProperty] private string _creatureId = "";
    [ObservableProperty] private string _creatureSeed = "";
    [ObservableProperty] private string _secondarySeed = "";
    [ObservableProperty] private string _speciesSeed = "";
    [ObservableProperty] private string _genusSeed = "";
    [ObservableProperty] private string _biome = "";
    [ObservableProperty] private string _creatureType = "";
    [ObservableProperty] private bool _predator;
    [ObservableProperty] private string _scale = "";
    [ObservableProperty] private string _trust = "";
    [ObservableProperty] private bool _hasFur;
    [ObservableProperty] private string _helpfulness = "0";
    [ObservableProperty] private string _aggression = "0";
    [ObservableProperty] private string _independence = "0";
    [ObservableProperty] private string _hungry = "0";
    [ObservableProperty] private string _lonely = "0";
    [ObservableProperty] private string _customSpeciesName = "";
    [ObservableProperty] private bool _eggModified;
    [ObservableProperty] private bool _hasBeenSummoned;
    [ObservableProperty] private bool _allowUnmodifiedReroll;

    /// <summary>Whether the pet slot this companion sits in has been bought.</summary>
    [ObservableProperty] private bool _slotUnlocked;

    // Timestamps the game keeps for a companion, edited as dates rather than raw
    // Unix seconds.
    [ObservableProperty] private DateTimeOffset _birthTime = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset _lastEggTime = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset _lastTrustIncreaseTime = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset _lastTrustDecreaseTime = DateTimeOffset.Now;

    /// <summary>
    /// The UA value, which some saves store as a hex string and others as a number. The
    /// checkbox records which, so a round trip writes back the form it found.
    /// </summary>
    [ObservableProperty] private string _ua = "0";
    [ObservableProperty] private bool _uaIsHex;

    // Battle affinity, and which affinities it beats and loses to.
    [ObservableProperty] private string _battleAffinity = "";
    [ObservableProperty] private string _battleWeak = "";
    [ObservableProperty] private string _battleStrong = "";

    /// <summary>Shown in place of the accessory list when the creature has no slots.</summary>
    [ObservableProperty] private string _accessoryNote = "";

    /// <summary>The species this creature id resolves to, or the id marked unrecognised.</summary>
    [ObservableProperty] private string _species = "";
    [ObservableProperty] private string _boneScaleSeed = "";
    [ObservableProperty] private string _colourBaseSeed = "";

    // --- Pet battle ------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<BattleMoveSlotViewModel> _moveSlots = new();
    [ObservableProperty] private ObservableCollection<BattleTeamSlotViewModel> _teamSlots = new();
    [ObservableProperty] private ObservableCollection<string> _statClasses = new(CompanionBattleIo.StatClasses);
    [ObservableProperty] private bool _useStatOverrides;
    [ObservableProperty] private int _healthClassIndex = 3;
    [ObservableProperty] private int _agilityClassIndex = 3;
    [ObservableProperty] private int _combatClassIndex = 3;
    [ObservableProperty] private string _averageClass = "C";
    [ObservableProperty] private int _treatsHealth;
    [ObservableProperty] private int _treatsAgility;
    [ObservableProperty] private int _treatsCombat;
    [ObservableProperty] private int _genesAvailable;
    [ObservableProperty] private string _genesLevel = "";
    [ObservableProperty] private double _mutationProgress;
    [ObservableProperty] private int _victories;

    // --- Accessories -----------------------------------------------------------
    [ObservableProperty] private ObservableCollection<AccessorySlotViewModel> _accessorySlots = new();
    [ObservableProperty] private bool _hasAccessories;

    // --- Slots -----------------------------------------------------------------
    [ObservableProperty] private string _totalSlotsLabel = "";

    /// <summary>Reads a Unix timestamp, falling back to now when the save has none.</summary>
    private static DateTimeOffset ReadTime(JsonObject companion, string key)
    {
        try { return DateTimeOffset.FromUnixTimeSeconds(companion.GetLong(key)).ToLocalTime(); }
        catch { return DateTimeOffset.Now; }
    }

    private static void WriteTime(JsonObject companion, string key, DateTimeOffset value)
    {
        try { companion.Set(key, value.ToUnixTimeSeconds()); } catch { }
    }

    /// <summary>Whether the given pet slot has been bought.</summary>
    private bool IsSlotUnlocked(int slotIndex)
    {
        try
        {
            var slots = _playerState?.GetArray("UnlockedPetSlots");
            return slots is not null && slotIndex >= 0 && slotIndex < slots.Length
                && slots.GetBool(slotIndex);
        }
        catch { return false; }
    }

    // ============================== Creature builder ===============================

    /// <summary>The part groups for the selected creature, in the order they apply.</summary>
    public ObservableCollection<DescriptorGroupViewModel> DescriptorGroups { get; } = new();

    /// <summary>Shown when the creature type has no part data to offer.</summary>
    [ObservableProperty] private string _descriptorSummary = "";
    [ObservableProperty] private bool _hasDescriptorGroups;

    /// <summary>
    /// Rebuilds the part pickers. Choosing a part can expose further groups beneath it,
    /// so this runs again after every change rather than once when the creature loads.
    /// </summary>
    private void LoadDescriptors(JsonObject companion)
    {
        DescriptorGroups.Clear();

        var current = CompanionDescriptorIo.Read(companion);
        var entry = CreaturePartDatabase.GetForCreatureId(companion.GetString("CreatureID"));

        if (entry is null)
        {
            HasDescriptorGroups = false;
            DescriptorSummary = current.Count > 0
                ? UiStrings.Format("companion.raw_descriptors", current.Count) + string.Join(", ", current)
                : UiStrings.Get("companion.no_part_data");
            return;
        }

        HasDescriptorGroups = true;
        DescriptorSummary = "";

        foreach (var group in CreaturePartDatabase.GetFlatGroups(entry, current))
        {
            var vm = new DescriptorGroupViewModel(group, current);
            vm.SelectionChanged += _ => WriteDescriptors();
            DescriptorGroups.Add(vm);
        }
    }

    /// <summary>
    /// Writes the chosen parts back and rebuilds the groups, since the new selection may
    /// expose or hide others.
    /// </summary>
    private void WriteDescriptors()
    {
        var comp = SelectedCompanion?.CompanionData;
        if (comp is null) return;

        CompanionDescriptorIo.Write(comp,
            DescriptorGroups.Select(g => g.SelectedId).OfType<string>());

        LoadDescriptors(comp);
    }

    /// <summary>
    /// Rolls a new descriptor id without touching the parts, which is what makes the
    /// game re-derive the creature from them.
    /// </summary>
    [RelayCommand]
    private void RegenerateDescriptorId() => WriteDescriptors();

    /// <summary>Opens the community Creature Builder, which edits the same descriptors.</summary>
    [RelayCommand]
    private void OpenCreatureBuilder()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = CreatureBuilderUrl,
                UseShellExecute = true,
            });
        }
        catch { }
    }

    private const string CreatureBuilderUrl = "https://nmscd.com/CreatureBuilder/";

    partial void OnSelectedCompanionChanged(CompanionEntryViewModel? value)
    {
        HasSelection = value != null;
        if (value != null) LoadCompanionDetails(value);
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _saveData = saveData;
        _database = database;
        _iconManager = iconManager;

        try
        {
            Companions.Clear();
            _playerState = null;

            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;
            _playerState = playerState;

            LoadSlots(playerState.GetArray("Pets"), "Pet");
            LoadSlots(playerState.GetArray("Eggs"), "Egg");

            CountLabel = UiStrings.Format("companion.total_slots", Companions.Count);

            if (Companions.Count > 0)
                SelectedCompanion = Companions[0];
        }
        catch { }
    }

    private void LoadSlots(JsonArray? array, string prefix)
    {
        if (array == null) return;
        for (int i = 0; i < array.Length; i++)
        {
            try
            {
                var comp = array.GetObject(i);
                bool occupied = false;
                try
                {
                    var seedArr = comp.GetArray("CreatureSeed");
                    if (seedArr != null && seedArr.Length >= 2)
                        occupied = seedArr.GetBool(0);
                }
                catch { }

                string customName = "";
                try { customName = comp.GetString("CustomName") ?? ""; } catch { }

                string label;
                if (!occupied)
                    label = $"{prefix} {i} (Empty)";
                else if (string.IsNullOrEmpty(customName) || customName == "^")
                    label = $"{prefix} {i}";
                else
                    label = $"{prefix} {i} - {customName}";

                Companions.Add(new CompanionEntryViewModel
                {
                    Label = label,
                    CompanionData = comp,
                    Source = prefix,
                    OriginalIndex = i,
                    IsOccupied = occupied
                });
            }
            catch
            {
                // A slot the parser cannot read still occupies an index, so it is listed
                // rather than skipped, which would shift every slot after it.
                string errLabel = UiStrings.Format("companion.error_format", prefix, i);
                Companions.Add(new CompanionEntryViewModel
                {
                    Label = errLabel,
                    Source = prefix,
                    OriginalIndex = i,
                    IsOccupied = true,
                });
            }
        }
    }

    private void LoadCompanionDetails(CompanionEntryViewModel entry)
    {
        LoadAccessories(entry);
        if (entry.CompanionData is { } battleSource) LoadBattle(battleSource);

        try
        {
            var comp = entry.CompanionData;
            if (comp == null) return;

            CreatureId = comp.GetString("CreatureID") ?? "";
            CompanionName = comp.GetString("CustomName") ?? "";
            LoadDescriptors(comp);

            // A creature the databases do not know still has an id worth showing, marked
            // so it is not mistaken for a recognised species.
            string stripped = CreatureId.TrimStart('^');
            var known = CompanionDatabase.Entries.FirstOrDefault(e =>
                string.Equals(e.Id.TrimStart('^'), stripped, StringComparison.OrdinalIgnoreCase));
            Species = known is not null
                ? known.Species
                : string.IsNullOrEmpty(stripped)
                    ? ""
                    : UiStrings.Format("companion.unrecognised_species", stripped);

            try
            {
                var seedArr = comp.GetArray("CreatureSeed");
                CreatureSeed = seedArr != null && seedArr.Length >= 2 ? seedArr.GetString(1) ?? "" : "";
            }
            catch { CreatureSeed = ""; }

            try
            {
                var secArr = comp.GetArray("CreatureSecondarySeed");
                SecondarySeed = secArr != null && secArr.Length >= 2 ? secArr.GetString(1) ?? "" : "";
            }
            catch { SecondarySeed = ""; }

            SpeciesSeed = comp.GetString("SpeciesSeed") ?? "";
            GenusSeed = comp.GetString("GenusSeed") ?? "";

            try { Predator = comp.GetBool("Predator"); } catch { Predator = false; }

            try
            {
                var biomeObj = comp.GetObject("Biome");
                Biome = biomeObj?.GetString("Biome") ?? "";
            }
            catch { Biome = ""; }

            try
            {
                var ctObj = comp.GetObject("CreatureType");
                CreatureType = ctObj?.GetString("CreatureType") ?? "";
            }
            catch { CreatureType = ""; }

            try { Scale = comp.GetDouble("Scale").ToString("G17", CultureInfo.InvariantCulture); } catch { Scale = ""; }
            try { Trust = comp.GetDouble("Trust").ToString("G17", CultureInfo.InvariantCulture); } catch { Trust = ""; }
            try { HasFur = comp.GetBool("HasFur"); } catch { HasFur = false; }

            try
            {
                var traits = comp.GetArray("Traits");
                Helpfulness = traits != null && traits.Length > 0 ? traits.GetDouble(0).ToString("G17", CultureInfo.InvariantCulture) : "0";
                Aggression = traits != null && traits.Length > 1 ? traits.GetDouble(1).ToString("G17", CultureInfo.InvariantCulture) : "0";
                Independence = traits != null && traits.Length > 2 ? traits.GetDouble(2).ToString("G17", CultureInfo.InvariantCulture) : "0";
            }
            catch { Helpfulness = "0"; Aggression = "0"; Independence = "0"; }

            try
            {
                var moods = comp.GetArray("Moods");
                Hungry = moods != null && moods.Length > 0 ? moods.GetDouble(0).ToString("G17", CultureInfo.InvariantCulture) : "0";
                Lonely = moods != null && moods.Length > 1 ? moods.GetDouble(1).ToString("G17", CultureInfo.InvariantCulture) : "0";
            }
            catch { Hungry = "0"; Lonely = "0"; }

            try
            {
                string csn = comp.GetString("CustomSpeciesName") ?? "";
                CustomSpeciesName = csn == "^" ? "" : csn.TrimStart('^');
            }
            catch { CustomSpeciesName = ""; }

            try { EggModified = comp.GetBool("EggModified"); } catch { EggModified = false; }
            try { HasBeenSummoned = comp.GetBool("HasBeenSummoned"); } catch { HasBeenSummoned = false; }
            try { AllowUnmodifiedReroll = comp.GetBool("AllowUnmodifiedReroll"); } catch { AllowUnmodifiedReroll = false; }

            BirthTime = ReadTime(comp, "BirthTime");
            LastEggTime = ReadTime(comp, "LastEggTime");
            LastTrustIncreaseTime = ReadTime(comp, "LastTrustIncreaseTime");
            LastTrustDecreaseTime = ReadTime(comp, "LastTrustDecreaseTime");

            // UA is a hex string in some saves and a number in others.
            try
            {
                if (comp.GetValue("UA") is string hex
                    && hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    UaIsHex = true;
                    Ua = hex;
                }
                else
                {
                    UaIsHex = false;
                    Ua = comp.GetLong("UA").ToString(CultureInfo.InvariantCulture);
                }
            }
            catch { UaIsHex = false; Ua = "0"; }

            SlotUnlocked = IsSlotUnlocked(entry.OriginalIndex);

            try
            {
                var bsArr = comp.GetArray("BoneScaleSeed");
                BoneScaleSeed = bsArr != null && bsArr.Length >= 2 ? bsArr.GetString(1) ?? "" : "";
            }
            catch { BoneScaleSeed = ""; }

            try
            {
                var cbArr = comp.GetArray("ColourBaseSeed");
                ColourBaseSeed = cbArr != null && cbArr.Length >= 2 ? cbArr.GetString(1) ?? "" : "";
            }
            catch { ColourBaseSeed = ""; }
        }
        catch { }
    }

    [RelayCommand]
    private void SaveCompanionChanges()
    {
        if (SelectedCompanion?.CompanionData == null) return;
        var comp = SelectedCompanion.CompanionData;

        SaveBattle(comp);
        SaveAccessories(SelectedCompanion);

        comp.Set("CustomName", CompanionName);
        comp.Set("CreatureID", CreatureId);
        comp.Set("Predator", Predator);
        comp.Set("HasFur", HasFur);
        comp.Set("EggModified", EggModified);
        comp.Set("HasBeenSummoned", HasBeenSummoned);
        comp.Set("AllowUnmodifiedReroll", AllowUnmodifiedReroll);

        WriteTime(comp, "BirthTime", BirthTime);
        WriteTime(comp, "LastEggTime", LastEggTime);
        WriteTime(comp, "LastTrustIncreaseTime", LastTrustIncreaseTime);
        WriteTime(comp, "LastTrustDecreaseTime", LastTrustDecreaseTime);

        // Written back in whichever form the save used.
        try
        {
            if (UaIsHex) comp.Set("UA", Ua);
            else if (long.TryParse(Ua, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ua))
                comp.Set("UA", ua);
        }
        catch { }

        if (_playerState is not null && SelectedCompanion is { } sel)
            CompanionLogic.SetSlotUnlocked(_playerState, sel.OriginalIndex, SlotUnlocked);

        if (NumericParseHelper.TryParseDouble(Scale, out double scaleVal)) comp.Set("Scale", scaleVal);
        if (NumericParseHelper.TryParseDouble(Trust, out double trustVal)) comp.Set("Trust", trustVal);

        var csn = string.IsNullOrEmpty(CustomSpeciesName) ? "^" : $"^{CustomSpeciesName.TrimStart('^')}";
        comp.Set("CustomSpeciesName", csn);

        try
        {
            var traits = comp.GetArray("Traits");
            if (traits != null)
            {
                if (NumericParseHelper.TryParseDouble(Helpfulness, out double h) && traits.Length > 0) traits.Set(0, h);
                if (NumericParseHelper.TryParseDouble(Aggression, out double a) && traits.Length > 1) traits.Set(1, a);
                if (NumericParseHelper.TryParseDouble(Independence, out double ind) && traits.Length > 2) traits.Set(2, ind);
            }
        }
        catch { }

        try
        {
            var moods = comp.GetArray("Moods");
            if (moods != null)
            {
                if (NumericParseHelper.TryParseDouble(Hungry, out double hu) && moods.Length > 0) moods.Set(0, hu);
                if (NumericParseHelper.TryParseDouble(Lonely, out double lo) && moods.Length > 1) moods.Set(1, lo);
            }
        }
        catch { }

        WriteSeed(comp, "CreatureSeed", CreatureSeed);
        WriteSeed(comp, "CreatureSecondarySeed", SecondarySeed);
        WriteSeed(comp, "BoneScaleSeed", BoneScaleSeed);
        WriteSeed(comp, "ColourBaseSeed", ColourBaseSeed);

        var normalized = SeedHelper.NormalizeSeed(SpeciesSeed);
        if (normalized != null) comp.Set("SpeciesSeed", normalized);
        normalized = SeedHelper.NormalizeSeed(GenusSeed);
        if (normalized != null) comp.Set("GenusSeed", normalized);
    }

    private static void WriteSeed(JsonObject comp, string key, string value)
    {
        var normalized = SeedHelper.NormalizeSeed(value);
        var arr = comp.GetArray(key);
        if (arr != null && arr.Length >= 2)
        {
            bool hasValue = normalized != null && normalized != "0x0";
            arr.Set(0, hasValue);
            arr.Set(1, hasValue ? normalized! : "0x0");
        }
    }

    [RelayCommand]
    private void GenerateSeed(string fieldName)
    {
        byte[] bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        string seed = "0x" + BitConverter.ToString(bytes).Replace("-", "");

        switch (fieldName)
        {
            case "Creature": CreatureSeed = seed; break;
            case "Secondary": SecondarySeed = seed; break;
            case "Species": SpeciesSeed = seed; break;
            case "Genus": GenusSeed = seed; break;
            case "BoneScale": BoneScaleSeed = seed; break;
            case "ColourBase": ColourBaseSeed = seed; break;
        }
    }

    [RelayCommand]
    private async Task DeleteCompanionAsync()
    {
        if (Dialogs is not null &&
            !await Dialogs.ConfirmAsync(UiStrings.Get("companion.delete_title"),
                UiStrings.Get("companion.delete_confirm"), Services.DialogIcon.Warning))
            return;

        if (SelectedCompanion?.CompanionData == null) return;
        CompanionLogic.DeleteCompanion(SelectedCompanion.CompanionData);
        SelectedCompanion.IsOccupied = false;
        SelectedCompanion.Label = $"{SelectedCompanion.Source} {SelectedCompanion.OriginalIndex} (Empty)";
        HasSelection = false;
    }

    // ================================= Pet battle ==================================

    /// <summary>Populates the battle tab from the selected companion.</summary>
    private void LoadBattle(JsonObject companion)
    {
        // The move a slot accepts is fixed by the movesets, so the option lists are
        // built once per selection rather than per companion field.
        var slots = new ObservableCollection<BattleMoveSlotViewModel>();
        for (int i = 0; i < 5; i++)
            slots.Add(new BattleMoveSlotViewModel(i + 1, CompanionBattleIo.AllowedMoves(i + 1)));

        JsonArray? moves = null;
        try { moves = companion.GetArray("PetBattlerMoves"); } catch { }

        for (int i = 0; i < slots.Count; i++)
        {
            string id = "";
            if (moves is not null && i < moves.Length)
            {
                try { id = (moves.GetString(i) ?? "").TrimStart('^'); } catch { }
            }
            slots[i].SelectMove(string.IsNullOrEmpty(id) ? null : id);
        }
        // The legacy move list is still in the save even though the game stopped reading
        // it, so its values are shown beside each slot.
        JsonArray? legacyMoves = null;
        try { legacyMoves = companion.GetArray("PetBattlerMoveList"); } catch { }
        foreach (var slot in slots) slot.LoadLegacyValues(legacyMoves);

        MoveSlots = slots;

        // Affinity comes from the creature and the biome it was found in, and decides
        // which other affinities it beats and loses to.
        string gameAffinity = "";
        try
        {
            string creatureId = companion.GetString("CreatureID") ?? "";
            string biome = companion.GetObject("Biome")?.GetString("Biome") ?? "";
            string affinity = PetBiomeAffinityMap.ResolveAffinity(creatureId, biome);

            BattleAffinity = string.IsNullOrEmpty(affinity)
                ? ""
                : PetBiomeAffinityMap.GetAffinityDisplayName(affinity);
            gameAffinity = PetBiomeAffinityMap.GetAffinityGameName(affinity);
        }
        catch { BattleAffinity = ""; }

        var matchup = PetBiomeAffinityMap.GetAffinityMatchup(gameAffinity);
        if (matchup is { } m)
        {
            BattleWeak = PetBiomeAffinityMap.FormatAffinityList(m.Weak);
            BattleStrong = PetBiomeAffinityMap.FormatAffinityList(m.Strong);
        }
        else
        {
            BattleWeak = UiStrings.Get("common.na");
            BattleStrong = UiStrings.Get("common.na");
        }

        try { UseStatOverrides = companion.GetBool("PetBattlerUseCoreStatClassOverrides"); }
        catch { UseStatOverrides = false; }

        JsonArray? overrides = null;
        try { overrides = companion.GetArray("PetBattlerCoreStatClassOverrides"); } catch { }
        HealthClassIndex = ClassIndex(CompanionBattleIo.ReadClassOverride(overrides, 0));
        AgilityClassIndex = ClassIndex(CompanionBattleIo.ReadClassOverride(overrides, 1));
        CombatClassIndex = ClassIndex(CompanionBattleIo.ReadClassOverride(overrides, 2));

        JsonArray? treats = null;
        try { treats = companion.GetArray("PetBattlerTreatsEaten"); } catch { }
        TreatsHealth = ReadTreat(treats, 0);
        TreatsAgility = ReadTreat(treats, 1);
        TreatsCombat = ReadTreat(treats, 2);

        try { GenesAvailable = Math.Clamp(companion.GetInt("PetBattlerTreatsAvailable"), 0, 1000); }
        catch { GenesAvailable = 0; }
        try { MutationProgress = companion.GetDouble("PetBattleProgressToTreat"); }
        catch { MutationProgress = 0; }
        try { Victories = Math.Clamp(companion.GetInt("PetBattlerVictories"), 0, 999999); }
        catch { Victories = 0; }

        UpdateDerivedBattleValues();
        LoadBattleTeam();
    }

    private static int ReadTreat(JsonArray? treats, int index)
    {
        try
        {
            return treats is not null && index < treats.Length
                ? Math.Clamp(treats.GetInt(index), 0, 10) : 0;
        }
        catch { return 0; }
    }

    private static int ClassIndex(string value)
    {
        int i = Array.IndexOf(CompanionBattleIo.StatClasses, value);
        return i >= 0 ? i : 3;   // default to C
    }

    partial void OnHealthClassIndexChanged(int value) => UpdateDerivedBattleValues();
    partial void OnAgilityClassIndexChanged(int value) => UpdateDerivedBattleValues();
    partial void OnCombatClassIndexChanged(int value) => UpdateDerivedBattleValues();
    partial void OnUseStatOverridesChanged(bool value) => UpdateDerivedBattleValues();
    partial void OnTreatsHealthChanged(int value) => UpdateDerivedBattleValues();
    partial void OnTreatsAgilityChanged(int value) => UpdateDerivedBattleValues();
    partial void OnTreatsCombatChanged(int value) => UpdateDerivedBattleValues();

    private void UpdateDerivedBattleValues()
    {
        string C(int i) => i >= 0 && i < CompanionBattleIo.StatClasses.Length
            ? CompanionBattleIo.StatClasses[i] : "C";

        // Without the override the game derives the class from the creature itself, so
        // there is no fixed average to show.
        AverageClass = UseStatOverrides
            ? CompanionBattleIo.AverageClass(C(HealthClassIndex), C(AgilityClassIndex), C(CombatClassIndex))
            : UiStrings.Get("common.procedural");

        // The genes level is what the treats already eaten add up to.
        int eaten = TreatsHealth + TreatsAgility + TreatsCombat;
        GenesLevel = eaten.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>Builds the three team slots from the unlocked, occupied pets.</summary>
    private void LoadBattleTeam()
    {
        var available = new List<(int Index, string Label)>();
        var pets = _playerState?.GetArray("Pets");
        var unlocked = _playerState?.GetArray("UnlockedPetSlots");

        if (pets is not null)
        {
            for (int i = 0; i < pets.Length; i++)
            {
                var pet = pets.GetObject(i);
                if (pet is null) continue;

                // A locked or empty slot is not a valid team member.
                bool isUnlocked = unlocked is not null && i < unlocked.Length && unlocked.GetBool(i);
                if (!isUnlocked) continue;

                string name = pet.GetString("CustomName") ?? "";
                bool unnamed = string.IsNullOrEmpty(name) || name == "^";
                available.Add((i, unnamed
                    ? $"Pet {i.ToString(CultureInfo.CurrentCulture)}"
                    : $"Pet {i.ToString(CultureInfo.CurrentCulture)} - {name}"));
            }
        }

        var slots = new ObservableCollection<BattleTeamSlotViewModel>();
        var team = _playerState?.GetArray("PetBattleTeam");
        for (int i = 0; i < 3; i++)
        {
            var slot = new BattleTeamSlotViewModel(i + 1, available);
            if (team is not null && i < team.Length)
            {
                try { slot.SelectPet(team.GetInt(i)); } catch { }
            }
            slots.Add(slot);
        }
        TeamSlots = slots;
    }

    /// <summary>Writes the battle fields back into the companion and the player state.</summary>
    private void SaveBattle(JsonObject companion)
    {
        var moves = companion.GetArray("PetBattlerMoves");
        if (moves is not null)
        {
            for (int i = 0; i < MoveSlots.Count && i < moves.Length; i++)
            {
                string? id = MoveSlots[i].SelectedMoveId;
                moves.Set(i, string.IsNullOrEmpty(id) ? "" : "^" + id);
            }
        }

        companion.Set("PetBattlerUseCoreStatClassOverrides", UseStatOverrides);

        var overrides = companion.GetArray("PetBattlerCoreStatClassOverrides");
        string Cls(int i) => i >= 0 && i < CompanionBattleIo.StatClasses.Length
            ? CompanionBattleIo.StatClasses[i] : "C";
        CompanionBattleIo.WriteClassOverride(overrides, 0, Cls(HealthClassIndex));
        CompanionBattleIo.WriteClassOverride(overrides, 1, Cls(AgilityClassIndex));
        CompanionBattleIo.WriteClassOverride(overrides, 2, Cls(CombatClassIndex));

        var treats = companion.GetArray("PetBattlerTreatsEaten");
        if (treats is not null)
        {
            if (treats.Length > 0) treats.Set(0, TreatsHealth);
            if (treats.Length > 1) treats.Set(1, TreatsAgility);
            if (treats.Length > 2) treats.Set(2, TreatsCombat);
        }

        companion.Set("PetBattlerTreatsAvailable", GenesAvailable);
        companion.Set("PetBattleProgressToTreat", MutationProgress);
        companion.Set("PetBattlerVictories", Victories);

        var team = _playerState?.GetArray("PetBattleTeam");
        if (team is not null)
        {
            for (int i = 0; i < TeamSlots.Count && i < team.Length; i++)
                team.Set(i, TeamSlots[i].SelectedPetIndex);
        }
    }

    /// <summary>Clears every battle field, returning the pet to an unbattled state.</summary>
    [RelayCommand]
    private void ResetBattleData()
    {
        if (SelectedCompanion?.CompanionData is not { } companion) return;
        CompanionLogic.ResetBattleData(companion);
        LoadBattle(companion);
    }

    // ================================= Accessories =================================

    /// <summary>
    /// Builds the accessory slots this creature supports. The layout depends on the
    /// creature and on its descriptors, since some species resolve their accessories
    /// through descriptors defined on another species.
    /// </summary>
    private void LoadAccessories(CompanionEntryViewModel entry)
    {
        AccessorySlots = new ObservableCollection<AccessorySlotViewModel>();
        HasAccessories = false;
        AccessoryNote = UiStrings.Get("companion.no_accessories");

        if (entry.CompanionData is not { } companion || entry.Source != "Pet") return;

        string creatureId = companion.GetString("CreatureID") ?? "";
        var descriptors = new List<string>();
        var descriptorArray = companion.GetArray("Descriptors");
        if (descriptorArray is not null)
        {
            for (int i = 0; i < descriptorArray.Length; i++)
            {
                string d = (descriptorArray.GetString(i) ?? "").TrimStart('^');
                if (d.Length > 0) descriptors.Add(d);
            }
        }

        var layout = CompanionAccessoryDatabase.GetSlotLayoutForCreature(creatureId, descriptors);
        if (layout.Length == 0) return;

        var pac = FindAccessoryEntry(entry.OriginalIndex);
        var slots = new ObservableCollection<AccessorySlotViewModel>();

        // Save indices are positional within the creature's own layout, so the row index
        // is the index - deriving it from the slot enum reads the wrong entry for any
        // creature whose layout is not the default order.
        for (int i = 0; i < layout.Length; i++)
        {
            var vm = new AccessorySlotViewModel(layout[i]);
            vm.LoadFrom(pac, i);
            slots.Add(vm);
        }

        AccessorySlots = slots;
        HasAccessories = true;
    }

    private JsonObject? FindAccessoryEntry(int petIndex)
    {
        try
        {
            var pac = _playerState?.GetArray("PetAccessoryCustomisation");
            return pac is not null && petIndex < pac.Length ? pac.GetObject(petIndex) : null;
        }
        catch { return null; }
    }

    private void SaveAccessories(CompanionEntryViewModel entry)
    {
        var pac = FindAccessoryEntry(entry.OriginalIndex);
        if (pac is null) return;

        for (int i = 0; i < AccessorySlots.Count; i++)
            AccessorySlots[i].SaveInto(pac, i);
    }

    // ==================================== Eggs =====================================

    /// <summary>
    /// Backdates the companion's birth time by a day, which is what makes an egg ready
    /// to hatch.
    /// </summary>
    [RelayCommand]
    private async Task MakeHatchableAsync()
    {
        if (SelectedCompanion?.CompanionData is not { } companion) return;

        try
        {
            long birthTime = companion.GetLong("BirthTime");
            companion.Set("BirthTime", birthTime - 86400);
            LoadCompanionDetails(SelectedCompanion);
        }
        catch
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("companion.make_hatchable"),
                    UiStrings.Get("companion.make_hatchable_error"), Services.DialogIcon.Warning);
        }
    }

    /// <summary>
    /// Copies a pet into a free egg slot, replacing one the user picks when the slots are
    /// full, and offers to drop the egg item into the exosuit.
    /// </summary>
    [RelayCommand]
    private async Task InduceEggAsync()
    {
        if (Dialogs is null || _playerState is null) return;
        if (SelectedCompanion is not { Source: "Pet" } entry ||
            entry.CompanionData is not { } pet) return;

        string title = UiStrings.Get("companion.place_egg_in_exosuit");

        var eggs = _playerState.GetArray("Eggs");
        if (eggs is null || eggs.Length == 0)
        {
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("companion.induce_egg_no_slots"),
                Services.DialogIcon.Warning);
            return;
        }

        int target = -1;
        for (int i = 0; i < eggs.Length; i++)
        {
            var slot = eggs.GetObject(i);
            string id = slot?.GetString("CreatureID") ?? "";
            if (string.IsNullOrEmpty(id) || id == "^") { target = i; break; }
        }

        if (target < 0)
        {
            // Every slot holds an egg, so one has to be given up.
            var labels = new List<string>();
            for (int i = 0; i < eggs.Length; i++)
            {
                string name = eggs.GetObject(i)?.GetString("CustomName") ?? "";
                labels.Add(string.IsNullOrEmpty(name) || name == "^"
                    ? $"Egg {(i + 1).ToString(CultureInfo.CurrentCulture)}"
                    : name);
            }

            int? chosen = await Dialogs.ChooseAsync(title,
                UiStrings.Get("companion.induce_egg_select_title"), labels);
            if (chosen is not { } index) return;

            if (!await Dialogs.ConfirmAsync(title,
                    UiStrings.Format("companion.induce_egg_replace_confirm", labels[index])))
                return;

            target = index;
        }

        try
        {
            CompanionEggBuilder.CopyPetToEgg(pet, eggs.GetObject(target));
        }
        catch
        {
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("companion.induce_egg_error"),
                Services.DialogIcon.Error);
            return;
        }

        if (_saveData is not null && _database is not null) LoadData(_saveData, _database, _iconManager);

        if (await Dialogs.ConfirmAsync(title, UiStrings.Get("companion.induce_egg_place_prompt")))
            await PlaceEggInExosuitAsync(target);
    }

    /// <summary>Adds the egg item to the first free exosuit cargo slot.</summary>
    private async Task PlaceEggInExosuitAsync(int eggSlot)
    {
        if (Dialogs is null || _playerState is null) return;
        string title = UiStrings.Get("companion.place_egg_in_exosuit");

        var inventory = _playerState.GetObject("Inventory");
        var slots = inventory?.GetArray("Slots");
        if (inventory is null || slots is null)
        {
            await Dialogs.ShowMessageAsync(title, UiStrings.Get("companion.place_egg_no_inventory"),
                Services.DialogIcon.Warning);
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots.GetObject(i);
            if (slot is null) continue;

            string id = (slot.Get("Id")?.ToString() ?? "").TrimStart('^');
            if (id.Length > 0 && id != "YOURSLOTITEM") continue;

            // Egg items are numbered from EGG_STAND1 upward, matching the egg slot.
            slot.Set("Id", $"^EGG_STAND{(eggSlot + 1).ToString(CultureInfo.InvariantCulture)}");
            slot.Set("Type", "Product");
            slot.Set("Amount", 1);
            slot.Set("MaxAmount", 1);

            await Dialogs.ShowMessageAsync(title, UiStrings.Get("companion.place_egg_success"));
            return;
        }

        await Dialogs.ShowMessageAsync(title, UiStrings.Get("companion.place_egg_full"),
            Services.DialogIcon.Warning);
    }

    [RelayCommand]
    private void ResetAccessory()
    {
        if (SelectedCompanion?.CompanionData == null) return;
        CompanionLogic.ResetAccessoryCustomisation(SelectedCompanion.CompanionData);
    }

    [RelayCommand]
    private async Task ExportCompanion()
    {
        if (SelectedCompanion?.CompanionData == null || SaveFilePickerFunc == null) return;
        SaveCompanionChanges();
        var cfg = ExportConfig.Instance;
        var vars = new Dictionary<string, string> { ["name"] = CompanionName };
        string? path = await SaveFilePickerFunc(UiStrings.Get("common.export"),
            cfg.CompanionExt.TrimStart('.'),
            ExportConfig.BuildFileName(cfg.CompanionTemplate, cfg.CompanionExt, vars));
        if (string.IsNullOrEmpty(path)) return;

        try { CompanionLogic.ExportCompanion(SelectedCompanion.CompanionData, path); }
        catch (Exception ex)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                    UiStrings.Format("companion.export_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task ImportCompanion()
    {
        if (_playerState == null || OpenFilePickerFunc == null) return;
        var cfg = ExportConfig.Instance;
        string? path = await OpenFilePickerFunc(UiStrings.Get("common.import"), cfg.CompanionExt);
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var pets = _playerState.GetArray("Pets");
            var eggs = _playerState.GetArray("Eggs");
            var target = pets ?? eggs;

            if (target is null)
            {
                if (Dialogs is not null)
                    await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                        UiStrings.Get("companion.no_arrays_found"), Services.DialogIcon.Error);
                return;
            }

            // A companion file may not fit the array it was aimed at — an egg into Pets,
            // say — so the other one is tried before giving up.
            try { CompanionLogic.ImportCompanion(target, path); }
            catch (InvalidOperationException)
            {
                var fallback = target == pets ? eggs : pets;
                if (fallback is null) throw;
                CompanionLogic.ImportCompanion(fallback, path);
            }

            LoadData(_saveData!, _database!, null);
        }
        catch (Exception ex)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                    UiStrings.Format("companion.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToPetsJsonAsync() => GoToJsonAsync("PlayerStateData", "Pets");

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToEggsJsonAsync() => GoToJsonAsync("PlayerStateData", "Eggs");

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToBattleTeamJsonAsync() => GoToJsonAsync("PlayerStateData", "PetBattleTeam");

    /// <summary>Opens the selected companion where it lives, in Pets or in Eggs.</summary>
    [RelayCommand]
    private Task GoToSelectedJsonAsync()
    {
        if (SelectedCompanion is not { } entry) return Task.CompletedTask;

        string array = string.Equals(entry.Source, "Egg", StringComparison.OrdinalIgnoreCase)
            ? "Eggs" : "Pets";
        return GoToJsonAsync("PlayerStateData", array, $"[{entry.OriginalIndex}]");
    }

    // The tooltips name the section they open, so they are formatted rather than bound.
    public string GoToPetsTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("goto_json.nav_pets"));

    public string GoToEggsTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("goto_json.nav_eggs"));

    public string GoToBattleTeamTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("goto_json.nav_battle_team"));

    public string GoToSelectedTooltip =>
        UiStrings.Format("goto_json.tooltip_section",
            UiStrings.Get("goto_json.nav_pets") + " " + UiStrings.Get("goto_json.nav_details"));

    public override void ApplyLocalisation()
    {
        OnPropertyChanged(nameof(GoToPetsTooltip));
        OnPropertyChanged(nameof(GoToEggsTooltip));
        OnPropertyChanged(nameof(GoToBattleTeamTooltip));
        OnPropertyChanged(nameof(GoToSelectedTooltip));
    }

    public override void SaveData(JsonObject saveData)
    {
        SaveCompanionChanges();
    }
}

public partial class CompanionEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private bool _isOccupied;
    public JsonObject? CompanionData { get; set; }
    public string Source { get; set; } = "";
    public int OriginalIndex { get; set; }

    public override string ToString() => Label;
}
