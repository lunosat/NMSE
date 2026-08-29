using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using NMSE.Core;
using NMSE.Core.Utilities;
using NMSE.Data;
using NMSE.Models;
using NMSE.UI.ViewModels.Controls;

namespace NMSE.UI.ViewModels.Panels;

public partial class ExocraftViewModel : PanelViewModelBase
{
    private JsonArray? _vehicleOwnership;
    private JsonObject? _playerState;
    private JsonObject? _savedPlayerState;
    private readonly List<int> _addedVehicleIndices = new();

    [ObservableProperty] private ObservableCollection<string> _vehicleList = new();
    [ObservableProperty] private int _selectedVehicleIndex = -1;

    [ObservableProperty] private string _vehicleName = "";
    [ObservableProperty] private bool _thirdPersonCamera;
    [ObservableProperty] private bool _minotaurAI;
    [ObservableProperty] private bool _isPrimaryVehicle;

    [ObservableProperty] private InventoryGridViewModel _cargoGrid = new();
    [ObservableProperty] private InventoryGridViewModel _techGrid = new();

    // --- Stations -------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<ExocraftStationViewModel> _individualStations = new();
    [ObservableProperty] private ObservableCollection<ExocraftStationViewModel> _baseStations = new();
    [ObservableProperty] private ExocraftStationViewModel? _selectedStation;
    [ObservableProperty] private bool _hasStationSelection;
    [ObservableProperty] private bool _isDeployed;
    [ObservableProperty] private string _deployedStatus = "";


    private JsonArray? _baseBuildingObjects;
    private JsonArray? _persistentPlayerBases;

    private JsonObject? _saveDataRef;

    public ExocraftViewModel()
    {
        CargoGrid.SetIsCargoInventory(true);
        CargoGrid.SetSuperchargeDisabled(true);
        CargoGrid.SetInventoryOwnerType("Vehicle");
        CargoGrid.SetInventoryGroup("Vehicle");

        TechGrid.SetIsTechInventory(true);
        TechGrid.SetSuperchargeDisabled(true);
        TechGrid.SetSlotToggleDisabled(true);
        TechGrid.SetInventoryOwnerType("Vehicle");
        TechGrid.SetInventoryGroup("Vehicle");
    }

    public override IEnumerable<Controls.InventoryGridViewModel> Grids => [CargoGrid, TechGrid];

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _saveDataRef = saveData;
        CargoGrid.SetDatabase(database);
        CargoGrid.SetIconManager(iconManager);
        TechGrid.SetDatabase(database);
        TechGrid.SetIconManager(iconManager);

        try
        {
            VehicleList.Clear();
            _addedVehicleIndices.Clear();
            CargoGrid.LoadInventory(null);
            TechGrid.LoadInventory(null);

            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            _savedPlayerState = playerState;
            _playerState = playerState;
            LoadStations(playerState);

            _vehicleOwnership = playerState.GetArray("VehicleOwnership");
            if (_vehicleOwnership == null) return;

            foreach (var (index, name) in ExocraftLogic.VehicleTypes)
            {
                if (index < _vehicleOwnership.Length)
                {
                    VehicleList.Add(ExocraftLogic.GetLocalisedVehicleTypeName(name));
                    _addedVehicleIndices.Add(index);
                }
            }

            if (VehicleList.Count > 0)
                SelectedVehicleIndex = 0;

            try { ThirdPersonCamera = saveData.GetObject("CommonStateData")?.GetBool("UsesThirdPersonVehicleCam") ?? false; } catch { ThirdPersonCamera = false; }
            try { MinotaurAI = playerState.GetBool("VehicleAIControlEnabled"); } catch { MinotaurAI = false; }
        }
        catch { }
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToVehicleJsonAsync() => GoToJsonAsync("PlayerStateData", "VehicleOwnership");

    [RelayCommand]
    private Task GoToVehicleCargoJsonAsync()
    {
        int idx = SelectedVehicleArrayIndex;
        return idx < 0
            ? Task.CompletedTask
            : GoToJsonAsync("PlayerStateData", "VehicleOwnership", $"[{idx}]", "Inventory");
    }

