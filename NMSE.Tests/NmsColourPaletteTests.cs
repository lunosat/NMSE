using Avalonia.Media;
using NMSE.Core.Utilities;
using Xunit;

namespace NMSE.Tests;

/// <summary>
/// Tests for the NmsColourPalette utility class covering palette contents,
/// closest-colour matching, and normalised RGBA conversion.
/// </summary>
public class NmsColourPaletteTests
{
    [Fact]
    public void PaintPalette_Contains20Colours()
    {
        Assert.Equal(20, NmsColourPalette.PaintPalette.Length);
    }

    [Fact]
    public void PaintPalette_AllColoursHaveNames()
    {
        foreach (var entry in NmsColourPalette.PaintPalette)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Name),
                $"Palette entry at index {Array.IndexOf(NmsColourPalette.PaintPalette, entry)} has no name.");
        }
    }

    [Fact]
    public void PaintPalette_NamesAreUnique()
    {
        var names = NmsColourPalette.PaintPalette.Select(e => e.Name).ToList();
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void PaintPalette_ContainsWhiteAndBlack()
    {
        Assert.Contains(NmsColourPalette.PaintPalette, e => e.Colour == Color.FromRgb(255, 255, 255));
        Assert.Contains(NmsColourPalette.PaintPalette, e => e.Colour == Color.FromRgb(0, 0, 0));
    }

    [Theory]
    [InlineData(255, 255, 255, 8)]   // White is ninth (index 8)
    [InlineData(0, 0, 0, 19)]        // Black is last (index 19)
    [InlineData(255, 133, 0, 1)]     // Orange is second (index 1)
    public void FindClosestPaletteIndex_ExactMatch_ReturnsCorrectIndex(int r, int g, int b, int expectedIndex)
    {
        var colour = Color.FromRgb((byte)r, (byte)g, (byte)b);
        Assert.Equal(expectedIndex, NmsColourPalette.FindClosestPaletteIndex(colour));
    }

    [Fact]
    public void FindClosestPaletteIndex_NearWhite_ReturnsWhite()
    {
        // Almost white should match White (index 8)
        var nearWhite = Color.FromRgb(250, 252, 253);
        Assert.Equal(8, NmsColourPalette.FindClosestPaletteIndex(nearWhite));
    }

    [Fact]
    public void FindClosestPaletteIndex_NearBlack_ReturnsBlack()
    {
        // Almost black should match Black (index 19)
        var nearBlack = Color.FromRgb(5, 3, 2);
        Assert.Equal(19, NmsColourPalette.FindClosestPaletteIndex(nearBlack));
    }

    [Theory]
    [InlineData(255, 255, 255)]
    [InlineData(0, 0, 0)]
    [InlineData(128, 57, 57)]
    public void ToNormalisedRgba_ReturnsCorrectValues(int r, int g, int b)
    {
        var colour = Color.FromRgb((byte)r, (byte)g, (byte)b);
        var rgba = NmsColourPalette.ToNormalisedRgba(colour);

        Assert.Equal(4, rgba.Length);
        Assert.Equal(Math.Round(r / 255.0, 4), rgba[0]);
        Assert.Equal(Math.Round(g / 255.0, 4), rgba[1]);
        Assert.Equal(Math.Round(b / 255.0, 4), rgba[2]);
        Assert.Equal(1.0, rgba[3]); // Alpha always 1.0
    }

    [Fact]
    public void ToNormalisedRgba_White_AllOnes()
    {
        var rgba = NmsColourPalette.ToNormalisedRgba(Colors.White);
        Assert.Equal(1.0, rgba[0]);
        Assert.Equal(1.0, rgba[1]);
        Assert.Equal(1.0, rgba[2]);
        Assert.Equal(1.0, rgba[3]);
    }

    [Fact]
    public void ToNormalisedRgba_Black_AllZerosExceptAlpha()
    {
        var rgba = NmsColourPalette.ToNormalisedRgba(Colors.Black);
        Assert.Equal(0.0, rgba[0]);
        Assert.Equal(0.0, rgba[1]);
        Assert.Equal(0.0, rgba[2]);
        Assert.Equal(1.0, rgba[3]);
    }

    [Fact]
    public void PaintPalette_AllColoursHaveFullAlpha()
    {
        // Verify all palette colours have A=255 (fully opaque)
        foreach (var entry in NmsColourPalette.PaintPalette)
        {
            Assert.Equal(255, entry.Colour.A);
        }
    }

    [Fact]
    public void ShipPalettes_InitiallyEmpty()
    {
        // Ship palettes are not loaded by default
        Assert.Empty(NmsColourPalette.ShipPalettes);
    }

    [Fact]
    public void GetPaletteColours_UnknownId_ReturnsNull()
    {
        Assert.Null(NmsColourPalette.GetPaletteColours("SHIP_NONEXISTENT"));
    }

    [Fact]
    public void GetPaletteColours_EmptyId_ReturnsNull()
    {
        Assert.Null(NmsColourPalette.GetPaletteColours(""));
    }

    [Fact]
    public void LoadShipPalettes_NonexistentFile_ReturnsFalse()
    {
        Assert.False(NmsColourPalette.LoadShipPalettes("/nonexistent/path/Colour Palettes.json"));
    }

    [Fact]
    public void LoadShipPalettes_ValidJson_LoadsEntries()
    {
        var tempFile = Path.GetTempFileName();
        var clearFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, """
                [
                  {
                    "PaletteID": "SHIP_TEST",
                    "Colours": [
                      { "Index": 0, "Name": "Red", "R": 255, "G": 0, "B": 0, "A": 255 },
                      { "Index": 1, "Name": "Blue", "R": 0, "G": 0, "B": 255, "A": 255 }
                    ]
                  }
                ]
                """);

            bool result = NmsColourPalette.LoadShipPalettes(tempFile);
            Assert.True(result);

            var entries = NmsColourPalette.GetPaletteColours("SHIP_TEST");
            Assert.NotNull(entries);
            Assert.Equal(2, entries!.Length);
            Assert.Equal("Red", entries[0].Name);
            Assert.Equal(Color.FromRgb(255, 0, 0), entries[0].Colour);
            Assert.Equal("Blue", entries[1].Name);
        }
        finally
        {
            // Restore empty state by loading an empty palette array
            File.WriteAllText(clearFile, "[]");
            NmsColourPalette.LoadShipPalettes(clearFile);
            File.Delete(tempFile);
            File.Delete(clearFile);
        }
    }

    [Fact]
    public void LoadShipPalettes_AllPalettesIncluded()
    {
        // The bundled Colour Palettes.json should contain all 14 non-NULL palettes
        // including SHIP (the default) and SHIP_METALLIC
        string jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Resources", "json", "Colour Palettes.json");
        if (!File.Exists(jsonPath)) return; // Skip if running without bundled resources

        bool loaded = NmsColourPalette.LoadShipPalettes(jsonPath);
        Assert.True(loaded);

        // Both ship palettes must be present
        Assert.NotNull(NmsColourPalette.GetPaletteColours("SHIP"));
        Assert.NotNull(NmsColourPalette.GetPaletteColours("SHIP_METALLIC"));

        // SHIP palette should have 20 colours
        var shipEntries = NmsColourPalette.GetPaletteColours("SHIP");
        Assert.Equal(20, shipEntries!.Length);

        // The NULL palette must NOT be present (it has no colour names and is all white)
        Assert.Null(NmsColourPalette.GetPaletteColours("NULL"));
    }
}
