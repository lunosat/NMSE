using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;
using NMSE.UI.ViewModels.Controls;
using NMSE.Core.Utilities;
using System.Globalization;

namespace NMSE.UI.ViewModels.Panels;

public partial class FreighterViewModel : PanelViewModelBase
{
    private JsonObject? _playerState;

    /// <summary>The save's freighter base, kept so backup and restore can target it.</summary>
    private JsonObject? _freighterBase;
    private GameItemDatabase? _database;
    private IconManager? _iconManager;

    [ObservableProperty] private string _freighterName = "";
    [ObservableProperty] private ObservableCollection<string> _freighterTypes = new();
    [ObservableProperty] private int _selectedTypeIndex = -1;
    [ObservableProperty] private ObservableCollection<string> _freighterClasses = new(FreighterLogic.FreighterClasses);
    [ObservableProperty] private int _selectedClassIndex = -1;

    [ObservableProperty] private string _homeSeed = "";
    [ObservableProperty] private string _modelSeed = "";

    [ObservableProperty] private double _hyperdrive;
    [ObservableProperty] private double _fleetCoordination;

    [ObservableProperty] private string _baseItemsText = "";

    [ObservableProperty] private ObservableCollection<string> _crewRaces = new();
    [ObservableProperty] private int _selectedCrewRaceIndex = -1;
    [ObservableProperty] private string _crewSeed = "";

    [ObservableProperty] private ObservableCollection<string> _roomList = new();

    [ObservableProperty] private InventoryGridViewModel _cargoGrid = new();
    [ObservableProperty] private InventoryGridViewModel _techGrid = new();

    private FreighterLogic.FreighterTypeItem[] _typeItems = [];
    private FreighterLogic.CrewRaceItem[] _crewRaceItems = [];

    public FreighterViewModel()
    {
        CargoGrid.SetIsCargoInventory(true);
        CargoGrid.SetInventoryOwnerType("Freighter");
        CargoGrid.SetInventoryGroup("Freighter");

        TechGrid.SetIsTechInventory(true);
        TechGrid.SetInventoryOwnerType("Freighter");
        TechGrid.SetInventoryGroup("Freighter");
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        _database = database;
        _iconManager = iconManager;
        CargoGrid.SetDatabase(database);
        CargoGrid.SetIconManager(iconManager);
        TechGrid.SetDatabase(database);
        TechGrid.SetIconManager(iconManager);

        try
        {
            _playerState = saveData.GetObject("PlayerStateData");
            if (_playerState == null) return;

            RefreshTypeItems();
            RefreshCrewRaceItems();

            var data = FreighterLogic.LoadFreighterData(_playerState);
            _freighterBase = data.FreighterBase;

            FreighterName = data.Name;

            if (data.TypeDisplayName != null)
                SelectTypeByName(data.TypeDisplayName);
            else
                SelectedTypeIndex = -1;

            SelectedClassIndex = data.ClassIndex >= 0 ? data.ClassIndex : -1;
            HomeSeed = data.HomeSeed;
            ModelSeed = data.ModelSeed;
            Hyperdrive = data.Hyperdrive;
            FleetCoordination = data.FleetCoordination;

            BaseItemsText = data.FreighterBase != null ? data.BaseItemCount.ToString(CultureInfo.InvariantCulture) : "N/A";

            CargoGrid.LoadInventory(data.CargoInventory);
            TechGrid.LoadInventory(data.TechInventory);

            RoomList.Clear();
            foreach (var room in FreighterLogic.DetectFreighterRooms(data.FreighterBase))
                RoomList.Add(room);

            try
            {
                var npc = _playerState.GetObject("CurrentFreighterNPC");
                if (npc != null)
                {
                    string filename = npc.GetString("Filename") ?? "";
                    if (FreighterLogic.NpcResourceToRace.TryGetValue(filename, out string? race))
                        SelectCrewRaceByName(race);
                    else
                        SelectedCrewRaceIndex = -1;

                    try
                    {
                        var seedArr = npc.GetArray("Seed");
                        CrewSeed = (seedArr != null && seedArr.Length >= 2) ? (seedArr.GetString(1) ?? "") : "";
                    }
                    catch { CrewSeed = ""; }
                }
            }
            catch { }
        }
        catch { }
    }

    public override void SaveData(JsonObject saveData)
    {
        try
        {
            var playerState = saveData.GetObject("PlayerStateData");
            if (playerState == null) return;

            FreighterLogic.SaveFreighterData(playerState, new FreighterLogic.FreighterSaveValues
            {
                Name = FreighterName,
                SelectedTypeName = GetSelectedTypeInternalName(),
                ClassIndex = SelectedClassIndex,
                HomeSeed = HomeSeed,
                ModelSeed = ModelSeed,
                Hyperdrive = Hyperdrive,
                FleetCoordination = FleetCoordination,
            });

            try
            {
                var npc = playerState.GetObject("CurrentFreighterNPC");
                if (npc != null)
                {
                    string? selectedRace = GetSelectedCrewRaceInternalName();
                    if (!string.IsNullOrEmpty(selectedRace) && FreighterLogic.RaceToNpcResource.TryGetValue(selectedRace, out string? resource))
                        npc.Set("Filename", resource);

                    var seedArr = npc.GetArray("Seed");
                    var normalizedCrewSeed = SeedHelper.NormalizeSeed(CrewSeed);
                    if (seedArr != null && seedArr.Length >= 2 && normalizedCrewSeed != null)
                        seedArr.Set(1, normalizedCrewSeed);
                }
            }
            catch { }
        }
        catch { }
    }

