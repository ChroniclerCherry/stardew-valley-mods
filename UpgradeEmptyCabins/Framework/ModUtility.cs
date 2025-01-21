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
        foreach (Building building in Game1.getFarm().buildings)
        {
            if (building.isCabin && building.GetIndoors() is Cabin { IsOwnerActivated: false } indoors)
                yield return (building, indoors);
        }
    }
}
