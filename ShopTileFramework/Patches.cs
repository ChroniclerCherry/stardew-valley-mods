using Harmony;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework
{
    [HarmonyPatch(typeof(StardewValley.Utility), "getPurchaseAnimalStock")]
    internal class PatchGetPurchaseAnimalStock
    {
        //we want ours to override BFAV's
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref List<StardewValley.Object> __result)
        {
            ModEntry.monitor.Log("Applying prefix patch to Utility.getPurchaseAnimalStock...", StardewModdingAPI.LogLevel.Debug);
            try
            {
                // Grab the farm animals for purchase
                var stock = ModEntry.BFAV.GetAnimalShopStock(Game1.getFarm());
                var newAnimalStock = new List<StardewValley.Object>();
                foreach (var animal in stock)
                {
                    if (!ModEntry.ExcludeFromMarnie.Contains(animal.Name))
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
