using NMSE.IO;
using NMSE.Models;
using NMSE.Data;

namespace NMSE.Tests;

/// <summary>
/// Tests for JSON export/import round-trip, ensuring difficulty and other
/// metadata survive the export -> import -> save cycle.
/// </summary>
public class SaveRoundTripTests
{
    [Fact]
    public void ContextSave_ImportWithTransforms_PreservesDifficulty()
    {
        // Simulates a modern context-based save exported and reimported.
        // RegisterContextTransforms must be called after import so that
        // ExtractMetaInfo (and all panels) can resolve PlayerStateData.
        var root = CreateContextSaveData();
        SaveFileManager.RegisterContextTransforms(root);
        
        var originalMeta = MetaFileWriter.ExtractMetaInfo(root);
        Assert.Equal(2, originalMeta.DifficultyPreset);   // Normal = 2
        Assert.Equal("Normal", originalMeta.DifficultyPresetTag);
        
        string tmpPath = Path.Combine(Path.GetTempPath(), $"nmse_ctxrt_{Guid.NewGuid()}.json");
        try
        {
            root.ExportToFile(tmpPath);
            
            // Import back (as OnImportJson does)
            var imported = JsonObject.ImportFromFile(tmpPath);
            SaveFileManager.RegisterContextTransforms(imported); // Critical for context saves
            
            var importedMeta = MetaFileWriter.ExtractMetaInfo(imported);
            
            Assert.Equal(2, importedMeta.DifficultyPreset);
            Assert.Equal("Normal", importedMeta.DifficultyPresetTag);
            Assert.Equal(4720, importedMeta.BaseVersion);
            Assert.Equal("TestSave", importedMeta.SaveName);
            Assert.Equal("Test Summary", importedMeta.SaveSummary);
        }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }

    [Fact]
    public void ContextSave_ImportWithoutTransforms_LosesDifficulty()
    {
        // Documents the bug: without RegisterContextTransforms, context saves
        // lose difficulty metadata because PlayerStateData cannot be resolved.
        var root = CreateContextSaveData();
        SaveFileManager.RegisterContextTransforms(root);
        
        var originalMeta = MetaFileWriter.ExtractMetaInfo(root);
        Assert.Equal(2, originalMeta.DifficultyPreset);
        
        string tmpPath = Path.Combine(Path.GetTempPath(), $"nmse_ctxbug_{Guid.NewGuid()}.json");
        try
        {
            root.ExportToFile(tmpPath);
            
            // Import WITHOUT RegisterContextTransforms (old behaviour)
            var imported = JsonObject.ImportFromFile(tmpPath);
            var importedMeta = MetaFileWriter.ExtractMetaInfo(imported);
            
            // PlayerStateData is under BaseContext, so without transforms it's invisible
            Assert.Equal(0, importedMeta.DifficultyPreset);   // Lost!
            Assert.Null(importedMeta.DifficultyPresetTag);     // Lost!
        }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }

    [Fact]
    public void LegacySave_ImportRoundTrip_PreservesDifficulty()
    {
        // Legacy saves have PlayerStateData at root level, so transforms are
        // not needed. Verify the round-trip preserves all difficulty metadata.
        var root = new JsonObject();
        root.Add("Version", 4720);
        
        var commonState = new JsonObject();
        commonState.Add("SaveName", "TestSave");
        commonState.Add("TotalPlayTime", 3600);
        root.Add("CommonStateData", commonState);
        
        var playerState = new JsonObject();
        playerState.Add("Health", 8);
        playerState.Add("SaveSummary", "Test Summary");
        var diffState = new JsonObject();
        var preset = new JsonObject();
        preset.Add("DifficultyPresetType", "Normal");
        diffState.Add("Preset", preset);
        var easiest = new JsonObject();
        easiest.Add("DifficultyPresetType", "Custom");
        diffState.Add("EasiestUsedPreset", easiest);
        var hardest = new JsonObject();
        hardest.Add("DifficultyPresetType", "Survival");
        diffState.Add("HardestUsedPreset", hardest);
        playerState.Add("DifficultyState", diffState);
        root.Add("PlayerStateData", playerState);
        
        var originalMeta = MetaFileWriter.ExtractMetaInfo(root);
        Assert.Equal(2, originalMeta.DifficultyPreset);
        Assert.Equal("Normal", originalMeta.DifficultyPresetTag);
        
        string tmpPath = Path.Combine(Path.GetTempPath(), $"nmse_legrt_{Guid.NewGuid()}.json");
        try
        {
            root.ExportToFile(tmpPath);
            var imported = JsonObject.ImportFromFile(tmpPath);
            SaveFileManager.RegisterContextTransforms(imported); // Safe no-op for legacy saves
            
            var importedMeta = MetaFileWriter.ExtractMetaInfo(imported);
            
            Assert.Equal(2, importedMeta.DifficultyPreset);
            Assert.Equal("Normal", importedMeta.DifficultyPresetTag);
            Assert.Equal(4720, importedMeta.BaseVersion);
        }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }

