using NMSE.Data;

namespace NMSE.Tests;

/// <summary>
/// Tests for <see cref="ShipCustomisationDatabase"/> (defined in StarshipDatabase.cs),
/// covering file loading, config parsing, and resource-path lookup.
/// Any test that references a bundled resource file skips gracefully when that file
/// is not present so that CI does not fail in environments without bundled assets.
/// </summary>
public class ShipCustomisationDatabaseTests
{
    // Minimal valid JSON that represents one ship customisation config.
    private const string SingleConfigJson = """
        [
          {
            "ConfigKey": "Sail",
            "BaseResource": "MODELS/COMMON/SPACECRAFT/SAILSHIP/SAILSHIP_PROC.SCENE.MBIN",
            "Slots": [
              {
                "SlotID": "SAIL_BODY",
                "Label": "FUSELAGE",
                "Items": [
                  {
                    "ItemID": "SAIL_BODYA",
                    "DescriptorGroupIDs": [ "SAIL_BODYA", "SAIL_BODYA_ALT" ]
                  },
                  {
                    "ItemID": "SAIL_BODYB",
                    "DescriptorGroupIDs": [ "SAIL_BODYB" ]
                  }
                ]
              },
              {
                "SlotID": "SAIL_WING",
                "Label": "WING",
                "Items": []
              }
            ],
            "TextureGroups": [
              {
                "GroupID": "SHIP_SAIL",
                "Options": [ "COATING", "PANELS", "METALBOLT" ]
              }
            ],
            "PaletteIDs": [ "SHIP", "SHIP_METALLIC" ]
          }
        ]
        """;

