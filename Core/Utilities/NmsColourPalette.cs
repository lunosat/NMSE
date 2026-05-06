using System.Drawing;
using System.Text.Json;

namespace NMSE.Core.Utilities;

/// <summary>
/// Provides colour palette data used by the NMS game engine for various
/// customisation systems. Manages both the fixed pet accessory paint palette
/// and all named paint palettes parsed from game data (ships, vehicles, companions,
/// players, etc.) loaded from Colour Palettes.json.
/// </summary>
public static class NmsColourPalette
{
    /// <summary>
    /// A named entry in an NMS colour palette, pairing a display-friendly name
    /// (for tooltips/accessibility) with the actual colour value.
    /// </summary>
    public readonly record struct PaletteEntry(string Name, Color Colour);

    /// <summary>
    /// The 20 Paint palette colours extracted from game save data. These are the only
    /// colours the game allows for pet accessory customisation. Values are derived from
    /// the normalised RGBA floats stored in save files, converted to 0-255 RGB.
    /// </summary>
    public static readonly PaletteEntry[] PaintPalette =
    [
        // Row 1 (top): light / bright variants, left-to-right matching the in-game grid.
        new("Light Red",      Color.FromArgb(222, 103, 103)),  //  0
        new("Orange",         Color.FromArgb(255, 133,   0)),  //  1
        new("Gold",           Color.FromArgb(244, 169,  14)),  //  2
        new("Mint",           Color.FromArgb( 81, 188, 126)),  //  3
        new("Light Teal",     Color.FromArgb(116, 201, 186)),  //  4
        new("Cornflower Blue",Color.FromArgb( 60, 144, 222)),  //  5
        new("Lavender",       Color.FromArgb(173, 134, 207)),  //  6
        new("Pink",           Color.FromArgb(229, 132, 194)),  //  7
		// Yellow goes here for ships, but isn't used for pet accessories so is omitted for now.
        new("White",          Color.FromArgb(255, 255, 255)),  //  8
        new("Light Grey",     Color.FromArgb(170, 170, 170)),  //  9

        // Row 2 (bottom): dark / muted variants, same column order.
        new("Dark Red",       Color.FromArgb(128,  57,  57)),  // 10
        new("Brown",          Color.FromArgb(138,  94,  71)),  // 11
        new("Tan",            Color.FromArgb(192, 157, 112)),  // 12
        new("Dark Green",     Color.FromArgb( 62, 122,  87)),  // 13
        new("Dark Teal",      Color.FromArgb( 62, 111, 112)),  // 14
        new("Dark Slate",     Color.FromArgb( 59,  74, 103)),  // 15
        new("Dark Purple",    Color.FromArgb( 94,  71, 112)),  // 16
        new("Mauve",          Color.FromArgb(143,  85, 108)),  // 17
		// Yellow goes here for ships, but isn't used for pet accessories so is omitted for now.
        new("Dark Grey",      Color.FromArgb( 76,  76,  76)),  // 18
        new("Black",          Color.FromArgb(  0,   0,   0)),  // 19
    ];

    /// <summary>
    /// All named NMS paint palettes loaded from Colour Palettes.json.
    /// Covers ships (SHIP, SHIP_METALLIC), freighters (FREIGHTER, PIRATEFREIGHTER),
    /// vehicles (VEHICLE, BIKE, TRUCK, SKIFF, HOVERCRAFT, SUBMARINE, MECH),
    /// companions (PET), and players (PLAYER).
    /// Keyed by palette ID (e.g. "SHIP", "SHIP_METALLIC"). Empty until LoadShipPalettes is called.
    /// </summary>
    public static IReadOnlyDictionary<string, PaletteEntry[]> ShipPalettes =>
        _shipPalettes;

    private static Dictionary<string, PaletteEntry[]> _shipPalettes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Loads ship paint palette colour data from the given JSON file path
    /// (typically "Colour Palettes.json" from the Resources/json/ directory).
    /// Each entry in the JSON array must have "PaletteID" and a "Colours" array
    /// with "Name", "R", "G", "B" integer fields.
    /// Returns true if at least one palette was loaded successfully.
    /// </summary>
    public static bool LoadShipPalettes(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return false;

        try
        {
            var content = File.ReadAllBytes(jsonPath);
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return false;

            var loaded = new Dictionary<string, PaletteEntry[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                string paletteId = elem.TryGetProperty("PaletteID", out var pidProp)
                    ? pidProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(paletteId)) continue;

                if (!elem.TryGetProperty("Colours", out var coloursArr) ||
                    coloursArr.ValueKind != JsonValueKind.Array)
                    continue;

                var entries = new List<PaletteEntry>();
                foreach (var colElem in coloursArr.EnumerateArray())
                {
                    string name = colElem.TryGetProperty("Name", out var nameProp)
                        ? nameProp.GetString() ?? $"Colour {entries.Count}" : $"Colour {entries.Count}";
                    int r = colElem.TryGetProperty("R", out var rp) ? rp.GetInt32() : 255;
                    int g = colElem.TryGetProperty("G", out var gp) ? gp.GetInt32() : 255;
                    int b = colElem.TryGetProperty("B", out var bp) ? bp.GetInt32() : 255;
                    entries.Add(new PaletteEntry(name, Color.FromArgb(r, g, b)));
                }

                if (entries.Count > 0)
                    loaded[paletteId] = entries.ToArray();
            }

            _shipPalettes = loaded;
            return loaded.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the palette entries for the given palette ID (e.g. "SHIP", "SHIP_METALLIC",
    /// "FREIGHTER"), or null if the palette is not loaded. Falls back to null rather than the
    /// pet PaintPalette so callers can detect an unloaded palette explicitly.
    /// </summary>
    public static PaletteEntry[]? GetPaletteColours(string paletteId)
    {
        if (string.IsNullOrEmpty(paletteId)) return null;
        _shipPalettes.TryGetValue(paletteId, out var entries);
        return entries;
    }

    /// <summary>
    /// Finds the closest palette colour to the given colour using Euclidean distance
    /// in RGB space. Returns the index into <see cref="PaintPalette"/>, or -1 if
    /// the palette is empty.
    /// </summary>
    public static int FindClosestPaletteIndex(Color colour)
    {
        if (PaintPalette.Length == 0) return -1;

        int bestIndex = 0;
        int bestDist = int.MaxValue;

        for (int i = 0; i < PaintPalette.Length; i++)
        {
            var p = PaintPalette[i].Colour;
            int dr = colour.R - p.R;
            int dg = colour.G - p.G;
            int db = colour.B - p.B;
            int dist = dr * dr + dg * dg + db * db;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    /// <summary>
    /// Converts a palette colour to the normalised RGBA float array used in NMS save files.
    /// Returns [R, G, B, A] where each component is in the 0.0 to 1.0 range.
    /// </summary>
    public static double[] ToNormalisedRgba(Color colour)
    {
        return
        [
            Math.Round(colour.R / 255.0, 4),
            Math.Round(colour.G / 255.0, 4),
            Math.Round(colour.B / 255.0, 4),
            1.0,
        ];
    }
}
