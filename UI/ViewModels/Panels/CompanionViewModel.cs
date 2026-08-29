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

    partial void OnSelectedCompanionChanged(CompanionEntryViewModel? value)
    {
        HasSelection = value != null;
        if (value != null) LoadCompanionDetails(value);
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        try
        {
            Companions.Clear();
            _playerState = null;

            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;
            _playerState = playerState;

            LoadSlots(playerState.GetArray("Pets"), "Pet");
            LoadSlots(playerState.GetArray("Eggs"), "Egg");

            CountLabel = $"Total slots: {Companions.Count}";

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
            catch { }
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
    private void DeleteCompanion()
    {
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
        MoveSlots = slots;

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
    partial void OnTreatsHealthChanged(int value) => UpdateDerivedBattleValues();
    partial void OnTreatsAgilityChanged(int value) => UpdateDerivedBattleValues();
    partial void OnTreatsCombatChanged(int value) => UpdateDerivedBattleValues();

    private void UpdateDerivedBattleValues()
    {
        string C(int i) => i >= 0 && i < CompanionBattleIo.StatClasses.Length
            ? CompanionBattleIo.StatClasses[i] : "C";

        AverageClass = CompanionBattleIo.AverageClass(C(HealthClassIndex), C(AgilityClassIndex), C(CombatClassIndex));

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
        string? path = await SaveFilePickerFunc("Export Companion", cfg.CompanionExt.TrimStart('.'),
            ExportConfig.BuildDialogFilter(cfg.CompanionExt, "Companion files"));
        if (string.IsNullOrEmpty(path)) return;
        try { CompanionLogic.ExportCompanion(SelectedCompanion.CompanionData, path); }
        catch { }
    }

    [RelayCommand]
    private async Task ImportCompanion()
    {
        if (_playerState == null || OpenFilePickerFunc == null) return;
        var cfg = ExportConfig.Instance;
        string? path = await OpenFilePickerFunc("Import Companion",
            ExportConfig.BuildImportFilter(cfg.CompanionExt, "Companion files", ".pet", ".cmp"));
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var pets = _playerState.GetArray("Pets");
            var eggs = _playerState.GetArray("Eggs");
            var target = pets ?? eggs;
            if (target == null) return;

            try { CompanionLogic.ImportCompanion(target, path); }
            catch (InvalidOperationException)
            {
                var fallback = target == pets ? eggs : pets;
                if (fallback != null) CompanionLogic.ImportCompanion(fallback, path);
                else return;
            }
        }
        catch { }
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
