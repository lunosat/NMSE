using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using Avalonia.Media;
using NMSE.UI.ViewModels.Controls;

namespace NMSE.UI.ViewModels.Panels;

public partial class StarshipViewModel : PanelViewModelBase
{
    private JsonArray? _shipOwnership;
    private JsonObject? _playerState;
    private JsonObject? _saveData;
    private GameItemDatabase? _database;
    private IconManager? _iconManager;
    private int _primaryShipIndex;

    [ObservableProperty] private ObservableCollection<string> _shipList = new();
    [ObservableProperty] private int _selectedShipIndex = -1;
    [ObservableProperty] private string _primaryShipLabel = "";

    private readonly List<int> _shipDataIndices = new();

    [ObservableProperty] private string _shipName = "";
    [ObservableProperty] private string _shipSeed = "";
    [ObservableProperty] private ObservableCollection<string> _shipTypes = new();
    [ObservableProperty] private int _selectedTypeIndex = -1;
    [ObservableProperty] private ObservableCollection<string> _shipClasses = new(StarshipLogic.ShipClasses);
    [ObservableProperty] private int _selectedClassIndex = -1;
    [ObservableProperty] private bool _useOldColours;

    [ObservableProperty] private double _damage;
    [ObservableProperty] private double _shield;
    [ObservableProperty] private double _hyperdrive;
    [ObservableProperty] private double _maneuver;

    [ObservableProperty] private InventoryGridViewModel _cargoGrid = new();
    [ObservableProperty] private InventoryGridViewModel _techGrid = new();

    [ObservableProperty] private bool _isCorvette;

    // --- Customisation -----------------------------------------------------
    [ObservableProperty] private bool _customisationAvailable;
    [ObservableProperty] private string _customisationMessage = "";
    [ObservableProperty] private string _sceneResource = "";
    [ObservableProperty] private ObservableCollection<string> _sceneResources = new();
    [ObservableProperty] private ObservableCollection<ShipPartSlotViewModel> _partSlots = new();
    [ObservableProperty] private ObservableCollection<ShipTextureGroupViewModel> _textureGroups = new();
    [ObservableProperty] private ObservableCollection<string> _palettes = new();
    [ObservableProperty] private int _selectedPaletteIndex = -1;
    [ObservableProperty] private ObservableCollection<ShipColourChannelViewModel> _colourChannels = new();
    [ObservableProperty] private bool _showSailColourWarning;

    /// <summary>Ship index the customisation tab is currently showing, so it is saved only for that ship.</summary>
    private int _customisationShipIndex = -1;
    private ShipCustomisationConfig? _customisationConfig;

    // --- Archive -----------------------------------------------------------
    [ObservableProperty] private ObservableCollection<string> _archivedShips = new();
    [ObservableProperty] private int _selectedArchiveIndex = -1;

    private readonly List<int> _archiveDataIndices = new();

    // --- Corvette ----------------------------------------------------------
    [ObservableProperty] private bool _isCorvetteOptimised;

    private StarshipLogic.ShipTypeItem[] _typeItems = [];

    public StarshipViewModel()
    {
        CargoGrid.SetIsCargoInventory(true);
        CargoGrid.SetInventoryOwnerType("Ship");
        CargoGrid.SetInventoryGroup("Ship");

        TechGrid.SetIsTechInventory(true);
        TechGrid.SetInventoryOwnerType("Ship");
        TechGrid.SetInventoryGroup("Ship");
    }

    public override IEnumerable<Controls.InventoryGridViewModel> Grids => [CargoGrid, TechGrid];

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _iconManager = iconManager;
        _saveData = saveData;

        CargoGrid.SetDatabase(database);
        CargoGrid.SetIconManager(iconManager);
        TechGrid.SetDatabase(database);
        TechGrid.SetIconManager(iconManager);

