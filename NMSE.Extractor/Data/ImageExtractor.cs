using NMSE.Extractor.Config;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NMSE.Extractor.Data;

/// <summary>
/// Extracts item icons from game files (extracted directory) and converts DDS to PNG.
/// Uses the portable ImageMagick magick.exe for DDS to PNG conversion.
/// </summary>
public static class ImageExtractor
{
    private static readonly string[] IconJsonFiles =
    {
        "Buildings.json", "Constructed Technology.json", "Food.json", "Corvette.json",
        "Curiosities.json", "Exocraft.json", "Fish.json",
        "Others.json", "Products.json", "Raw Materials.json", "Starships.json",
        "Technology.json", "Technology Module.json", "Trade.json", "Upgrades.json",
        "none.json"
    };

    public static string SanitizeFilename(string idStr)
    {
        if (string.IsNullOrEmpty(idStr)) return "unknown";
        return Regex.Replace(idStr, @"[\\/:*?""<>|]", "_").Trim();
    }

    public static List<(string Id, string IconPath)> CollectIdIconPairs(string jsonDir)
    {
        var seenIds = new HashSet<string>();
        var pairs = new List<(string, string)>();

        foreach (string filename in IconJsonFiles)
        {
            string path = Path.Combine(jsonDir, filename);
            if (!File.Exists(path)) continue;

            try
            {
                string json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    string idVal = item.TryGetProperty("Id", out var idProp) ? idProp.GetString() ?? "" :
                                   item.TryGetProperty("id", out idProp) ? idProp.GetString() ?? "" : "";

                    string iconVal = item.TryGetProperty("IconPath", out var iconProp) ? iconProp.GetString() ?? "" :
                                     item.TryGetProperty("iconPath", out iconProp) ? iconProp.GetString() ?? "" :
                                     item.TryGetProperty("Icon", out iconProp) ? iconProp.GetString() ?? "" :
                                     item.TryGetProperty("icon", out iconProp) ? iconProp.GetString() ?? "" : "";

                    if (string.IsNullOrEmpty(idVal) || string.IsNullOrEmpty(iconVal)) continue;
                    if (!seenIds.Add(idVal)) continue;
                    pairs.Add((idVal, iconVal));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Skip {filename}: {ex.Message}");
            }
        }
        return pairs;
    }

