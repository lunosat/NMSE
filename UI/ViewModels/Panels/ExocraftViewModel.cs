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
    [ObservableProperty] private string _stationDetails = "";

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
        }
        catch { }
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
                        inBases.Add(new ExocraftStationViewModel(obj!, baseEntry, o));
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
        => StationDetails = value?.Details ?? "";

    [RelayCommand]
    private async Task DeleteStationAsync()
    {
        if (Dialogs is null || SelectedStation is null) return;

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
    public string Details { get; }

    public ExocraftStationViewModel(JsonObject station, JsonObject? owningBase, int index)
    {
        Station = station;
        OwningBase = owningBase;
        Index = index;

        string objectId = CatalogueLogic.StripCaretPrefix(station.GetString("ObjectID") ?? "");
        string display = StarshipDatabase.GetDisplayName(objectId);
        DisplayName = string.IsNullOrEmpty(display) ? objectId : display;

        var lines = new System.Text.StringBuilder();
        string baseName = owningBase?.GetString("Name") is { Length: > 0 } n
            ? n
            : UiStrings.Get("exocraft.station_not_in_base");
        lines.AppendLine(CultureInfo.CurrentCulture,
            $"{UiStrings.Get("exocraft.station_base_name")} {baseName}");

        long timestamp = 0;
        try { timestamp = station.GetInt("Timestamp"); } catch { }
        if (timestamp > 0)
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_timestamp")} {timestamp.ToString(CultureInfo.InvariantCulture)}");

        var address = station.Get("UniverseAddress") ?? owningBase?.Get("GalacticAddress");
        var voxel = ExocraftLogic.ParseGalacticAddressToVoxel(address);
        if (voxel is { } v)
        {
            string hex = CoordinateHelper.VoxelToPortalCode(v.VoxelX, v.VoxelY, v.VoxelZ, v.SolarSystemIndex, v.PlanetIndex);
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_portal_code")} {hex}");
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_portal_code_dec")} {CoordinateHelper.PortalHexToDec(hex)}");
            string booster = CoordinateHelper.VoxelToSignalBooster(v.VoxelX, v.VoxelY, v.VoxelZ, v.SolarSystemIndex);
            lines.AppendLine(CultureInfo.CurrentCulture,
                $"{UiStrings.Get("exocraft.station_signal_booster")} {booster}");
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_voxel_x")} {v.VoxelX.ToString(CultureInfo.InvariantCulture)}");
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_voxel_y")} {v.VoxelY.ToString(CultureInfo.InvariantCulture)}");
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_voxel_z")} {v.VoxelZ.ToString(CultureInfo.InvariantCulture)}");
            lines.AppendLine(CultureInfo.CurrentCulture, $"{UiStrings.Get("exocraft.station_solar_system")} {v.SolarSystemIndex.ToString(CultureInfo.InvariantCulture)}");
        }
        else
        {
            lines.AppendLine(UiStrings.Get("exocraft.station_no_galaxy_info"));
        }

        Details = lines.ToString();
    }

    public override string ToString() => DisplayName;
}
