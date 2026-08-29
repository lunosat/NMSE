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
}
