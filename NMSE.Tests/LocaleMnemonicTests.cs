using NMSE.UI.Localization;

namespace NMSE.Tests;

/// <summary>
/// The UI string table is shared with the WinForms build, so every label arrives
/// with WinForms access-key markup that has to be rewritten for Avalonia.
/// </summary>
public class LocaleMnemonicTests
{
    [Theory]
    [InlineData("&File", "_File")]
    [InlineData("E&xit", "E_xit")]
    [InlineData("&Load Save File...", "_Load Save File...")]
    [InlineData("Restore Backup (&All)", "Restore Backup (_All)")]
    public void ConvertMnemonics_RewritesAccessKeys(string input, string expected)
        => Assert.Equal(expected, LocaleManager.ConvertMnemonics(input));

    [Theory]
    [InlineData("Bases & Storage")]
    [InlineData("Upgrades & Crafting")]
    [InlineData("Fish & Chips")]
    public void ConvertMnemonics_LeavesSpacedAmpersandAlone(string input)
        => Assert.Equal(input, LocaleManager.ConvertMnemonics(input));

    [Fact]
    public void ConvertMnemonics_UnescapesDoubledAmpersand()
        => Assert.Equal("A & B", LocaleManager.ConvertMnemonics("A && B"));

    [Fact]
    public void ConvertMnemonics_EscapesLiteralUnderscore()
        => Assert.Equal("snake__case", LocaleManager.ConvertMnemonics("snake_case"));

    [Fact]
    public void ConvertMnemonics_TrailingAmpersandStaysLiteral()
        => Assert.Equal("Tom &", LocaleManager.ConvertMnemonics("Tom &"));

    [Theory]
    [InlineData("")]
    [InlineData("Plain label")]
    public void ConvertMnemonics_PassesThroughUnmarkedText(string input)
        => Assert.Equal(input, LocaleManager.ConvertMnemonics(input));

    /// <summary>
    /// Every string in the shipped table must survive conversion, and a label with a
    /// spaced ampersand must never come back with an underscore in its place - the
    /// defect that rendered "Bases &amp; Storage" as "Bases _ Storage".
    /// </summary>
    [Fact]
    public void ConvertMnemonics_NeverTurnsASpacedAmpersandIntoAnUnderscore()
    {
        foreach (var sample in new[] { "Bases & Storage", "A & B & C", "& leading" })
            Assert.DoesNotContain("_", LocaleManager.ConvertMnemonics(sample), StringComparison.Ordinal);
    }
}
