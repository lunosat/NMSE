using NMSE.Core;
using NMSE.IO;
using NMSE.Models;

namespace NMSE.Tests;

/// <summary>
/// End-to-end round-trip tests for Expedition context saves,
/// verifying difficulty, ActiveContext, and StartingSeasonNumber
/// survive the export -> import -> save cycle.
/// </summary>
public class ExpeditionRoundTripTests
{
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
        playerState.Add("StartingSeasonNumber", 22);
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

    [Fact]
    public void ExpeditionSave_RoundTrip_DifficultyStateUnchanged()
    {
        var root = CreateExpeditionSaveData();
        SaveFileManager.RegisterContextTransforms(root);

        var originalMeta = MetaFileWriter.ExtractMetaInfo(root);
        Assert.Equal(2, originalMeta.DifficultyPreset);
        Assert.Equal("Normal", originalMeta.DifficultyPresetTag);

        string tmpPath = Path.Combine(Path.GetTempPath(), $"nmse_exped_{Guid.NewGuid()}.json");
        try
        {
            root.ExportToFile(tmpPath);

            var imported = JsonObject.ImportFromFile(tmpPath);
            SaveFileManager.RegisterContextTransforms(imported);

            var importedMeta = MetaFileWriter.ExtractMetaInfo(imported);
            Assert.Equal(2, importedMeta.DifficultyPreset);
            Assert.Equal("Normal", importedMeta.DifficultyPresetTag);
            Assert.Equal(4720, importedMeta.BaseVersion);
            Assert.Equal("ExpeditionSave", importedMeta.SaveName);
            Assert.Equal("Expedition Summary", importedMeta.SaveSummary);
        }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }

    [Fact]
    public void ExpeditionSave_ReadsStartingSeasonNumber_FromPlayerStateData()
    {
        var root = CreateExpeditionSaveData();
        SaveFileManager.RegisterContextTransforms(root);

        var psd = root.GetObject("PlayerStateData");
        Assert.NotNull(psd);

        int seasonNum = psd.GetInt("StartingSeasonNumber");
        Assert.Equal(22, seasonNum);
    }

    [Fact]
    public void ExpeditionSave_ActiveContextSurvivesRoundTrip()
    {
        var root = CreateExpeditionSaveData();
        SaveFileManager.RegisterContextTransforms(root);

        string tmpPath = Path.Combine(Path.GetTempPath(), $"nmse_expact_{Guid.NewGuid()}.json");
        try
        {
            root.ExportToFile(tmpPath);
            var imported = JsonObject.ImportFromFile(tmpPath);
            SaveFileManager.RegisterContextTransforms(imported);
            SaveFileManager.TryDetectActiveContext(imported);

            Assert.True(SaveContext.IsExpeditionSave);
            Assert.Equal("Season", imported.Get("ActiveContext") as string);
        }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }
}
