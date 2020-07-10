using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace StardewAquarium.Patches
{
    static class LegendaryFishPatches
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static bool _fishingStarted;

        private static int PufferChickID { get => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1; }
        private static Dictionary<int, int[]> _trackedFish;

        private static Dictionary<int, string> LegendaryFish = new Dictionary<int, string>()
        {
            {159,"CrimsonFish"},{160,"Angler"},{163,"Legend"},{682,"MutantCarp"},{775,"GlacierFish"}
        };

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {

            _helper = helper;
            _monitor = monitor;

            HarmonyInstance harmony = ModEntry.harmony;

            //this patch returns the pufferchick as a legendary fish during the fishing minigame
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.isFishBossFish)),
                postfix: new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.isFishBossFish_AddPufferchick))
            );

            //patch handles making legendary fish recatchable and adds the pufferchick
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                prefix: new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.getFish_prefix)),
                postfix: new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.getFish_PostFix))
            );

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            int id = PufferChickID;
            if (LegendaryFish.ContainsKey(id)) return;
            LegendaryFish.Add(id, ModEntry.PufferChickName);

        }

        private static void Player_InventoryChanged(object sender, StardewModdingAPI.Events.InventoryChangedEventArgs e)
        {
            if (!_fishingStarted)
                return;

            _fishingStarted = false;

            foreach (Item i in e?.Added)
            {
                if (!(i is Object obj)) return;

                if (!_trackedFish.ContainsKey(obj.ParentSheetIndex))
                    return;

                obj.Price = 0;
            }

            _trackedFish.Clear();

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

        public static bool getFish_prefix()
        {
            if (!ModEntry.RecatchLegends)
                return true;

            //saves any fishdata we want to recatch so that the game doesn't know it's already been caught
            _trackedFish = new Dictionary<int, int[]>();
            foreach (int fishId in LegendaryFish.Keys)
            {
                //if the player has caught this legendary before
                if (Game1.player.fishCaught.ContainsKey(fishId)
                    && Game1.player.fishCaught.TryGetValue(fishId, out int[] freshValues)
                    && !Utils.PlayerInventoryContains(fishId)) //if the player doesn't have the fish in their inventory
                {
                    string fishname = LegendaryFish[fishId];
                    //only saving state of fish if it has not yet been donated
                    if (Utils.IsUnDonatedFish(fishname))
                    {
                        //save stats of fish and remove it from player's records
                        _trackedFish.Add(fishId, freshValues);
                        Game1.player.fishCaught.Remove(fishId);
                    }
                }
            }

            if (_trackedFish.Count > 0)
            {
                _fishingStarted = true;
                _monitor.Log($"Allowing recatch of legendaries:{_trackedFish.Keys}");
            }

            return true;
        }

        public static void getFish_PostFix(GameLocation __instance, Farmer who, ref Object __result)
        {
            //checks if player should get pufferchick
            var puff = GetFishPufferchick(__instance, who);
            if (puff != null)
                __result = puff;

            if (_trackedFish?.Count == 0) return;
            _monitor.Log($"Returning fish to player records:{_trackedFish.Keys}");

            //if the fish is one we're tracking, change its sell price to 0
            if (_trackedFish.ContainsKey(__result.ParentSheetIndex))
                __result.Price = 0;

            foreach (var fishID in _trackedFish.Keys)
            {
                int[] stashedData = _trackedFish[fishID];
                Game1.player.fishCaught[fishID] = stashedData;
            }

        }

        private static Object GetFishPufferchick(GameLocation loc, Farmer who)
        {
            if (loc.Name != ModEntry.data.ExteriorMapName) //only happens on the exterior museum map
                return null;

            if (!who.fishCaught.ContainsKey(128)) //has caught a pufferfish before
                return null;

            if (who.stats.ChickenEggsLayed == 0) //has had a chicken lay at least one egg
                return null;

            //base of 1% and an additional 5% per fish donated
            if (Game1.random.NextDouble() > 0.01 + 0.005 * Utils.GetNumDonatedFish())
                return null;

            int id = PufferChickID;

            if (who.fishCaught.ContainsKey(id)) return null;

            return new Object(id, 1);
        }
    }
}
