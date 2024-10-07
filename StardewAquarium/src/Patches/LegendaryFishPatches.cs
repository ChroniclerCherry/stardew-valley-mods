using HarmonyLib;

using StardewValley;
using StardewValley.Tools;

using Object = StardewValley.Object;

namespace StardewAquarium.Patches
{
    static class LegendaryFishPatches
    {
        private static string? PufferChickID => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName);
        private static string? LegendaryBaitId => ModEntry.JsonAssets?.GetObjectId(ModEntry.LegendaryBaitName);

        private const string LegendId = "163";
        private const string MutantCarpId = "682";

        public static void Initialize()
        {
            Harmony harmony = ModEntry.Harmony;

            //patch handles making the pufferchick catchable
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(GameLocation_getFish_Prefix))
            );
        }

        public static bool GameLocation_getFish_Prefix(GameLocation __instance, Farmer who, int waterDepth, ref Item __result)
        {
            //checks if player should get pufferchick
            __result = GetFishPufferchick(__instance, who);
            return __result is null;
        }

        private static Object GetFishPufferchick(GameLocation loc, Farmer who)
        {
            if (loc.Name != ModEntry.Data.ExteriorMapName) //only happens on the exterior museum map
                return null;

            if (!who.fishCaught.ContainsKey("128")) //has caught a pufferfish before
                return null;

            if (who.stats.ChickenEggsLayed == 0) //has had a chicken lay at least one egg
                return null;

            if (who.CurrentTool is FishingRod rod && rod.GetBait()?.ItemId == LegendaryBaitId)
            {
                return new Object(PufferChickID, 1);
            }
            if (who.fishCaught.ContainsKey(PufferChickID)) return null;

            //base of 1% and an additional 0.5% per fish donated
            double pufferChance = 0.01 + 0.005 * Utils.GetNumDonatedFish();

            if (Game1.random.NextDouble() > pufferChance)
                return null;

            Object pufferchick = new Object(PufferChickID, 1);
            pufferchick.SetTempData("IsBossFish", true); //Make pufferchick boss fish in 1.6+
            return pufferchick;
        }
    }
}
