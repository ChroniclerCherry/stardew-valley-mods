using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;
using StardewValley.Locations;
using Object = StardewValley.Object;
using System;
using StardewValley.Network;

namespace StardewAquarium.Patches
{
    static class LegendaryFishPatches
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static bool _fishingStarted;

        private static int PufferChickID => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1;
        private static int _trackedFishId = -1;
        private static int[] _trackedFishStats;

        private const int CrimsonFishId = 159;
        private const int AnglerId = 160;
        private const int LegendId = 163;
        private const int MutantCarpId = 682;
        private const int GlacierFishId = 775;

        private static Dictionary<int, string> LegendaryFish = new Dictionary<int, string>()
        {
            {CrimsonFishId,"CrimsonFish"},
            {AnglerId,"Angler"},
            {LegendId,"Legend"},
            {MutantCarpId,"MutantCarp"},
            {GlacierFishId,"GlacierFish"}
        };

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {

            _helper = helper;
            _monitor = monitor;

            HarmonyInstance harmony = ModEntry.Harmony;

            //this patch returns the pufferchick as a legendary fish during the fishing minigame
            harmony.Patch(
                AccessTools.Method(typeof(FishingRod), nameof(FishingRod.isFishBossFish)),
                postfix: new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.isFishBossFish_AddPufferchick))
            );

            //patch handles making the pufferchick catchable
            harmony.Patch(
                AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.GameLocation_getFish_Prefix))
            );

            //don't patch these if we don't wanna enable recatch behaviour
            if (!ModEntry.RecatchLegends) return;
            //makes crimsonfish recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Beach), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.Beach_getFish_prefix)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.ReturnFish))
            );

            //makes Angler recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Town), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.Town_getFish_prefix)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.ReturnFish))
            );

            //makes Legend recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Mountain), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.Mountain_getFish_prefix)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.ReturnFish))
            );
            //makes MutantCarp recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Sewer), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.Sewer_getFish_prefix)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.ReturnFish))
            );
            //makes GlacierFish recatchable
            harmony.Patch(
                AccessTools.Method(typeof(Forest), nameof(GameLocation.getFish)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.Forest_getFish_prefix)),
                new HarmonyMethod(typeof(LegendaryFishPatches), nameof(LegendaryFishPatches.ReturnFish))
            );

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        }

        public static void Forest_getFish_prefix()
        {
            _fishingStarted = HideFish(GlacierFishId);
        }
        public static void Sewer_getFish_prefix()
        {
            _fishingStarted = HideFish(MutantCarpId);
        }
        public static void Mountain_getFish_prefix()
        {
            _fishingStarted = HideFish(LegendId);
        }

        public static void Town_getFish_prefix()
        {
            _fishingStarted = HideFish(AnglerId);
        }

        public static void Beach_getFish_prefix()
        {
            _fishingStarted = HideFish(CrimsonFishId);
        }


        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            int id = PufferChickID;
            if (LegendaryFish.ContainsKey(id)) return;
            LegendaryFish.Add(id, ModEntry.PufferChickName);

        }

        private static bool HideFish(int fishId)
        {
            Utils.RecacheMasterMail();
            _trackedFishId = -1;
            if (Game1.player.fishCaught.ContainsKey(fishId)
                && Game1.player.fishCaught.TryGetValue(fishId, out int[] freshValues)
                && !Utils.PlayerInventoryContains(fishId)) //if the player doesn't have the fish in their inventory
            {
                string fishname = LegendaryFish[fishId];

                if (Utils.IsUnDonatedFish(fishname))
                {
                    //save stats of fish and remove it from player's records
                    _trackedFishId = fishId;
                    _trackedFishStats = freshValues;
                    Game1.player.fishCaught.Remove(fishId);

                    _monitor.Log($"Hiding {LegendaryFish[fishId]} records to allow recatch");
                    return true; //don't run original game code
                }
            }

            return false;
        }

        private static void ReturnFish()
        {
            if (_trackedFishId >= 0)
            {
                _monitor.Log($"Returning fish to player records:{_trackedFishId}");
                Game1.player.fishCaught[_trackedFishId] = _trackedFishStats;
            }
        }

        
        private static void Player_InventoryChanged(object sender, StardewModdingAPI.Events.InventoryChangedEventArgs e)
        {
            if (!_fishingStarted) return;

            if (e.Added == null) return;

            _fishingStarted = false;

            foreach (Item i in e.Added)
            {
                if (!(i is Object obj)) return;

                if (_trackedFishId != obj.ParentSheetIndex)
                    return;
                Game1.drawObjectDialogue(_helper.Translation.Get("DuplicateLegendaryCaught"));
                obj.Price = 0;
            }

            _trackedFishId = -1;

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

            Utils.RecacheMasterMail();

            //base of 1% and an additional 0.5% per fish donated
            double pufferChance = 0.01 + 0.005 * Utils.GetNumDonatedFish();

            if (Game1.random.NextDouble() > pufferChance)
                return null;

            int id = PufferChickID;

            if (ModEntry.RecatchLegends 
                && who.fishCaught.ContainsKey(id) 
                && Game1.player.fishCaught.TryGetValue(id, out int[] freshValues) 
                && !Utils.PlayerInventoryContains(id)
                && Utils.IsUnDonatedFish("Pufferchick"))
            {
                _fishingStarted = true;
                _trackedFishId = id;
                return new Object(id, 1, price: 0);
            }

            if (who.fishCaught.ContainsKey(id)) return null;

            return new Object(id, 1);
        }
    }
}
