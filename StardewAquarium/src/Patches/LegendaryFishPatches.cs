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

        private static string? PufferChickID => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName);
        private static string? LegendaryBaitId => ModEntry.JsonAssets?.GetObjectId(ModEntry.LegendaryBaitName);

        private const string CrimsonFishId = "159";
        private const string AnglerId = "160";
        private const string LegendId = "163";
        private const string MutantCarpId = "682";
        private const string GlacierFishId = "775";

        private static Dictionary<string, string> LegendaryFish = new Dictionary<string, string>()
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

            //patch handles making the pufferchick catchable
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(GameLocation_getFish_Prefix))
            );

            //All of the below patches were absolved into GameLocation.getFish, so the patch above will handle location based checks

            //makes crimsonfish recatchable
            /*harmony.Patch(
                AccessTools.Method(typeof(Beach), nameof(Beach.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Beach_getFish_prefix))
            );

            //makes Angler recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Town), nameof(Town.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Town_getFish_prefix))
            );

            //makes Legend recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Mountain), nameof(Mountain.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Mountain_getFish_prefix))
            );
            //makes MutantCarp recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Sewer), nameof(Sewer.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Sewer_getFish_prefix))
            );
            //makes GlacierFish recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Forest), nameof(Forest.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(Forest_getFish_prefix))
            );*/

        }

        public static bool Forest_getFish_prefix(int waterDepth, Farmer who, ref Item __result)
        {
            if (who == null || !(who.CurrentTool is FishingRod rod) ||
                rod.GetBait().ItemId != LegendaryBaitId) return true;
            if (who.Tile.X != 58 || who.Tile.Y != 87 || who.FishingLevel < 6 || waterDepth < 3) return true;
            if (!who.fishCaught.ContainsKey(GlacierFishId) || (!Game1.currentSeason.Equals("winter"))) return true;
            __result = new Object(GlacierFishId, 1);
            return false;

        }
        public static bool Sewer_getFish_prefix(Farmer who, ref Item __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.GetBait().ItemId != LegendaryBaitId) return true;
            if (!who.fishCaught.ContainsKey(MutantCarpId)) return true;
            __result = new Object(MutantCarpId, 1);
            return false;

        }
        public static bool Mountain_getFish_prefix(int waterDepth, Farmer who, ref Item __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.GetBait().ItemId != LegendaryBaitId) return true;
            if (!Game1.isRaining || who.FishingLevel < 10 || waterDepth < 4) return true;
            if (!who.fishCaught.ContainsKey(LegendId) || (!Game1.currentSeason.Equals("spring"))) return true;
            __result = new Object(LegendId, 1);
            return false;

        }

        public static bool Town_getFish_prefix(Farmer who, ref Item __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.GetBait().ItemId != LegendaryBaitId) return true;
            if (!(who.Tile.Y < 15f) || who.FishingLevel < 3) return true;
            if (!who.fishCaught.ContainsKey(AnglerId) || (!Game1.currentSeason.Equals("fall"))) return true;
            __result = new Object(AnglerId, 1);
            return false;

        }

        public static bool Beach_getFish_prefix(int waterDepth, Farmer who, ref Item __result)
        {
            if (Game1.player == null || !(Game1.player.CurrentTool is FishingRod rod) ||
                rod.GetBait().ItemId != LegendaryBaitId) return true;
            if (who.Tile.X < 82 || who.FishingLevel < 5 || waterDepth < 3) return true;
            if (!who.fishCaught.ContainsKey(CrimsonFishId) || (!Game1.currentSeason.Equals("summer"))) return true;
            __result = new Object(CrimsonFishId, 1);
            return false;

        }

        public static bool GameLocation_getFish_Prefix(GameLocation __instance, Farmer who, int waterDepth, ref Item __result)
        {
            //checks if player should get pufferchick
            switch (__instance)
            {
                case Beach:
                    return Beach_getFish_prefix(waterDepth, who, ref __result);
                case Town:
                    return Town_getFish_prefix(who, ref __result);
                case Mountain:
                    return Mountain_getFish_prefix(waterDepth, who, ref __result);
                case Sewer:
                    return Sewer_getFish_prefix(who, ref __result);
                case Forest:
                    return Forest_getFish_prefix(waterDepth, who, ref __result);
            }
            var puff = GetFishPufferchick(__instance, who);
            if (puff == null)
                return true;

            __result = puff;
            return false;

        }

        private static Object GetFishPufferchick(GameLocation loc, Farmer who)
        {
            if (loc.Name != ModEntry.Data.ExteriorMapName) //only happens on the exterior museum map
                return null;

            if (!who.fishCaught.ContainsKey("128")) //has caught a pufferfish before
                return null;

            if (who.stats.ChickenEggsLayed == 0) //has had a chicken lay at least one egg
                return null;

            if (who.CurrentTool is FishingRod rod &&
                rod.GetBait().ItemId == LegendaryBaitId)
            {
                return new Object(PufferChickID, 1);
            }
            if (who.fishCaught.ContainsKey(PufferChickID)) return null;

            //base of 1% and an additional 0.5% per fish donated
            double pufferChance = 0.01 + 0.005 * Utils.GetNumDonatedFish();

            if (Game1.random.NextDouble() > pufferChance)
                return null;

            var puffer = new Object(PufferChickID, 1);
            puffer.SetTempData("IsBossFish", true); //Make pufferchick boss fish in 1.6+

            return puffer;
        }
    }
}
