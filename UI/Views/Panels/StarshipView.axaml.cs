using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NMSE.UI.ViewModels.Panels;

namespace NMSE.UI.Views.Panels;

public partial class StarshipView : UserControl
{
    public StarshipView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Applies a colour picked from a channel's palette grid.
    /// </summary>
    /// <remarks>
    /// The swatch carries the colour as its DataContext and the owning channel in its
    /// Tag. The channel is passed rather than looked up, because the grid lives inside a
    /// flyout - a separate visual tree that an ancestor walk does not cross reliably.
    /// </remarks>
    private void OnColourSwatchClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: ShipPaletteSwatch swatch, Tag: ShipColourChannelViewModel channel } button)
            return;
        if (DataContext is not StarshipViewModel vm) return;

        vm.ApplyColourChoice(channel, swatch);

        foreach (var ancestor in button.GetVisualAncestors())
        {
            if (ancestor is Popup popup) { popup.Close(); break; }
        }
    }
}
