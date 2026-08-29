using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace NMSE.UI.Util;

/// <summary>
/// Loads and caches the 16 portal glyph images (UI-GLYPH1.PNG .. UI-GLYPH16.PNG),
/// which map to the hex digits 0-F of a portal address.
/// </summary>
public static class GlyphImageCache
{
    private static readonly Dictionary<char, Bitmap?> _cache = new();
    private static string? _basePath;

    /// <summary>Sets the directory holding the glyph images and drops any cached ones.</summary>
    public static void SetBasePath(string basePath)
    {
        _basePath = basePath;
        foreach (var bmp in _cache.Values) bmp?.Dispose();
        _cache.Clear();
    }

    /// <summary>Gets the glyph for a hex digit (0-9, A-F), or null when unavailable.</summary>
    public static Bitmap? Get(char hexDigit)
    {
        hexDigit = char.ToUpperInvariant(hexDigit);
        if (_cache.TryGetValue(hexDigit, out var cached)) return cached;

        Bitmap? img = Load(hexDigit);
        _cache[hexDigit] = img;
        return img;
    }

    private static Bitmap? Load(char hexDigit)
    {
        if (string.IsNullOrEmpty(_basePath)) return null;

        // Glyph files are numbered 1-16, mapping to hex digits 0-F.
        int index = hexDigit is >= '0' and <= '9'
            ? hexDigit - '0' + 1
            : hexDigit is >= 'A' and <= 'F'
                ? hexDigit - 'A' + 11
                : -1;
        if (index < 1) return null;

        string path = Path.Combine(_basePath, $"UI-GLYPH{index}.PNG");
        if (!File.Exists(path)) return null;

        try
        {
            // Read through a MemoryStream so the file is not held open; these
            // bitmaps live in a static cache for the process lifetime.
            using var ms = new MemoryStream(File.ReadAllBytes(path));
            return new Bitmap(ms);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Renders a portal address as a row of glyph images. Digits with no glyph
/// available fall back to the character itself in a monospace face.
/// </summary>
/// <remarks>
/// The WinForms original built one <c>Panel</c> per glyph inside a
/// <c>FlowLayoutPanel</c>. Drawing the whole strip in a single control keeps
/// twelve extra visuals out of the tree and lets Skia do the scaling.
/// </remarks>
public class PortalGlyphStrip : Control
{
    public static readonly StyledProperty<string?> PortalCodeProperty =
        AvaloniaProperty.Register<PortalGlyphStrip, string?>(nameof(PortalCode));

    public static readonly StyledProperty<double> GlyphSizeProperty =
        AvaloniaProperty.Register<PortalGlyphStrip, double>(nameof(GlyphSize), 22d);

    public static readonly StyledProperty<IBrush> GlyphBackgroundProperty =
        AvaloniaProperty.Register<PortalGlyphStrip, IBrush>(
            nameof(GlyphBackground), new SolidColorBrush(Color.FromRgb(60, 60, 60)));

    static PortalGlyphStrip()
    {
        AffectsMeasure<PortalGlyphStrip>(PortalCodeProperty, GlyphSizeProperty);
        AffectsRender<PortalGlyphStrip>(PortalCodeProperty, GlyphSizeProperty, GlyphBackgroundProperty);
    }

    /// <summary>The portal address to draw, normally 12 hex digits.</summary>
    public string? PortalCode
    {
        get => GetValue(PortalCodeProperty);
        set => SetValue(PortalCodeProperty, value);
    }

    /// <summary>Width and height of each glyph cell, in device-independent pixels.</summary>
    public double GlyphSize
    {
        get => GetValue(GlyphSizeProperty);
        set => SetValue(GlyphSizeProperty, value);
    }

    /// <summary>Fill painted behind each glyph.</summary>
    public IBrush GlyphBackground
    {
        get => GetValue(GlyphBackgroundProperty);
        set => SetValue(GlyphBackgroundProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int count = PortalCode?.Length ?? 0;
        return new Size(count * GlyphSize, count == 0 ? 0 : GlyphSize);
    }

    public override void Render(DrawingContext context)
    {
        string? code = PortalCode;
        if (string.IsNullOrEmpty(code)) return;

        double size = GlyphSize;
        var typeface = new Typeface(FontFamily.Parse("monospace"), FontStyle.Normal, FontWeight.Bold);

        for (int i = 0; i < code.Length; i++)
        {
            var cell = new Rect(i * size, 0, size, size);
            var glyph = GlyphImageCache.Get(code[i]);

            if (glyph is not null)
            {
                context.FillRectangle(GlyphBackground, cell);
                context.DrawImage(glyph, new Rect(glyph.Size), cell);
            }
            else
            {
                var text = new FormattedText(
                    code[i].ToString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    size * 0.6,
                    Foreground ?? Brushes.Gray);

                context.DrawText(text, new Point(
                    cell.X + (size - text.Width) / 2,
                    cell.Y + (size - text.Height) / 2));
            }
        }
    }

    /// <summary>Brush used for the fallback character when a glyph is missing.</summary>
    public IBrush? Foreground
    {
        get => GetValue(TextElement.ForegroundProperty);
        set => SetValue(TextElement.ForegroundProperty, value);
    }
}
