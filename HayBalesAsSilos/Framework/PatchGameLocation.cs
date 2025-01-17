using System.Linq;
using StardewValley;

namespace HayBalesAsSilos.Framework;

internal class PatchGameLocation
{
    public static void After_GetHayCapacity(ref GameLocation __instance, ref int __result)
    {
        if (!ModEntry.GetAllAffectedMaps().Contains(Game1.currentLocation))
            return;

        if (__result > 0 || !ModEntry.Config.RequiresConstructedSilo)
        {
            int hayBales = __instance.Objects.Values.Count(p => p.QualifiedItemId == ModEntry.HayBaleQualifiedId);
            if (hayBales == 0)
                return;

            __result += hayBales * ModEntry.Config.HayPerBale;
        }
    }
}
