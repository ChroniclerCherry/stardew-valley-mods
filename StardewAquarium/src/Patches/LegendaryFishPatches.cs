using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewAquarium.Patches
{
    static class LegendaryFishPatches
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static int PufferChickID => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1;
        private static int LegendaryBaitId => ModEntry.JsonAssets?.GetObjectId(ModEntry.LegendaryBaitName) ?? -1;

        private const int CrimsonFishId = 159;
        private const int AnglerId = 160;
        private const int LegendId = 163;
        private const int MutantCarpId = 682;
        private const int GlacierFishId = 775;

        private static Dictionary<int, string> LegendaryFish = new Dictionary<int, string>()
        {
            {CrimsonFishId,"Crimsonfish"},
            {AnglerId,"Angler"},
            {LegendId,"Legend"},
            {MutantCarpId,"MutantCarp"},
            {GlacierFishId,"Glacierfish"}
        };

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {

            _helper = helper;
            _monitor = monitor;

            Harmony harmony = ModEntry.Harmony;

            //this patch returns the pufferchick as a legendary fish during the fishing minigame
            harmony.Patch(
                AccessTools.Method(typeof(FishingRod), nameof(FishingRod.isFishBossFish)),
                postfix: new HarmonyMethod(typeof(LegendaryFishPatches), nameof(isFishBossFish_AddPufferchick))
            );

            //patch handles making the pufferchick catchable
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(GameLocation_getFish_Prefix))
            );

            //makes crimsonfish recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Beach), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Beach_getFish_prefix))
            );

            //makes Angler recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Town), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Town_getFish_prefix))
            );

            //makes Legend recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Mountain), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Mountain_getFish_prefix))
            );
            //makes MutantCarp recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Sewer), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Sewer_getFish_prefix))
            );
            //makes GlacierFish recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Forest), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Forest_getFish_prefix))
            );

        }

        public static bool Forest_getFish_prefix(int waterDepth, Farmer who, ref Object __result)
        {
            if (who == null || !(who.CurrentTool is FishingRod rod) ||
                rod.getBaitAttachmentIndex() != LegendaryBaitId) return true;
            if (who.getTileX() != 58 || who.getTileY() != 87 || who.FishingLevel < 6 || waterDepth < 3) return true;
            if (!who.fishCaught.ContainsKey(GlacierFishId) || (!Game1.currentSeason.Equals("winter"))) return true;
            __result = new Object(GlacierFishId, 1);
            return false;

        }
        public static bool Sewer_getFish_prefix(Farmer who, ref Object __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.getBaitAttachmentIndex() != LegendaryBaitId) return true;
            if (!who.fishCaught.ContainsKey(MutantCarpId)) return true;
            __result = new Object(MutantCarpId, 1);
            return false;

        }
        public static bool Mountain_getFish_prefix(int waterDepth, Farmer who, ref Object __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.getBaitAttachmentIndex() != LegendaryBaitId) return true;
            if (!Game1.isRaining || who.FishingLevel < 10 || waterDepth < 4) return true;
            if (!who.fishCaught.ContainsKey(LegendId) || (!Game1.currentSeason.Equals("spring"))) return true;
            __result = new Object(LegendId, 1);
            return false;

        }

        public static bool Town_getFish_prefix(Farmer who, ref Object __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.getBaitAttachmentIndex() != LegendaryBaitId) return true;
            if (!(who.getTileLocation().Y < 15f) || who.FishingLevel < 3) return true;
            if (!who.fishCaught.ContainsKey(AnglerId) || (!Game1.currentSeason.Equals("fall"))) return true;
            __result = new Object(AnglerId, 1);
            return false;

        }

        public static bool Beach_getFish_prefix(int waterDepth, Farmer who, ref Object __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.getBaitAttachmentIndex() != LegendaryBaitId) return true;
            if (who.getTileX() < 82 || who.FishingLevel < 5 || waterDepth < 3) return true;
            if (!who.fishCaught.ContainsKey(CrimsonFishId) || (!Game1.currentSeason.Equals("summer"))) return true;
            __result = new Object(CrimsonFishId, 1);
            return false;

        }

        /// <summary>
        /// Makes pufferchick return as a legendary fish during the fishing minigame
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="index"></param>
        public static void isFishBossFish_AddPufferchick(ref bool __result, int index)
        {
            if (index == PufferChickID)
                __result = true;
        }

        public static bool GameLocation_getFish_Prefix(GameLocation __instance, Farmer who, ref Object __result)
        {
            //checks if player should get pufferchick
            var puff = GetFishPufferchick(__instance, who);
            if (puff == null) return true;

            __result = puff;
            return false;

        }

        private static Object GetFishPufferchick(GameLocation loc, Farmer who)
        {
            if (loc.Name != ModEntry.Data.ExteriorMapName) //only happens on the exterior museum map
                return null;

            if (!who.fishCaught.ContainsKey(128)) //has caught a pufferfish before
                return null;

            if (who.stats.ChickenEggsLayed == 0) //has had a chicken lay at least one egg
                return null;

            if (who.CurrentTool is FishingRod rod &&
                rod.getBaitAttachmentIndex() == LegendaryBaitId)
            {
                return new Object(PufferChickID, 1);
            }
            if (who.fishCaught.ContainsKey(PufferChickID)) return null;

            //base of 1% and an additional 0.5% per fish donated
            double pufferChance = 0.01 + 0.005 * Utils.GetNumDonatedFish();

            if (Game1.random.NextDouble() > pufferChance)
                return null;

            return new Object(PufferChickID, 1);
        }
    }
}