        try
        {
            RefreshTypeItems();

            _playerState = saveData.GetObject("PlayerStateData");
            if (_playerState == null) return;

            _shipOwnership = _playerState.GetArray("ShipOwnership");
            if (_shipOwnership == null) return;

            _primaryShipIndex = 0;
            try { _primaryShipIndex = _playerState.GetInt("PrimaryShip"); } catch { }
            PrimaryShipLabel = StarshipLogic.GetPrimaryShipName(_shipOwnership, _primaryShipIndex);

            RefreshShipList();
            RefreshArchive();

            if (ShipList.Count > 0)
            {
                int selectIdx = 0;
                for (int i = 0; i < _shipDataIndices.Count; i++)
                {
                    if (_shipDataIndices[i] == _primaryShipIndex)
                    {
                        selectIdx = i;
                        break;
                    }
                }
                SelectedShipIndex = Math.Clamp(selectIdx, 0, ShipList.Count - 1);
            }
        }
        catch { }
    }

    public override void SaveData(JsonObject saveData)
    {
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            var ships = playerState.GetArray("ShipOwnership");
            if (ships == null || SelectedShipIndex < 0 || SelectedShipIndex >= _shipDataIndices.Count) return;

            int idx = _shipDataIndices[SelectedShipIndex];
            if (idx >= ships.Length) return;

            var ship = ships.GetObject(idx);

            var values = new StarshipLogic.ShipSaveValues
            {
                Name = ShipName,
                SelectedTypeName = GetSelectedTypeInternalName(),
                ClassIndex = SelectedClassIndex,
                Seed = ShipSeed,
                Damage = Damage,
                Shield = Shield,
                Hyperdrive = Hyperdrive,
                Maneuver = Maneuver,
                UseOldColours = UseOldColours,
                ShipIndex = idx,
                PrimaryShipIndex = _primaryShipIndex
            };

            StarshipLogic.SaveShipData(ship, playerState, values);
            SaveCustomisation(idx);
        }
        catch { }
    }

    partial void OnSelectedShipIndexChanged(int value)
    {
        if (value < 0 || value >= ShipList.Count) return;
        if (_shipOwnership == null) return;

        try
        {
            int idx = _shipDataIndices[value];
            if (idx >= _shipOwnership.Length) return;

            var ship = _shipOwnership.GetObject(idx);
            var data = StarshipLogic.LoadShipData(ship, _playerState, idx);

            ShipName = data.Name;
            SelectTypeByName(data.ShipTypeName);
            SelectedClassIndex = data.ClassIndex;
            ShipSeed = data.Seed;
            UseOldColours = data.UseOldColours;
            Damage = data.Damage;
            Shield = data.Shield;
            Hyperdrive = data.Hyperdrive;
            Maneuver = data.Maneuver;

            IsCorvette = StarshipLogic.IsCorvette(data.Filename);

            string ownerType = StarshipLogic.GetOwnerTypeForShip(data.ShipTypeName);
            CargoGrid.SetInventoryOwnerType(ownerType);
            TechGrid.SetInventoryOwnerType(ownerType);

            CargoGrid.LoadInventory(data.Inventory);
            TechGrid.LoadInventory(data.TechInventory);

            CargoGrid.MaxSupportedText = data.CargoMaxLabel;
            TechGrid.MaxSupportedText = data.TechMaxLabel;

            SceneResource = data.Filename;
            LoadCustomisation(idx, data.Filename);
            RefreshCorvetteState(idx);
        }
        catch { }
    }

    // ================================ Customisation ================================

    /// <summary>Builds the customisation tab for a ship, or explains why it is unavailable.</summary>
    private void LoadCustomisation(int shipIndex, string filename)
    {
        PartSlots = new ObservableCollection<ShipPartSlotViewModel>();
        TextureGroups = new ObservableCollection<ShipTextureGroupViewModel>();
        ColourChannels = new ObservableCollection<ShipColourChannelViewModel>();
        Palettes = new ObservableCollection<string>();
        SelectedPaletteIndex = -1;
        _customisationConfig = null;
        _customisationShipIndex = -1;

        RefreshSceneResources();

        // Corvettes are built from parts rather than customised, so the tab does not apply.
        if (StarshipLogic.IsCorvette(filename))
        {
            CustomisationAvailable = false;
            CustomisationMessage = UiStrings.Get("starship.customisation_corvette_disabled");
            return;
        }

        var config = ShipCustomisationDatabase.GetConfigByResource(filename);
        if (config is null)
        {
            CustomisationAvailable = false;
            CustomisationMessage = UiStrings.Get("starship.customisation_no_config");
            return;
        }

        _customisationConfig = config;
        _customisationShipIndex = shipIndex;
        CustomisationAvailable = true;
        CustomisationMessage = "";

        var ccd = StarshipLogic.GetShipCustomisation(_playerState?.GetArray("CharacterCustomisationData"), shipIndex);
        var descriptorGroups = ShipCustomisationIo.ReadDescriptorGroups(ccd);

        foreach (var slot in config.Slots)
        {
            var vm = new ShipPartSlotViewModel(slot);
            vm.SelectFromDescriptorGroups(descriptorGroups);
            PartSlots.Add(vm);
        }

        var textures = ShipCustomisationIo.ReadTextureOptions(ccd);
        foreach (var group in config.TextureGroups)
        {
            var vm = new ShipTextureGroupViewModel(group);
            vm.SelectOption(textures.GetValueOrDefault(group.GroupID));
            TextureGroups.Add(vm);
        }

        foreach (string paletteId in config.PaletteIDs) Palettes.Add(paletteId);
        if (Palettes.Count > 0)
        {
            string current = ShipCustomisationIo.ReadPaletteId(ccd);
            int idx = Palettes.ToList().FindIndex(p =>
                string.Equals(p, current, StringComparison.OrdinalIgnoreCase));
            SelectedPaletteIndex = idx >= 0 ? idx : 0;
        }

        foreach (var extra in config.ExtraColourChannels)
        {
            string label = string.IsNullOrEmpty(extra.LabelKey)
                ? extra.PaletteName
                : UiStrings.Get(extra.LabelKey);
            var vm = new ShipColourChannelViewModel(label, extra.PaletteName, extra.ColourAlt, extra.DisplayPaletteId);
            vm.SetColour(ShipCustomisationIo.ReadColour(ccd, extra.PaletteName, extra.ColourAlt));
            vm.LoadChoices(SelectedPaletteIndex >= 0 ? Palettes[SelectedPaletteIndex] : null);
            ColourChannels.Add(vm);
        }

        // Sails take their colour from a channel the game may ignore for some hulls.
        ShowSailColourWarning = config.ExtraColourChannels
            .Any(c => c.PaletteName.Contains("Sail", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Writes the customisation selections back, if the tab is showing this ship.</summary>
    private void SaveCustomisation(int shipIndex)
    {
        if (!CustomisationAvailable || _customisationShipIndex != shipIndex) return;

        var ccd = StarshipLogic.GetShipCustomisation(_playerState?.GetArray("CharacterCustomisationData"), shipIndex);
        if (ccd is null) return;

        string? palette = SelectedPaletteIndex >= 0 && SelectedPaletteIndex < Palettes.Count
            ? Palettes[SelectedPaletteIndex]
            : null;

        ShipCustomisationIo.Write(ccd, PartSlots, TextureGroups, palette);

        foreach (var channel in ColourChannels)
        {
            if (channel.Swatch is SolidColorBrush brush)
                ShipCustomisationIo.WriteColour(ccd, channel.PaletteName, channel.ColourAlt, brush.Color);
        }
    }

    /// <summary>Applies a colour the user picked from a channel's palette grid.</summary>
    public void ApplyColourChoice(ShipColourChannelViewModel channel, ShipPaletteSwatch swatch)
        => channel.SetColour(swatch.Colour);

    partial void OnSelectedPaletteIndexChanged(int value)
    {
        if (value < 0 || value >= Palettes.Count) return;
        // The palette determines which grid each swatch offers.
        foreach (var channel in ColourChannels) channel.LoadChoices(Palettes[value]);
    }

    /// <summary>Offers the resource paths that have customisation data, plus the known ship types.</summary>
    private void RefreshSceneResources()
    {
        var paths = new List<string>();
        foreach (string path in ShipCustomisationDatabase.AllResourcePaths) paths.Add(path);
        // Only ships with a non-canonical resource carry a concrete path of their own;
        // for the rest the canonical filename comes from the type name.
        foreach (var item in _typeItems)
        {
            string? path = item.CustomFilename ?? StarshipLogic.LookupFilenameForType(item.InternalName);
            if (!string.IsNullOrEmpty(path) && !paths.Contains(path, StringComparer.OrdinalIgnoreCase))
                paths.Add(path);
        }
        SceneResources = new ObservableCollection<string>(paths);
    }

    /// <summary>Re-points the ship at a different model, which changes its customisation options.</summary>
    [RelayCommand]
    private void ApplyScene()
    {
        if (SelectedShipIndex < 0 || _shipOwnership is null) return;
        string resource = SceneResource.Trim();
        if (resource.Length == 0) return;

        int idx = _shipDataIndices[SelectedShipIndex];
        if (idx >= _shipOwnership.Length) return;

        var shipResource = _shipOwnership.GetObject(idx).GetObject("Resource");
        if (shipResource is null) return;
        if (string.Equals(shipResource.GetString("Filename") ?? "", resource, StringComparison.OrdinalIgnoreCase))
            return;

        shipResource.Set("Filename", resource);

        var (displayName, _, _) = StarshipLogic.GetShipInfo(resource);
        SelectTypeByName(displayName);
        IsCorvette = StarshipLogic.IsCorvette(resource);
        LoadCustomisation(idx, resource);
    }

    // =================================== Corvette ==================================

    private void RefreshCorvetteState(int shipIndex)
    {
        IsCorvetteOptimised = IsCorvette
            && StarshipLogic.IsCorvetteOptimised(_saveData?.GetObject("PlayerStateData")?.GetArray("PersistentPlayerBases")
                ?? _playerState?.GetArray("PersistentPlayerBases"), shipIndex);
    }

    /// <summary>
    /// Reorders a corvette's building objects into the game's expected build order.
    /// An unsorted list makes the game render parts in the wrong sequence.
    /// </summary>
    [RelayCommand]
    private async Task OptimiseCorvetteAsync()
    {
        if (SelectedShipIndex < 0 || Dialogs is null) return;
        int idx = _shipDataIndices[SelectedShipIndex];

        var bases = _playerState?.GetArray("PersistentPlayerBases");
        if (bases is null)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.corvette_no_base"), Services.DialogIcon.Warning);
            return;
        }

        int moved = StarshipLogic.OptimiseCorvetteBase(bases, idx);
        RefreshCorvetteState(idx);
        await Dialogs.ShowMessageAsync(UiStrings.Get("starship.optimise"),
            UiStrings.Format("starship.optimise_done", moved.ToString(System.Globalization.CultureInfo.CurrentCulture)));
    }

    // =================================== Archive ===================================

    private void RefreshArchive()
    {
        ArchivedShips = new ObservableCollection<string>();
        _archiveDataIndices.Clear();

        var archived = _playerState?.GetArray("ArchivedShips") ?? _saveData?.GetObject("PlayerStateData")?.GetArray("ArchivedShips");
        if (archived is null) return;

        foreach (var item in StarshipLogic.BuildArchivedShipList(archived))
        {
            ArchivedShips.Add(item.DisplayName);
            _archiveDataIndices.Add(item.ArchiveIndex);
        }
    }

    /// <summary>Moves the selected ship into the archive, freeing its owned slot.</summary>
    [RelayCommand]
    private async Task ArchiveShipAsync()
    {
        if (SelectedShipIndex < 0 || _shipOwnership is null || Dialogs is null) return;
        int idx = _shipDataIndices[SelectedShipIndex];

        if (idx == _primaryShipIndex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.archive_primary_blocked"), Services.DialogIcon.Warning);
            return;
        }

        var ship = _shipOwnership.GetObject(idx);
        if (StarshipLogic.IsCorvette(StarshipLogic.LoadShipData(ship, _playerState, idx).Filename))
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.archive_corvette_blocked"), Services.DialogIcon.Warning);
            return;
        }

        var archived = _playerState?.GetArray("ArchivedShips");
        if (archived is null)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.archive_no_slots"), Services.DialogIcon.Warning);
            return;
        }

        int slot = StarshipLogic.FindEmptyArchivedShipSlot(archived);
        if (slot < 0)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.archive_no_slots"), Services.DialogIcon.Warning);
            return;
        }

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("starship.archive_move_title"),
                UiStrings.Get("starship.archive_move_confirm")))
            return;

        StarshipLogic.MoveShipToArchive(ship, idx, archived.GetObject(slot),
            _playerState?.GetArray("CharacterCustomisationData"), UseOldColours);

        RefreshShipList();
        RefreshArchive();
    }

    /// <summary>Brings an archived ship back into an empty owned slot.</summary>
    [RelayCommand]
    private async Task ImportFromArchiveAsync()
    {
        if (SelectedArchiveIndex < 0 || _shipOwnership is null || Dialogs is null) return;

        var archived = _playerState?.GetArray("ArchivedShips");
        if (archived is null) return;

        int target = StarshipLogic.FindEmptySlot(_shipOwnership);
        if (target < 0)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("starship.archive_no_list_slots"), Services.DialogIcon.Warning);
            return;
        }

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("starship.archive_import_title"),
                UiStrings.Get("starship.archive_import_confirm")))
            return;

        int slot = _archiveDataIndices[SelectedArchiveIndex];
        StarshipLogic.ImportShipFromArchive(archived.GetObject(slot), _shipOwnership.GetObject(target),
            target, _playerState?.GetArray("CharacterCustomisationData"));

        RefreshShipList();
        RefreshArchive();
    }

    [RelayCommand]
    private void GenerateSeed()
    {
        ShipSeed = $"0x{Random.Shared.NextInt64():X16}";
    }

    [RelayCommand]
    private void MakePrimary()
    {
        if (_shipOwnership == null || SelectedShipIndex < 0 || SelectedShipIndex >= _shipDataIndices.Count) return;
        int idx = _shipDataIndices[SelectedShipIndex];
        if (idx >= _shipOwnership.Length) return;

        _primaryShipIndex = idx;
        PrimaryShipLabel = StarshipLogic.GetPrimaryShipName(_shipOwnership, _primaryShipIndex);
    }

    private void RefreshShipList()
    {
        ShipList.Clear();
        _shipDataIndices.Clear();
        if (_shipOwnership == null) return;
        foreach (var item in StarshipLogic.BuildShipList(_shipOwnership))
        {
            ShipList.Add(item.DisplayName);
            _shipDataIndices.Add(item.DataIndex);
        }
    }

    private void RefreshTypeItems()
    {
        _typeItems = StarshipLogic.GetShipTypeItems();
        ShipTypes.Clear();
        foreach (var item in _typeItems)
            ShipTypes.Add(item.DisplayName);
    }

    private string? GetSelectedTypeInternalName()
    {
        if (SelectedTypeIndex < 0 || SelectedTypeIndex >= _typeItems.Length) return null;
        return _typeItems[SelectedTypeIndex].InternalName;
    }

    private void SelectTypeByName(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName)) { SelectedTypeIndex = -1; return; }
        for (int i = 0; i < _typeItems.Length; i++)
        {
            if (_typeItems[i].InternalName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedTypeIndex = i;
                return;
            }
        }
        SelectedTypeIndex = -1;
    }

    /// <summary>File dialog funcs set by view code-behind.</summary>
    public Func<string, string, Task<string?>>? SaveFileFunc { get; set; }
    public Func<string, Task<string?>>? OpenFileFunc { get; set; }

    [RelayCommand]
    private async Task ExportShip()
    {
        if (_shipOwnership == null || SelectedShipIndex < 0 || SaveFileFunc == null) return;
        int idx = _shipDataIndices[SelectedShipIndex];
        if (idx >= _shipOwnership.Length) return;

        var ship = _shipOwnership.GetObject(idx);
        if (ship == null) return;

        var cfg = ExportConfig.Instance;
        var vars = new Dictionary<string, string>
        {
            ["ship_name"] = ShipName,
            ["type"] = ShipTypes.Count > 0 && SelectedTypeIndex >= 0 ? ShipTypes[SelectedTypeIndex] : "",
            ["class"] = ShipClasses.Count > 0 && SelectedClassIndex >= 0 ? ShipClasses[SelectedClassIndex] : ""
        };
        string fileName = ExportConfig.BuildFileName(cfg.StarshipTemplate, cfg.StarshipExt, vars);
        var path = await SaveFileFunc(fileName, cfg.StarshipExt);
        if (path != null)
            ship.ExportToFile(path);
    }

    [RelayCommand]
    private async Task ImportShip()
    {
        if (_shipOwnership == null || SelectedShipIndex < 0 || OpenFileFunc == null) return;
        int idx = _shipDataIndices[SelectedShipIndex];
        if (idx >= _shipOwnership.Length) return;

        var path = await OpenFileFunc(ExportConfig.Instance.StarshipExt);
        if (path == null) return;

        var imported = JsonObject.ImportFromFile(path);
        if (imported == null) return;

        _shipOwnership.Set(idx, imported);
        OnSelectedShipIndexChanged(SelectedShipIndex);
    }

    [RelayCommand]
    private void DeleteShip()
    {
        if (_shipOwnership == null || SelectedShipIndex < 0) return;
        int idx = _shipDataIndices[SelectedShipIndex];
        if (idx >= _shipOwnership.Length) return;

        _shipOwnership.RemoveAt(idx);
        RefreshShipList();
        if (ShipList.Count > 0)
            SelectedShipIndex = 0;
    }
}
