using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace NMSE.UI.Util;

/// <summary>
/// Manages the embedded NMS fonts for use in the application.
/// Loads the TTFs from embedded resources via PrivateFontCollection and
/// exposes helper methods to create Font instances at various sizes.
/// </summary>
public static class FontManager
{
    private static PrivateFontCollection? _fontCollection;
    private static FontFamily? _nmsFont;
    private static bool _initialized;
    
    private static PrivateFontCollection? _glyphFontCollection;
    private static FontFamily? _glyphFont;
    private static bool _glyphInitialized;

    private const string FontResourceName = "NMSE.Resources.app.NMSGeoSans_Kerned.ttf";
    private const string GlyphFontResourceName = "NMSE.Resources.app.NMS_Glyphs_Mono.ttf";

    /// <summary>The loaded NMS GeoSans font family, or null if loading failed.</summary>
    public static FontFamily? NmsFont
    {
        get
        {
            EnsureInitialized();
            return _nmsFont;
        }
    }

    /// <summary>
    /// Creates a Font using the embedded NMS GeoSans font at the given size and style.
    /// Falls back to the default control font family if the embedded font could not be loaded.
    /// </summary>
    /// <param name="size">Font size in points.</param>
    public static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
    {
        EnsureInitialized();
        var family = _nmsFont ?? Control.DefaultFont.FontFamily;
        return new Font(family, size, style);
    }

    /// <summary>The loaded NMS portal glyph font family, or null if loading failed.</summary>
    public static FontFamily? GlyphFont
    {
        get
        {
            EnsureGlyphInitialized();
            return _glyphFont;
        }
    }

    /// <summary>
    /// Creates a Font using the embedded NMS portal glyph font at the given size.
    /// Falls back to Consolas if the embedded font could not be loaded.
    /// </summary>
    /// <param name="size">Font size in points.</param>
    public static Font CreateGlyphFont(float size)
    {
        EnsureGlyphInitialized();
        var family = _glyphFont ?? FontFamily.GenericMonospace;
        return new Font(family, size, FontStyle.Regular);
    }

    /// <summary>
    /// Creates a heading font (bold) at the specified size using the NMS GeoSans font.
    /// </summary>
    public static Font CreateHeadingFont(float size)
        => CreateFont(size, FontStyle.Bold);

    /// <summary>
    /// Applies the NMS heading font to a Label and enables GDI+ rendering
    /// (UseCompatibleTextRendering) so the embedded PrivateFontCollection font
    /// is used without requiring a system-level install.
    /// </summary>
    public static void ApplyHeadingFont(Label label, float size)
    {
        label.Font = CreateHeadingFont(size);
        label.UseCompatibleTextRendering = true;
    }

    /// <summary>
    /// Applies the NMS font to a Label with the given style and enables GDI+ rendering.
    /// </summary>
    public static void ApplyFont(Label label, float size, FontStyle style = FontStyle.Regular)
    {
        label.Font = CreateFont(size, style);
        label.UseCompatibleTextRendering = true;
    }

    /// <summary>
    /// Ensures the embedded NMS GeoSans font is loaded once.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        _nmsFont = LoadEmbeddedFont(FontResourceName, out _fontCollection, "font");
    }

    /// <summary>
    /// Ensures the glyph font is loaded once.
    /// </summary>
    private static void EnsureGlyphInitialized()
    {
        if (_glyphInitialized) return;
        _glyphInitialized = true;

        _glyphFont = LoadEmbeddedFont(GlyphFontResourceName, out _glyphFontCollection, "glyph font");
    }

    /// <summary>
    /// Loads an embedded TrueType font from assembly resources into a private font collection.
    /// </summary>
    /// <param name="resourceName">The embedded resource name for the font file.</param>
    /// <param name="collection">The created <see cref="PrivateFontCollection"/>, or null on failure.</param>
    /// <param name="debugName">A descriptive name used for debug logging.</param>
    /// <returns>The loaded <see cref="FontFamily"/>, or null if loading failed.</returns>
    private static FontFamily? LoadEmbeddedFont(string resourceName, out PrivateFontCollection? collection, string debugName)
    {
        collection = null;
        try
        {
            var assembly = typeof(FontManager).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                System.Diagnostics.Debug.WriteLine($"FontManager: embedded resource '{resourceName}' not found.");
                return null;
            }

            byte[] fontData = new byte[stream.Length];
            stream.ReadExactly(fontData);

            collection = new PrivateFontCollection();
            var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            try
            {
                collection.AddMemoryFont(handle.AddrOfPinnedObject(), fontData.Length);
            }
            finally
            {
                handle.Free();
            }

            return collection.Families.Length > 0 ? collection.Families[0] : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FontManager: failed to load {debugName}: {ex.Message}");
            collection = null;
            return null;
        }
    }
}

