using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewAquarium
{
    class LegendaryRecatch
    {

        private Dictionary<int, string> LegendaryFish = new Dictionary<int, string>()
        {
            {159,"CrimsonFish"},{160,"Angler"},{163,"Legend"},{682,"MutantCarp"},{775,"GlacierFish"}
        };

        private Dictionary<int,int[]> UndonatedLegends = new Dictionary<int, int[]>();

        private bool currentlyFishing = false;

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        public LegendaryRecatch(IModHelper helper, IMonitor monitor)
        {
            _monitor = monitor;
            _helper = helper;

            _helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            _helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        }

        private void Player_InventoryChanged(object sender, StardewModdingAPI.Events.InventoryChangedEventArgs e)
        {
            if (UndonatedLegends?.Count == 0 || e.Added?.Count() <= 0)
                return;

            Object item = e.Added.First() as Object;
            if (item == null)
            {
                UndonatedLegends.Clear();
                return;
            }

            int index = item.ParentSheetIndex;
            if (LegendaryFish.ContainsKey(index) && UndonatedLegends.ContainsKey(index))
            {
                Game1.drawObjectDialogue(_helper.Translation.Get("DuplicateLegendaryCaught"));
                item.Price = 0;
            }
            UndonatedLegends.Clear();
        }

        private void hideFish()
        {
            UndonatedLegends.Clear();
            currentlyFishing = true;

            foreach (int fishId in LegendaryFish.Keys)
            {
                if (Game1.player.fishCaught.ContainsKey(fishId) && Game1.player.fishCaught.TryGetValue(fishId, out int[] freshValues))
                {
                    string fishname = LegendaryFish[fishId];
                    if (Utils.IsUnDonatedFish(fishname))
                    {
                        UndonatedLegends.Add(fishId,freshValues);
                        Game1.player.fishCaught.Remove(fishId);
                    }
                }
            }

            if (UndonatedLegends.Count > 0)
                _monitor.Log($"Allowing recatch of legendaries:{UndonatedLegends.Keys}");
        }

        private void returnFish()
        {
            if (UndonatedLegends.Count > 0)
                _monitor.Log($"Returning fish IDs to player records:{UndonatedLegends.Keys}");
            foreach (var fishID in UndonatedLegends.Keys)
            {
                int[] stashedData = UndonatedLegends[fishID];
                if (Game1.player.fishCaught.TryGetValue(fishID, out int[] newValues))
                {
                    int newCount = newValues[0];
                    int newSize = newValues[1];

                    stashedData[0] += newCount;
                    stashedData[1] = Math.Max(stashedData[1], newSize);
                }

                Game1.player.fishCaught[fishID] = stashedData;
                Game1.stats.checkForFishingAchievements();
            }

            currentlyFishing = false;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            bool isFishing = Game1.player.UsingTool && Game1.player.CurrentTool is FishingRod rod;
            if (isFishing)
            {
                if (!currentlyFishing)
                    hideFish();
            }
            else if (currentlyFishing)
            {
                returnFish();
            }

        }
    }
}
