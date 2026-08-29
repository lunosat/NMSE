using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NMSE.Models;

namespace NMSE.Config;

/// <summary>
/// Application configuration manager.
/// Manages app settings, window state, and user preferences via JSON config file.
/// </summary>
public class AppConfig
{
    private const string ConfigFileName = "NMSE.conf";
    private static AppConfig? _instance;
    private readonly Dictionary<string, string> _properties = new();
    private string? _configPath;

    public static AppConfig Instance => _instance ??= new AppConfig();

    /// <summary>Maximum number of recent directories to store in the MRU list.</summary>
    public const int MaxRecentDirectories = 5;

    public string? LastDirectory
    {
        get => GetProperty("LastDirectory");
        set => SetProperty("LastDirectory", value);
    }

    /// <summary>
    /// Gets or sets the recent directories MRU list, stored as pipe-separated paths.
    /// </summary>
    public List<string> RecentDirectories
    {
        get
        {
            var raw = GetProperty("RecentDirectories");
            if (string.IsNullOrEmpty(raw)) return new List<string>();
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        set
        {
            if (value is null || value.Count == 0)
                SetProperty("RecentDirectories", null);
            else
                SetProperty("RecentDirectories", string.Join("|", value));
        }
    }

    /// <summary>
    /// Adds a directory to the recent directories MRU list.
    /// Moves it to the front if already present, trims to <see cref="MaxRecentDirectories"/>,
    /// and ensures <paramref name="defaultDir"/> (if provided) is always present.
    /// </summary>
    /// <param name="directory">The directory to add/promote to the front.</param>
    /// <param name="defaultDir">The OS-detected default save directory (always kept in the list).</param>
    public void AddRecentDirectory(string directory, string? defaultDir = null)
    {
        var list = RecentDirectories;

        // Remove existing entry (case-insensitive on Windows, case-sensitive elsewhere)
        list.RemoveAll(d => string.Equals(d, directory, PathComparison));

        // Insert at front (most recent)
        list.Insert(0, directory);

        // Ensure default directory is always in the list
        if (!string.IsNullOrEmpty(defaultDir) &&
            !list.Any(d => string.Equals(d, defaultDir, PathComparison)))
        {
            list.Add(defaultDir);
        }

        // Trim to max size, but never evict the default directory
        while (list.Count > MaxRecentDirectories)
        {
            int removeIdx = list.FindLastIndex(d => !string.Equals(d, defaultDir, PathComparison));
            if (removeIdx >= 0)
                list.RemoveAt(removeIdx);
            else
                break; // Only default entries remain
        }

        RecentDirectories = list;
        LastDirectory = directory;
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    public string? Theme
    {
        get => GetProperty("Theme");
        set => SetProperty("Theme", value);
    }

    /// <summary>
    /// Last selected BCP 47 language tag (e.g. "en-GB", "ja-JP").
    /// Defaults to "en-GB" if not set.
    /// </summary>
    /// <summary>
    /// User-configured backup directory path, or null to use the default
    /// (EXE-relative "Save Backups" with TEMP fallback).
    /// </summary>
    public string? BackupDirectory
    {
        get => GetProperty("BackupDirectory");
        set => SetProperty("BackupDirectory", value);
    }

    /// <summary>
    /// Gets or sets the recent backup directories MRU list, stored as pipe-separated paths.
    /// </summary>
    public List<string> RecentBackupDirectories
    {
        get
        {
            var raw = GetProperty("RecentBackupDirectories");
            if (string.IsNullOrEmpty(raw)) return new List<string>();
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        set
        {
            if (value is null || value.Count == 0)
                SetProperty("RecentBackupDirectories", null);
            else
                SetProperty("RecentBackupDirectories", string.Join("|", value));
        }
    }

    /// <summary>
    /// Adds a backup directory to the recent backup directories MRU list.
    /// </summary>
    public void AddRecentBackupDirectory(string directory)
    {
        var list = RecentBackupDirectories;
        list.RemoveAll(d => string.Equals(d, directory, PathComparison));
        list.Insert(0, directory);
        while (list.Count > MaxRecentDirectories)
            list.RemoveAt(list.Count - 1);
        RecentBackupDirectories = list;
        BackupDirectory = directory;
    }

    public string Language
    {
        get => GetProperty("Language") ?? "en-GB";
        set => SetProperty("Language", value);
    }

    public int MainFrameX
    {
        get => int.TryParse(GetProperty("MainFrame.X"), out int v) ? v : 100;
        set => SetProperty("MainFrame.X", value.ToString(CultureInfo.InvariantCulture));
    }

    public int MainFrameY
    {
        get => int.TryParse(GetProperty("MainFrame.Y"), out int v) ? v : 100;
        set => SetProperty("MainFrame.Y", value.ToString(CultureInfo.InvariantCulture));
    }

    public int MainFrameWidth
    {
        get => int.TryParse(GetProperty("MainFrame.Width"), out int v) ? v : 1200;
        set => SetProperty("MainFrame.Width", value.ToString(CultureInfo.InvariantCulture));
    }

    public int MainFrameHeight
    {
        get => int.TryParse(GetProperty("MainFrame.Height"), out int v) ? v : 800;
        set => SetProperty("MainFrame.Height", value.ToString(CultureInfo.InvariantCulture));
    }

    public static string BuildSaveScopeKey(string? saveFilePath)
    {
        if (string.IsNullOrWhiteSpace(saveFilePath))
            return "unknown";

        string normalized;
        try
        {
            normalized = Path.GetFullPath(saveFilePath).Trim();
        }
        catch
        {
            normalized = saveFilePath.Trim();
        }

        if (OperatingSystem.IsWindows())
            normalized = normalized.ToLowerInvariant();

        byte[] data = Encoding.UTF8.GetBytes(normalized);
        byte[] hash = SHA256.HashData(data);
        return Convert.ToHexString(hash[..8]).ToLowerInvariant();
    }

    private static string BuildPinnedSlotsPropertyKey(string saveScopeKey, string inventoryKey)
        => $"PinnedSlots.{saveScopeKey}.{inventoryKey}";

    public HashSet<(int x, int y)> GetPinnedSlots(string saveScopeKey, string inventoryKey)
    {
        var result = new HashSet<(int x, int y)>();
        string? raw = GetProperty(BuildPinnedSlotsPropertyKey(saveScopeKey, inventoryKey));
        if (string.IsNullOrWhiteSpace(raw))
            return result;

        var entries = raw.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var parts = entry.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;

            if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                result.Add((x, y));
        }

        return result;
    }

    public void SetPinnedSlots(string saveScopeKey, string inventoryKey, IEnumerable<(int x, int y)> pinnedSlots)
    {
        string key = BuildPinnedSlotsPropertyKey(saveScopeKey, inventoryKey);
        string value = string.Join(";",
            pinnedSlots
                .Distinct()
                .OrderBy(p => p.y)
                .ThenBy(p => p.x)
                .Select(p => $"{p.x},{p.y}"));

        SetProperty(key, string.IsNullOrEmpty(value) ? null : value);
    }

    /// <summary>
    /// Resolves the directory the configuration file lives in.
    /// </summary>
    /// <remarks>
    /// On Windows the file sits next to the executable so it survives an update, which
    /// copies new files in without removing existing ones.
    /// <para>
    /// That location does not work on Linux or macOS. Every packaging format the Linux
    /// build ships in - AppImage, Flatpak and a distro package - mounts or installs the
    /// application read-only, so a write beside the executable fails and every setting is
    /// lost on exit. Settings go to <c>$XDG_CONFIG_HOME/NMSE</c> instead (defaulting to
    /// <c>~/.config</c> per the XDG base directory specification), and to
    /// <c>~/Library/Application Support/NMSE</c> on macOS.
    /// </para>
    /// </remarks>
    internal static string ResolveConfigDirectory()
    {
        if (OperatingSystem.IsWindows())
            return AppContext.BaseDirectory;

        if (OperatingSystem.IsMacOS())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "NMSE");

        string? xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        // The spec requires an absolute path; a relative value is to be ignored.
        if (string.IsNullOrEmpty(xdg) || !Path.IsPathRooted(xdg))
            xdg = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

        return Path.Combine(xdg, "NMSE");
    }

    /// <summary>Creates the config directory and loads settings from disk if available.</summary>
    /// <remarks>
    /// A one-time migration copies an existing config from the previous locations - the
    /// application directory, and before that <c>%AppData%\NMSE\</c> - so upgrading users
    /// keep their settings.
    /// </remarks>
    public void Initialize()
    {
        string configDir = ResolveConfigDirectory();
        try { Directory.CreateDirectory(configDir); } catch { /* handled by the write path */ }
        _configPath = Path.Combine(configDir, ConfigFileName);

        if (!File.Exists(_configPath))
            MigrateLegacyConfig(_configPath);

        if (File.Exists(_configPath))
            Load();
    }

    /// <summary>
    /// Copies the first config found in a previously used location into
    /// <paramref name="destination"/>. Best effort: a failure just starts fresh.
    /// </summary>
    private static void MigrateLegacyConfig(string destination)
    {
        foreach (string candidate in EnumerateLegacyConfigPaths())
        {
            try
            {
                if (!File.Exists(candidate)) continue;
                if (string.Equals(Path.GetFullPath(candidate), Path.GetFullPath(destination),
                        StringComparison.Ordinal))
                    continue;
                File.Copy(candidate, destination, overwrite: false);
                return;
            }
            catch
            {
                // Try the next candidate.
            }
        }
    }

    private static IEnumerable<string> EnumerateLegacyConfigPaths()
    {
        // Beside the executable: where every build wrote until the XDG move.
        yield return Path.Combine(AppContext.BaseDirectory, ConfigFileName);

        // %AppData%\NMSE\ on Windows; harmless elsewhere, where the folder will not exist.
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(appData))
            yield return Path.Combine(appData, "NMSE", ConfigFileName);
    }

    public string? GetProperty(string key) =>
        _properties.GetValueOrDefault(key);

    /// <summary>Sets or removes a configuration property by key.</summary>
    public void SetProperty(string key, string? value)
    {
        if (value is null)
            _properties.Remove(key);
        else
            _properties[key] = value;
    }

    /// <summary>Loads configuration properties from the JSON config file on disk.</summary>
    public void Load()
    {
        if (_configPath is null || !File.Exists(_configPath)) return;

        try
        {
            var json = File.ReadAllText(_configPath);
            var obj = JsonObject.Parse(json);
            foreach (var name in obj.Names())
            {
                var value = obj.Get(name);
                if (value is not null)
                    _properties[name] = value.ToString()!;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
        }
    }

    /// <summary>Persists the current configuration properties to the JSON config file.</summary>
    public void Save()
    {
        if (_configPath is null) return;

        try
        {
            var obj = new JsonObject();
            foreach (var kvp in _properties.OrderBy(k => k.Key))
                obj.Add(kvp.Key, kvp.Value);
            File.WriteAllText(_configPath, obj.ToFormattedString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
        }
    }
}
