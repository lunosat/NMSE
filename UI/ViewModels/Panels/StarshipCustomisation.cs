using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Core.Utilities;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>One customisable part slot: a labelled choice between "(None)" and the slot's items.</summary>
public partial class ShipPartSlotViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedIndex;

    public string Label { get; }
    public ShipCustomisationSlot Slot { get; }
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>Items parallel to <see cref="Options"/>; null at index 0, the "(None)" entry.</summary>
    public List<ShipCustomisationItem?> Items { get; } = new();

    public ShipPartSlotViewModel(ShipCustomisationSlot slot)
    {
        Slot = slot;
        Label = slot.Label + ":";

        Options.Add(UiStrings.Get("starship.customisation_none"));
        Items.Add(null);
        foreach (var item in slot.Items)
        {
            Options.Add(item.ItemID);
            Items.Add(item);
        }
    }

    public ShipCustomisationItem? SelectedItem =>
        SelectedIndex > 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

    /// <summary>Selects the item whose descriptor groups appear in the save, or "(None)".</summary>
    public void SelectFromDescriptorGroups(HashSet<string> current)
    {
        for (int i = 1; i < Items.Count; i++)
        {
            if (Items[i] is { } item && item.DescriptorGroupIDs.Any(current.Contains))
            {
                SelectedIndex = i;
                return;
            }
        }
        SelectedIndex = 0;
    }
}

/// <summary>One paint-style group: a labelled choice between "(None)" and the group's options.</summary>
public partial class ShipTextureGroupViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedIndex;

    public string GroupId { get; }
    public string Label { get; }
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>The ids behind <see cref="Options"/>, offset by the leading "(None)".</summary>
    private List<string> RawOptions { get; } = new();

    public ShipTextureGroupViewModel(ShipCustomisationTextureGroup group)
    {
        GroupId = group.GroupID;
        Label = UiStrings.Format("starship.customisation_paint_style_label", group.GroupID);

        Options.Add(UiStrings.Get("starship.customisation_none"));
        // The combo shows readable names; RawOptions keeps the ids the save stores.
        foreach (string option in group.Options)
        {
            RawOptions.Add(option);
            Options.Add(ShipCustomisationNames.TextureOption(option));
        }
    }

    /// <summary>The chosen option as the save spells it, or null when "(None)" is selected.</summary>
    public string? SelectedOption =>
        SelectedIndex > 0 && SelectedIndex - 1 < RawOptions.Count ? RawOptions[SelectedIndex - 1] : null;

    public void SelectOption(string? option)
    {
        if (string.IsNullOrEmpty(option)) { SelectedIndex = 0; return; }

        int idx = RawOptions.FindIndex(o => string.Equals(o, option, StringComparison.OrdinalIgnoreCase));
        SelectedIndex = idx >= 0 ? idx + 1 : 0;
    }
}

/// <summary>
/// One colour channel of the ship, shown as a swatch the user picks a palette colour into.
/// </summary>
public partial class ShipColourChannelViewModel : ObservableObject
{
    [ObservableProperty] private IBrush _swatch = Brushes.Gray;

    public string Label { get; }
    public string PaletteName { get; }
    public string ColourAlt { get; }
    public string DisplayPaletteId { get; }

    /// <summary>The palette this channel picks from, resolved when the swatch is opened.</summary>
    public ObservableCollection<ShipPaletteSwatch> Choices { get; } = new();

    public ShipColourChannelViewModel(string label, string paletteName, string colourAlt, string displayPaletteId)
    {
        Label = label;
        PaletteName = paletteName;
        ColourAlt = colourAlt;
        DisplayPaletteId = displayPaletteId;
    }

    public void SetColour(Color colour) => Swatch = new SolidColorBrush(colour);

    /// <summary>Fills <see cref="Choices"/> from the named palette, or the default when unknown.</summary>
    public void LoadChoices(string? paletteIdOverride)
    {
        Choices.Clear();
        string paletteId = string.IsNullOrEmpty(DisplayPaletteId) ? (paletteIdOverride ?? "") : DisplayPaletteId;

        var entries = NmsColourPalette.GetPaletteColours(paletteId) ?? NmsColourPalette.PaintPalette;
        foreach (var entry in entries)
            Choices.Add(new ShipPaletteSwatch(entry.Name, entry.Colour, new SolidColorBrush(entry.Colour)));
    }
}

/// <summary>Human-readable names for the raw palette and texture identifiers.</summary>
public static class ShipCustomisationNames
{
    public static string Palette(string rawId) => rawId switch
    {
        "SHIP" => UiStrings.Get("starship.palette_default"),
        "SHIP_METALLIC" => UiStrings.Get("starship.palette_metallic"),
        _ => rawId,
    };

    public static string TextureOption(string rawId) => rawId switch
    {
        "COATING" => UiStrings.Get("starship.texture_coating"),
        "PANELS" => UiStrings.Get("starship.texture_panels"),
        "STEALTH" => UiStrings.Get("starship.texture_stealth"),
        "METALBOLT" => UiStrings.Get("starship.texture_metalbolt"),
        _ => rawId,
    };
}

/// <summary>A single selectable colour in a palette grid.</summary>
public sealed record ShipPaletteSwatch(string Name, Color Colour, IBrush Brush);

/// <summary>
/// Reads and writes the CharacterCustomisationData entry that holds a ship's parts,
/// paint styles, palette and colours.
/// </summary>
/// <remarks>
/// Every identifier in the save carries a <c>^</c> prefix, and an empty PaletteID means
/// the game's default "SHIP" palette rather than "no palette". Both are normalised here
/// so the rest of the panel deals in plain names.
/// </remarks>
internal static class ShipCustomisationIo
{
    private const string DefaultPalette = "SHIP";

