using NMSE.Core;
using NMSE.Models;

namespace NMSE.Tests;

/// <summary>
/// The raw editor's change view compares the document against a snapshot taken when the
/// panel loaded, so the snapshot has to be a value rather than a live reference.
/// </summary>
public class RawJsonDiffTests
{
    private static JsonObject Doc(int units)
    {
        var player = new JsonObject();
        player.Add("Units", units);

        var root = new JsonObject();
        root.Add("PlayerStateData", player);
        return root;
    }

    [Fact]
    public void ComputeCompactDiff_ReportsNothingForAnUnchangedDocument()
    {
        var doc = Doc(100);
        string baseline = RawJsonLogic.ToDisplayString(doc);

        Assert.Empty(RawJsonLogic.ComputeCompactDiff(baseline, RawJsonLogic.ToDisplayString(doc)));
    }

    /// <summary>
    /// Editing the document in place must still diff against the snapshot. A snapshot
    /// held as a reference to the same object would compare equal to itself and report
    /// nothing, which is the failure mode this guards.
    /// </summary>
    [Fact]
    public void ComputeCompactDiff_ReportsAnInPlaceEdit()
    {
        var doc = Doc(100);
        string baseline = RawJsonLogic.ToDisplayString(doc);

        doc.GetObject("PlayerStateData")!.Set("Units", 103);

        var diff = RawJsonLogic.ComputeCompactDiff(baseline, RawJsonLogic.ToDisplayString(doc));

        Assert.NotEmpty(diff);
        Assert.Contains(diff, l => l.Type == RawJsonLogic.DiffLineType.Removed && l.Text.Contains("100", StringComparison.Ordinal));
        Assert.Contains(diff, l => l.Type == RawJsonLogic.DiffLineType.Added && l.Text.Contains("103", StringComparison.Ordinal));
    }
}
