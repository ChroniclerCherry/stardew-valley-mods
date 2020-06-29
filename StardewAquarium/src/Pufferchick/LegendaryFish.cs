using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewAquarium
{
    static class LegendaryFish
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            HarmonyInstance harmony = HarmonyInstance.Create("Cherry.StardewAquarium");
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.isFishBossFish)),
                postfix: new HarmonyMethod(typeof(LegendaryFish), nameof(LegendaryFish.isFishBossFish_AddPufferchick))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                prefix: new HarmonyMethod(typeof(LegendaryFish), nameof(LegendaryFish.getFish_checkPufferChick))
            );
        }

        public static void isFishBossFish_AddPufferchick(ref bool __result, int index)
        {
            if (index == ModEntry.JsonAssets.GetObjectId("Pufferchick"))
                __result = true;
        }

        public static bool getFish_checkPufferChick(GameLocation __instance, Farmer who, ref Object __result)
        {
            if (__instance.Name != "ExteriorMuseum")
                return true;

            if (!who.fishCaught.ContainsKey(128))
                return true;

            if (who.stats.ChickenEggsLayed == 0)
                return true;

            if (Game1.random.NextDouble() < 0.01 + (0.05 * Utils.GetNumDonatedFish()))
                return true;

            int id = ModEntry.JsonAssets.GetObjectId("Pufferchick");

            if (who.fishCaught.ContainsKey(id))
            {
                if (!Utils.IsUnDonatedFish("Pufferchick")) return true;

                __result = new Object(id,1,false,0);
                return false;
            }

            __result = new Object(id, 1);
            return false;
        }
    }
}
