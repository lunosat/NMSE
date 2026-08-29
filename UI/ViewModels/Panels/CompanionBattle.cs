using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using NMSE.Core;
using NMSE.Core.Utilities;
using NMSE.Data;
using NMSE.Models;

namespace NMSE.UI.ViewModels.Panels;

/// <summary>One of the five battle move slots, offering the moves that slot accepts.</summary>
public partial class BattleMoveSlotViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedIndex;

    public string Label { get; }

    /// <summary>Display names, with "(None)" first.</summary>
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>Move ids parallel to <see cref="Options"/>; null at index 0.</summary>
    public List<string?> MoveIds { get; } = new();

    /// <summary>Description of the selected move, shown beside the slot.</summary>
    public string Detail => SelectedMoveId is { } id && PetBattleMoveDatabase.ById.TryGetValue(id, out var move)
        ? DescribeMove(move)
        : "";

    public string? SelectedMoveId =>
        SelectedIndex > 0 && SelectedIndex < MoveIds.Count ? MoveIds[SelectedIndex] : null;

    public BattleMoveSlotViewModel(int slotNumber, IEnumerable<PetBattleMoveEntry> allowed)
    {
        Label = UiStrings.Format("companion.battle_move_slot",
            slotNumber.ToString(CultureInfo.CurrentCulture));

        Options.Add(UiStrings.Get("companion.battle_move_none"));
        MoveIds.Add(null);

        foreach (var move in allowed)
        {
            Options.Add(move.ToString());
            MoveIds.Add(move.Id);
        }
    }

    partial void OnSelectedIndexChanged(int value) => OnPropertyChanged(nameof(Detail));

    /// <summary>
    /// Adds a move the companion already has but which is not offered for selection,
    /// so it still displays rather than reading as empty.
    /// </summary>
    public void EnsurePresent(string moveId)
    {
        if (MoveIds.Contains(moveId, StringComparer.OrdinalIgnoreCase)) return;
        if (!PetBattleMoveDatabase.ById.TryGetValue(moveId, out var move)) return;

        Options.Add(move.ToString());
        MoveIds.Add(move.Id);
    }

    public void SelectMove(string? moveId)
    {
        if (string.IsNullOrEmpty(moveId)) { SelectedIndex = 0; return; }

        EnsurePresent(moveId);
        int idx = MoveIds.FindIndex(id => string.Equals(id, moveId, StringComparison.OrdinalIgnoreCase));
        SelectedIndex = idx > 0 ? idx : 0;
    }

    private static string DescribeMove(PetBattleMoveEntry move)
    {
        string Yes() => UiStrings.Get("companion.battle_move_detail_yes");
        string No() => UiStrings.Get("companion.battle_move_detail_no");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(CultureInfo.CurrentCulture,
            $"{UiStrings.Get("companion.battle_move_detail_target")} {move.TargetDisplay}");
        sb.AppendLine(CultureInfo.CurrentCulture,
            $"{UiStrings.Get("companion.battle_move_detail_type")} {move.IconStyleDisplay}");
        sb.AppendLine(CultureInfo.CurrentCulture,
            $"{UiStrings.Get("companion.battle_move_detail_basic")} {(move.BasicMove ? Yes() : No())}");
        sb.AppendLine(CultureInfo.CurrentCulture,
            $"{UiStrings.Get("companion.battle_move_detail_multiturn")} {(move.MultiTurnMove ? Yes() : No())}");

        if (!string.IsNullOrEmpty(move.LocIDToDescribeStat))
            sb.AppendLine(CultureInfo.CurrentCulture,
                $"{UiStrings.Get("companion.battle_move_detail_stat")} {move.LocalisedDescription}");

        for (int i = 0; i < move.Phases.Count; i++)
        {
            var phase = move.Phases[i];
            sb.AppendLine(CultureInfo.CurrentCulture,
                $"{UiStrings.Format("companion.battle_move_detail_phase", (i + 1).ToString(CultureInfo.CurrentCulture))} " +
                $"{phase}");
        }

        return sb.ToString().TrimEnd();
    }
}

