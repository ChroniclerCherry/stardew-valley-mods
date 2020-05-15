using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;

namespace MultiYieldCrop
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
            __state = __instance.dayOfCurrentPhase.Get();
        }

        public static void CropHarvest_postfix(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester,
            Crop __instance, bool __result, int __state)
        {

            //ignore if this is a forage crop or the crop is dead or not fully grown
            if (__instance.forageCrop.Get() || __instance.dead.Get() || !__instance.fullyGrown.Get())
                return;

            if (__instance.regrowAfterHarvest.Get() > 0 &&__state > 1)
                return;
            
            if (!__result && __instance.regrowAfterHarvest.Get() <= 0)
                return;

            try
            {
                int cropID = __instance.indexOfHarvest.Get();
                string cropName = new StardewValley.Object(__instance.indexOfHarvest.Get(), 1, false).Name;
                MultiYieldCrops.instance.SpawnHarvest(new Vector2(xTile,yTile),
                    cropName, soil.fertilizer.Get(),junimoHarvester);

            } catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CropHarvest_postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void BushPerformUseAction_postfix(Vector2 tileLocation, Bush __instance)
        {
            //not implemented yet
            if ( __instance.inBloom(Game1.currentSeason, Game1.dayOfMonth) && __instance.size.Get() == Bush.greenTeaBush)
            {
                MultiYieldCrops.instance.SpawnHarvest(tileLocation, "Tea Leaves", HoeDirt.noFertilizer);
            }
        }

    }
}
