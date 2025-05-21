using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace UpgradeEmptyCabins.Framework;

internal static class ModUtility
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get an empty cabin by its unique interior name.</summary>
    /// <param name="name">The unique name for its interior location.</param>
    public static (Building building, Cabin indoors)? GetEmptyCabin(string name)
    {
        foreach (var pair in GetEmptyCabins())
        {
            if (pair.building.GetIndoorsName() == name)
                return pair;
        }

        return null;
    }

    /// <summary>Get all empty cabins in the save.</summary>
    public static IEnumerable<(Building building, Cabin indoors)> GetEmptyCabins()
    {
        foreach (GameLocation location in Game1.locations)
        {
            foreach (Building building in location.buildings)
            {
                if (building.isCabin && building.GetIndoors() is Cabin { IsOwnerActivated: false } indoors)
                    yield return (building, indoors);
            }
        }
    }

    /// <summary>Get a detailed description for a cabin to help identify it in lists.</summary>
    /// <param name="cabin">The cabin building.</param>
    public static string GetCabinDescription(Building cabin)
    {
        string cabinName = cabin.skinId.Value is null or ""
            ? "Stone Cabin"
            : cabin.skinId.Value;

        GameLocation parentLocation = cabin.GetParentLocation();

        return $"{cabinName} at {parentLocation.DisplayName} tile position {cabin.tileX.Value + 2} {cabin.tileY.Value + 1}"; // show tile position of door
    }
}
