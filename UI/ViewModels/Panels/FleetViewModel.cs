using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

public partial class FleetViewModel : PanelViewModelBase
{
    [ObservableProperty] private string _frigateTabHeader = "Frigates";
    [ObservableProperty] private string _squadronTabHeader = "Squadron";

    public FrigateViewModel Frigates { get; } = new();
    public SquadronViewModel Squadron { get; } = new();

    /// <summary>
    /// The two hosted panels are not in the shell's panel list, so the services it
    /// assigns to this one have to be passed down.
    /// </summary>
    private void ForwardServices()
    {
        foreach (var child in new PanelViewModelBase[] { Frigates, Squadron })
        {
            child.Dialogs ??= Dialogs;
            child.SaveFilePickerFunc ??= SaveFilePickerFunc;
            child.OpenFilePickerFunc ??= OpenFilePickerFunc;
        }
    }

    public override void LoadData(JsonObject saveData, GameItemDatabase database, IconManager? iconManager)
    {
        ForwardServices();
        Frigates.LoadData(saveData, database, iconManager);
        Squadron.LoadData(saveData);
    }

    public override void SaveData(JsonObject saveData)
    {
        Frigates.SaveData(saveData);
        Squadron.SaveData();
    }
}
