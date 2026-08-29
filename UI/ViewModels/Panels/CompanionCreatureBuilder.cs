using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>
/// One part group of a creature — a head, a body, a tail — and the option chosen for it.
/// </summary>
/// <remarks>
/// Choosing a part can expose further groups beneath it, so the list of groups is rebuilt
/// after every change rather than fixed when the creature loads.
/// </remarks>
public partial class DescriptorGroupViewModel : ObservableObject
{
    /// <summary>Raised when the user picks a part, not when the panel loads one.</summary>
    public event Action<DescriptorGroupViewModel>? SelectionChanged;

    private bool _loading;

    public string GroupId { get; }
    public string Label { get; }

    /// <summary>The parts on offer, with a leading "(none)" entry.</summary>
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>Ids behind <see cref="Options"/>, offset by the leading "(none)".</summary>
    private readonly List<string> _optionIds = new();

    [ObservableProperty] private int _selectedIndex;

    public DescriptorGroupViewModel(DescriptorGroup group, IReadOnlyCollection<string> selected)
    {
        GroupId = group.GroupId;
        Label = group.GroupId.Trim('_') + ":";

        _loading = true;
        Options.Add(UiStrings.Get("companion.none"));

        for (int i = 0; i < group.Descriptors.Count; i++)
        {
            var option = group.Descriptors[i];
            Options.Add(option.ToString());
            _optionIds.Add(option.Id);

            if (selected.Contains(option.Id, StringComparer.OrdinalIgnoreCase))
                SelectedIndex = i + 1;   // +1 for the "(none)" entry
        }
        _loading = false;
    }

    /// <summary>The chosen part's id, or null when "(none)" is selected.</summary>
    public string? SelectedId =>
        SelectedIndex > 0 && SelectedIndex - 1 < _optionIds.Count ? _optionIds[SelectedIndex - 1] : null;

    partial void OnSelectedIndexChanged(int value)
    {
        if (!_loading) SelectionChanged?.Invoke(this);
    }
}

/// <summary>
/// Reads and writes a companion's Descriptors array, which is what the game's own
/// Creature Builder edits.
/// </summary>
/// <remarks>
/// The array holds the chosen part ids followed by a ten-digit descriptor id, all
/// carrying a caret prefix. The trailing id is regenerated on every write, which is what
/// makes the game re-derive the creature rather than reuse a cached one.
/// </remarks>
internal static class CompanionDescriptorIo
{
    /// <summary>The part ids currently on the companion, without their caret prefixes.</summary>
    internal static List<string> Read(JsonObject companion)
    {
        var result = new List<string>();
        try
        {
            var arr = companion.GetArray("Descriptors");
            if (arr is null) return result;

            for (int i = 0; i < arr.Length; i++)
            {
                string value = arr.GetString(i) ?? "";
                if (value.Length > 1) result.Add(value.TrimStart('^'));
            }
        }
        catch { }
        return result;
    }

    /// <summary>
    /// Replaces the array with the given parts plus a fresh descriptor id. Writes in
    /// place so the companion keeps the array instance the save already holds.
    /// </summary>
    internal static void Write(JsonObject companion, IEnumerable<string> partIds)
    {
        var arr = companion.GetArray("Descriptors");
        if (arr is null) return;

        for (int i = arr.Length - 1; i >= 0; i--) arr.RemoveAt(i);
        foreach (string id in partIds) arr.Add("^" + id);
        arr.Add("^" + CreaturePartDatabase.NewDescriptorId());
    }
}
