using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Core.Utilities;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class DiscoveryViewModel : PanelViewModelBase
{
    private JsonObject? _playerState;
    private JsonObject? _saveData;

    /// <summary>Word list, supplied by the shell since it is loaded alongside the item database.</summary>
    public WordDatabase? WordDb { get; set; }
    private GameItemDatabase? _database;
    private IconManager? _iconManager;

    /// <summary>Exposed so the view can hand them to the item picker dialog.</summary>
    public GameItemDatabase? Database => _database;
    public IconManager? IconMgr => _iconManager;

    [ObservableProperty] private int _selectedTabIndex;

    public RecipeViewModel Recipe { get; } = new();

    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _knownTechs = new();
    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _knownProducts = new();
    [ObservableProperty] private ObservableCollection<GlyphViewModel> _glyphs = new();
    [ObservableProperty] private ObservableCollection<FishEntryViewModel> _fishEntries = new();

    [ObservableProperty] private string _techFilter = "";
    [ObservableProperty] private string _productFilter = "";

    [ObservableProperty] private DiscoveryItemViewModel? _selectedTech;
    [ObservableProperty] private DiscoveryItemViewModel? _selectedProduct;

    [ObservableProperty] private string _statusText = "";

    // --- Words ---------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<WordRowViewModel> _words = new();
    [ObservableProperty] private string _wordFilter = "";
    [ObservableProperty] private ObservableCollection<string> _wordRaces = new();
    [ObservableProperty] private int _selectedWordRaceIndex;

    private WordDatabase? _wordDatabase;
    private JsonArray? _knownWordGroups;
    private readonly List<WordRowViewModel> _allWords = new();

    // --- Known specials -------------------------------------------------------
    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _knownSpecials = new();
    [ObservableProperty] private DiscoveryItemViewModel? _selectedSpecial;
    [ObservableProperty] private string _specialFilter = "";

    // --- Locations ------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<TeleportLocationViewModel> _locations = new();
    [ObservableProperty] private TeleportLocationViewModel? _selectedLocation;
    [ObservableProperty] private string _locationFilter = "";

    private JsonArray? _teleportEndpoints;

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _iconManager = iconManager;
        Recipe.LoadData(saveData, database, iconManager);
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;
            _playerState = playerState;

            _saveData = saveData;

            LoadKnownItems(playerState, "KnownTech", KnownTechs);
            LoadKnownItems(playerState, "KnownProducts", KnownProducts);
            LoadKnownItems(playerState, "KnownSpecials", KnownSpecials);
            LoadGlyphs(playerState);
            LoadFish(playerState);
            LoadWords(playerState, WordDb);
            LoadLocations(playerState);
        }
        catch { }
    }

    private void LoadKnownItems(JsonObject playerState, string arrayName, ObservableCollection<DiscoveryItemViewModel> target)
    {
        target.Clear();
        var ids = CatalogueLogic.LoadKnownItemIds(playerState, arrayName);
        foreach (var id in ids)
        {
            var dbItem = _database?.GetItem(id);
            target.Add(new DiscoveryItemViewModel
            {
                Id = id,
                Name = dbItem?.Name ?? id,
                Category = dbItem?.ItemType ?? ""
            });
        }
    }

    private void LoadGlyphs(JsonObject playerState)
    {
        Glyphs.Clear();
        int runesBitfield = CatalogueLogic.LoadGlyphBitfield(playerState);
        for (int i = 0; i < 16; i++)
        {
            int mask = 1 << i;
            Glyphs.Add(new GlyphViewModel
            {
                Index = i,
                Label = $"Glyph {i + 1}",
                IsKnown = (runesBitfield & mask) == mask
            });
        }
    }

    private void LoadFish(JsonObject playerState)
    {
        FishEntries.Clear();
        try
        {
            var fishingRecord = playerState.GetObject("FishingRecord");
            if (fishingRecord == null) return;

            var productList = fishingRecord.GetArray("ProductList");
            var countList = fishingRecord.GetArray("ProductCountList");
            var largestList = fishingRecord.GetArray("LargestCatchList");
            if (productList == null) return;

            for (int i = 0; i < productList.Length; i++)
            {
                string productId = productList.GetString(i) ?? "";
                if (string.IsNullOrEmpty(productId) || productId == "^") continue;

                int catchCount = 0;
                double largestCatch = 0;
                if (countList != null && i < countList.Length)
                    try { catchCount = countList.GetInt(i); } catch { }
                if (largestList != null && i < largestList.Length)
                    try { largestCatch = largestList.GetDouble(i); } catch { }

                string lookupId = productId.StartsWith('^') ? productId[1..] : productId;
                var dbItem = string.IsNullOrEmpty(lookupId) ? null : _database?.GetItem(lookupId);

                FishEntries.Add(new FishEntryViewModel
                {
                    ProductId = productId,
                    Name = dbItem?.Name ?? lookupId,
                    CatchCount = catchCount,
                    LargestCatch = largestCatch,
                    ArrayIndex = i
                });
            }
        }
        catch { }
    }

    [RelayCommand]
    private void RemoveTech()
    {
        if (SelectedTech != null)
        {
            KnownTechs.Remove(SelectedTech);
            SelectedTech = null;
        }
    }

    [RelayCommand]
    private void RemoveProduct()
    {
        if (SelectedProduct != null)
        {
            KnownProducts.Remove(SelectedProduct);
            SelectedProduct = null;
        }
    }

    [RelayCommand]
    private void LearnAllGlyphs()
    {
        foreach (var g in Glyphs) g.IsKnown = true;
    }

    [RelayCommand]
    private void UnlearnAllGlyphs()
    {
        foreach (var g in Glyphs) g.IsKnown = false;
    }

    // ==================================== Words ====================================

    /// <summary>
    /// Builds the word list. A word can belong to several races, and each race tracks it
    /// separately, so the race selector decides which flag the checkbox reflects.
    /// </summary>
    private void LoadWords(JsonObject playerState, WordDatabase? database)
    {
        _wordDatabase = database;
        _knownWordGroups = playerState.GetArray("KnownWordGroups");
        _allWords.Clear();

        WordRaces = new ObservableCollection<string>(
            CatalogueLogic.RaceColumns.Select(r => r.Name));
        if (WordRaces.Count > 0 && SelectedWordRaceIndex < 0) SelectedWordRaceIndex = 0;

        if (database is null) { Words = new(); return; }

        foreach (var entry in database.Words)
            _allWords.Add(new WordRowViewModel(entry, this));

        RefreshWordKnownFlags();
        ApplyWordFilter();
    }

    /// <summary>Re-reads each word's known flag for the selected race.</summary>
    private void RefreshWordKnownFlags()
    {
        if (_knownWordGroups is null) return;
        int race = CurrentRaceOrdinal;

        foreach (var row in _allWords)
        {
            string? group = row.Entry.GetGroupForRace(race);
            row.SetKnownWithoutWriting(group is not null &&
                CatalogueLogic.IsWordKnown(_knownWordGroups, group, race));
            row.AppliesToRace = group is not null;
        }
    }

    internal int CurrentRaceOrdinal =>
        SelectedWordRaceIndex >= 0 && SelectedWordRaceIndex < CatalogueLogic.RaceColumns.Length
            ? CatalogueLogic.RaceColumns[SelectedWordRaceIndex].Index
            : 0;

    /// <summary>Writes a single word's known flag back into the save.</summary>
    internal void WriteWordKnown(WordRowViewModel row, bool known)
    {
        if (_knownWordGroups is null) return;
        int race = CurrentRaceOrdinal;
        string? group = row.Entry.GetGroupForRace(race);
        if (group is null) return;

        CatalogueLogic.SetWordKnown(_knownWordGroups, group, race, known);
    }

    partial void OnSelectedWordRaceIndexChanged(int value)
    {
        RefreshWordKnownFlags();
        ApplyWordFilter();
    }

    partial void OnWordFilterChanged(string value) => ApplyWordFilter();

    private void ApplyWordFilter()
    {
        string filter = WordFilter?.Trim() ?? "";
        Words = new ObservableCollection<WordRowViewModel>(
            _allWords.Where(w => w.AppliesToRace &&
                (filter.Length == 0 ||
                 w.Text.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                 w.Id.Contains(filter, StringComparison.OrdinalIgnoreCase))));
    }

    [RelayCommand]
    private void LearnAllWords() => SetAllWords(known: true);

    [RelayCommand]
    private void UnlearnAllWords() => SetAllWords(known: false);

    private void SetAllWords(bool known)
    {
        if (_knownWordGroups is null || _wordDatabase is null) return;

        // Only the words shown are affected, so a filter narrows the operation.
        var entries = Words.Select(w => w.Entry).ToList();
        // The helper takes the race columns to apply to; restrict it to the selected one.
        var column = CatalogueLogic.RaceColumns
            .Where(r => r.Index == CurrentRaceOrdinal).ToArray();
        CatalogueLogic.SetWordFlagsForEntries(_knownWordGroups, entries, column, known);
        RefreshWordKnownFlags();
    }

    // ================================== Locations ==================================

    /// <summary>
    /// Lists the save's teleport endpoints, which is where every discovered base,
    /// station and other travel target is recorded.
    /// </summary>
    private void LoadLocations(JsonObject playerState)
    {
        _teleportEndpoints = playerState.GetArray("TeleportEndpoints");
        RefreshLocations();
    }

    private void RefreshLocations()
    {
        var list = new ObservableCollection<TeleportLocationViewModel>();
        if (_teleportEndpoints is not null)
        {
            string filter = LocationFilter?.Trim() ?? "";
            for (int i = 0; i < _teleportEndpoints.Length; i++)
            {
                var endpoint = _teleportEndpoints.GetObject(i);
                if (endpoint is null) continue;

                var row = new TeleportLocationViewModel(i, endpoint);
                if (filter.Length == 0 ||
                    row.TypeName.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
                    row.PortalHex.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    row.Galaxy.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(row);
            }
        }
        Locations = list;
    }

    partial void OnLocationFilterChanged(string value) => RefreshLocations();

    [RelayCommand]
    private async Task DeleteLocationAsync()
    {
        if (Dialogs is null || _teleportEndpoints is null || SelectedLocation is null) return;

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("discovery.delete_location_title"),
                UiStrings.Format("discovery.delete_location_single", SelectedLocation.TypeName),
                Services.DialogIcon.Warning))
            return;

        _teleportEndpoints.RemoveAt(SelectedLocation.Index);
        RefreshLocations();
    }

    /// <summary>
    /// Moves the player to the selected endpoint's system by copying its address, and
    /// sets the spawn state so the game places the player in their ship on load.
    /// </summary>
    [RelayCommand]
    private async Task TravelToSystemAsync()
    {
        if (Dialogs is null || SelectedLocation?.Endpoint is null ||
            _playerState is null || _saveData is null) return;

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("discovery.travel_title"),
                UiStrings.Get("discovery.travel_confirm")))
            return;

        try
        {
            var target = SelectedLocation.Endpoint.GetObject("UniverseAddress");
            var targetGalactic = target?.GetObject("GalacticAddress");
            if (target is null || targetGalactic is null) return;

            var playerAddress = _playerState.GetObject("UniverseAddress");
            var playerGalactic = playerAddress?.GetObject("GalacticAddress");
            if (playerAddress is null || playerGalactic is null) return;

            playerAddress.Set("RealityIndex", target.GetInt("RealityIndex"));
            playerGalactic.Set("VoxelX", targetGalactic.GetInt("VoxelX"));
            playerGalactic.Set("VoxelY", targetGalactic.GetInt("VoxelY"));
            playerGalactic.Set("VoxelZ", targetGalactic.GetInt("VoxelZ"));
            playerGalactic.Set("SolarSystemIndex", targetGalactic.GetInt("SolarSystemIndex"));
            // System-level travel: the planet index is meaningless at the target.
            playerGalactic.Set("PlanetIndex", 0);

            _saveData.GetObject("SpawnStateData")?.Set("LastKnownPlayerState", "InShip");

            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.travel_complete_title"),
                UiStrings.Get("discovery.travel_complete"));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.travel_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    public override void SaveData(JsonObject saveData)
    {
        var playerState = saveData.GetObject("PlayerStateData");
        if (playerState == null) return;

        SaveKnownItems(playerState, "KnownTech", KnownTechs);
        SaveKnownItems(playerState, "KnownProducts", KnownProducts);
        SaveGlyphs(playerState);
    }

    private static void SaveKnownItems(JsonObject playerState, string arrayName, ObservableCollection<DiscoveryItemViewModel> items)
    {
        var ids = items.Select(i => i.Id).ToList();
        CatalogueLogic.SaveKnownItemIds(playerState, arrayName, ids);
    }

    private void SaveGlyphs(JsonObject playerState)
    {
        int runesBitfield = 0;
        for (int i = 0; i < Glyphs.Count && i < 16; i++)
        {
            if (Glyphs[i].IsKnown)
                runesBitfield |= (1 << i);
        }
        CatalogueLogic.SaveGlyphBitfield(playerState, runesBitfield);
    }

    [RelayCommand]
    private void ClearTechFilter() { TechFilter = ""; }

    [RelayCommand]
    private void ClearProductFilter() { ProductFilter = ""; }

    [RelayCommand]
    private Task AddTechAsync() => AddKnownItemAsync(KnownTechs, CatalogueLogic.IsLearnableTechnology,
        "discovery.add_technology");

    [RelayCommand]
    private Task AddProductAsync() => AddKnownItemAsync(KnownProducts, CatalogueLogic.IsLearnableProduct,
        "discovery.add_product");

    [RelayCommand]
    private Task AddSpecialAsync() => AddKnownItemAsync(KnownSpecials, _ => true, "discovery.add_special");

    [RelayCommand]
    private void RemoveSpecial()
    {
        if (SelectedSpecial is null) return;
        KnownSpecials.Remove(SelectedSpecial);
        SelectedSpecial = null;
    }

    /// <summary>
    /// Adds an item the player does not already know. The picker offers the whole item
    /// database, so the predicate keeps each list to what it can legitimately contain.
    /// </summary>
    private async Task AddKnownItemAsync(ObservableCollection<DiscoveryItemViewModel> target,
        Func<GameItem, bool> isEligible, string titleKey)
    {
        if (PickItemFunc is null || _database is null) return;

        string? id = await PickItemFunc(UiStrings.Get(titleKey));
        if (string.IsNullOrEmpty(id)) return;

        string bare = CatalogueLogic.StripCaretPrefix(id);
        if (target.Any(i => string.Equals(CatalogueLogic.StripCaretPrefix(i.Id), bare,
                StringComparison.OrdinalIgnoreCase)))
            return;   // already known

        var item = _database.GetItem(bare) ?? _database.GetItem("^" + bare);
        if (item is not null && !isEligible(item)) return;

        target.Add(new DiscoveryItemViewModel
        {
            Id = CatalogueLogic.EnsureCaretPrefix(bare),
            Name = item?.Name ?? bare,
            Category = item?.Category ?? "",
        });
    }

    /// <summary>Opens the item picker; supplied by the view, which owns the dialog.</summary>
    public Func<string, Task<string?>>? PickItemFunc { get; set; }

    [RelayCommand]
    private void ExportTech() { /* TODO: export known tech */ }

    [RelayCommand]
    private void ImportTech() { /* TODO: import known tech */ }

    [RelayCommand]
    private void ExportProduct() { /* TODO: export known products */ }

    [RelayCommand]
    private void ImportProduct() { /* TODO: import known products */ }

    // Go to JSON — one target per tab, mirroring the buttons the WinForms panel put in
    // each filter row.
    [RelayCommand] private Task GoToTechJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownTech");
    [RelayCommand] private Task GoToProductJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownProducts");
    [RelayCommand] private Task GoToSpecialJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownSpecials");
    [RelayCommand] private Task GoToWordJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownWordGroups");
    [RelayCommand] private Task GoToGlyphJsonAsync() => GoToJsonAsync("PlayerStateData", "KnownPortalRunes");
    [RelayCommand] private Task GoToLocationJsonAsync() => GoToJsonAsync("PlayerStateData", "TeleportEndpoints");
    [RelayCommand] private Task GoToFishJsonAsync() => GoToJsonAsync("PlayerStateData", "FishingRecord");
}