/// <summary>One of the three battle team slots, choosing among the unlocked pets.</summary>
public partial class BattleTeamSlotViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedIndex;

    public string Label { get; }
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>Pet indices parallel to <see cref="Options"/>; -1 at index 0.</summary>
    public List<int> PetIndices { get; } = new();

    public BattleTeamSlotViewModel(int slotNumber, IEnumerable<(int Index, string Label)> pets)
    {
        Label = UiStrings.Format("companion.battle_team_slot",
            slotNumber.ToString(CultureInfo.CurrentCulture));

        Options.Add(UiStrings.Get("companion.battle_team_none"));
        PetIndices.Add(-1);

        foreach (var (index, label) in pets)
        {
            Options.Add(label);
            PetIndices.Add(index);
        }
    }

    public int SelectedPetIndex =>
        SelectedIndex > 0 && SelectedIndex < PetIndices.Count ? PetIndices[SelectedIndex] : -1;

    public void SelectPet(int petIndex)
    {
        int idx = PetIndices.IndexOf(petIndex);
        SelectedIndex = idx > 0 ? idx : 0;
    }
}

/// <summary>
/// Reads and writes a companion's pet battle fields.
/// </summary>
/// <remarks>
/// The numeric fields are clamped for display but the raw values are kept, so a value
/// set outside the normal range - through the raw JSON editor, say - survives unless the
/// user actually edits that control.
/// </remarks>
internal static class CompanionBattleIo
{
    internal static readonly string[] StatClasses = { "S", "A", "B", "C" };

    internal static string ReadClassOverride(JsonArray? overrides, int index)
    {
        try
        {
            if (overrides is not null && index < overrides.Length)
                return overrides.GetObject(index)?.GetString("InventoryClass") ?? "C";
        }
        catch { }
        return "C";
    }

    internal static void WriteClassOverride(JsonArray? overrides, int index, string value)
    {
        try
        {
            if (overrides is null || index >= overrides.Length) return;
            overrides.GetObject(index)?.Set("InventoryClass", value);
        }
        catch { }
    }

    /// <summary>Moves allowed in a slot, taken from the movesets that define that slot.</summary>
    internal static List<PetBattleMoveEntry> AllowedMoves(int slotNumber)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allowed = new List<PetBattleMoveEntry>();

        foreach (var moveset in PetBattleMovesetDatabase.Movesets)
        {
            var slot = moveset.Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
            if (slot is null) continue;

            foreach (var option in slot.Options)
            {
                if (!seen.Add(option.Template)) continue;
                if (!PetBattleMoveDatabase.ById.TryGetValue(option.Template, out var move)) continue;
                // Restricted moves stay out of the picker but are still shown when held.
                if (CompanionLogic.RestrictedMoveIds.Contains(move.Id, StringComparer.OrdinalIgnoreCase)) continue;
                allowed.Add(move);
            }
        }
        return allowed;
    }

    /// <summary>Average of the three stat classes, which is what the game shows as the pet's class.</summary>
    internal static string AverageClass(string health, string agility, string combat)
    {
        int Score(string c) => c switch { "S" => 3, "A" => 2, "B" => 1, _ => 0 };
        int average = (int)Math.Round((Score(health) + Score(agility) + Score(combat)) / 3.0,
            MidpointRounding.AwayFromZero);
        return average switch { 3 => "S", 2 => "A", 1 => "B", _ => "C" };
    }
}

