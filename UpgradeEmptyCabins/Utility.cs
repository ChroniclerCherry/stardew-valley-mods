using StardewValley;
using StardewValley.Buildings;
using System.Collections.Generic;

namespace UpgradeEmptyCabins
{
    internal static class ModUtility
    {
        public static Building GetCabin(string name)
        {
            foreach (var cabin in GetCabins())
                if (cabin.nameOfIndoors == name)
                    return cabin;
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
}
