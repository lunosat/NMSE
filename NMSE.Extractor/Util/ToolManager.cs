using NMSE.Extractor.Config;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace NMSE.Extractor.Util;

public static class ToolManager
{
    // Client for version-check redirects (fast, no response body).
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        DefaultRequestHeaders = { { "User-Agent", "NMSE-Extractor/1.0" } },
        Timeout = TimeSpan.FromSeconds(15),
    };

    // Client that follows redirects for actual file downloads (may be large).
    private static readonly HttpClient DownloadClient = new(new HttpClientHandler { AllowAutoRedirect = true })
    {
        DefaultRequestHeaders = { { "User-Agent", "NMSE-Extractor/1.0" } },
        Timeout = TimeSpan.FromMinutes(5),
    };

    /// <summary>
    /// Resolves the latest release tag from a GitHub "/releases/latest/" URL
    /// by reading the Location header from the redirect response.
    /// </summary>
    public static async Task<string?> GetLatestReleaseTagAsync(string latestUrl)
    {
        try
        {
            var response = await Http.GetAsync(latestUrl);
            if (response.StatusCode is HttpStatusCode.Redirect
                or HttpStatusCode.MovedPermanently
                or HttpStatusCode.Found)
            {
                var location = response.Headers.Location?.ToString();
                if (!string.IsNullOrEmpty(location))
                {
                    // Location is like: https://github.com/.../releases/tag/v1.2.3
                    var tag = location.Split('/').Last();
                    return tag;
                }
            }
        }
        catch { /* swallow */ }
        return null;
    }

    private static string? ReadVersionFile(string versionFilePath)
    {
        return File.Exists(versionFilePath) ? File.ReadAllText(versionFilePath).Trim() : null;
    }

    private static void WriteVersionFile(string versionFilePath, string tag)
    {
        File.WriteAllText(versionFilePath, tag);
    }

    /// <summary>
    /// Ensures hgpaktool.exe is present and up-to-date in the tools directory.
    /// Downloads and extracts from the zip if needed.
    /// </summary>
    public static async Task EnsureHgPakToolAsync(string toolsDir)
    {
        Directory.CreateDirectory(toolsDir);
        string exePath = Path.Combine(toolsDir, "hgpaktool.exe");
        string versionFile = Path.Combine(toolsDir, "hgpaktool.version");

        string? latestTag = await GetLatestReleaseTagAsync(ExtractorConfig.HgPakToolLatestUrl);
        string? currentTag = ReadVersionFile(versionFile);

        if (File.Exists(exePath) && latestTag != null && latestTag == currentTag)
        {
            Console.WriteLine($"[OK] hgpaktool is up to date ({currentTag})");
            return;
        }

        Console.WriteLine($"[INFO] Downloading hgpaktool ({latestTag ?? "latest"})...");
        byte[] zipBytes = await DownloadClient.GetByteArrayAsync(ExtractorConfig.HgPakToolZipUrl);

        using var zipStream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                string destPath = Path.Combine(toolsDir, entry.Name);
                entry.ExtractToFile(destPath, overwrite: true);
                Console.WriteLine($"  Extracted {entry.Name}");
            }
        }

        if (latestTag != null)
            WriteVersionFile(versionFile, latestTag);

        Console.WriteLine("[OK] hgpaktool ready.");
    }

    /// <summary>
    /// Ensures MBINCompiler.exe is present and up-to-date in the tools directory.
    /// </summary>
    public static async Task EnsureMbinCompilerAsync(string toolsDir)
    {
        Directory.CreateDirectory(toolsDir);
        string exePath = Path.Combine(toolsDir, "MBINCompiler.exe");
        string versionFile = Path.Combine(toolsDir, "MBINCompiler.version");

        string? latestTag = await GetLatestReleaseTagAsync(ExtractorConfig.MbinCompilerLatestUrl);
        string? currentTag = ReadVersionFile(versionFile);

        if (File.Exists(exePath) && latestTag != null && latestTag == currentTag)
        {
            Console.WriteLine($"[OK] MBINCompiler is up to date ({currentTag})");
            return;
        }

        Console.WriteLine($"[INFO] Downloading MBINCompiler ({latestTag ?? "latest"})...");
        byte[] exeBytes = await DownloadClient.GetByteArrayAsync(ExtractorConfig.MbinCompilerUrl);
        await File.WriteAllBytesAsync(exePath, exeBytes);

        if (latestTag != null)
            WriteVersionFile(versionFile, latestTag);

        Console.WriteLine("[OK] MBINCompiler ready.");
    }

    /// <summary>
    /// Ensures 7zr.exe is present in the tools directory.
    /// Downloads from the official 7-Zip GitHub releases if needed.
    /// </summary>
    public static async Task Ensure7zrAsync(string toolsDir)
    {
        Directory.CreateDirectory(toolsDir);
        string exePath = Path.Combine(toolsDir, "7zr.exe");
        string versionFile = Path.Combine(toolsDir, "7zr.version");

        string? latestTag = await GetLatestReleaseTagAsync(ExtractorConfig.SevenZipLatestUrl);
        string? currentTag = ReadVersionFile(versionFile);

        if (File.Exists(exePath) && latestTag != null && latestTag == currentTag)
        {
            Console.WriteLine($"[OK] 7zr is up to date ({currentTag})");
            return;
        }

        if (latestTag == null)
        {
            if (File.Exists(exePath))
            {
                Console.WriteLine("[OK] 7zr.exe already present (could not check latest version)");
                return;
            }
            throw new InvalidOperationException("Could not resolve latest 7-Zip release tag.");
        }

        string downloadUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, ExtractorConfig.SevenZipDownloadPattern, latestTag);
        Console.WriteLine($"[INFO] Downloading 7zr.exe ({latestTag})...");
        byte[] exeBytes = await DownloadClient.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(exePath, exeBytes);

        WriteVersionFile(versionFile, latestTag);
        Console.WriteLine("[OK] 7zr.exe ready.");
    }

    /// <summary>
    /// Attempts to validate an installed magick.exe by listing supported formats
    /// and verifying DDS read/write support is present.
    /// Some releases (e.g. 7.1.2-24 compiled with VS2026) ship without DDS support.
    /// Also tests actual BC7 DDS read/write — the game now uses BC7-compressed DDS
    /// textures and some ImageMagick builds claim DDS support via -list format but
    /// hang on real BC7 conversions.
    /// </summary>
    private static bool ValidateMagick(string magickPath)
    {
        try
        {
            // First: check that DDS is listed as a supported format
            if (!ValidateMagickListFormats(magickPath))
                return false;

            // Second: test actual BC7 DDS read/write by creating a small DDS
            // and converting it back. Some builds claim DDS support but hang
            // on BC7-compressed textures (which is what No Man's Sky uses).
            return ValidateMagickBc7Conversion(magickPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Quick check: magick.exe -list format must show DDS* rw.
    /// </summary>
    private static bool ValidateMagickListFormats(string magickPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = magickPath,
            Arguments = "-list format",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        using var vp = Process.Start(psi);
        if (vp == null) return false;

        var vOut = vp.StandardOutput.ReadToEndAsync();
        var vErr = vp.StandardError.ReadToEndAsync();

        if (!vp.WaitForExit(15_000) || vp.ExitCode != 0)
        {
            try { vp.Kill(); } catch { }
            return false;
        }

        string output = vOut.Result;
        return output.Contains(" DDS* rw", StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that magick.exe can actually read and write BC7-compressed DDS files.
    /// Some builds pass -list format but hang on real BC7 conversion.
    /// </summary>
    private static bool ValidateMagickBc7Conversion(string magickPath)
    {
        string? tmpDir = null;
        try
        {
            tmpDir = Path.Combine(Path.GetTempPath(), $"nmse_img_validate_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tmpDir);

            string ddsFile = Path.Combine(tmpDir, "test_bc7.dds");
            string pngFile = Path.Combine(tmpDir, "test_bc7.png");

            // Step 1: create a BC7 DDS from an inline-generated image
            // The -define forces ImageMagick to use BC7 compression.
            if (!RunMagickSilent(magickPath, $"-size 4x4 xc:red -define dds:compression=bc7 \"{ddsFile}\"", 15_000))
                return false;

            if (!File.Exists(ddsFile))
                return false;

            // Step 2: convert the BC7 DDS back to PNG
            if (!RunMagickSilent(magickPath, $"\"{ddsFile}\" \"{pngFile}\"", 15_000))
                return false;

            return File.Exists(pngFile);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (tmpDir != null)
            {
                try { Directory.Delete(tmpDir, recursive: true); } catch { }
            }
        }
    }

    /// <summary>
    /// Run magick.exe with redirected output and a timeout. Returns true if the
    /// process exits with code 0 within the timeout.
    /// </summary>
    private static bool RunMagickSilent(string magickPath, string arguments, int timeoutMs)
    {
        var psi = new ProcessStartInfo
        {
            FileName = magickPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi);
        if (process == null) return false;

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(timeoutMs))
        {
            try { process.Kill(); } catch { }
            return false;
        }

        // Drain remaining output and close streams
        using var cts = new CancellationTokenSource(3_000);
        try
        {
            Task.WhenAll(stdoutTask, stderrTask).Wait(cts.Token);
        }
        catch
        {
            // Best-effort: streams might be stuck, close them to unblock
            try { process.StandardOutput.Close(); } catch { }
            try { process.StandardError.Close(); } catch { }
        }

        return process.ExitCode == 0;
    }

    /// <summary>
    /// Extracts a downloaded ImageMagick 7z archive and validates the binary.
    /// Returns true on success, false if extraction or validation fails.
    /// </summary>
    private static bool ExtractAndValidateImageMagick(string sevenZrPath, string archivePath, string imDir, string magickPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = sevenZrPath,
                Arguments = $"x \"{archivePath}\" -o\"{imDir}\" -y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(120_000))
            {
                try { process.Kill(); } catch { }
                return false;
            }

            _ = stdoutTask.GetAwaiter().GetResult();
            _ = stderrTask.GetAwaiter().GetResult();

            if (!File.Exists(magickPath)) return false;
            return ValidateMagick(magickPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates fallback ImageMagick tags by decrementing the build number of the latest tag.
    /// The tag format is "major.minor.patch-build" (e.g. "7.1.2-22").
    /// </summary>
    private static IEnumerable<string> GetFallbackImageMagickTags(string latestTag, int maxFallbacks = 5)
    {
        yield return latestTag;

        int dashIdx = latestTag.LastIndexOf('-');
        if (dashIdx < 0 || !int.TryParse(latestTag[(dashIdx + 1)..], out int buildNum))
            yield break;

        string prefix = latestTag[..dashIdx];
        for (int i = 0; i < maxFallbacks; i++)
        {
            buildNum--;
            if (buildNum < 1) yield break;
            yield return $"{prefix}-{buildNum}";
        }
    }

    /// <summary>
    /// Ensures ImageMagick portable is present and up-to-date in tools/imagemagick/.
    /// Downloads the portable .7z from GitHub releases and extracts using 7zr.exe.
    /// If the selected release is broken (e.g. compiled without DDS support), falls back
    /// to progressively older releases up to 5 versions back.
    /// </summary>
    /// <remarks>
    /// TEMPORARY: Pinned to 7.1.2-22 because 7.1.2-24 has a BC7 regression (hangs on
    /// actual BC7 DDS read/write even though -list format claims support).
    /// To restore dynamic-latest behaviour, uncomment the GetLatestReleaseTagAsync call
    /// and remove the hardcoded fallback below. See Git history for the original code.
    /// </remarks>
    public static async Task EnsureImageMagickAsync(string toolsDir)
    {
        string imDir = Path.Combine(toolsDir, ExtractorConfig.ImageMagickSubfolder);
        Directory.CreateDirectory(imDir);
        string magickPath = Path.Combine(imDir, "magick.exe");
        string versionFile = Path.Combine(imDir, "imagemagick.version");

        // TEMPORARY: pinned to known working version; restore dynamic lookup later
        // string? latestTag = await GetLatestReleaseTagAsync(ExtractorConfig.ImageMagickLatestUrl);
        string latestTag = "7.1.2-22";
        string? currentTag = ReadVersionFile(versionFile);

        // Quick path: already installed and up to date, and binary still works
        if (File.Exists(magickPath) && latestTag != null && latestTag == currentTag)
        {
            if (ValidateMagick(magickPath))
            {
                Console.WriteLine($"[OK] ImageMagick is up to date ({currentTag})");
                return;
            }
            Console.WriteLine($"[WARN] ImageMagick {currentTag} binary is broken, re-downloading...");
        }

        // TEMPORARY: null-check skipped since latestTag is always assigned above
        // When restoring the dynamic lookup, uncomment this block:
        //if (latestTag == null)
        //{
        //    if (File.Exists(magickPath) && ValidateMagick(magickPath))
        //    {
        //        Console.WriteLine("[OK] ImageMagick already present (could not check latest version)");
        //        return;
        //    }
        //    throw new InvalidOperationException("Could not resolve latest ImageMagick release tag and no working binary found.");
        //}

        // Ensure 7zr.exe is available for extraction
        string sevenZrPath = Path.Combine(toolsDir, "7zr.exe");
        if (!File.Exists(sevenZrPath))
            await Ensure7zrAsync(toolsDir);

        string archivePath = Path.Combine(toolsDir, "imagemagick_portable.7z");
        string? lastAttemptedTag = null;

        // Try the pinned tag first, then progressively older versions as fallback
        foreach (string candidateTag in GetFallbackImageMagickTags(latestTag))
        {
            if (candidateTag == currentTag && File.Exists(magickPath) && ValidateMagick(magickPath))
            {
                // Current version is already the best available
                Console.WriteLine($"[OK] ImageMagick is up to date ({currentTag})");
                return;
            }

            if (candidateTag == lastAttemptedTag) continue;
            lastAttemptedTag = candidateTag;

            // Clean any previous failed extraction
            if (Directory.Exists(imDir))
            {
                try { Directory.Delete(imDir, recursive: true); } catch { }
            }
            Directory.CreateDirectory(imDir);

            string downloadUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, ExtractorConfig.ImageMagickDownloadPattern, candidateTag);

            Console.WriteLine($"[INFO] Downloading ImageMagick portable ({candidateTag})...");
            byte[] archiveBytes;
            try
            {
                archiveBytes = await DownloadClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(archivePath, archiveBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Failed to download {candidateTag}: {ex.Message}");
                continue; // Try next older version
            }

            Console.WriteLine("[INFO] Extracting ImageMagick...");
            if (ExtractAndValidateImageMagick(sevenZrPath, archivePath, imDir, magickPath))
            {
                WriteVersionFile(versionFile, candidateTag);
                Console.WriteLine("[OK] ImageMagick ready.");
                return;
            }

            Console.WriteLine($"[WARN] ImageMagick {candidateTag} failed validation, trying older version...");
        }

        // Clean up archive
        try { File.Delete(archivePath); } catch { }

        throw new InvalidOperationException(
            $"ImageMagick download failed for {latestTag} and {ExtractorConfig.ImageMagickMaxFallbacks} older versions. " +
            "None of the downloaded binaries passed validation (DDS format support check).");
    }

    /// <summary>
    /// Downloads mapping.json from the MBINCompiler releases and saves to the map output directory.
    /// </summary>
    public static async Task DownloadMappingJsonAsync(string mapDir)
    {
        Directory.CreateDirectory(mapDir);
        string destPath = Path.Combine(mapDir, "mapping.json");

        Console.WriteLine("[INFO] Downloading mapping.json...");
        byte[] data = await DownloadClient.GetByteArrayAsync(ExtractorConfig.MappingJsonUrl);
        await File.WriteAllBytesAsync(destPath, data);
        Console.WriteLine("[OK] mapping.json saved.");
    }
}