/// <summary>
/// One accessory slot on a companion: which model it wears, its two colours and its scale.
/// </summary>
public partial class AccessorySlotViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedIndex;
    [ObservableProperty] private double _scale = 1.0;
    [ObservableProperty] private Avalonia.Media.IBrush _primaryColour = Avalonia.Media.Brushes.Gray;
    [ObservableProperty] private Avalonia.Media.IBrush _altColour = Avalonia.Media.Brushes.Gray;

    public AccessorySlot Slot { get; }
    public string Label { get; }

    /// <summary>Display names, with "(None)" first.</summary>
    public ObservableCollection<string> Options { get; } = new();

    /// <summary>Accessory ids parallel to <see cref="Options"/>; null at index 0.</summary>
    public List<string?> Ids { get; } = new();

    /// <summary>Palette the two swatches pick from.</summary>
    public ObservableCollection<ShipPaletteSwatch> Choices { get; } = new();

    public AccessorySlotViewModel(AccessorySlot slot)
    {
        Slot = slot;
        Label = UiStrings.Get(slot switch
        {
            AccessorySlot.Right => "companion.accessory_slot_right",
            AccessorySlot.Left  => "companion.accessory_slot_left",
            AccessorySlot.Front => "companion.accessory_slot_front",
            AccessorySlot.Back  => "companion.accessory_slot_back",
            _                   => "companion.accessory_slot_chest",
        });

        Options.Add(UiStrings.Get("companion.accessory_none"));
        Ids.Add(null);

        foreach (var entry in CompanionAccessoryDatabase.GetEntriesForSlot(slot))
        {
            Options.Add(entry.ToString());
            Ids.Add(entry.Id);
        }

        foreach (var entry in NmsColourPalette.PaintPalette)
            Choices.Add(new ShipPaletteSwatch(entry.Name, entry.Colour,
                new Avalonia.Media.SolidColorBrush(entry.Colour)));
    }

    public string? SelectedId =>
        SelectedIndex > 0 && SelectedIndex < Ids.Count ? Ids[SelectedIndex] : null;

    /// <summary>Reads this slot out of a PetAccessoryCustomisation entry.</summary>
    public void LoadFrom(JsonObject? pacEntry, int saveIndex)
    {
        var data = SlotData(pacEntry, saveIndex);
        if (data is null) { SelectedIndex = 0; return; }

        string id = (data.GetString("Descriptor") ?? "").TrimStart('^');
        int idx = Ids.FindIndex(x => string.Equals(x, id, StringComparison.OrdinalIgnoreCase));
        SelectedIndex = idx > 0 ? idx : 0;

        try { Scale = data.GetDouble("Scale"); } catch { Scale = 1.0; }

        PrimaryColour = ReadColour(data, "PrimaryColour");
        AltColour = ReadColour(data, "AltColour");
    }

    /// <summary>Writes this slot back into a PetAccessoryCustomisation entry.</summary>
    public void SaveInto(JsonObject pacEntry, int saveIndex)
    {
        var data = SlotData(pacEntry, saveIndex);
        if (data is null) return;

        string? id = SelectedId;
        data.Set("Descriptor", string.IsNullOrEmpty(id) ? "" : "^" + id);
        data.Set("Scale", Scale);

        WriteColour(data, "PrimaryColour", PrimaryColour);
        WriteColour(data, "AltColour", AltColour);
    }

    private static JsonObject? SlotData(JsonObject? pacEntry, int saveIndex)
    {
        try
        {
            var data = pacEntry?.GetArray("Data");
            return data is not null && saveIndex < data.Length ? data.GetObject(saveIndex) : null;
        }
        catch { return null; }
    }

    private static Avalonia.Media.IBrush ReadColour(JsonObject data, string key)
    {
        try
        {
            var arr = data.GetArray(key);
            if (arr is null || arr.Length < 3) return Avalonia.Media.Brushes.Gray;

            return new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(
                (byte)Math.Clamp(Math.Round(arr.GetDouble(0) * 255.0), 0, 255),
                (byte)Math.Clamp(Math.Round(arr.GetDouble(1) * 255.0), 0, 255),
                (byte)Math.Clamp(Math.Round(arr.GetDouble(2) * 255.0), 0, 255)));
        }
        catch { return Avalonia.Media.Brushes.Gray; }
    }

    private static void WriteColour(JsonObject data, string key, Avalonia.Media.IBrush brush)
    {
        if (brush is not Avalonia.Media.SolidColorBrush solid) return;
        try
        {
            var arr = data.GetArray(key);
            if (arr is null || arr.Length < 4) return;

            var rgba = NmsColourPalette.ToNormalisedRgba(solid.Color);
            for (int i = 0; i < 4; i++) arr.Set(i, rgba[i]);
        }
        catch { }
    }
}

