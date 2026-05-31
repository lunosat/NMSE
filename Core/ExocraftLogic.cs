using NMSE.Core.Utilities;
using NMSE.Data;

namespace NMSE.Core;

/// <summary>
/// Defines vehicle (exocraft) types and provides helpers for building export filenames.
/// </summary>
internal static class ExocraftLogic
{
    /// <summary>
    /// Known vehicle types with their save data indices and display names.
    /// </summary>
    internal static readonly (int Index, string Name)[] VehicleTypes =
    [
        (0, "Roamer"),
        (1, "Nomad"),
        (2, "Colossus"),
        (3, "Pilgrim"),
        (5, "Nautilon"),
        (6, "Minotaur")
    ];

    internal static readonly Dictionary<string, string> VehicleTypeLocKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Roamer"] = "exocraft.type_roamer",
        ["Nomad"] = "exocraft.type_nomad",
        ["Colossus"] = "exocraft.type_colossus",
        ["Pilgrim"] = "exocraft.type_pilgrim",
        ["Nautilon"] = "exocraft.type_nautilon",
        ["Minotaur"] = "exocraft.type_minotaur",
    };

    internal static string GetLocalisedVehicleTypeName(string internalName)
    {
        if (VehicleTypeLocKeys.TryGetValue(internalName, out var key))
            return UiStrings.Get(key);
        return internalName;
    }

    // Per vehicle type:
    //   Standard exocraft:
    //   (Roamer, Nomad, Pilgrim)  -> [Exocraft, AllVehicles]
    //   Colossus: (truck)         -> [Colossus, Exocraft, AllVehicles]
    //   Nautilon: (sub)           -> [Submarine, AllVehicles]
    //   Minotaur: (mech)          -> [Mech, AllVehicles]
    /// <summary>
    /// Maps a vehicle display name to the Technology Category owner type
    /// used for inventory tech filtering. This determines which technology items
    /// can be installed in the vehicle's tech inventory.
    /// </summary>
    /// <param name="vehicleName">The vehicle display name (e.g. "Roamer", "Colossus", "Nautilon").</param>
    /// <returns>The Technology Category owner string for inventory filtering.</returns>
    internal static string GetOwnerTypeForVehicle(string vehicleName)
    {
        return vehicleName switch
        {
            "Colossus" => "Colossus",
            "Nautilon" => "Submarine",
            "Minotaur" => "Mech",
            _ => "Exocraft" // Roamer, Nomad, Pilgrim
        };
    }

    /// <summary>
    /// Builds a sanitized export filename for a vehicle inventory.
    /// </summary>
    /// <param name="vehicleName">The vehicle display name.</param>
    /// <param name="suffix">A suffix describing the inventory type (e.g. "cargo", "tech").</param>
    /// <returns>A filename-safe string ending with "_inv.json".</returns>
    internal static string BuildExportFileName(string vehicleName, string suffix)
    {
        string safeName = (vehicleName ?? "vehicle").Replace(' ', '_');
        return $"{safeName}_{suffix}_inv.json";
    }

    /// <summary>
    /// Builds a sanitized export filename for a whole vehicle export.
    /// </summary>
    /// <param name="vehicleName">The vehicle display name.</param>
    /// <returns>A filename-safe string ending with "_vehicle.json".</returns>
    internal static string BuildVehicleExportFileName(string vehicleName)
    {
        string safeName = (vehicleName ?? "vehicle").Replace(' ', '_');
        return $"{safeName}_vehicle.json";
    }

    /// <summary>
    /// Parses a GalacticAddress value (hex string or integer) from BaseBuildingObjects
    /// or PersistentPlayerBases into VoxelX, VoxelY, VoxelZ, SolarSystemIndex, PlanetIndex.
    /// Returns null if the address cannot be parsed.
    /// The GalacticAddress may be either:
    ///   - 12 hex digits (portal code format): {planet:1}{system:3}{y:2}{z:3}{x:3}
    ///   - 14 hex digits (UniverseAddress format): {planet:1}{system:3}{reality:2}{y:2}{z:3}{x:3}
    /// For 14-digit addresses, the 2-digit RealityIndex is stripped to obtain the portal code.
    /// </summary>
    internal static (int VoxelX, int VoxelY, int VoxelZ, int SolarSystemIndex, int PlanetIndex)? ParseGalacticAddressToVoxel(object? galacticAddressValue)
    {
        string normalised = CoordinateHelper.NormalizeGalacticAddress(galacticAddressValue);
        if (string.IsNullOrEmpty(normalised) || normalised.Length < 14)
            return null;

        string portalCode = normalised.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? normalised[2..]
            : normalised;

        // 14-hex-digit UniverseAddress format includes a 2-digit RealityIndex
        // between SolarSystemIndex and VoxelY — strip it to get the 12-digit portal code.
        if (portalCode.Length == 14)
            portalCode = string.Concat(portalCode.AsSpan(0, 4), portalCode.AsSpan(6, 8));

        if (portalCode.Length != 12)
            return null;

        if (CoordinateHelper.PortalCodeToVoxel(portalCode, out int vx, out int vy, out int vz, out int si, out int pi))
            return (vx, vy, vz, si, pi);

        return null;
    }
}
