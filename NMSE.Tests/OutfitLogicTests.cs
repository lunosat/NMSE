using NMSE.Core;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.Tests;

[Collection("MutableStaticDatabases")]
public class OutfitLogicTests
{
    private static bool _jsonLoaded;
    private static readonly object _loadLock = new();

    public OutfitLogicTests()
    {
        EnsureUiStringsLoaded();
    }

    private static void EnsureUiStringsLoaded()
    {
        if (_jsonLoaded) return;
        lock (_loadLock)
        {
            if (_jsonLoaded) return;
            var langDir = FindResourceLangDir();
            if (langDir != null)
            {
                UiStrings.SetDirectory(langDir);
                UiStrings.Load("en-GB");
            }
            _jsonLoaded = true;
        }
    }

    private static string? FindResourceLangDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Resources", "ui", "lang");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    // --- HasCustomisationData ---

    [Fact]
    public void HasCustomisationData_WithDescriptorGroups_ReturnsTrue()
    {
        var outfit = new JsonObject();
        var groups = new JsonArray();
        groups.Add("test");
        outfit.Set("DescriptorGroups", groups);
        Assert.True(OutfitLogic.HasCustomisationData(outfit));
    }

    [Fact]
    public void HasCustomisationData_EmptyDescriptorGroups_ReturnsFalse()
    {
        var outfit = new JsonObject();
        outfit.Set("DescriptorGroups", new JsonArray());
        Assert.False(OutfitLogic.HasCustomisationData(outfit));
    }

    [Fact]
    public void HasCustomisationData_NoDescriptorGroups_ReturnsFalse()
    {
        var outfit = new JsonObject();
        Assert.False(OutfitLogic.HasCustomisationData(outfit));
    }

    // --- GetOutfitDisplayName ---

    [Fact]
    public void GetOutfitDisplayName_UsesOutfitNames_WhenAvailable()
    {
        var outfitNames = new JsonArray();
        outfitNames.Add("My Custom Outfit");
        var outfit = new JsonObject();
        var groups = new JsonArray();
        groups.Add("test");
        outfit.Set("DescriptorGroups", groups);
        string name = OutfitLogic.GetOutfitDisplayName(outfitNames, outfit, 0);
        Assert.Equal("My Custom Outfit", name);
    }

    [Fact]
    public void GetOutfitDisplayName_FallsBackToSlotNumber_WhenNoName()
    {
        var outfitNames = new JsonArray();
        outfitNames.Add(""); // empty name
        var outfit = new JsonObject();
        var groups = new JsonArray();
        groups.Add("test");
        outfit.Set("DescriptorGroups", groups);
        string name = OutfitLogic.GetOutfitDisplayName(outfitNames, outfit, 0);
        Assert.Equal("Outfit 1", name);
    }

    [Fact]
    public void GetOutfitDisplayName_EmptySlot_WhenNoData()
    {
        var outfitNames = new JsonArray();
        outfitNames.Add("");
        var outfit = new JsonObject();
        string name = OutfitLogic.GetOutfitDisplayName(outfitNames, outfit, 2);
        Assert.Equal("Empty Slot", name);
    }

    // --- ImportOutfit ---

    [Fact]
    public void ImportOutfit_MergesFields()
    {
        var outfits = new JsonArray();
        outfits.Add(new JsonObject());
        var source = new JsonObject();
        source.Set("PaletteID", "TestPalette");
        source.Set("Scale", 1.5);
        var groups = new JsonArray();
        groups.Add("group1");
        source.Set("DescriptorGroups", groups);

        // Write source to temp file
        string tempPath = Path.GetTempFileName();
        try
        {
            source.ExportToFile(tempPath);
            OutfitLogic.ImportOutfit(outfits, 0, tempPath);
            var target = outfits.GetObject(0);
            Assert.NotNull(target);
            Assert.Equal("TestPalette", target.GetString("PaletteID"));
            Assert.Equal(1.5, target.GetDouble("Scale"));
            var importedGroups = target.GetArray("DescriptorGroups");
            Assert.NotNull(importedGroups);
            Assert.Equal(1, importedGroups.Length);
            Assert.Equal("group1", importedGroups.GetString(0));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void ImportOutfit_ExtendsArray_WhenIndexOutOfRange()
    {
        var outfits = new JsonArray();
        var source = new JsonObject();
        source.Set("PaletteID", "Test");
        string tempPath = Path.GetTempFileName();
        try
        {
            source.ExportToFile(tempPath);
            OutfitLogic.ImportOutfit(outfits, 5, tempPath);
            Assert.True(outfits.Length > 5);
            var target = outfits.GetObject(5);
            Assert.NotNull(target);
            Assert.Equal("Test", target.GetString("PaletteID"));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    // --- CopyToCustomData ---

    [Fact]
    public void CopyToCustomData_CopiesFields()
    {
        var outfit = new JsonObject();
        outfit.Set("PaletteID", "OutfitPalette");
        outfit.Set("Scale", 2.0);
        var groups = new JsonArray();
        groups.Add("g1");
        outfit.Set("DescriptorGroups", groups);
        var colours = new JsonArray();
        colours.Add("#FF0000");
        outfit.Set("Colours", colours);

        var playerState = new JsonObject();
        OutfitLogic.CopyToCustomData(outfit, playerState);

        var ccd = playerState.GetArray("CharacterCustomisationData");
        Assert.NotNull(ccd);
        Assert.Equal(1, ccd.Length);
        var entry = ccd.GetObject(0);
        Assert.NotNull(entry);
        var customData = entry.GetObject("CustomData");
        Assert.NotNull(customData);
        Assert.Equal("OutfitPalette", customData.GetString("PaletteID"));
        Assert.Equal(2.0, customData.GetDouble("Scale"));
        Assert.Equal(1, customData.GetArray("DescriptorGroups")?.Length);
        Assert.Equal("#FF0000", customData.GetArray("Colours")?.GetString(0));
    }

    [Fact]
    public void CopyToCustomData_CreatesArrays_WhenMissing()
    {
        var outfit = new JsonObject();
        outfit.Set("PaletteID", "Test");
        var playerState = new JsonObject();
        OutfitLogic.CopyToCustomData(outfit, playerState);
        var ccd = playerState.GetArray("CharacterCustomisationData");
        Assert.NotNull(ccd);
    }
}
