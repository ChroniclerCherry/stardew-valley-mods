/*
using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ShopTileFramework
{
    [HarmonyPatch(typeof(StardewValley.Utility), "getPurchaseAnimalStock")]
    internal class PatchGetPurchaseAnimalStock
    {
        //we want ours to override BFAV's patch
        [HarmonyPriority(Priority.First)]
        public static void PostFix(ref List<StardewValley.Object> __result)
        {
            ModEntry.monitor.Log("Applying prefix patch to Utility.getPurchaseAnimalStock...", StardewModdingAPI.LogLevel.Debug);
            try
            {
                //if the exclusion list is empty, fall back to original logic
                if (ModEntry.ExcludeFromMarnie.Count == 0)
                    return;

                List<StardewValley.Object> OriginalStock = __result;

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

            } catch (Exception ex)
            {
                ModEntry.monitor.Log("Failed Patching Utility.getPurchaseAnimalStock, falling back to vanilla logic.", StardewModdingAPI.LogLevel.Error);
                ModEntry.monitor.Log(ex.ToString());
            }
            
        }
    }
}
*/