using System.Linq;
using Avalonia.Controls;
using NMSE.UI.ViewModels.Panels;
using NMSE.UI.Views.Dialogs;

namespace NMSE.UI.Views.Panels;

public partial class DiscoveryView : UserControl
{
    public DiscoveryView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not DiscoveryViewModel vm) return;

        // The picker is a dialog, so the view owns it and the panel asks through this.
        vm.PickItemFunc = async title =>
        {
            if (vm.Database is null) return null;
            if (TopLevel.GetTopLevel(this) is not Window owner) return null;

            var picker = new ItemPickerDialog { Title = title };
            picker.Initialize(vm.Database, vm.IconMgr);
            return await picker.ShowDialog<string?>(owner);
        };
    }

    /// <summary>
    /// The grid owns the multi-selection, so the panel is told what is highlighted
    /// rather than reaching into the control for it.
    /// </summary>
    private void OnLocationsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not DiscoveryViewModel vm) return;

        vm.SelectedLocations.Clear();
        foreach (var row in LocationsGrid.SelectedItems.OfType<TeleportLocationViewModel>())
            vm.SelectedLocations.Add(row);
    }
}
