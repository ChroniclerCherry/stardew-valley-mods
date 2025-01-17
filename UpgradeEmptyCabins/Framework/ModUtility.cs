using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;

namespace UpgradeEmptyCabins.Framework;

internal static class ModUtility
{
    /*********
    ** Public methods
    *********/
    public static Building GetCabin(string name)
    {
        foreach (var cabin in GetCabins())
        {
            if (cabin.GetIndoorsName() == name)
                return cabin;
        }

        return null;
    }

    public static IEnumerable<Building> GetCabins()
    {
        foreach (var building in Game1.getFarm().buildings)
        {
            if (building.isCabin)
                yield return building;
        }
    }
}
