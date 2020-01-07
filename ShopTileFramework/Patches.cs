using Harmony;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework
{
    [HarmonyPatch(typeof(StardewValley.Utility), "getPurchaseAnimalStock")]
    internal class PatchGetPurchaseAnimalStock
    {
        //we want ours to override BFAV's patch
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref List<StardewValley.Object> __result)
        {
            ModEntry.monitor.Log("Applying prefix patch to Utility.getPurchaseAnimalStock...", StardewModdingAPI.LogLevel.Debug);
            try
            {
                //if the exclusion list is empty, fall back to original logic
                if (ModEntry.ExcludeFromMarnie.Count == 0)
                    return true;

                List<StardewValley.Object> OriginalStock;

                //if BFAV isn't installed/not enabled, grab the vanilla list
                if (ModEntry.BFAV == null || !ModEntry.BFAV.IsEnabled())
                {
                    OriginalStock = __result;
                } else //otherwise use the BFAV list
                {
                    OriginalStock = ModEntry.BFAV.GetAnimalShopStock(Game1.getFarm());
                }

                //build a new list without the excluded animals
                var newAnimalStock = new List<StardewValley.Object>();
                foreach (var animal in OriginalStock)
                {
                    if (ModEntry.ExcludeFromMarnie.Contains(animal.Name))
                    {
                        ModEntry.monitor.Log($"{animal.Name} was removed from Marnie's stock.");   
                    } else
                    {
                        newAnimalStock.Add(animal);
                    }
                }

                __result = newAnimalStock;

                return false;
            } catch
            {
                ModEntry.monitor.Log("Failed Patching Utility.getPurchaseAnimalStock, falling back to vanilla logic.", StardewModdingAPI.LogLevel.Error);
                return true;
            }
            
        }
    }
}
