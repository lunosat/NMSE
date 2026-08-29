using NMSE.IO;

namespace NMSE.Tests;

/// <summary>
/// On Linux the game runs under a compatibility layer, so its save lives inside a Wine
/// prefix whose location depends on the launcher and on which drive the library sits.
/// </summary>
public class LinuxSavePathTests
{
    private static string CreateTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "nmse_savepath_" + Guid.NewGuid().ToString("N")[..12]);
        Directory.CreateDirectory(dir);
        return dir;
    }

    // --- libraryfolders.vdf --------------------------------------------------

    [Fact]
    public void ParseLibraryFolders_ReadsEveryLibraryPath()
    {
        string dir = CreateTempDir();
        try
        {
            string vdf = Path.Combine(dir, "libraryfolders.vdf");
            // Valve's KeyValues layout, tab-separated as Steam writes it.
            File.WriteAllText(vdf,
                "\"libraryfolders\"\n{\n" +
                "\t\"0\"\n\t{\n\t\t\"path\"\t\t\"/home/u/.local/share/Steam\"\n\t\t\"label\"\t\t\"\"\n\t}\n" +
                "\t\"1\"\n\t{\n\t\t\"path\"\t\t\"/mnt/games/SteamLibrary\"\n\t\t\"label\"\t\t\"\"\n\t}\n}\n");

            var paths = SaveFileManager.ParseLibraryFolders(vdf).ToList();

            Assert.Contains("/home/u/.local/share/Steam", paths);
            Assert.Contains("/mnt/games/SteamLibrary", paths);
        }
        finally { try { Directory.Delete(dir, true); } catch { } }
    }

    [Fact]
    public void ParseLibraryFolders_ReturnsNothingForMissingFile()
        => Assert.Empty(SaveFileManager.ParseLibraryFolders(
            Path.Combine(Path.GetTempPath(), "nmse_does_not_exist_" + Guid.NewGuid().ToString("N"), "x.vdf")));

    [Fact]
    public void ParseLibraryFolders_IgnoresGarbage()
    {
        string dir = CreateTempDir();
        try
        {
            string vdf = Path.Combine(dir, "libraryfolders.vdf");
            File.WriteAllText(vdf, "this is not a vdf file at all");
            Assert.Empty(SaveFileManager.ParseLibraryFolders(vdf));
        }
        finally { try { Directory.Delete(dir, true); } catch { } }
    }

    // --- candidate directories ----------------------------------------------

    [Fact]
    public void EnumerateLinuxSaveDirectories_CoversTheProtonPrefix()
    {
        var dirs = SaveFileManager.EnumerateLinuxSaveDirectories("/home/u").ToList();

        Assert.Contains(dirs, d => d == Path.Combine("/home/u", ".local", "share", "Steam",
            "steamapps", "compatdata", "275850", "pfx", "drive_c", "users", "steamuser",
            "AppData", "Roaming", "HelloGames", "NMS"));
    }

    /// <summary>
    /// <c>~/.steam/steam</c> and <c>~/.steam/root</c> are the symlinks most distributions
    /// still ship, and were the only Steam layout on Linux for years.
    /// </summary>
    [Theory]
    [InlineData(".steam", "steam")]
    [InlineData(".steam", "root")]
    public void EnumerateLinuxSaveDirectories_CoversTheLegacySteamSymlinks(string a, string b)
    {
        var dirs = SaveFileManager.EnumerateLinuxSaveDirectories("/home/u").ToList();
        string expected = Path.Combine("/home/u", a, b, "steamapps", "compatdata", "275850",
            "pfx", "drive_c", "users", "steamuser", "AppData", "Roaming", "HelloGames", "NMS");
        Assert.Contains(expected, dirs);
    }

    [Fact]
    public void EnumerateLinuxSaveDirectories_CoversFlatpakSteam()
    {
        var dirs = SaveFileManager.EnumerateLinuxSaveDirectories("/home/u").ToList();
        Assert.Contains(dirs, d => d.Contains("com.valvesoftware.Steam", StringComparison.Ordinal));
    }

    [Fact]
    public void EnumerateLinuxSaveDirectories_CoversTheDefaultWinePrefix()
    {
        var dirs = SaveFileManager.EnumerateLinuxSaveDirectories("/home/u").ToList();
        Assert.Contains(dirs, d => d.StartsWith(Path.Combine("/home/u", ".wine"), StringComparison.Ordinal));
    }

    /// <summary>
    /// A library on another drive is listed only in libraryfolders.vdf, so the file has to
    /// be read to find a save that does not live under the home directory.
    /// </summary>
    [Fact]
    public void EnumerateLinuxSaveDirectories_FollowsLibrariesOnOtherDrives()
    {
        string home = CreateTempDir();
        try
        {
            string steamApps = Path.Combine(home, ".local", "share", "Steam", "steamapps");
            Directory.CreateDirectory(steamApps);
            File.WriteAllText(Path.Combine(steamApps, "libraryfolders.vdf"),
                "\"libraryfolders\"\n{\n\t\"0\"\n\t{\n\t\t\"path\"\t\t\"/mnt/ssd/SteamLibrary\"\n\t}\n}\n");

            var dirs = SaveFileManager.EnumerateLinuxSaveDirectories(home).ToList();

            Assert.Contains(dirs, d => d.StartsWith("/mnt/ssd/SteamLibrary", StringComparison.Ordinal));
        }
        finally { try { Directory.Delete(home, true); } catch { } }
    }

    /// <summary>
    /// NMS maps its save folder into Steam Cloud, so a copy lives under
    /// userdata/&lt;steamID3&gt;/275850/ac/. When the Proton prefix has been cleared by a
    /// reinstall or a Proton version change, that copy is the only one left.
    /// </summary>
    [Fact]
    public void EnumerateLinuxSaveDirectories_CoversSteamAutoCloud()
    {
        string home = CreateTempDir();
        try
        {
            string userData = Path.Combine(home, ".local", "share", "Steam", "userdata", "1146865822");
            Directory.CreateDirectory(userData);

            var dirs = SaveFileManager.EnumerateLinuxSaveDirectories(home).ToList();

            Assert.Contains(Path.Combine(userData, "275850", "ac", "WinAppDataRoaming",
                "HelloGames", "NMS"), dirs);
        }
        finally { try { Directory.Delete(home, true); } catch { } }
    }

    [Fact]
    public void EnumerateLinuxSaveDirectories_SurvivesAnEmptyHome()
    {
        string home = CreateTempDir();
        try
        {
            // Must not throw on a home with none of the expected directories present.
            Assert.NotEmpty(SaveFileManager.EnumerateLinuxSaveDirectories(home).ToList());
        }
        finally { try { Directory.Delete(home, true); } catch { } }
    }

    [Fact]
    public void EnumerateSteamLibraryRoots_DoesNotRepeatARoot()
    {
        var roots = SaveFileManager.EnumerateSteamLibraryRoots("/home/u").ToList();
        Assert.Equal(roots.Count, roots.Distinct(StringComparer.Ordinal).Count());
    }
}
