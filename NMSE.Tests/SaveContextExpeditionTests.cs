using NMSE.Core;
using NMSE.IO;

namespace NMSE.Tests;

public class SaveContextExpeditionTests
{
    [Fact]
    public void Reset_ClearsExpeditionFlag()
    {
        SaveContext.SetExpedition(true);
        Assert.True(SaveContext.IsExpeditionSave);
        SaveContext.Reset();
        Assert.False(SaveContext.IsExpeditionSave);
    }

    [Fact]
    public void DetectActiveContextFromJson_ExpeditionJson_ReturnsTrue()
    {
        const string json = """{"Version":4731,"ActiveContext":"Season","ExpeditionContext":{}}""";
        bool found = SaveFileManager.DetectActiveContextFromJson(json, out bool isExpedition);
        Assert.True(found);
        Assert.True(isExpedition);
    }

    [Fact]
    public void DetectActiveContextFromJson_NormalJson_ReturnsFalse()
    {
        const string json = """{"Version":4731,"ActiveContext":"Main","BaseContext":{}}""";
        bool found = SaveFileManager.DetectActiveContextFromJson(json, out bool isExpedition);
        Assert.True(found);
        Assert.False(isExpedition);
    }

    [Fact]
    public void DetectActiveContextFromJson_WithObfuscatedKey_DetectsSeason()
    {
        const string json = """{"Version":4731,"XTp":"Season","2YS":{}}""";
        bool found = SaveFileManager.DetectActiveContextFromJson(json, out bool isExpedition);
        Assert.True(found);
        Assert.True(isExpedition);
    }

    [Fact]
    public void DetectActiveContextFromJson_NullEmpty_ReturnsFalse()
    {
        bool foundNull = SaveFileManager.DetectActiveContextFromJson(null!, out bool expNull);
        Assert.False(foundNull);
        Assert.False(expNull);

        bool foundEmpty = SaveFileManager.DetectActiveContextFromJson("", out bool expEmpty);
        Assert.False(foundEmpty);
        Assert.False(expEmpty);
    }

    [Fact]
    public void DetectActiveContextFromJson_NoActiveContext_ReturnsFalse()
    {
        const string json = """{"Version":4731,"SomeOtherKey":"Value"}""";
        bool found = SaveFileManager.DetectActiveContextFromJson(json, out bool isExpedition);
        Assert.False(found);
        Assert.False(isExpedition);
    }

    [Fact]
    public void DetectActiveContextFast_ExpeditionSaveFile_DetectsSeason()
    {
        string path = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\_ref\expedition_save\save8.hg");
        if (!File.Exists(path))
            return; // skip if fixture not found in test context

        bool found = SaveFileManager.DetectActiveContextFast(path, out bool isExpedition);
        Assert.True(found);
        Assert.True(isExpedition);
    }

    [Fact]
    public void DetectActiveContextFast_NonExistentFile_ReturnsFalse()
    {
        bool found = SaveFileManager.DetectActiveContextFast(@"Z:\nonexistent\file.hg", out bool isExpedition);
        Assert.False(found);
        Assert.False(isExpedition);
    }
}
