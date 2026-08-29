using NMSE.Core;
using NMSE.Data;
using NMSE.IO;
using NMSE.Models;

namespace NMSE.Tests;

/// <summary>
/// Deleting a ship must not renumber the ships after it. ShipOwnership is indexed by
/// PrimaryShip and runs parallel to ShipUsesLegacyColours and CharacterCustomisationData,
/// so removing an element silently repoints all three.
/// </summary>
public class ShipSlotIntegrityTests
{
    private static string? SavePath => Environment.GetEnvironmentVariable("NMSE_TEST_SAVE");

    private static void RegisterMapper()
    {
        string probe = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            string candidate = Path.Combine(probe, "Resources", "map", "mapping.json");
            if (File.Exists(candidate))
            {
                var mapper = new JsonNameMapper();
                mapper.Load(candidate);
                JsonParser.SetDefaultMapper(mapper);
                return;
            }
            probe = Path.GetFullPath(Path.Combine(probe, ".."));
        }
        Assert.Fail("mapping.json not found");
    }

    [Fact]
    public void DeletingAShipLeavesTheSlotsAndPrimaryIndexAlone()
    {
        string? source = SavePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source)) return; // no save supplied

        RegisterMapper();

        var root = SaveFileManager.LoadSaveFile(source);
        SaveFileManager.RegisterContextTransforms(root);

        var playerState = root.GetObject("PlayerStateData");
        Assert.NotNull(playerState);

        var ships = playerState!.GetArray("ShipOwnership");
        Assert.NotNull(ships);

        int slotCount = ships!.Length;
        var validBefore = StarshipLogic.BuildShipList(ships);

        // A save with one ship cannot exercise a delete; the panel refuses it anyway.
        if (validBefore.Count < 2) return;

        // Delete one that is not the primary, so the primary index must survive untouched.
        int primary = playerState.GetInt("PrimaryShip");
        var victim = validBefore.First(s => s.DataIndex != primary);

        StarshipLogic.DeleteShipData(ships.GetObject(victim.DataIndex));
        StarshipLogic.ResetShipCustomisation(playerState.GetArray("CharacterCustomisationData"), victim.DataIndex);

        // The array keeps its length: the slot is invalidated, not removed.
        Assert.Equal(slotCount, ships.Length);

        var validAfter = StarshipLogic.BuildShipList(ships);
        Assert.Equal(validBefore.Count - 1, validAfter.Count);

        // Every surviving ship still answers to the index it had before.
        foreach (var ship in validAfter)
        {
            var before = validBefore.First(s => s.DataIndex == ship.DataIndex);
            Assert.Equal(before.DisplayName, ship.DisplayName);
        }

        // And the primary still points at the ship it did.
        Assert.Equal(primary, playerState.GetInt("PrimaryShip"));
        Assert.Contains(validAfter, s => s.DataIndex == primary);
    }
}
