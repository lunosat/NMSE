using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class FleetViewModel : PanelViewModelBase
{
    [ObservableProperty] private string _freighterTabHeader = "Freighter";
    [ObservableProperty] private string _frigateTabHeader = "Frigates";
    [ObservableProperty] private string _squadronTabHeader = "Squadron";

    public FreighterViewModel Freighter { get; } = new();
    public FrigateViewModel Frigates { get; } = new();
    public SquadronViewModel Squadron { get; } = new();

    /// <summary>
    /// The two hosted panels are not in the shell's panel list, so the services it
    /// assigns to this one have to be passed down.
    /// </summary>
    private void ForwardServices()
    {
        foreach (var child in new PanelViewModelBase[] { Freighter, Frigates, Squadron })
        {
            child.Dialogs ??= Dialogs;
            child.SaveFilePickerFunc ??= SaveFilePickerFunc;
            child.OpenFilePickerFunc ??= OpenFilePickerFunc;
            child.GoToJsonFunc ??= GoToJsonFunc;
        }
    }

    public override void ApplyLocalisation()
    {
        FreighterTabHeader = UiStrings.Get("fleet.tab_freighter");
        FrigateTabHeader = UiStrings.Get("fleet.tab_frigates");
        SquadronTabHeader = UiStrings.Get("fleet.tab_squadron");

        Freighter.ApplyLocalisation();
        Frigates.ApplyLocalisation();
        Squadron.ApplyLocalisation();
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        ForwardServices();
        Freighter.LoadData(saveData, database, iconManager);
        Frigates.LoadData(saveData, database, iconManager);
        Squadron.LoadData(saveData);
    }

    public override void SaveData(JsonObject saveData)
    {
        Freighter.SaveData(saveData);
        Frigates.SaveData(saveData);
        Squadron.SaveData();
    }
}
