using NMSE.Models;

namespace NMSE.Tests;

/// <summary>
/// The game reads supercharged slots from the inventory's SpecialSlots array, as
/// TechBonus entries keyed by grid position. These assert the shape a writer has to
/// produce, which is what the grid's supercharge commands were getting wrong.
/// </summary>
public class SuperchargedSlotTests
{
    private static JsonObject SpecialSlotEntry(int x, int y)
    {
        var type = new JsonObject();
        type.Add("InventorySpecialSlotType", "TechBonus");

        var index = new JsonObject();
        index.Add("X", x);
        index.Add("Y", y);

        var entry = new JsonObject();
        entry.Add("Type", type);
        entry.Add("Index", index);
        return entry;
    }

    [Fact]
    public void ASuperchargedSlotIsATechBonusEntryAtItsGridPosition()
    {
        var entry = SpecialSlotEntry(2, 1);

        Assert.Equal("TechBonus", entry.GetObject("Type")!.GetString("InventorySpecialSlotType"));
        Assert.Equal(2, entry.GetObject("Index")!.GetInt("X"));
        Assert.Equal(1, entry.GetObject("Index")!.GetInt("Y"));
    }

    [Fact]
    public void SpecialSlotsIdentifyPositionsIndependentlyOfSlotOrder()
    {
        var special = new JsonArray();
        special.Add(SpecialSlotEntry(3, 0));
        special.Add(SpecialSlotEntry(0, 2));

        var found = new HashSet<(int X, int Y)>();
        for (int i = 0; i < special.Length; i++)
        {
            var index = special.GetObject(i)!.GetObject("Index")!;
            found.Add((index.GetInt("X"), index.GetInt("Y")));
        }

        Assert.Contains((3, 0), found);
        Assert.Contains((0, 2), found);
        Assert.DoesNotContain((0, 3), found);
    }

    /// <summary>
    /// A SuperCharged boolean on the slot itself is not part of the save format; a grid
    /// that writes one leaves the inventory unchanged as far as the game is concerned.
    /// </summary>
    [Fact]
    public void ASlotFlagIsNotWhatMarksASlotSupercharged()
    {
        var inventory = new JsonObject();
        var slots = new JsonArray();
        var slot = new JsonObject();
        slot.Add("SuperCharged", true);
        slots.Add(slot);
        inventory.Add("Slots", slots);

        Assert.Null(inventory.GetArray("SpecialSlots"));
    }
}