/// <summary>
/// Copies a pet into an egg slot the way the game does when an egg is induced.
/// </summary>
internal static class CompanionEggBuilder
{
    private static readonly string[] ScalarFields =
    {
        "Scale", "CreatureID", "CustomName", "CustomSpeciesName",
        "Predator", "UA", "AllowUnmodifiedReroll", "HasFur", "Trust", "EggModified",
    };

    /// <summary>
    /// Arrays are replaced wholesale rather than copied element by element: descriptors
    /// vary in length by species, and copying in place would truncate to the shorter one.
    /// </summary>
    private static readonly string[] ArrayFields =
    {
        "Descriptors", "CreatureSeed", "CreatureSecondarySeed",
        "ColourBaseSeed", "BoneScaleSeed", "Traits",
    };

    private static readonly string[] BattleScalars =
    {
        "PetBattlerUseCoreStatClassOverrides", "PetBattlerTreatsAvailable",
        "PetBattleProgressToTreat", "PetBattlerVictories",
    };

    private static readonly string[] BattleArrays =
    {
        "PetBattlerCoreStatClassOverrides", "PetBattlerTreatsEaten", "PetBattlerMoveList",
    };

    internal static void CopyPetToEgg(JsonObject pet, JsonObject egg)
    {
        foreach (string key in ScalarFields)
        {
            try { if (pet.Get(key) is { } value) egg.Set(key, value); } catch { }
        }

        foreach (string key in ArrayFields)
        {
            try { if (pet.GetArray(key) is { } array) egg.Set(key, array.DeepClone()); } catch { }
        }

        foreach (string key in new[] { "SpeciesSeed", "GenusSeed" })
        {
            try { egg.Set(key, pet.GetString(key) ?? "0x0"); } catch { }
        }

        CopyNested(pet, egg, "Biome", "Biome", "Lush");
        CopyNested(pet, egg, "CreatureType", "CreatureType", "None");

        foreach (string key in new[] { "LastTrustIncreaseTime", "LastTrustDecreaseTime" })
        {
            try { egg.Set(key, pet.GetLong(key)); } catch { }
        }

        // The egg is newly laid, and remembers when its parent was born.
        try { egg.Set("BirthTime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()); } catch { }
        try { egg.Set("LastEggTime", pet.GetLong("BirthTime")); } catch { }
        try { egg.Set("HasBeenSummoned", false); } catch { }

        // A newly induced egg starts with the low moods the game gives it.
        try
        {
            var moods = egg.GetArray("Moods");
            if (moods is not null && moods.Length >= 2)
            {
                moods.Set(0, 0.01);
                moods.Set(1, 0.02);
            }
        }
        catch { }

        foreach (string key in BattleScalars)
        {
            try { if (pet.Get(key) is { } value) egg.Set(key, value); } catch { }
        }

        foreach (string key in BattleArrays)
        {
            try { if (pet.GetArray(key) is { } array) egg.Set(key, array.DeepClone()); } catch { }
        }
    }

    private static void CopyNested(JsonObject pet, JsonObject egg, string container, string field, string fallback)
    {
        try
        {
            var source = pet.GetObject(container);
            var target = egg.GetObject(container);
            if (source is not null && target is not null)
                target.Set(field, source.GetString(field) ?? fallback);
        }
        catch { }
    }
}