public partial class DiscoveryItemViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _category = "";
}

public partial class GlyphViewModel : ObservableObject
{
    [ObservableProperty] private int _index;
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private bool _isKnown;
}

public partial class FishEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _productId = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private int _catchCount;
    [ObservableProperty] private double _largestCatch;
    [ObservableProperty] private int _arrayIndex;
}

/// <summary>A word and whether the selected race knows it.</summary>
public partial class WordRowViewModel : ObservableObject
{
    private readonly DiscoveryViewModel _owner;
    private bool _suppressWrite;

    [ObservableProperty] private bool _isKnown;

    /// <summary>False when the word has no group for the selected race, so it is filtered out.</summary>
    public bool AppliesToRace { get; set; } = true;

    public WordEntry Entry { get; }
    public string Id => Entry.Id;
    public string Text => Entry.Text;

    public WordRowViewModel(WordEntry entry, DiscoveryViewModel owner)
    {
        Entry = entry;
        _owner = owner;
    }

    /// <summary>Updates the flag from the save without writing it straight back.</summary>
    internal void SetKnownWithoutWriting(bool known)
    {
        _suppressWrite = true;
        IsKnown = known;
        _suppressWrite = false;
    }

    partial void OnIsKnownChanged(bool value)
    {
        if (!_suppressWrite) _owner.WriteWordKnown(this, value);
    }
}