    /// <summary>
    /// Convert a DDS file to PNG using ImageMagick's magick.exe.
    /// Uses async reads to prevent deadlock when the process fills its error buffer.
    /// Kills the process on timeout (15s) instead of leaking zombie processes.
    /// Explicitly closes output streams after the process exits so that
    /// ReadToEndAsync tasks don't hang if magick.exe neglects to close
    /// its stdout/stderr pipe handles on exit.
    /// </summary>
    public static bool DdsToPng(string magickPath, string source, string dest)
    {
        if (!File.Exists(source))
        {
            Console.WriteLine($"[WARN] DdsToPng: source not found: {source}");
            return false;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = magickPath,
                Arguments = $"\"{source}\" \"{dest}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine($"[WARN] DdsToPng: Process.Start returned null for {source}");
                return false;
            }

            // Read both streams asynchronously to avoid deadlock
            // when the process fills the stderr buffer (e.g. unsupported DDS format warnings).
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            bool exited = process.WaitForExit(15_000);
            if (!exited)
            {
                Console.WriteLine($"[WARN] DdsToPng: magick.exe timed out on {Path.GetFileName(source)} — killing");
                try { process.Kill(); } catch { }
                return false;
            }

            // The async reads should complete within a few seconds of the
            // process exiting (OS pipes are flushed). If they don't, the
            // pipe is probably stuck open because magick.exe didn't close
            // its stdout/stderr handles on exit. Force-close to unblock.
            if (!Task.WhenAll(stdoutTask, stderrTask).Wait(3_000))
            {
                Console.WriteLine($"[WARN] DdsToPng: output pipe hung after exit for {Path.GetFileName(source)} — closing streams");
                process.StandardOutput.Close();
                process.StandardError.Close();
                return false;
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] DdsToPng: exception for {Path.GetFileName(source)}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Find magick.exe in tools/imagemagick/ directory.
    /// </summary>
    public static string? FindMagickExe(string toolsDir)
    {
        string imDir = Path.Combine(toolsDir, ExtractorConfig.ImageMagickSubfolder);
        string magickPath = Path.Combine(imDir, "magick.exe");
        if (File.Exists(magickPath)) return magickPath;

        // Also check the tools directory directly
        magickPath = Path.Combine(toolsDir, "magick.exe");
        if (File.Exists(magickPath)) return magickPath;

        return null;
    }

    public static (int Success, int Skipped) ExtractIcons(
        string jsonDir, string extractedRoot, string outputDir, string toolsDir)
    {
        var pairs = CollectIdIconPairs(jsonDir);
        if (pairs.Count == 0)
        {
            Console.WriteLine("[WARN] No id+icon pairs found in JSON files.");
            return (0, 0);
        }

        Console.WriteLine($"[INFO] Found {pairs.Count} items with icons");
        Directory.CreateDirectory(outputDir);

        string? magickPath = FindMagickExe(toolsDir);
        if (magickPath == null)
        {
            Console.WriteLine("[ERROR] ImageMagick magick.exe not found. Cannot convert DDS to PNG.");
            return (0, pairs.Count);
        }

        int success = 0, skipped = 0;
        // Show progress every 5% of total or once per file, whichever is more visible
        int progressInterval = Math.Max(1, Math.Min(10, pairs.Count / 20));

        for (int i = 0; i < pairs.Count; i++)
        {
            var (idVal, iconPath) = pairs[i];

            bool showProgress = (i + 1) % progressInterval == 0 || i + 1 == pairs.Count;
            if (showProgress)
                PakExtractor.WriteProgress($"  [{i + 1}/{pairs.Count}] Converting icons...");

            string source = Path.Combine(extractedRoot, iconPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(source)) { skipped++; continue; }

            string safeId = SanitizeFilename(idVal);
            string dest = Path.Combine(outputDir, $"{safeId}.png");
            if (DdsToPng(magickPath, source, dest))
                success++;
            else
                skipped++;
        }

        return (success, skipped);
    }

    /// <summary>
    /// Normalize extracted folder to use lowercase paths (matching game texture references).
    /// </summary>
    public static void NormalizeExtracted(string extractedRoot)
    {
        string srcDir = Path.Combine(extractedRoot, "TEXTURES");
        if (!Directory.Exists(srcDir))
        {
            srcDir = Path.Combine(extractedRoot, "textures");
            if (!Directory.Exists(srcDir))
            {
                Console.WriteLine("[WARN] No TEXTURES folder found in extracted.");
                return;
            }
        }

        string destTextures = Path.Combine(extractedRoot, "textures");
        if (Path.GetFullPath(srcDir).Equals(Path.GetFullPath(destTextures), StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine("[INFO] Normalizing to extracted/textures/ (lowercase paths)...");
        var files = Directory.EnumerateFiles(srcDir, "*", SearchOption.AllDirectories).ToArray();
        int total = files.Length;

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            string relative = Path.GetRelativePath(srcDir, file).ToLowerInvariant();
            string destFile = Path.Combine(destTextures, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
            File.Move(file, destFile, overwrite: true);

            if ((i + 1) % 500 == 0 || i + 1 == total)
                PakExtractor.WriteProgress($"  [{i + 1}/{total}] files normalized");
        }

        if (total > 0)
            PakExtractor.FinishProgress();

        if (Directory.Exists(srcDir) &&
            !Path.GetFullPath(srcDir).Equals(Path.GetFullPath(destTextures), StringComparison.OrdinalIgnoreCase))
        {
            Directory.Delete(srcDir, recursive: true);
        }
    }
}
