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
    [ObservableProperty] private FishEntryViewModel? _selectedFish;
    [ObservableProperty] private string _fishFilter = "";

    /// <summary>The fish rows the filter leaves visible.</summary>
    [ObservableProperty] private ObservableCollection<FishEntryViewModel> _filteredFish = new();

    partial void OnFishFilterChanged(string value) => ApplyFishFilter();

    private void ApplyFishFilter()
    {
        string filter = FishFilter.Trim();
        FilteredFish = new ObservableCollection<FishEntryViewModel>(
            string.IsNullOrEmpty(filter)
                ? FishEntries
                : FishEntries.Where(f =>
                    f.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || f.ProductId.Contains(filter, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Localisation keys for <see cref="CatalogueLogic.RaceColumns"/>, in the same order.
    /// </summary>
    private static readonly string[] RaceLocKeys =
        ["common.race_gek", "common.race_vykeen", "common.race_korvax",
         "discovery.race_atlas", "discovery.race_autophage"];

    [ObservableProperty] private string _techFilter = "";

    // The three known-item lists are filtered into their own collections; the grids bind
    // to these rather than to the full lists, which is what the filter boxes edit.
    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _filteredTechs = new();
    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _filteredProducts = new();
    [ObservableProperty] private ObservableCollection<DiscoveryItemViewModel> _filteredSpecials = new();

    partial void OnTechFilterChanged(string value) => ApplyFilters();
    partial void OnProductFilterChanged(string value) => ApplyFilters();
    partial void OnSpecialFilterChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        FilteredTechs = FilterItems(KnownTechs, TechFilter);
        FilteredProducts = FilterItems(KnownProducts, ProductFilter);
        FilteredSpecials = FilterItems(KnownSpecials, SpecialFilter);
    }

    /// <summary>Matches on name, category or id, as the panel's filter boxes did.</summary>
    private static ObservableCollection<DiscoveryItemViewModel> FilterItems(
        IEnumerable<DiscoveryItemViewModel> source, string filter)
    {
        filter = filter?.Trim() ?? "";
        return new ObservableCollection<DiscoveryItemViewModel>(
            filter.Length == 0
                ? source
                : source.Where(i =>
                    i.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                    || i.Category.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                    || i.Id.Contains(filter, StringComparison.OrdinalIgnoreCase)));
    }
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
            ApplyFilters();
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
                Label = UiStrings.Format("discovery.glyph_n", i + 1),
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
            _fishingRecord = fishingRecord;
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

        ApplyFishFilter();
    }

    /// <summary>
    /// Marks the words in a file as known. The file carries ids, so a word the database
    /// does not list is skipped rather than invented.
    /// </summary>
    [RelayCommand]
    private async Task ImportWordsAsync()
    {
        if (Dialogs is null || OpenFilePickerFunc is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("discovery.import_title"),
            ExportConfig.Instance.DiscoveryExt);
        if (path is null) return;

        try
        {
            var ids = ReadFirstArray(JsonObject.ImportFromFile(path));
            if (ids.Count == 0)
            {
                await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                    UiStrings.Get("discovery.import_no_words"), Services.DialogIcon.Warning);
                return;
            }

            var wanted = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            int added = 0;

            foreach (var word in _allWords.Where(w => !w.IsKnown && wanted.Contains(w.Id)))
            {
                word.IsKnown = true;
                added++;
            }

            ApplyWordFilter();
            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                UiStrings.Format("discovery.import_words_success", added));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>Replaces the teleport endpoints with the ones in a file.</summary>
    [RelayCommand]
    private async Task ImportLocationsAsync()
    {
        if (Dialogs is null || OpenFilePickerFunc is null || _playerState is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("discovery.import_title"),
            ExportConfig.Instance.DiscoveryExt);
        if (path is null) return;

        try
        {
            var imported = JsonObject.ImportFromFile(path);

            JsonArray? source = null;
            foreach (string name in imported.Names())
            {
                try { source = imported.GetArray(name); } catch { }
                if (source is not null && source.Length > 0) break;
            }

            if (source is null || source.Length == 0)
            {
                await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                    UiStrings.Get("discovery.import_no_locations"), Services.DialogIcon.Warning);
                return;
            }

            var endpoints = _playerState.GetArray("TeleportEndpoints");
            if (endpoints is null) return;

            int added = 0;
            for (int i = 0; i < source.Length; i++)
            {
                var entry = source.GetObject(i);
                if (entry is null) continue;
                endpoints.Add(entry);
                added++;
            }

            _teleportEndpoints = endpoints;
            RefreshLocations();

            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                UiStrings.Format("discovery.import_locations_success", added));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>Overwrites the whole fishing record with the one in a file.</summary>
    [RelayCommand]
    private async Task ImportFishAsync()
    {
        if (Dialogs is null || OpenFilePickerFunc is null || _fishingRecord is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("discovery.import_title"),
            ExportConfig.Instance.DiscoveryExt);
        if (path is null) return;

        try
        {
            var record = JsonObject.ImportFromFile(path).GetObject("FishingRecord");
            if (record is null)
            {
                await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                    UiStrings.Get("discovery.import_no_fish"), Services.DialogIcon.Warning);
                return;
            }

            foreach (string name in record.Names())
                _fishingRecord.Set(name, record.Get(name));

            if (_playerState is not null) LoadFish(_playerState);

            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                UiStrings.Get("discovery.import_fish_success"));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>The first array in an imported file, read as strings.</summary>
    private static List<string> ReadFirstArray(JsonObject imported)
    {
        foreach (string name in imported.Names())
        {
            JsonArray? arr = null;
            try { arr = imported.GetArray(name); } catch { }
            if (arr is null) continue;

            var ids = new List<string>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                string id = arr.GetString(i) ?? "";
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
            }
            if (ids.Count > 0) return ids;
        }
        return [];
    }

    /// <summary>
    /// The recipe panel is hosted here rather than in the shell's panel list, so it is
    /// told about a language change through this one.
    /// </summary>
    public override void ApplyLocalisation()
    {
        WordRaces = new ObservableCollection<string>(RaceLocKeys.Select(UiStrings.Get));
        Recipe.ApplyLocalisation();
    }

    // ===================================== Fish =====================================

    /// <summary>The fishing record's arrays, held so the fish tab can edit them.</summary>
    private JsonObject? _fishingRecord;

    /// <summary>
    /// Adds a fish to the first free slot. The record is fixed-length, with "^" marking
    /// an unused entry, so this fills a gap rather than growing the array.
    /// </summary>
    [RelayCommand]
    private async Task AddFishAsync()
    {
        if (Dialogs is null || PickItemFunc is null) return;

        var productList = _fishingRecord?.GetArray("ProductList");
        if (productList is null) return;

        int empty = -1;
        for (int i = 0; i < productList.Length; i++)
        {
            string value;
            try { value = productList.GetString(i) ?? ""; } catch { empty = i; break; }
            if (string.IsNullOrEmpty(value) || value == "^") { empty = i; break; }
        }

        if (empty < 0)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.add_fish_title"),
                UiStrings.Get("discovery.no_fish_slots"));
            return;
        }

        string? picked = await PickItemFunc(UiStrings.Get("discovery.add_fish_title"));
        if (string.IsNullOrEmpty(picked)) return;

        string saveId = picked.StartsWith('^') ? picked : "^" + picked;
        productList.Set(empty, saveId);

        if (_fishingRecord!.GetArray("ProductCountList") is { } counts && empty < counts.Length)
            counts.Set(empty, 0);
        if (_fishingRecord.GetArray("LargestCatchList") is { } largest && empty < largest.Length)
            largest.Set(empty, 0.0);

        if (_playerState is not null) LoadFish(_playerState);
    }

    /// <summary>Clears the selected slot back to the unused marker.</summary>
    [RelayCommand]
    private void RemoveFish()
    {
        if (SelectedFish is null || _fishingRecord is null) return;

        int idx = SelectedFish.ArrayIndex;
        if (_fishingRecord.GetArray("ProductList") is { } products && idx < products.Length)
            products.Set(idx, "^");
        if (_fishingRecord.GetArray("ProductCountList") is { } counts && idx < counts.Length)
            counts.Set(idx, 0);
        if (_fishingRecord.GetArray("LargestCatchList") is { } largest && idx < largest.Length)
            largest.Set(idx, 0.0);

        if (_playerState is not null) LoadFish(_playerState);
    }

    [RelayCommand]
    private void RemoveTech()
    {
        if (SelectedTech != null)
        {
            KnownTechs.Remove(SelectedTech);
            SelectedTech = null;
            ApplyFilters();
        }
    }

    [RelayCommand]
    private void RemoveProduct()
    {
        if (SelectedProduct != null)
        {
            KnownProducts.Remove(SelectedProduct);
            ApplyFilters();
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

        // RaceColumns carries the internal names; the picker shows the localised ones.
        WordRaces = new ObservableCollection<string>(RaceLocKeys.Select(UiStrings.Get));
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
        if (Dialogs is null || _teleportEndpoints is null) return;

        // The grid allows several at once; fall back to the one highlighted.
        var targets = SelectedLocations.Count > 0
            ? SelectedLocations.ToList()
            : SelectedLocation is null ? [] : [SelectedLocation];
        if (targets.Count == 0) return;

        string message = targets.Count == 1
            ? UiStrings.Format("discovery.delete_location_single", targets[0].TypeName)
            : UiStrings.Format("discovery.delete_location_multi", targets.Count);

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("discovery.delete_location_title"),
                message, Services.DialogIcon.Warning))
            return;

        // Removing from the end keeps the earlier indices valid.
        foreach (var target in targets.OrderByDescending(t => t.Index))
            _teleportEndpoints.RemoveAt(target.Index);

        SelectedLocations.Clear();
        RefreshLocations();
    }

    /// <summary>Every location the grid has highlighted, kept in step by the view.</summary>
    public List<TeleportLocationViewModel> SelectedLocations { get; } = new();

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
        ApplyFilters();
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

        // Building the specials list walks the whole item database, which takes a moment
        // on a slower machine, so the status line says what is happening.
        StatusText = UiStrings.Get("discovery.loading_specials");
        string? id = await PickItemFunc(UiStrings.Get(titleKey));
        StatusText = "";
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

        ApplyFilters();
    }

    /// <summary>Opens the item picker; supplied by the view, which owns the dialog.</summary>
    public Func<string, Task<string?>>? PickItemFunc { get; set; }

    [RelayCommand]
    private Task ExportTechAsync() => ExportItemsAsync(KnownTechs, "KnownTech");

    [RelayCommand]
    private Task ImportTechAsync() => ImportItemsAsync(KnownTechs);

    [RelayCommand]
    private Task ExportProductAsync() => ExportItemsAsync(KnownProducts, "KnownProducts");

    [RelayCommand]
    private Task ImportProductAsync() => ImportItemsAsync(KnownProducts);

    /// <summary>
    /// Writes the list's ids under one named array, which is the shape the importer
    /// below looks for.
    /// </summary>
    private async Task ExportItemsAsync(ObservableCollection<DiscoveryItemViewModel> items, string arrayName)
    {
        if (Dialogs is null || SaveFilePickerFunc is null) return;

        var config = ExportConfig.Instance;
        var vars = new Dictionary<string, string> { ["name"] = arrayName };
        string? path = await SaveFilePickerFunc(UiStrings.Get("common.export"),
            config.DiscoveryExt.TrimStart('.'),
            ExportConfig.BuildFileName(config.DiscoveryTemplate, config.DiscoveryExt, vars));
        if (path is null) return;

        try
        {
            var arr = new JsonArray();
            foreach (var item in items) arr.Add(item.Id);

            var root = new JsonObject();
            root.Set(arrayName, arr);
            root.ExportToFile(path);
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.export_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>
    /// Adds the ids from a file that are not already known. The first array in the file
    /// is taken, so a file exported under any of the list names is accepted.
    /// </summary>
    private async Task ImportItemsAsync(ObservableCollection<DiscoveryItemViewModel> items)
    {
        if (Dialogs is null || OpenFilePickerFunc is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("discovery.import_title"),
            ExportConfig.Instance.DiscoveryExt);
        if (path is null) return;

        try
        {
            var imported = JsonObject.ImportFromFile(path);

            JsonArray? arr = null;
            foreach (string name in imported.Names())
            {
                try { arr = imported.GetArray(name); } catch { }
                if (arr is not null) break;
            }

            if (arr is null || arr.Length == 0)
            {
                await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                    UiStrings.Get("discovery.import_no_items"), Services.DialogIcon.Warning);
                return;
            }

            var existing = new HashSet<string>(items.Select(i => i.Id), StringComparer.OrdinalIgnoreCase);
            int added = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                string id = arr.GetString(i) ?? "";
                if (string.IsNullOrEmpty(id) || !existing.Add(id)) continue;

                string lookupId = CatalogueLogic.StripCaretPrefix(id);
                var dbItem = _database?.GetItem(lookupId);
                items.Add(new DiscoveryItemViewModel
                {
                    Id = id,
                    Name = dbItem?.Name ?? lookupId,
                    Category = dbItem?.ItemType ?? "",
                });
                added++;
            }

            ApplyFilters();
            await Dialogs.ShowMessageAsync(UiStrings.Get("discovery.import_title"),
                UiStrings.Format("discovery.import_success_items", added));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("discovery.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

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

    /// <summary>The galaxy's core colour, shown as a dot beside its name.</summary>
    public Avalonia.Media.IBrush CoreBrush { get; private set; } =
        Avalonia.Media.Brushes.Transparent;

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

        // Galaxies cycle through core colours; the dot is how the panel showed which.
        CoreBrush = new Avalonia.Media.SolidColorBrush(
            GalaxyDatabase.GetGalaxyCoreColorValue(realityIndex));

        int vx = galactic.GetInt("VoxelX"), vy = galactic.GetInt("VoxelY"), vz = galactic.GetInt("VoxelZ");
        int system = galactic.GetInt("SolarSystemIndex"), planet = galactic.GetInt("PlanetIndex");

        PortalHex = CoordinateHelper.VoxelToPortalCode(vx, vy, vz, system, planet);
        PortalDec = CoordinateHelper.PortalHexToDec(PortalHex);
        SignalBooster = CoordinateHelper.VoxelToSignalBooster(vx, vy, vz, system);
    }
}
