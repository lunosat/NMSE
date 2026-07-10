using NMSE.Data;
using NMSE.Models;

namespace NMSE.Core;

/// <summary>
/// Handles player outfit data operations including listing, export, import,
/// and copying outfit data to the active character customisation slot.
/// </summary>
internal static class OutfitLogic
{
    /// <summary>
    /// JSON property names that constitute an outfit's customisation data.
    /// </summary>
    private static readonly string[] OutfitDataFields =
        ["DescriptorGroups", "PaletteID", "Colours", "TextureOptions", "BoneScales", "Scale"];

    /// <summary>
    /// Gets the outfit display name for a given index, using either the user-defined name
    /// from <c>OutfitNames</c> or a fallback.
    /// </summary>
    /// <param name="outfitNames">The <c>OutfitNames</c> array from PlayerStateData.</param>
    /// <param name="outfit">The outfit JSON object at <c>Outfits[index]</c>.</param>
    /// <param name="index">The outfit slot index.</param>
    /// <returns>A display string such as "Outfit 1" or the custom name.</returns>
    internal static string GetOutfitDisplayName(JsonArray? outfitNames, JsonObject outfit, int index)
    {
        // Prefer user-defined name
        if (outfitNames != null && index < outfitNames.Length)
        {
            try
            {
                var name = outfitNames.GetString(index);
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch { }
        }

        // Check whether the outfit has any customisation data
        if (!HasCustomisationData(outfit))
        {
            return UiStrings.Format("outfits.empty_slot", index + 1);
        }

        return UiStrings.Format("outfits.slot_number", index + 1);
    }

    /// <summary>
    /// Determines whether an outfit object contains non-default customisation data.
    /// </summary>
    internal static bool HasCustomisationData(JsonObject outfit)
    {
        try
        {
            var groups = outfit.GetArray("DescriptorGroups");
            return groups != null && groups.Length > 0;
        }
        catch { return false; }
    }

    /// <summary>
    /// Exports a single outfit to a JSON file.
    /// </summary>
    /// <param name="outfit">The outfit JSON object from the <c>Outfits</c> array.</param>
    /// <param name="filePath">The destination file path.</param>
    internal static void ExportOutfit(JsonObject outfit, string filePath)
    {
        outfit.ExportToFile(filePath);
    }

    /// <summary>
    /// Imports an outfit from a JSON file into the specified slot of the <c>Outfits</c> array.
    /// All matching customisation fields are merged from the imported data into the target outfit.
    /// </summary>
    /// <param name="outfits">The <c>Outfits</c> array from PlayerStateData.</param>
    /// <param name="index">The target slot index.</param>
    /// <param name="filePath">The source file path.</param>
    internal static void ImportOutfit(JsonArray outfits, int index, string filePath)
    {
        var imported = JsonObject.ImportFromFile(filePath);
        if (index < 0 || index >= outfits.Length)
        {
            // Extend the array if needed (should not happen with known slot count, but be safe)
            while (outfits.Length <= index)
                outfits.Add(new JsonObject());
        }

        var target = outfits.GetObject(index);
        if (target == null)
        {
            target = new JsonObject();
            outfits.Set(index, target);
        }

        // Merge customisation fields from the imported data into the target outfit
        foreach (var field in OutfitDataFields)
        {
            try
            {
                var value = imported.Get(field);
                if (value != null)
                    target.Set(field, value);
            }
            catch { }
        }
    }

    /// <summary>
    /// Copies an outfit's customisation data to the active character customisation slot
    /// (<c>CharacterCustomisationData[0].CustomData</c>). This applies the outfit to the player.
    /// </summary>
    /// <param name="outfit">The source outfit object.</param>
    /// <param name="playerState">The <c>PlayerStateData</c> object.</param>
    internal static void CopyToCustomData(JsonObject outfit, JsonObject playerState)
    {
        try
        {
            var ccd = playerState.GetArray("CharacterCustomisationData");
            if (ccd == null || ccd.Length == 0)
            {
                // Create it if missing
                var entry = new JsonObject();
                entry.Set("CustomData", new JsonObject());
                ccd = new JsonArray();
                ccd.Add(entry);
                playerState.Set("CharacterCustomisationData", ccd);
            }

            var targetEntry = ccd.GetObject(0);
            var customData = targetEntry?.GetObject("CustomData");
            if (customData == null)
            {
                customData = new JsonObject();
                targetEntry?.Set("CustomData", customData);
            }

            foreach (var field in OutfitDataFields)
            {
                try
                {
                    var value = outfit.Get(field);
                    if (value != null)
                        customData.Set(field, value);
                }
                catch { }
            }
        }
        catch { }
    }
}
