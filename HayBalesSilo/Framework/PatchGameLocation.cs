using System.Linq;
using StardewValley;
using StardewValley.GameData.Buildings;

namespace HayBalesSilo.Framework
{
    public class PatchGameLocation
    {
        internal static void After_GetHayCapacity(ref GameLocation __instance, ref int __result)
        {
            if (!ModEntry.GetAllAffectedMaps().Contains(Game1.currentLocation))
                return;

            if (__result > 0 || !ModEntry.Config.RequiresConstructedSilo)
            {
                int hayBales = __instance.Objects.Values.Count(p => p.QualifiedItemId == ModEntry.HayBaleQualifiedId);
                if (hayBales == 0)
                    return;

                int hayPerSilo = Game1.buildingData.TryGetValue("Silo", out BuildingData data)
                    ? data.HayCapacity
                    : 240;

                __result += hayBales * (ModEntry.Config.HayBaleEquivalentToHowManySilos * hayPerSilo);
            }
        }
    }
}