    [RelayCommand]
    private Task GoToBaseBuildingJsonAsync() => GoToJsonAsync("PlayerStateData", "BaseBuildingObjects");

    [RelayCommand]
    private Task GoToBaseStationsJsonAsync() => GoToJsonAsync("PlayerStateData", "PersistentPlayerBases");

    public string GoToVehicleListTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("exocraft.title"));

    public string GoToVehicleCargoTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("common.cargo"));

    public string GoToBaseBuildingTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("exocraft.individual_stations"));

    public string GoToBaseStationsTooltip =>
        UiStrings.Format("goto_json.tooltip_section", UiStrings.Get("goto_json.nav_base_stations"));

    public override void ApplyLocalisation()
    {
        OnPropertyChanged(nameof(GoToVehicleListTooltip));
        OnPropertyChanged(nameof(GoToVehicleCargoTooltip));
        OnPropertyChanged(nameof(GoToBaseBuildingTooltip));
        OnPropertyChanged(nameof(GoToBaseStationsTooltip));
        OnIsDeployedChanged(IsDeployed);
    }

    /// <summary>Reveals this data where it lives in the raw editor.</summary>
    [RelayCommand]
    private Task GoToStationsJsonAsync() => GoToJsonAsync("PlayerStateData", "BaseBuildingObjects");

    public override void SaveData(JsonObject saveData)
    {
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            var vehicles = playerState.GetArray("VehicleOwnership");
            if (vehicles == null || SelectedVehicleIndex < 0) return;

            int selIdx = SelectedVehicleIndex;
            if (selIdx >= _addedVehicleIndices.Count) return;
            int arrIdx = _addedVehicleIndices[selIdx];

            var vehicle = vehicles.GetObject(arrIdx);
            try { vehicle.Set("Name", VehicleName); } catch { }

            try { saveData.GetObject("CommonStateData")?.Set("UsesThirdPersonVehicleCam", ThirdPersonCamera); } catch { }
            try { playerState.Set("VehicleAIControlEnabled", MinotaurAI); } catch { }
        }
        catch { }
    }

    partial void OnSelectedVehicleIndexChanged(int value)
    {
        if (_vehicleOwnership == null || value < 0 || value >= _addedVehicleIndices.Count) return;

        try
        {
            int arrIdx = _addedVehicleIndices[value];
            var vehicle = _vehicleOwnership.GetObject(arrIdx);

            string vehicleName = GetSelectedVehicleInternalName();
            string ownerType = ExocraftLogic.GetOwnerTypeForVehicle(vehicleName);
            CargoGrid.SetInventoryOwnerType(ownerType);
            TechGrid.SetInventoryOwnerType(ownerType);

            CargoGrid.LoadInventory(vehicle.GetObject("Inventory"));
            TechGrid.LoadInventory(vehicle.GetObject("Inventory_TechOnly"));

            try { VehicleName = vehicle.GetString("Name") ?? ""; } catch { VehicleName = ""; }

            try
            {
                int primaryIdx = _savedPlayerState?.GetInt("PrimaryVehicle") ?? -1;
                IsPrimaryVehicle = (arrIdx == primaryIdx);
            }
            catch { IsPrimaryVehicle = false; }

            // A vehicle with no Location is in storage, not standing in the world; only
            // a deployed one can be recalled.
            try
            {
                long location = 0;
                try { location = vehicle.GetLong("Location"); } catch { }
                IsDeployed = location != 0;
            }
            catch { IsDeployed = false; }
        }
        catch { }
    }

    partial void OnIsDeployedChanged(bool value)
    {
        DeployedStatus = UiStrings.Get(value ? "exocraft.status_deployed" : "exocraft.status_not_deployed");
    }

    private string GetSelectedVehicleInternalName()
    {
        int selIdx = SelectedVehicleIndex;
        if (selIdx < 0 || selIdx >= _addedVehicleIndices.Count) return "vehicle";
        int arrIdx = _addedVehicleIndices[selIdx];
        foreach (var (index, name) in ExocraftLogic.VehicleTypes)
        {
            if (index == arrIdx) return name;
        }
        return "vehicle";
    }

    /// <summary>The array index of the selected vehicle, or -1.</summary>
    private int SelectedVehicleArrayIndex =>
        SelectedVehicleIndex >= 0 && SelectedVehicleIndex < _addedVehicleIndices.Count
            ? _addedVehicleIndices[SelectedVehicleIndex] : -1;

    [RelayCommand]
    private async Task ExportAsync()
    {
        int idx = SelectedVehicleArrayIndex;
        if (Dialogs is null || _vehicleOwnership is null || idx < 0)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.export"),
                    UiStrings.Get("exocraft.no_vehicle_selected"));
            return;
        }

        if (SaveFilePickerFunc is null) return;
        string suggested = ExocraftLogic.BuildVehicleExportFileName(VehicleName);
        string? path = await SaveFilePickerFunc(UiStrings.Get("exocraft.export"),
            ExportConfig.Instance.ExocraftExt.TrimStart('.'), suggested);
        if (path is null) return;

        try
        {
            _vehicleOwnership.GetObject(idx).ExportToFile(path);
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("common.export_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        int idx = SelectedVehicleArrayIndex;
        if (Dialogs is null || _vehicleOwnership is null || idx < 0)
        {
            if (Dialogs is not null)
                await Dialogs.ShowMessageAsync(UiStrings.Get("common.import"),
                    UiStrings.Get("exocraft.no_vehicle_selected"));
            return;
        }

        if (OpenFilePickerFunc is null) return;
        string? path = await OpenFilePickerFunc(UiStrings.Get("exocraft.import"),
            ExportConfig.Instance.ExocraftExt);
        if (path is null) return;

        try
        {
            var imported = JsonObject.ImportFromFile(path);
            // Files exported by NomNom wrap the vehicle in a Data envelope.
            imported = InventoryImportHelper.UnwrapNomNom(imported, "Vehicle");

            var vehicle = _vehicleOwnership.GetObject(idx);
            foreach (string name in imported.Names())
                vehicle.Set(name, imported.Get(name));

            CargoGrid.LoadInventory(vehicle.GetObject("Inventory"));
            TechGrid.LoadInventory(vehicle.GetObject("Inventory_TechOnly"));

            await Dialogs.ShowMessageAsync(UiStrings.Get("common.import"),
                UiStrings.Get("exocraft.import_success"));
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("common.import_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>
    /// Sends the exocraft back to storage by clearing its deployed location, which is
    /// how the game marks a vehicle as not standing anywhere in the world.
    /// </summary>
    [RelayCommand]
    private async Task UndeployAsync()
    {
        int idx = SelectedVehicleArrayIndex;
        if (Dialogs is null || _vehicleOwnership is null || idx < 0) return;

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("exocraft.undeploy_title"),
                UiStrings.Get("exocraft.undeploy_confirm")))
            return;

        try
        {
            var vehicle = _vehicleOwnership.GetObject(idx);
            vehicle.Set("Location", 0);

            // Direction is a quaternion; the resting value points straight down.
            var direction = vehicle.GetArray("Direction");
            if (direction is not null && direction.Length >= 4)
            {
                direction.Set(0, 0.0);
                direction.Set(1, 0.0);
                direction.Set(2, 0.0);
                direction.Set(3, -1.0);
            }
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("exocraft.undeploy_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    // =================================== Stations ==================================

    /// <summary>
    /// Lists exocraft summoning stations. They appear either as loose base-building
    /// objects or as objects inside a base, and both kinds are identified by a GARAGE
    /// object id.
    /// </summary>
    private void LoadStations(JsonObject playerState)
    {
        _baseBuildingObjects = playerState.GetArray("BaseBuildingObjects");
        _persistentPlayerBases = playerState.GetArray("PersistentPlayerBases");

        var individual = new ObservableCollection<ExocraftStationViewModel>();
        var inBases = new ObservableCollection<ExocraftStationViewModel>();

        if (_baseBuildingObjects is not null)
        {
            for (int i = 0; i < _baseBuildingObjects.Length; i++)
            {
                var obj = _baseBuildingObjects.GetObject(i);
                if (IsStation(obj))
                    individual.Add(new ExocraftStationViewModel(obj!, null, i));
            }
        }

        if (_persistentPlayerBases is not null)
        {
            for (int b = 0; b < _persistentPlayerBases.Length; b++)
            {
                var baseEntry = _persistentPlayerBases.GetObject(b);
                var objects = baseEntry?.GetArray("Objects");
                if (objects is null) continue;

                for (int o = 0; o < objects.Length; o++)
                {
                    var obj = objects.GetObject(o);
                    if (IsStation(obj))
                        inBases.Add(new ExocraftStationViewModel(obj!, baseEntry, o, b));
                }
            }
        }

        IndividualStations = individual;
        BaseStations = inBases;
    }

    private static bool IsStation(JsonObject? obj) =>
        obj?.GetString("ObjectID") is { } id &&
        id.Contains("GARAGE", StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedStationChanged(ExocraftStationViewModel? value)
        => HasStationSelection = value is not null;

    [RelayCommand]
    private async Task DeleteStationAsync()
    {
        if (Dialogs is null) return;

        if (SelectedStation is null)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Get("exocraft.no_station_selected"), Services.DialogIcon.Warning);
            return;
        }

        bool inBase = SelectedStation.OwningBase is not null;
        string message = inBase
            ? UiStrings.Get("exocraft.delete_base_station_confirm")
            : UiStrings.Get("exocraft.delete_station_confirm");

        if (!await Dialogs.ConfirmAsync(UiStrings.Get("exocraft.delete_station_title"), message,
                Services.DialogIcon.Warning))
            return;

        var container = inBase
            ? SelectedStation.OwningBase!.GetArray("Objects")
            : _baseBuildingObjects;

        container?.RemoveAt(SelectedStation.Index);
        SelectedStation = null;
        if (_playerState is not null) LoadStations(_playerState);
    }
}

/// <summary>
/// An exocraft summoning station, either loose in the world or part of a base.
/// </summary>
public partial class ExocraftStationViewModel : ObservableObject
{
    public JsonObject Station { get; }

    /// <summary>The base this station belongs to, or null when it stands on its own.</summary>
    public JsonObject? OwningBase { get; }

    /// <summary>Index within its container, used to remove it.</summary>
    public int Index { get; }

    public string DisplayName { get; }
    public string BaseName { get; }
    public string Timestamp { get; }
    public string GalacticAddress { get; }
    public string RegionSeed { get; }

    /// <summary>
    /// A station inside a base takes its coordinates from the base entry, which has no
    /// region seed of its own, so the row is hidden rather than shown as unknown.
    /// </summary>
    public bool ShowRegionSeed { get; }

    public string Position { get; }
    public string Galaxy { get; }
    public string PortalCode { get; }
    public string PortalCodeDec { get; }
    public string SignalBooster { get; }
    public string VoxelX { get; }
    public string VoxelY { get; }
    public string VoxelZ { get; }
    public string SolarSystem { get; }
    public string Planet { get; }

    public ExocraftStationViewModel(JsonObject station, JsonObject? owningBase, int index, int baseIndex = 0)
    {
        Station = station;
        OwningBase = owningBase;
        Index = index;

        string objectId = CatalogueLogic.StripCaretPrefix(station.GetString("ObjectID") ?? "");
        string display = StarshipDatabase.GetDisplayName(objectId);
        DisplayName = string.IsNullOrEmpty(display) ? objectId : display;

        BaseName = owningBase is null
            ? UiStrings.Get("exocraft.station_not_in_base")
            : owningBase.GetString("Name") is { Length: > 0 } named
                ? named
                : UiStrings.Format("base.fallback_base_name", baseIndex + 1);

        long timestamp = 0;
        try { timestamp = station.GetLong("Timestamp"); }
        catch { try { timestamp = station.GetInt("Timestamp"); } catch { } }
        Timestamp = FormatTimestamp(timestamp);

        // A station in a base is positioned relative to that base, so the address that
        // locates it belongs to the base entry, not to the object.
        object? address = null;
        try { address = owningBase is not null ? owningBase.Get("GalacticAddress") : station.Get("GalacticAddress"); }
        catch { }
        address ??= station.Get("UniverseAddress");

        GalacticAddress = CoordinateHelper.NormalizeGalacticAddress(address);
        PortalCode = ExtractPortalCode(GalacticAddress);

        ShowRegionSeed = owningBase is null;
        string? seed = null;
        if (ShowRegionSeed)
        {
            try { seed = station.Get("RegionSeed")?.ToString(); } catch { }
        }
        RegionSeed = seed ?? UiStrings.Get("common.unknown");

        JsonArray? positionArray = null;
        try { positionArray = station.GetArray("Position"); } catch { }
        Position = FormatPosition(positionArray);

        // The galaxy comes from the address the station carries, not from wherever the
        // player happens to be standing. A bare 12-digit portal code has none embedded.
        int? realityIndex = GetRealityIndexFromAddress(GalacticAddress);
        Galaxy = realityIndex is int ri
            ? $"{GalaxyDatabase.GetGalaxyDisplayName(ri)} ({GalaxyDatabase.GetGalaxyType(ri)})"
            : UiStrings.Get("exocraft.station_no_galaxy_info");

        var voxel = ExocraftLogic.ParseGalacticAddressToVoxel(address);
        int vx = 0, vy = 0, vz = 0, si = 0, pi = 0;
        if (voxel is { } v)
        {
            vx = v.VoxelX; vy = v.VoxelY; vz = v.VoxelZ;
            si = v.SolarSystemIndex; pi = v.PlanetIndex;
        }

        PortalCodeDec = CoordinateHelper.PortalHexToDec(PortalCode);
        SignalBooster = CoordinateHelper.VoxelToSignalBooster(vx, vy, vz, si);
        VoxelX = vx.ToString(CultureInfo.CurrentCulture);
        VoxelY = vy.ToString(CultureInfo.CurrentCulture);
        VoxelZ = vz.ToString(CultureInfo.CurrentCulture);
        SolarSystem = si.ToString(CultureInfo.CurrentCulture);
        Planet = pi.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Pulls the 12-hex-digit portal code out of a normalised address. A 14-digit
    /// UniverseAddress carries a reality index in the middle, which is dropped here.
    /// </summary>
    private static string ExtractPortalCode(string galacticAddrHex)
    {
        if (string.IsNullOrEmpty(galacticAddrHex)
            || !galacticAddrHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return "";

        string raw = galacticAddrHex[2..];
        return raw.Length switch
        {
            12 => raw,
            14 => string.Concat(raw.AsSpan(0, 4), raw.AsSpan(6, 8)),
            _ => "",
        };
    }

    /// <summary>
    /// The galaxy index, which only the 14-digit form carries, at digits 4-5.
    /// </summary>
    private static int? GetRealityIndexFromAddress(string galacticAddrHex)
    {
        if (string.IsNullOrEmpty(galacticAddrHex)) return null;

        string raw = galacticAddrHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? galacticAddrHex[2..]
            : galacticAddrHex;

        return raw.Length == 14
            && int.TryParse(raw.AsSpan(4, 2), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out int reality)
            ? reality
            : null;
    }

    private static string FormatTimestamp(long timestamp)
    {
        if (timestamp <= 0) return UiStrings.Get("common.unknown");
        try { return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime.ToString("g", CultureInfo.CurrentCulture); }
        catch { return timestamp.ToString(CultureInfo.CurrentCulture); }
    }

    private static string FormatPosition(JsonArray? positionArray)
    {
        if (positionArray is null || positionArray.Length < 3)
            return UiStrings.Get("common.unknown");

        try
        {
            return $"X: {positionArray.GetDouble(0).ToString("F2", CultureInfo.CurrentCulture)}, "
                 + $"Y: {positionArray.GetDouble(1).ToString("F2", CultureInfo.CurrentCulture)}, "
                 + $"Z: {positionArray.GetDouble(2).ToString("F2", CultureInfo.CurrentCulture)}";
        }
        catch { return UiStrings.Get("common.unknown"); }
    }

    public override string ToString() => DisplayName;
}
