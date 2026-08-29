using System.ComponentModel;
using NMSE.Data;

namespace NMSE.UI.Localization;

/// <summary>
/// Singleton that wraps <see cref="UiStrings"/> with INotifyPropertyChanged
/// so that compiled-binding-based MarkupExtensions refresh automatically
/// when the UI language changes.
/// </summary>
public sealed class LocaleManager : INotifyPropertyChanged
{
    public static LocaleManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocaleManager() { }

    /// <summary>
    /// Indexer used by <see cref="LocaleExtension"/> compiled bindings.
    /// The <paramref name="key"/> is resolved through <see cref="UiStrings.Get"/>
    /// and its WinForms mnemonics are rewritten for Avalonia.
    /// </summary>
    public string this[string key] => ConvertMnemonics(UiStrings.Get(key));

    /// <summary>
    /// Rewrites WinForms access-key markup as Avalonia's.
    /// </summary>
    /// <remarks>
    /// The string table is shared with the WinForms build, where <c>&amp;</c> before a
    /// character marks the access key and <c>&amp;&amp;</c> is a literal ampersand.
    /// Avalonia uses <c>_</c> and <c>__</c> for the same two roles, so both markers
    /// have to be translated.
    /// <para>
    /// Replacing every <c>&amp;</c> is not enough: labels such as
    /// <c>"Bases &amp; Storage"</c> carry a real ampersand and would render as
    /// "Bases _ Storage". An ampersand only introduces an access key when it is
    /// followed by a non-whitespace character. Any literal underscore already in the
    /// text must also be doubled, or Avalonia would read it as a marker of its own.
    /// </para>
    /// </remarks>
    internal static string ConvertMnemonics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (text.IndexOf('&') < 0 && text.IndexOf('_') < 0) return text;

        var sb = new System.Text.StringBuilder(text.Length + 4);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '_')
            {
                // Escape an underscore that is part of the text itself.
                sb.Append("__");
            }
            else if (c == '&')
            {
                if (i + 1 < text.Length && text[i + 1] == '&')
                {
                    sb.Append('&');   // "&&" is an escaped ampersand
                    i++;
                }
                else if (i + 1 < text.Length && !char.IsWhiteSpace(text[i + 1]))
                {
                    sb.Append('_');   // access key marker
                }
                else
                {
                    sb.Append('&');   // trailing or spaced ampersand: literal
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Call after <see cref="UiStrings.Load"/> to refresh every bound UI string.
    /// Raising PropertyChanged with "Item" causes Avalonia to re-read the indexer.
    /// </summary>
    public void NotifyLanguageChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
    }
}
