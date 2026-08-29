using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using NMSE.UI.ViewModels.Panels;

namespace NMSE.UI.Views.Panels;

public partial class CompanionView : UserControl
{
    public CompanionView()
    {
        InitializeComponent();
    }

    private void OnAccessoryPrimaryClick(object? sender, RoutedEventArgs e)
        => ApplySwatch(sender, primary: true);

    private void OnAccessoryAltClick(object? sender, RoutedEventArgs e)
        => ApplySwatch(sender, primary: false);

    /// <summary>
    /// Applies a colour picked from an accessory's palette grid. The swatch carries the
    /// colour as its DataContext and the slot in its Tag, because the grid lives in a
    /// flyout - a separate visual tree an ancestor walk does not cross reliably.
    /// </summary>
    private static void ApplySwatch(object? sender, bool primary)
    {
        if (sender is not Button { DataContext: ShipPaletteSwatch swatch, Tag: AccessorySlotViewModel slot } button)
            return;

        var brush = new SolidColorBrush(swatch.Colour);
        if (primary) slot.PrimaryColour = brush;
        else slot.AltColour = brush;

        foreach (var ancestor in button.GetVisualAncestors())
        {
            if (ancestor is Popup popup) { popup.Close(); break; }
        }
    }
}
