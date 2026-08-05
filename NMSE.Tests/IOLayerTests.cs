using NMSE.IO;
using NMSE.Data;
using NMSE.Models;
using System.IO.Compression;
using NMSE.Config;

namespace NMSE.Tests;

/// <summary>
/// Tests for IO layer classes: BinaryIO, Lz4Compressor, and SaveFileManager.
/// </summary>
public class IOLayerTests
{
    // --- BinaryIO: Int32 LE round-trip -------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(0x12345678)]
    public void BinaryIO_ReadWriteInt32LE_RoundTrip(int value)
    {
        using var ms = new MemoryStream();
        BinaryIO.WriteInt32LE(ms, value);
        ms.Position = 0;
        int result = BinaryIO.ReadInt32LE(ms);
        Assert.Equal(value, result);
    }

    [Fact]
    public void BinaryIO_WriteInt32LE_ProducesLittleEndianBytes()
    {
        using var ms = new MemoryStream();
        BinaryIO.WriteInt32LE(ms, 0x04030201);
        byte[] bytes = ms.ToArray();
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0x01, bytes[0]);
        Assert.Equal(0x02, bytes[1]);
        Assert.Equal(0x03, bytes[2]);
        Assert.Equal(0x04, bytes[3]);
    }

    // --- BinaryIO: Int64 LE round-trip -------------------------------

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void BinaryIO_ReadWriteInt64LE_RoundTrip(long value)
    {
        using var ms = new MemoryStream();
        BinaryIO.WriteInt64LE(ms, value);
        ms.Position = 0;
        long result = BinaryIO.ReadInt64LE(ms);
        Assert.Equal(value, result);
    }

    // --- BinaryIO: Base64 round-trip ---------------------------------

    [Fact]
    public void BinaryIO_Base64EncodeDecode_RoundTrip()
    {
        byte[] data = { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD, 0x80, 0x7F };
        string encoded = BinaryIO.Base64Encode(data);
        byte[] decoded = BinaryIO.Base64Decode(encoded);
        Assert.Equal(data, decoded);
    }

    [Fact]
    public void BinaryIO_Base64Encode_EmptyArray_ReturnsEmptyString()
    {
        Assert.Equal("", BinaryIO.Base64Encode(Array.Empty<byte>()));
    }

    [Fact]
    public void BinaryIO_Base64Decode_EmptyString_ReturnsEmptyArray()
    {
        Assert.Empty(BinaryIO.Base64Decode(""));
    }

    [Fact]
    public void BinaryIO_Base64Encode_ProducesValidBase64()
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        string encoded = BinaryIO.Base64Encode(data);
        Assert.Equal("SGVsbG8sIFdvcmxkIQ==", encoded);
    }

    // --- BinaryIO: ReadAllBytes --------------------------------------

    [Fact]
    public void BinaryIO_ReadAllBytes_ReadsEntireStream()
    {
        byte[] expected = { 1, 2, 3, 4, 5, 6, 7, 8 };
        using var ms = new MemoryStream(expected);
        byte[] result = BinaryIO.ReadAllBytes(ms);
        Assert.Equal(expected, result);
    }

    // --- BinaryIO: ReadFully -----------------------------------------

    [Fact]
    public void BinaryIO_ReadFully_ThrowsOnShortRead()
    {
        using var ms = new MemoryStream(new byte[] { 1, 2 });
        byte[] buf = new byte[4];
        Assert.Throws<IOException>(() => BinaryIO.ReadFully(ms, buf, 0, 4));
    }

    // --- Lz4Compressor: round-trip -----------------------------------

    [Fact]
    public void Lz4Compressor_CompressDecompress_RoundTrip_SimpleData()
    {
        byte[] original = System.Text.Encoding.UTF8.GetBytes(
            "Hello World! Hello World! Hello World! Hello World! " +
            "This is a test of LZ4 compression and decompression.");
        
        byte[] compressed = new byte[Lz4Compressor.MaxCompressedLength(original.Length)];
        int compressedLen = Lz4Compressor.Compress(original, 0, original.Length,
            compressed, 0, compressed.Length);

        Assert.True(compressedLen > 0);

        byte[] decompressed = new byte[original.Length];
        int decompressedLen = Lz4Compressor.Decompress(compressed, 0, compressedLen,
            decompressed, 0, decompressed.Length);

        Assert.Equal(original.Length, decompressedLen);
        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void Lz4Compressor_CompressDecompress_RoundTrip_RepetitiveData()
    {
        // Highly repetitive data should compress well
        byte[] original = new byte[4096];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)(i % 16);

        byte[] compressed = new byte[Lz4Compressor.MaxCompressedLength(original.Length)];
        int compressedLen = Lz4Compressor.Compress(original, 0, original.Length,
            compressed, 0, compressed.Length);

        Assert.True(compressedLen > 0);
        Assert.True(compressedLen < original.Length, "Repetitive data should compress smaller");

        byte[] decompressed = new byte[original.Length];
        int decompressedLen = Lz4Compressor.Decompress(compressed, 0, compressedLen,
            decompressed, 0, decompressed.Length);

        Assert.Equal(original.Length, decompressedLen);
        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void Lz4Compressor_Compress_EmptyInput_ReturnsZero()
    {
        byte[] compressed = new byte[64];
        int len = Lz4Compressor.Compress(Array.Empty<byte>(), 0, 0, compressed, 0, compressed.Length);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Lz4Compressor_MaxCompressedLength_ReturnsPositive()
    {
        Assert.True(Lz4Compressor.MaxCompressedLength(100) > 100);
        Assert.True(Lz4Compressor.MaxCompressedLength(0) >= 0);
    }

    [Fact]
    public void Lz4Compressor_MaxCompressedLength_NegativeInput_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Lz4Compressor.MaxCompressedLength(-1));
    }

    [Fact]
    public void Lz4Compressor_CompressDecompress_RoundTrip_RandomData()
    {
        var rng = new Random(42);
        byte[] original = new byte[8192];
        rng.NextBytes(original);

        byte[] compressed = new byte[Lz4Compressor.MaxCompressedLength(original.Length)];
        int compressedLen = Lz4Compressor.Compress(original, 0, original.Length,
            compressed, 0, compressed.Length);

        Assert.True(compressedLen > 0);

        byte[] decompressed = new byte[original.Length];
        int decompressedLen = Lz4Compressor.Decompress(compressed, 0, compressedLen,
            decompressed, 0, decompressed.Length);

        Assert.Equal(original.Length, decompressedLen);
        Assert.Equal(original, decompressed);
    }

    // --- PS4 NOMANSKY save file tests --------------------------------

    private static string? FindRefPath(params string[] parts)
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(new[] { dir }.Concat(parts).ToArray());
            if (File.Exists(candidate) || Directory.Exists(candidate)) return candidate;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return null;
    }

    private static void EnsureMapperLoaded()
    {
        var mapperPath = FindRefPath("Resources", "map", "mapping.json");
        if (mapperPath == null) return;
        var mapper = new JsonNameMapper();
        mapper.Load(mapperPath);
        JsonParser.SetDefaultMapper(mapper);
    }

    [Fact]
    public void SaveFileManager_LoadSaveFile_PS4NomanSky_ReturnsJsonObject()
    {
        var savePath = FindRefPath("_ref", "saves", "ps4", "savedata02.hg");
        if (savePath == null) return; // skip if reference save not available

        EnsureMapperLoaded();

        var save = SaveFileManager.LoadSaveFile(savePath);
        Assert.NotNull(save);
        Assert.Equal(4720, save.GetInt("Version"));
    }

    [Fact]
    public void SaveFileManager_LoadSaveFile_PS4NomanSky_PlayerStateDataAccessible()
    {
        var savePath = FindRefPath("_ref", "saves", "ps4", "savedata02.hg");
        if (savePath == null) return;

        EnsureMapperLoaded();

        var save = SaveFileManager.LoadSaveFile(savePath);
        Assert.NotNull(save);

        // Debug: check what top-level keys exist
        var topKeys = save.Names();
        Assert.NotEmpty(topKeys);

        // Check ActiveContext exists
        var activeContext = save.Get("ActiveContext");
        Assert.NotNull(activeContext);
        Assert.Equal("Main", activeContext);

        // Check BaseContext exists
        var baseContext = save.GetValue("BaseContext");
        Assert.NotNull(baseContext);
        Assert.IsType<JsonObject>(baseContext);

        // Check BaseContext.PlayerStateData exists
        var bcPsd = save.GetValue("BaseContext.PlayerStateData");
        Assert.NotNull(bcPsd);

        // PlayerStateData should be accessible via transform
        var psd = save.GetValue("PlayerStateData");
        Assert.NotNull(psd);
        Assert.IsType<JsonObject>(psd);
    }

    [Fact]
    public void SaveFileManager_DetectGameModeFast_PS4NomanSky_ReturnsNonZero()
    {
        var savePath = FindRefPath("_ref", "saves", "ps4", "savedata02.hg");
        if (savePath == null) return;

        int gameMode = SaveFileManager.DetectGameModeFast(savePath);
        Assert.True(gameMode >= 0,
            $"DetectGameModeFast should return a valid game mode, got {gameMode}");
    }

    [Fact]
    public void SaveFileManager_DetectSaveNameFast_PS4NomanSky_DoesNotThrow()
    {
        var savePath = FindRefPath("_ref", "saves", "ps4", "savedata02.hg");
        if (savePath == null) return;

        string saveName = SaveFileManager.DetectSaveNameFast(savePath);
        Assert.NotNull(saveName); // may be empty, but should not be null or throw
    }

    [Fact]
    public void SaveFileManager_DetectPlatform_PS4Directory_ReturnsPS4()
    {
        var saveDir = FindRefPath("_ref", "saves", "ps4");
        if (saveDir == null) return;

        var platform = SaveFileManager.DetectPlatform(saveDir);
        Assert.Equal(SaveFileManager.Platform.PS4, platform);
    }

    // --- PS4 SaveWizard streaming (.hg with NOMANSKY header) tests ---------

    [Fact]
    public void SaveFileManager_IsNomanSkyFile_SaveWizardStreaming_ReturnsTrue()
    {
        var savePath = FindRefPath("_ref", "ps4", "ps4_other", "savedata02.hg");
        if (savePath == null) return;

        Assert.True(SaveFileManager.IsNomanSkyFile(savePath));
    }

    [Fact]
    public void SaveFileManager_IsNomanSkyFile_HTOSPlainJson_ReturnsFalse()
    {
        var savePath = FindRefPath("_ref", "ps4", "savedata02.hg");
        if (savePath == null) return;

        Assert.False(SaveFileManager.IsNomanSkyFile(savePath));
    }

    [Fact]
    public void SaveFileManager_LoadSaveFile_SaveWizardStreaming_ParsesJson()
    {
        var savePath = FindRefPath("_ref", "ps4", "ps4_other", "savedata02.hg");
        if (savePath == null) return;

        EnsureMapperLoaded();
        var save = SaveFileManager.LoadSaveFile(savePath);
        Assert.NotNull(save);
        Assert.True(save.GetInt("Version") > 0, "SaveWizard streaming file should have Version");
    }

    [Fact]
    public void SaveFileManager_SaveNomanSkyFile_RoundTripPreservesData()
    {
        var savePath = FindRefPath("_ref", "ps4", "ps4_other", "savedata02.hg");
        if (savePath == null) return;

        EnsureMapperLoaded();

        // Load the original SaveWizard streaming file
        var originalHeader = new byte[0x70];
        using (var fs = new FileStream(savePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            fs.ReadExactly(originalHeader, 0, 0x70);

        var save = SaveFileManager.LoadSaveFile(savePath);
        Assert.NotNull(save);

        // Save to a temporary file using SaveNomanSkyFile
        string tmpFile = Path.Combine(Path.GetTempPath(), $"nmse_nomansky_test_{Guid.NewGuid():N}.hg");
        try
        {
            // Copy the original file to temporary location so SaveNomanSkyFile can read its header
            File.Copy(savePath, tmpFile, overwrite: true);

            // Save the data back
            SaveFileManager.SaveNomanSkyFile(tmpFile, save);

            // Verify the saved file starts with NOMANSKY header
            Assert.True(SaveFileManager.IsNomanSkyFile(tmpFile),
                "Saved file should start with NOMANSKY header");

            // Verify the saved file can be re-loaded
            var reloaded = SaveFileManager.LoadSaveFile(tmpFile);
            Assert.NotNull(reloaded);
            Assert.Equal(save.GetInt("Version"), reloaded.GetInt("Version"));

            // Verify the header is preserved (magic bytes, version, etc.)
            byte[] savedHeader = new byte[0x70];
            using (var fs = new FileStream(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.ReadExactly(savedHeader, 0, 0x70);

            // First 8 bytes should be "NOMANSKY"
            Assert.Equal((byte)'N', savedHeader[0]);
            Assert.Equal((byte)'O', savedHeader[1]);
            Assert.Equal((byte)'M', savedHeader[2]);
            Assert.Equal((byte)'A', savedHeader[3]);
            Assert.Equal((byte)'N', savedHeader[4]);
            Assert.Equal((byte)'S', savedHeader[5]);
            Assert.Equal((byte)'K', savedHeader[6]);
            Assert.Equal((byte)'Y', savedHeader[7]);

            // JSON size field at 0x5C should be reasonable (> 0)
            int jsonSize = savedHeader[0x5C] | (savedHeader[0x5D] << 8) |
                           (savedHeader[0x5E] << 16) | (savedHeader[0x5F] << 24);
            Assert.True(jsonSize > 0, $"JSON size at 0x5C should be positive, got {jsonSize}");

            // JSON size should match (file size - 0x70)
            long fileSize = new FileInfo(tmpFile).Length;
            Assert.Equal(jsonSize, (int)(fileSize - 0x70));
        }
        finally
        {
            try { File.Delete(tmpFile); } catch { }
        }
    }

    // --- Special character save file tests ----------------------------

    [Fact]
    public void SaveFileManager_LoadSaveFile_SpecialCharacterSave_SaveNameIsString()
    {
        EnsureMapperLoaded();
        var savePath = FindRefPath("_ref", "saves", "special_characters", "save.hg");
        if (savePath == null) return; // skip if reference save not available

        var data = SaveFileManager.LoadSaveFile(savePath);
        var commonState = data.GetObject("CommonStateData");
        Assert.NotNull(commonState);

        // The save name contains Greek λ (U+03BB) and Latin Ŧ (U+0166).
        // These are stored as raw UTF-8 bytes in the save file and must be
        // decoded as a Unicode string, not misclassified as BinaryData.
        var rawName = commonState!.Get("SaveName");
        Assert.IsType<string>(rawName);

        string name = commonState.GetString("SaveName")!;
        Assert.Contains("\u03BB", name); // λ
        Assert.Contains("\u0166", name); // Ŧ
        Assert.Contains("Breach", name);
    }

    [Fact]
    public void SaveFileManager_LoadSaveFile_SpecialCharacterSave_SettlementNamesAreStrings()
    {
        EnsureMapperLoaded();
        var savePath = FindRefPath("_ref", "saves", "special_characters", "save.hg");
        if (savePath == null) return;

        var data = SaveFileManager.LoadSaveFile(savePath);
        var playerState = data.GetObject("PlayerStateData");
        var settlements = playerState?.GetArray("SettlementStatesV2");
        if (settlements == null || settlements.Length == 0) return;

        // Verify that no settlement names are incorrectly classified as BinaryData.
        // Settlements 49 and 89 in the save have Greek characters in their names.
        int binaryNameCount = 0;
        for (int i = 0; i < settlements.Length; i++)
        {
            var settlement = settlements.GetObject(i);
            if (settlement == null) continue;
            var rawName = settlement.Get("Name");
            if (rawName is BinaryData)
                binaryNameCount++;
        }
        Assert.Equal(0, binaryNameCount);
    }

    // --- Backup and restore tests ------------------------------------

    [Fact]
    public void SaveFileManager_BackupSaveDirectory_IncludesSaveAndMetaFiles()
    {
        string tmpDir = CreateTempDir();
        string cacheDir = Path.Combine(tmpDir, "cache");
        Directory.CreateDirectory(cacheDir);

        try
        {
            // Create test files: .hg and meta.json should be included, everything else excluded
            File.WriteAllText(Path.Combine(tmpDir, "save.hg"), "test save data");
            File.WriteAllText(Path.Combine(tmpDir, "meta.json"), "{}");
            File.WriteAllText(Path.Combine(cacheDir, "other.hg"), "nested hg");
            File.WriteAllText(Path.Combine(cacheDir, "texture.dds"), "fake dds");
            File.WriteAllText(Path.Combine(cacheDir, "random.bin"), "should be excluded");

            string zipPath = Path.Combine(Path.GetTempPath(), $"nmse_backup_test_{Guid.NewGuid():N}.zip");
            try
            {
                using var zip = CreateBackupZip(tmpDir, zipPath);

                var entryNames = zip.Entries.Select(e => e.FullName).ToList();
                Assert.Contains(entryNames, e => e.Contains("save.hg", StringComparison.Ordinal));
                Assert.Contains(entryNames, e => e.Contains("other.hg", StringComparison.Ordinal));
                Assert.Contains(entryNames, e => e.Contains("meta.json", StringComparison.Ordinal));
                Assert.DoesNotContain(entryNames, e => e.EndsWith(".dds", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entryNames, e => e.EndsWith(".bin", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                try { File.Delete(zipPath); } catch { }
            }
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_BackupSaveDirectory_PS4IncludesMemoryDat()
    {
        string tmpDir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(tmpDir, "memory.dat"), "PS4 MONOLITHIC SAVE");
            File.WriteAllText(Path.Combine(tmpDir, "note.txt"), "not part of the save");

            string zipPath = Path.Combine(Path.GetTempPath(), $"nmse_backup_test_{Guid.NewGuid():N}.zip");
            try
            {
                using var zip = CreateBackupZip(tmpDir, zipPath);

                var entryNames = zip.Entries.Select(e => e.FullName).ToList();
                Assert.Contains(entryNames, e => e.Equals("memory.dat", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(entryNames, e => e.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                try { File.Delete(zipPath); } catch { }
            }
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_BackupSaveDirectory_XboxIncludesBlobsAndIndex()
    {
        string tmpDir = CreateTempDir();
        try
        {
            string blobDir = Path.Combine(tmpDir, "containers", "index", "3", "c0ffee");
            Directory.CreateDirectory(blobDir);
            File.WriteAllText(Path.Combine(tmpDir, "containers.index"), "index");
            File.WriteAllText(Path.Combine(blobDir, "1234ABCDEF"), "blob data");

            string zipPath = Path.Combine(Path.GetTempPath(), $"nmse_backup_test_{Guid.NewGuid():N}.zip");
            try
            {
                using var zip = CreateBackupZip(tmpDir, zipPath);

                var entryNames = zip.Entries.Select(e => e.FullName).ToList();
                Assert.Contains(entryNames, e => e.Equals("containers.index", StringComparison.OrdinalIgnoreCase));
                Assert.Contains(entryNames, e => e.EndsWith("1234ABCDEF", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                try { File.Delete(zipPath); } catch { }
            }
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_RestoreFileFromBackup_FindsNestedEntry()
    {
        string tmpDir = CreateTempDir();
        try
        {
            string blobDir = Path.Combine(tmpDir, "containers", "index", "3", "c0ffee");
            Directory.CreateDirectory(blobDir);
            File.WriteAllText(Path.Combine(blobDir, "save.hg"), "restored content");
            string zipPath = Path.Combine(tmpDir, "backup.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(Path.Combine(blobDir, "save.hg"), "containers\\index\\3\\c0ffee\\save.hg");

            string dest = Path.Combine(tmpDir, "save.hg");
            Assert.True(SaveFileManager.RestoreFileFromBackup(zipPath, "save.hg", dest));
            Assert.Equal("restored content", File.ReadAllText(dest));
            Assert.False(SaveFileManager.RestoreFileFromBackup(zipPath, "missing.hg", dest));
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_RestoreBackupToDirectory_RestoresStructureAndSkipsTraversal()
    {
        string tmpDir = CreateTempDir();
        try
        {
            string srcSave = WriteTempFile(tmpDir, "src_save.hg", "save data");
            string srcOther = WriteTempFile(tmpDir, "src_other.hg", "other data");
            string zipPath = Path.Combine(tmpDir, "backup.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(srcSave, "save.hg");
                zip.CreateEntryFromFile(srcOther, "sub\\other.hg");
                zip.CreateEntry("..\\evil.hg"); // malicious entry - must be skipped
            }

            string destDir = Path.Combine(tmpDir, "restore");
            Directory.CreateDirectory(destDir);
            var written = SaveFileManager.RestoreBackupToDirectory(zipPath, destDir);

            Assert.Contains(written, w => w.EndsWith("save.hg", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(written, w => w.EndsWith("other.hg", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(written, w => w.Contains("evil", StringComparison.OrdinalIgnoreCase));
            Assert.Equal("save data", File.ReadAllText(Path.Combine(destDir, "save.hg")));
            Assert.Equal("other data", File.ReadAllText(Path.Combine(destDir, "sub", "other.hg")));
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_BackupContainsFile_MatchesByFileName()
    {
        string tmpDir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(tmpDir, "save.hg"), "data");
            string zipPath = Path.Combine(tmpDir, "backup.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(Path.Combine(tmpDir, "save.hg"), "containers\\index\\3\\save.hg");

            Assert.True(SaveFileManager.BackupContainsFile(zipPath, "save.hg"));
            Assert.True(SaveFileManager.BackupContainsFile(zipPath, "SAVE.HG")); // case-insensitive
            Assert.False(SaveFileManager.BackupContainsFile(zipPath, "save2.hg"));
        }
        finally
        {
            try { Directory.Delete(tmpDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_FindBackupZips_ReturnsNewestFirst()
    {
        string saveDir = CreateTempDir();
        string backupRoot = Path.Combine(Path.GetTempPath(), $"nmse_backup_root_{Guid.NewGuid():N}");
        string? previous = AppConfig.Instance.BackupDirectory;
        try
        {
            Directory.CreateDirectory(backupRoot);
            AppConfig.Instance.BackupDirectory = backupRoot;

            string dirName = new DirectoryInfo(saveDir).Name;
            string oldZip = Path.Combine(backupRoot, $"{dirName}_20260101_120000.zip");
            string newZip = Path.Combine(backupRoot, $"{dirName}_20260701_120000.zip");
            File.WriteAllText(oldZip, "old");
            File.WriteAllText(newZip, "new");
            File.SetCreationTimeUtc(oldZip, new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
            File.SetCreationTimeUtc(newZip, new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc));

            var zips = SaveFileManager.FindBackupZips(saveDir);

            Assert.Equal(2, zips.Count);
            Assert.Equal(newZip, zips[0]);
            Assert.Equal(oldZip, zips[1]);
        }
        finally
        {
            AppConfig.Instance.BackupDirectory = previous;
            try { Directory.Delete(saveDir, true); } catch { }
            try { Directory.Delete(backupRoot, true); } catch { }
        }
    }

    [Fact]
    public void SaveFileManager_FindBackupZips_DoesNotDuplicateOverlappingRoots()
    {
        string saveDir = CreateTempDir();
        string tempRoot = Path.Combine(Path.GetTempPath(), "NMSE", "Save Backups");
        string? previous = AppConfig.Instance.BackupDirectory;
        string zipPath = "";
        try
        {
            Directory.CreateDirectory(tempRoot);
            // Configure the same directory as the TEMP fallback root: the roots
            // list would previously contain it twice and list each backup twice.
            AppConfig.Instance.BackupDirectory = tempRoot;

            string dirName = new DirectoryInfo(saveDir).Name;
            zipPath = Path.Combine(tempRoot, $"{dirName}_20260701_120000.zip");
            File.WriteAllText(zipPath, "data");

            var roots = SaveFileManager.FindExistingBackupRoots();
            Assert.Equal(roots.Count, roots.Distinct().Count());

            var zips = SaveFileManager.FindBackupZips(saveDir);
            Assert.Single(zips);
            Assert.Equal(zipPath, zips[0]);
        }
        finally
        {
            AppConfig.Instance.BackupDirectory = previous;
            try { File.Delete(zipPath); } catch { }
            try { Directory.Delete(saveDir, true); } catch { }
        }
    }

    /// <summary>
    /// Creates a uniquely named temp directory and returns its path.
    /// </summary>
    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"nmse_backup_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Writes a file into the given directory, returning its path.
    /// </summary>
    private static string WriteTempFile(string dir, string name, string content)
    {
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Creates a backup ZIP of the given directory via the private
    /// <see cref="SaveFileManager"/> filter, returning the opened archive.
    /// </summary>
    private static ZipArchive CreateBackupZip(string sourceDir, string zipPath)
    {
        var method = typeof(SaveFileManager).GetMethod("CreateFilteredZip",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        method.Invoke(null, new object[] { sourceDir, zipPath });
        return ZipFile.OpenRead(zipPath);
    }
}