    // Helper: write JSON to a temp file, load it, then reset after the action.
    private static bool LoadTempJson(string json)
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmp, json);
            return ShipCustomisationDatabase.LoadFromFile(tmp);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    // Helper: reset database state to empty.
    private static void ResetDatabase()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmp, "[]");
            ShipCustomisationDatabase.LoadFromFile(tmp);
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    // --- LoadFromFile ---

    [Fact]
    public void LoadFromFile_NonexistentPath_ReturnsFalse()
    {
        Assert.False(ShipCustomisationDatabase.LoadFromFile("/nonexistent/path/Ship Customisation.json"));
    }

    [Fact]
    public void LoadFromFile_InvalidJson_ReturnsFalse()
    {
        Assert.False(LoadTempJson("this is not json"));
    }

    [Fact]
    public void LoadFromFile_JsonObjectNotArray_ReturnsFalse()
    {
        Assert.False(LoadTempJson("""{ "ConfigKey": "Fighter" }"""));
    }

    [Fact]
    public void LoadFromFile_EmptyArray_ReturnsTrueAndClearsConfigs()
    {
        try
        {
            // Seed some data first.
            LoadTempJson(SingleConfigJson);
            Assert.NotEmpty(ShipCustomisationDatabase.AllConfigs);

            // Now load empty array - should succeed and wipe configs.
            bool result = LoadTempJson("[]");
            Assert.True(result);
            Assert.Empty(ShipCustomisationDatabase.AllConfigs);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_ReturnsTrue()
    {
        try
        {
            Assert.True(LoadTempJson(SingleConfigJson));
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_PopulatesAllConfigs()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            Assert.Single(ShipCustomisationDatabase.AllConfigs);
        }
        finally
        {
            ResetDatabase();
        }
    }

    // --- Config field parsing ---

    [Fact]
    public void LoadFromFile_ValidJson_ConfigKeyAndBaseResourceParsed()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var cfg = ShipCustomisationDatabase.AllConfigs[0];
            Assert.Equal("Sail", cfg.ConfigKey);
            Assert.Equal("MODELS/COMMON/SPACECRAFT/SAILSHIP/SAILSHIP_PROC.SCENE.MBIN", cfg.BaseResource);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_SlotsParsed()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var slots = ShipCustomisationDatabase.AllConfigs[0].Slots;
            Assert.Equal(2, slots.Count);
            Assert.Equal("SAIL_BODY", slots[0].SlotID);
            Assert.Equal("FUSELAGE", slots[0].Label);
            Assert.Equal("SAIL_WING", slots[1].SlotID);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_ItemsParsed()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var items = ShipCustomisationDatabase.AllConfigs[0].Slots[0].Items;
            Assert.Equal(2, items.Count);
            Assert.Equal("SAIL_BODYA", items[0].ItemID);
            Assert.Equal(2, items[0].DescriptorGroupIDs.Count);
            Assert.Contains("SAIL_BODYA", items[0].DescriptorGroupIDs);
            Assert.Contains("SAIL_BODYA_ALT", items[0].DescriptorGroupIDs);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_TextureGroupsParsed()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var groups = ShipCustomisationDatabase.AllConfigs[0].TextureGroups;
            Assert.Single(groups);
            Assert.Equal("SHIP_SAIL", groups[0].GroupID);
            Assert.Equal(3, groups[0].Options.Count);
            Assert.Contains("COATING", groups[0].Options);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void LoadFromFile_ValidJson_PaletteIDsParsed()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var paletteIds = ShipCustomisationDatabase.AllConfigs[0].PaletteIDs;
            Assert.Equal(2, paletteIds.Count);
            Assert.Contains("SHIP", paletteIds);
            Assert.Contains("SHIP_METALLIC", paletteIds);
        }
        finally
        {
            ResetDatabase();
        }
    }

    // --- GetConfigByResource ---

    [Fact]
    public void GetConfigByResource_EmptyString_ReturnsNull()
    {
        Assert.Null(ShipCustomisationDatabase.GetConfigByResource(""));
    }

    [Fact]
    public void GetConfigByResource_UnknownPath_ReturnsNull()
    {
        Assert.Null(ShipCustomisationDatabase.GetConfigByResource("MODELS/UNKNOWN/SHIP.SCENE.MBIN"));
    }

    [Fact]
    public void GetConfigByResource_KnownPath_ReturnsConfig()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var cfg = ShipCustomisationDatabase.GetConfigByResource(
                "MODELS/COMMON/SPACECRAFT/SAILSHIP/SAILSHIP_PROC.SCENE.MBIN");
            Assert.NotNull(cfg);
            Assert.Equal("Sail", cfg!.ConfigKey);
        }
        finally
        {
            ResetDatabase();
        }
    }

    [Fact]
    public void GetConfigByResource_CaseInsensitive_ReturnsConfig()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            // Lookup with different case should still resolve.
            var cfg = ShipCustomisationDatabase.GetConfigByResource(
                "models/common/spacecraft/sailship/sailship_proc.scene.mbin");
            Assert.NotNull(cfg);
            Assert.Equal("Sail", cfg!.ConfigKey);
        }
        finally
        {
            ResetDatabase();
        }
    }

    // --- AllResourcePaths ---

    [Fact]
    public void AllResourcePaths_AfterLoad_ReturnsBaseResources()
    {
        try
        {
            LoadTempJson(SingleConfigJson);
            var paths = ShipCustomisationDatabase.AllResourcePaths;
            Assert.Single(paths);
            Assert.Equal("MODELS/COMMON/SPACECRAFT/SAILSHIP/SAILSHIP_PROC.SCENE.MBIN", paths[0]);
        }
        finally
        {
            ResetDatabase();
        }
    }

    // --- Bundled JSON (skip gracefully when the file is absent) ---

    [Fact]
    public void LoadFromFile_BundledJson_SucceedsAndContainsExpectedConfigs()
    {
        string jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Resources", "json", "Ship Customisation.json");

        if (!File.Exists(jsonPath)) return; // Skip if bundled assets are not present.

        try
        {
            bool result = ShipCustomisationDatabase.LoadFromFile(jsonPath);
            Assert.True(result);
            Assert.NotEmpty(ShipCustomisationDatabase.AllConfigs);

            // The bundled file has 5 configs: Fighter, Dropship, Scientific, Shuttle, Sail.
            Assert.Equal(5, ShipCustomisationDatabase.AllConfigs.Count);

            // Every config must have a non-empty BaseResource and at least one slot.
            foreach (var cfg in ShipCustomisationDatabase.AllConfigs)
            {
                Assert.False(string.IsNullOrEmpty(cfg.BaseResource),
                    $"Config '{cfg.ConfigKey}' has an empty BaseResource.");
                Assert.NotEmpty(cfg.Slots);
            }

            // AllResourcePaths must mirror AllConfigs.
            var paths = ShipCustomisationDatabase.AllResourcePaths;
            Assert.Equal(ShipCustomisationDatabase.AllConfigs.Count, paths.Count);
        }
        finally
        {
            ResetDatabase();
        }
    }
}
