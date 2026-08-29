using NMSE.Core;
using NMSE.Data;
using NMSE.IO;
using NMSE.Models;

namespace NMSE.Tests;

/// <summary>
/// Exercises load, edit and save against a real game save. Skipped unless
/// NMSE_TEST_SAVE points at one, so the suite stays runnable without it.
/// </summary>
public class RealSaveSmokeTest
{
    private static string? SavePath => Environment.GetEnvironmentVariable("NMSE_TEST_SAVE");

    /// <summary>
    /// NMS stores its keys obfuscated ("F2P", "&lt;h0"); mapping.json is what turns
    /// them back into names like PlayerStateData. Without it every lookup misses.
    /// </summary>
    private static void RegisterMapper()
    {
        // Resources are not copied into the test output; walk up to the repo tree.
        string? mappingPath = null;
        string probe = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            string candidate = Path.Combine(probe, "Resources", "map", "mapping.json");
            if (File.Exists(candidate)) { mappingPath = candidate; break; }
            probe = Path.GetFullPath(Path.Combine(probe, ".."));
        }
        Assert.True(mappingPath is not null, "mapping.json not found");
        Console.WriteLine($"mapping.json     : {mappingPath}");

        var mapper = new JsonNameMapper();
        mapper.Load(mappingPath!);
        JsonParser.SetDefaultMapper(mapper);
    }

    private static JsonObject Load(string path)
    {
        var root = SaveFileManager.LoadSaveFile(path);
        SaveFileManager.RegisterContextTransforms(root);
        return root;
    }

    [Fact]
    public void RealSave_RoundTripsThroughLoadEditSave()
    {
        string? source = SavePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source)) return; // no save supplied

        RegisterMapper();

        // Work on a copy: the round trip rewrites the file.
        string dir = Path.Combine(Path.GetTempPath(), "nmse_rt_" + Guid.NewGuid().ToString("N")[..10]);
        Directory.CreateDirectory(dir);
        string work = Path.Combine(dir, Path.GetFileName(source));
        File.Copy(source, work);

        try
        {
            var root = Load(work);
            var playerState = root.GetObject("PlayerStateData");
            Assert.NotNull(playerState);

            decimal units    = MainStatsLogic.ReadRawStatValue(playerState!, "Units");
            decimal nanites  = MainStatsLogic.ReadRawStatValue(playerState!, "Nanites");
            decimal quicksil = MainStatsLogic.ReadRawStatValue(playerState!, "Specials");
            decimal health   = MainStatsLogic.ReadRawStatValue(playerState!, "Health");

            var meta = MetaFileWriter.ExtractMetaInfo(root);
            Console.WriteLine($"READ  save name  : {meta.SaveName}");
            Console.WriteLine($"READ  base ver   : {meta.BaseVersion}");
            Console.WriteLine($"READ  difficulty : {meta.DifficultyPresetTag}");
            Console.WriteLine($"READ  units      : {units}");
            Console.WriteLine($"READ  nanites    : {nanites}");
            Console.WriteLine($"READ  quicksilver: {quicksil}");
            Console.WriteLine($"READ  health     : {health}");

            // A base version and a difficulty tag prove the obfuscated keys resolved;
            // zeroes would mean every read silently returned a default. The save name
            // is legitimately empty on a save the player never renamed.
            Assert.True(meta.BaseVersion > 0, "base version did not resolve");
            Assert.False(string.IsNullOrWhiteSpace(meta.DifficultyPresetTag), "difficulty did not resolve");

            // Edit one value, then write the file back out the way the app does.
            decimal newUnits = units + 12345;
            playerState!.Set("Units", (long)newUnits);
            SaveFileManager.SaveToFile(work, root, compress: true, writeMeta: false);

            // Reload from disk and confirm the edit survived compression and parsing.
            var reloaded = Load(work);
            var reloadedState = reloaded.GetObject("PlayerStateData");
            Assert.NotNull(reloadedState);

            decimal after = MainStatsLogic.ReadRawStatValue(reloadedState!, "Units");
            Console.WriteLine($"WROTE units      : {newUnits}");
            Console.WriteLine($"RELOAD units     : {after}");
            Assert.Equal(newUnits, after);

            // Everything else must be untouched.
            Assert.Equal(nanites,  MainStatsLogic.ReadRawStatValue(reloadedState!, "Nanites"));
            Assert.Equal(quicksil, MainStatsLogic.ReadRawStatValue(reloadedState!, "Specials"));
            Assert.Equal(health,   MainStatsLogic.ReadRawStatValue(reloadedState!, "Health"));

            var reloadedMeta = MetaFileWriter.ExtractMetaInfo(reloaded);
            Assert.Equal(meta.SaveName, reloadedMeta.SaveName);
            Assert.Equal(meta.BaseVersion, reloadedMeta.BaseVersion);
            Console.WriteLine("round trip OK: edit persisted, everything else preserved");
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }
}
