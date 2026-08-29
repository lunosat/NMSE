using System.Text.RegularExpressions;

namespace NMSE.Tests;

/// <summary>
/// Every locale key the WinForms panel used has to be reachable from the Avalonia one
/// that replaced it. This is the check that kept the port honest while it was being
/// written, and it is here so a later change cannot quietly drop a field again.
/// </summary>
public class PanelStringParityTests
{
    /// <summary>A key looks like "player.units" — a prefix, a dot, and a name.</summary>
    private static readonly Regex KeyPattern =
        new(@"""([A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z0-9_]+)+)""", RegexOptions.Compiled);

    /// <summary>The markup extension the views use: {loc:Locale some.key}.</summary>
    private static readonly Regex LocalePattern =
        new(@"\{loc:Locale\s+([A-Za-z0-9_.]+)\s*\}", RegexOptions.Compiled);

    /// <summary>
    /// Each WinForms panel and the Avalonia files that replaced it. Shared UI a panel
    /// delegates to counts as its own: the inventory grid, the modal item picker and the
    /// dialog service are where several panels' strings legitimately live.
    /// </summary>
    private static readonly (string Name, string[] WinForms, string[] Avalonia)[] Panels =
    [
        ("MainStats",    ["MainStatsPanel"],       ["MainStats", "Multiplayer"]),
        ("Exosuit",      ["ExosuitPanel"],         ["Exosuit"]),
        ("Multitool",    ["MultitoolPanel"],       ["Multitool"]),
        ("Starship",     ["StarshipPanel"],        ["Starship"]),
        ("Freighter",    ["FreighterPanel"],       ["Freighter"]),
        ("Fleet",        ["FleetPanel"],           ["Fleet"]),
        ("Frigate",      ["FrigatePanel"],         ["Frigate"]),
        ("Squadron",     ["SquadronPanel"],        ["Squadron"]),
        ("Exocraft",     ["ExocraftPanel"],        ["Exocraft"]),
        ("Companion",    ["CompanionPanel"],       ["Companion"]),
        ("Base",         ["BasePanel"],            ["Base"]),
        ("Catalogue",    ["CataloguePanel"],       ["Discovery", "Recipe"]),
        ("Milestone",    ["MilestonePanel"],       ["Milestone"]),
        ("Settlement",   ["SettlementPanel"],      ["Settlement"]),
        ("ByteBeat",     ["ByteBeatPanel"],        ["ByteBeat"]),
        ("Account",      ["AccountPanel"],         ["Account"]),
        ("ExportConfig", ["ExportConfigPanel"],    ["ExportConfig"]),
        ("Recipe",       ["RecipePanel"],          ["Recipe"]),
        ("RawJson",      ["RawJsonPanel"],         ["RawJson"]),
    ];

    /// <summary>Helper files a panel's strings may live in, beyond its own pair.</summary>
    private static readonly Dictionary<string, string[]> Extra = new()
    {
        ["Starship"]  = ["UI/ViewModels/Panels/StarshipCustomisation.cs"],
        ["Companion"] = ["UI/ViewModels/Panels/CompanionBattle.cs",
                         "UI/ViewModels/Panels/CompanionCreatureBuilder.cs"],
        ["Base"]      = ["UI/ViewModels/Panels/BaseMetadata.cs"],
    };

    private static readonly string[] SharedUi =
    [
        "UI/Views/Controls/InventoryGridControl.axaml",
        "UI/Views/Controls/InventoryGridControl.axaml.cs",
        "UI/ViewModels/Controls/InventoryGridViewModel.cs",
        "UI/Views/Dialogs/ItemPickerDialog.axaml",
        "UI/ViewModels/Dialogs/ItemPickerViewModel.cs",
        "UI/Views/Dialogs/DialogService.cs",
    ];

    /// <summary>Walks up from the test binary to the repository root.</summary>
    private static string RepoRoot()
    {
        string probe = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            if (File.Exists(Path.Combine(probe, "NMSE.csproj"))) return probe;
            probe = Path.GetFullPath(Path.Combine(probe, ".."));
        }
        throw new DirectoryNotFoundException("repository root not found");
    }

    /// <summary>The keys in these files that the string table actually defines.</summary>
    private static HashSet<string> KeysIn(string root, IEnumerable<string> relativePaths,
        IReadOnlySet<string> table)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (string relative in relativePaths)
        {
            string path = Path.Combine(root, relative);
            if (!File.Exists(path)) continue;

            string text = File.ReadAllText(path);
            foreach (Match m in KeyPattern.Matches(text)) found.Add(m.Groups[1].Value);
            foreach (Match m in LocalePattern.Matches(text)) found.Add(m.Groups[1].Value);
        }

        found.IntersectWith(table);
        return found;
    }

    [Fact]
    public void EveryPanelCoversTheStringsItsWinFormsCounterpartUsed()
    {
        string root = RepoRoot();

        string tablePath = Path.Combine(root, "Resources", "ui", "lang", "en-GB.json");
        Assert.True(File.Exists(tablePath), "en-GB.json not found");

        var table = System.Text.Json.JsonDocument.Parse(File.ReadAllText(tablePath))
            .RootElement.EnumerateObject()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        // The WinForms sources are kept as _legacy_ui for exactly this comparison; if
        // they are ever removed the check has nothing to compare against.
        if (!Directory.Exists(Path.Combine(root, "_legacy_ui", "Panels"))) return;

        var gaps = new List<string>();

        foreach (var (name, winForms, avalonia) in Panels)
        {
            var legacyFiles = winForms.SelectMany(p =>
                new[] { $"_legacy_ui/Panels/{p}.cs", $"_legacy_ui/Panels/{p}.Designer.cs" });

            var portedFiles = avalonia.SelectMany(p => new[]
            {
                $"UI/Views/Panels/{p}View.axaml",
                $"UI/Views/Panels/{p}View.axaml.cs",
                $"UI/ViewModels/Panels/{p}ViewModel.cs",
            }).Concat(Extra.GetValueOrDefault(name, [])).Concat(SharedUi);

            var expected = KeysIn(root, legacyFiles, table);
            if (expected.Count == 0) continue;   // no WinForms counterpart to compare with

            var missing = expected.Except(KeysIn(root, portedFiles, table)).OrderBy(k => k).ToList();
            if (missing.Count > 0)
                gaps.Add($"{name}: {string.Join(", ", missing)}");
        }

        Assert.True(gaps.Count == 0,
            "panels missing strings their WinForms counterpart used:\n  " + string.Join("\n  ", gaps));
    }
}