    [Fact]
    public void ContextSave_GameModePreserved()
    {
        // Verify that GameMode is correctly derived from DifficultyPreset
        // through the context transform chain.
        var root = CreateContextSaveData();
        SaveFileManager.RegisterContextTransforms(root);
        
        var meta = MetaFileWriter.ExtractMetaInfo(root);
        // Normal difficulty -> GameMode 1 (Normal)
        Assert.Equal(1, meta.GameMode);
    }

    [Fact]
    public void RegisterContextTransforms_SafeForLegacySaves()
    {
        // Calling RegisterContextTransforms on a legacy save (PlayerStateData at root)
        // should be a no-op and not break anything.
        var root = new JsonObject();
        root.Add("Version", 4720);
        var ps = new JsonObject();
        ps.Add("Health", 8);
        root.Add("PlayerStateData", ps);
        
        SaveFileManager.RegisterContextTransforms(root);
        
        // PlayerStateData is still directly accessible
        var resolved = root.GetObject("PlayerStateData");
        Assert.NotNull(resolved);
        Assert.Equal(8, resolved!.GetInt("Health"));
    }

    /// <summary>Creates a realistic modern context-based save structure.</summary>
    private static JsonObject CreateContextSaveData()
    {
        var root = new JsonObject();
        root.Add("Version", 4720);
        root.Add("ActiveContext", "Main");
        
        var commonState = new JsonObject();
        commonState.Add("SaveName", "TestSave");
        commonState.Add("TotalPlayTime", 3600);
        root.Add("CommonStateData", commonState);
        
        var baseContext = new JsonObject();
        var playerState = new JsonObject();
        playerState.Add("Health", 8);
        playerState.Add("SaveSummary", "Test Summary");
        var diffState = new JsonObject();
        var preset = new JsonObject();
        preset.Add("DifficultyPresetType", "Normal");
        diffState.Add("Preset", preset);
        var easiest = new JsonObject();
        easiest.Add("DifficultyPresetType", "Normal");
        diffState.Add("EasiestUsedPreset", easiest);
        var hardest = new JsonObject();
        hardest.Add("DifficultyPresetType", "Normal");
        diffState.Add("HardestUsedPreset", hardest);
        playerState.Add("DifficultyState", diffState);
        baseContext.Add("PlayerStateData", playerState);
        
        var spawnState = new JsonObject();
        spawnState.Add("LastKnownPlayerState", "Alive");
        baseContext.Add("SpawnStateData", spawnState);
        
        root.Add("BaseContext", baseContext);
        return root;
    }

    [Fact]
    public void ExpeditionSave_ImportWithTransforms_PreservesDifficulty()
    {
        var root = CreateExpeditionSaveData();
        SaveFileManager.RegisterContextTransforms(root);

        var meta = MetaFileWriter.ExtractMetaInfo(root);
        Assert.Equal(2, meta.DifficultyPreset);     // Normal = 2
        Assert.Equal("Normal", meta.DifficultyPresetTag);
    }

    [Fact]
    public void RegisterContextTransforms_ExpeditionSave_ResolvesPlayerStateData()
    {
        var root = CreateExpeditionSaveData();
        SaveFileManager.RegisterContextTransforms(root);
        var psd = root.GetObject("PlayerStateData");
        Assert.NotNull(psd);
        Assert.Equal(8, psd.GetInt("Health"));
    }

    /// <summary>Creates a realistic Expedition context-based save structure.</summary>
    private static JsonObject CreateExpeditionSaveData()
    {
        var root = new JsonObject();
        root.Add("Version", 4720);
        root.Add("ActiveContext", "Season");

        var commonState = new JsonObject();
        commonState.Add("SaveName", "ExpeditionSave");
        commonState.Add("TotalPlayTime", 7200);
        root.Add("CommonStateData", commonState);

        var expeditionContext = new JsonObject();
        var playerState = new JsonObject();
        playerState.Add("Health", 8);
        playerState.Add("SaveSummary", "Expedition Summary");
        var diffState = new JsonObject();
        var preset = new JsonObject();
        preset.Add("DifficultyPresetType", "Normal");
        diffState.Add("Preset", preset);
        var easiest = new JsonObject();
        easiest.Add("DifficultyPresetType", "Normal");
        diffState.Add("EasiestUsedPreset", easiest);
        var hardest = new JsonObject();
        hardest.Add("DifficultyPresetType", "Normal");
        diffState.Add("HardestUsedPreset", hardest);
        playerState.Add("DifficultyState", diffState);
        expeditionContext.Add("PlayerStateData", playerState);

        var spawnState = new JsonObject();
        spawnState.Add("LastKnownPlayerState", "Alive");
        expeditionContext.Add("SpawnStateData", spawnState);

        root.Add("ExpeditionContext", expeditionContext);
        return root;
    }
}
