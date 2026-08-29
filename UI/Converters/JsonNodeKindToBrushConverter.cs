using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia;
using NMSE.UI.ViewModels.Panels;

namespace NMSE.UI.Converters;

/// <summary>
/// Colours a JSON tree node by what it holds, reusing the editor's syntax palette so
/// the tree and the text view agree on what a string or a number looks like.
/// </summary>
public class JsonNodeKindToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string key = value switch
        {
            JsonNodeKind.Object  => "NmseInfoBrush",
            JsonNodeKind.Array   => "NmseSuccessBrush",
            JsonNodeKind.String  => "NmseCodeStringBrush",
            JsonNodeKind.Number  => "NmseCodeNumberBrush",
            JsonNodeKind.Boolean => "NmseCodeLiteralBrush",
            JsonNodeKind.Null    => "NmseText2Brush",
            JsonNodeKind.Binary  => "NmseWarningBrush",
            _                    => "NmseText0Brush",
        };

        // Application exposes its resources rather than the Control-level lookup, and the
        // theme variant has to be passed so the light and dark palettes both resolve.
        var app = Application.Current;
        if (app?.Resources.TryGetResource(key, app.ActualThemeVariant, out object? found) == true
            && found is IBrush brush)
            return brush;

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
