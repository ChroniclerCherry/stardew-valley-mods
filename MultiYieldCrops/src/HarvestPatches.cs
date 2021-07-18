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

        public static void CropHarvest_prefix(Crop __instance,out bool __state)
        {
            //checks if crop can be harvested
            __state = ((int)__instance.currentPhase >= __instance.phaseDays.Count - 1 && (!__instance.fullyGrown || (int)__instance.dayOfCurrentPhase <= 0));
        }

        public static void CropHarvest_postfix(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester,
            Crop __instance, bool __state)
        {

            if (!__state)
                return;

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