/// <summary>One teleport endpoint: a place the player can travel back to.</summary>
public partial class TeleportLocationViewModel : ObservableObject
{
    /// <summary>Raw teleporter types mapped to their string-table keys.</summary>
    private static readonly Dictionary<string, string> TypeKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Base"] = "location_type.base",
        ["Spacestation"] = "location_type.spacestation",
        ["Atlas"] = "location_type.atlas",
        ["PlanetAwayFromShip"] = "location_type.planet_away_from_ship",
        ["ExternalBase"] = "location_type.external_base",
        ["EmergencyGalaxyFix"] = "location_type.emergency_galaxy_fix",
        ["OnNexus"] = "location_type.on_nexus",
        ["SpacestationFixPosition"] = "location_type.spacestation_fix_position",
        ["Settlement"] = "location_type.settlement",
        ["Freighter"] = "location_type.freighter",
        ["Frigate"] = "location_type.frigate",
        ["BaseBuildingObject"] = "location_type.base_building_object",
    };

    public int Index { get; }
    public JsonObject Endpoint { get; }
    public string TypeName { get; }
    public string Galaxy { get; }
    public string PortalHex { get; }
    public string PortalDec { get; }
    public string SignalBooster { get; }

    public TeleportLocationViewModel(int index, JsonObject endpoint)
    {
        Index = index;
        Endpoint = endpoint;

        string rawType = endpoint.GetString("TeleporterType") ?? "";
        rawType = CatalogueLogic.StripCaretPrefix(rawType);
        TypeName = TypeKeys.TryGetValue(rawType, out string? key) ? UiStrings.Get(key) : rawType;

        var address = endpoint.GetObject("UniverseAddress");
        var galactic = address?.GetObject("GalacticAddress");

        if (galactic is null)
        {
            Galaxy = PortalHex = PortalDec = SignalBooster = "";
            return;
        }

        int realityIndex = address!.GetInt("RealityIndex");
        Galaxy = GalaxyDatabase.GetGalaxyName(realityIndex);

        int vx = galactic.GetInt("VoxelX"), vy = galactic.GetInt("VoxelY"), vz = galactic.GetInt("VoxelZ");
        int system = galactic.GetInt("SolarSystemIndex"), planet = galactic.GetInt("PlanetIndex");

        PortalHex = CoordinateHelper.VoxelToPortalCode(vx, vy, vz, system, planet);
        PortalDec = CoordinateHelper.PortalHexToDec(PortalHex);
        SignalBooster = CoordinateHelper.VoxelToSignalBooster(vx, vy, vz, system);
    }
}
