using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace MultiYieldCrops
{
    class HarvestPatches
    {

        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void CropHarvest_prefix(Crop __instance,out int __state)
        {
            //dayOfCurrentPhase counts down how many days left before a reharvestable crop can be harvested again
            __state = __instance.dayOfCurrentPhase.Value;
        }

        public static void CropHarvest_postfix(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester,
            Crop __instance, bool __result, int __state)
        {
            //__result is true is a crop was successfully harvested unless it's regrowable
            if (!__result && __instance.regrowAfterHarvest.Value < 0) return;

            //we reach this point if it's a regrowable crop and check its dayOfCurrentPhase from the prefix
            if (__instance.regrowAfterHarvest.Value > 0 &&__state > 1) return;

            try
            {
                int cropId = __instance.indexOfHarvest.Value;
                string cropName = new StardewValley.Object(cropId, 1, false).Name;
                MultiYieldCrop.MultiYieldCrops.instance.SpawnHarvest(new Vector2(xTile,yTile),
                    cropName, soil.fertilizer.Value,junimoHarvester);

            } catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CropHarvest_postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void BushPerformUseAction_postfix(Vector2 tileLocation, Bush __instance)
        {
            //not implemented yet
            if ( __instance.inBloom(Game1.currentSeason, Game1.dayOfMonth) && __instance.size.Value == Bush.greenTeaBush)
            {
                MultiYieldCrop.MultiYieldCrops.instance.SpawnHarvest(tileLocation, "Tea Leaves", HoeDirt.noFertilizer);
            }
        }

    }

}