    [RelayCommand]
    private void GenerateHomeSeed()
    {
        HomeSeed = $"0x{Random.Shared.NextInt64():X16}";
    }

    [RelayCommand]
    private void GenerateModelSeed()
    {
        ModelSeed = $"0x{Random.Shared.NextInt64():X16}";
    }

    [RelayCommand]
    private void GenerateCrewSeed()
    {
        CrewSeed = $"0x{Random.Shared.NextInt64():X16}";
    }

    private void RefreshTypeItems()
    {
        _typeItems = FreighterLogic.GetFreighterTypeItems();
        FreighterTypes.Clear();
        foreach (var item in _typeItems)
            FreighterTypes.Add(item.DisplayName);
    }

    private void RefreshCrewRaceItems()
    {
        _crewRaceItems = FreighterLogic.GetCrewRaceItems();
        CrewRaces.Clear();
        foreach (var item in _crewRaceItems)
            CrewRaces.Add(item.DisplayName);
    }

    private string? GetSelectedTypeInternalName()
    {
        if (SelectedTypeIndex < 0 || SelectedTypeIndex >= _typeItems.Length) return null;
        return _typeItems[SelectedTypeIndex].InternalName;
    }

    private string? GetSelectedCrewRaceInternalName()
    {
        if (SelectedCrewRaceIndex < 0 || SelectedCrewRaceIndex >= _crewRaceItems.Length) return null;
        return _crewRaceItems[SelectedCrewRaceIndex].InternalName;
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

    private void SelectCrewRaceByName(string? raceName)
    {
        if (string.IsNullOrEmpty(raceName)) { SelectedCrewRaceIndex = -1; return; }
        for (int i = 0; i < _crewRaceItems.Length; i++)
        {
            if (_crewRaceItems[i].InternalName.Equals(raceName, StringComparison.OrdinalIgnoreCase))
            {
                SelectedCrewRaceIndex = i;
                return;
            }
        }
        SelectedCrewRaceIndex = -1;
    }

    [RelayCommand]
    private async Task BackupBaseAsync()
    {
        if (Dialogs is null) return;

        if (_freighterBase is null)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("freighter.backup_title"),
                UiStrings.Get("freighter.backup_no_base"));
            return;
        }

        if (SaveFilePickerFunc is null) return;

        var config = ExportConfig.Instance;
        var vars = new Dictionary<string, string>
        {
            ["freighter_name"] = FreighterName,
            ["type"] = SelectedTypeIndex >= 0 && SelectedTypeIndex < FreighterTypes.Count
                ? FreighterTypes[SelectedTypeIndex] : "",
            ["class"] = SelectedClassIndex >= 0 && SelectedClassIndex < FreighterClasses.Count
                ? FreighterClasses[SelectedClassIndex] : "",
        };

        string? path = await SaveFilePickerFunc(UiStrings.Get("freighter.backup_title"),
            config.FreighterExt.TrimStart('.'),
            ExportConfig.BuildFileName(config.FreighterTemplate, config.FreighterExt, vars));
        if (path is null) return;

        try
        {
            _freighterBase.ExportToFile(path);
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("freighter.backup_failed", ex.Message), Services.DialogIcon.Error);
        }
    }

    /// <summary>
    /// Restores a freighter base backup over the save's existing freighter base. The
    /// base is matched by type and version rather than by index, which shifts.
    /// </summary>
    [RelayCommand]
    private async Task RestoreBaseAsync()
    {
        if (Dialogs is null || OpenFilePickerFunc is null || _playerState is null) return;

        string? path = await OpenFilePickerFunc(UiStrings.Get("freighter.restore_title"),
            ExportConfig.Instance.FreighterExt);
        if (path is null) return;

        try
        {
            var imported = JsonObject.ImportFromFile(path);
            var bases = _playerState.GetArray("PersistentPlayerBases");
            if (bases is null) return;

            for (int i = 0; i < bases.Length; i++)
            {
                var candidate = bases.GetObject(i);
                var baseType = candidate?.GetObject("BaseType");
                if (baseType is null) continue;
                if (baseType.GetString("PersistentBaseTypes") != "FreighterBase") continue;
                if (candidate!.GetInt("BaseVersion") < 3) continue;

                foreach (string name in imported.Names())
                    candidate.Set(name, imported.Get(name));

                _freighterBase = candidate;
                BaseItemsText = (candidate.GetArray("Objects")?.Length ?? 0)
                    .ToString(CultureInfo.InvariantCulture);

                await Dialogs.ShowMessageAsync(UiStrings.Get("freighter.restore_title"),
                    UiStrings.Get("freighter.restore_success"));
                return;
            }

            await Dialogs.ShowMessageAsync(UiStrings.Get("freighter.restore_title"),
                UiStrings.Get("freighter.restore_no_slot"), Services.DialogIcon.Warning);
        }
        catch (Exception ex)
        {
            await Dialogs.ShowMessageAsync(UiStrings.Get("common.error"),
                UiStrings.Format("freighter.restore_failed", ex.Message), Services.DialogIcon.Error);
        }
    }
}