    internal static string Strip(string? value) =>
        string.IsNullOrEmpty(value) ? "" :
        value.StartsWith('^') ? value[1..] : value;

    internal static HashSet<string> ReadDescriptorGroups(JsonObject? ccd)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dg = ccd?.GetObject("CustomData")?.GetArray("DescriptorGroups");
        if (dg is null) return set;

        for (int i = 0; i < dg.Length; i++)
        {
            string value = Strip(dg.Get(i)?.ToString());
            if (value.Length > 0) set.Add(value);
        }
        return set;
    }

    internal static string ReadPaletteId(JsonObject? ccd)
    {
        string id = Strip(ccd?.GetObject("CustomData")?.GetString("PaletteID"));
        return id.Length == 0 ? DefaultPalette : id;
    }

    internal static Dictionary<string, string> ReadTextureOptions(JsonObject? ccd)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var options = ccd?.GetObject("CustomData")?.GetArray("TextureOptions");
        if (options is null) return map;

        for (int i = 0; i < options.Length; i++)
        {
            var entry = options.GetObject(i);
            if (entry is null) continue;
            string group = Strip(entry.GetString("TextureOptionGroupName"));
            if (group.Length > 0) map[group] = Strip(entry.GetString("TextureOptionName"));
        }
        return map;
    }

    internal static Color ReadColour(JsonObject? ccd, string paletteName, string colourAlt)
    {
        var colours = ccd?.GetObject("CustomData")?.GetArray("Colours");
        if (colours is null) return Colors.Gray;

        for (int i = 0; i < colours.Length; i++)
        {
            var entry = colours.GetObject(i);
            var palette = entry?.GetObject("Palette");
            if (palette is null) continue;

            if (!string.Equals(palette.GetString("Palette") ?? "", paletteName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(palette.GetString("ColourAlt") ?? "", colourAlt, StringComparison.OrdinalIgnoreCase))
                continue;

            var rgb = entry!.GetArray("Colour");
            if (rgb is null || rgb.Length < 3) return Colors.Gray;

            return Color.FromRgb(
                (byte)Math.Clamp(Math.Round(rgb.GetDouble(0) * 255.0), 0, 255),
                (byte)Math.Clamp(Math.Round(rgb.GetDouble(1) * 255.0), 0, 255),
                (byte)Math.Clamp(Math.Round(rgb.GetDouble(2) * 255.0), 0, 255));
        }
        return Colors.Gray;
    }

    internal static bool WriteColour(JsonObject? ccd, string paletteName, string colourAlt, Color colour)
    {
        var colours = ccd?.GetObject("CustomData")?.GetArray("Colours");
        if (colours is null) return false;

        if (WriteColourInto(colours, paletteName, colourAlt, colour)) return true;

        // Older saves carry only the three main entries, so an Alternative1 channel has
        // to fall back to the Primary entry of the same palette.
        if (string.Equals(paletteName, "Undercoat", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(colourAlt, "Alternative1", StringComparison.OrdinalIgnoreCase))
            return WriteColourInto(colours, "Undercoat", "Primary", colour);

        return false;
    }

    private static bool WriteColourInto(JsonArray colours, string paletteName, string colourAlt, Color colour)
    {
        for (int i = 0; i < colours.Length; i++)
        {
            var entry = colours.GetObject(i);
            var palette = entry?.GetObject("Palette");
            if (palette is null) continue;

            if (!string.Equals(palette.GetString("Palette") ?? "", paletteName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(palette.GetString("ColourAlt") ?? "", colourAlt, StringComparison.OrdinalIgnoreCase))
                continue;

            var rgb = entry!.GetArray("Colour");
            if (rgb is null || rgb.Length < 4) return false;

            var normalised = NmsColourPalette.ToNormalisedRgba(colour);
            for (int c = 0; c < 4; c++) rgb.Set(c, normalised[c]);
            return true;
        }
        return false;
    }

    /// <summary>Rebuilds DescriptorGroups, PaletteID and TextureOptions from the current selections.</summary>
    internal static void Write(JsonObject? ccd,
        IEnumerable<ShipPartSlotViewModel> slots,
        IEnumerable<ShipTextureGroupViewModel> textures,
        string? paletteId)
    {
        var customData = ccd?.GetObject("CustomData");
        if (customData is null) return;

        var dg = customData.GetArray("DescriptorGroups");
        if (dg is not null)
        {
            for (int i = dg.Length - 1; i >= 0; i--) dg.RemoveAt(i);
            foreach (var slot in slots)
                foreach (string id in slot.SelectedItem?.DescriptorGroupIDs ?? Array.Empty<string>())
                    dg.Add("^" + id);
        }

        bool isDefault = string.IsNullOrEmpty(paletteId)
            || string.Equals(paletteId, DefaultPalette, StringComparison.OrdinalIgnoreCase);
        customData.Set("PaletteID", isDefault ? "^" : "^" + paletteId);

        var texOptions = customData.GetArray("TextureOptions");
        if (texOptions is not null)
        {
            for (int i = texOptions.Length - 1; i >= 0; i--) texOptions.RemoveAt(i);
            foreach (var group in textures)
            {
                if (group.SelectedOption is not { Length: > 0 } option) continue;
                var entry = new JsonObject();
                entry.Set("TextureOptionGroupName", "^" + group.GroupId);
                entry.Set("TextureOptionName", "^" + option);
                texOptions.Add(entry);
            }
        }
    }
}
