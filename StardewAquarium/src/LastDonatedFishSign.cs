using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Netcode;
using StardewAquarium.Models;
using StardewModdingAPI;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace StardewAquarium
{
    public class LastDonatedFishSign
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ModData _data { get => ModEntry.Data; }
        public int LastDonatedFish { get; set; } = -1;

        private const string objTilesheetName = "z_objects";
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.Player.Warped += Player_Warped;
            _helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

            _helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!MasterPlayerMail.Contains("AquariumCompleted"))
                return;

            var random = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
            int i = random.Next(0, Utils.FishIDs.Count - 1);
            LastDonatedFish = Utils.FishIDs[i];
        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == "Cherry.StardewAquarium" && e.Type == "FishDonated")
            {
                LastDonatedFish = e.ReadAs<int>();
                _monitor.Log($"The player {e.FromPlayerID} donated the fish of ID {LastDonatedFish}");
            }
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e?.NewLocation.Name == _data.ExteriorMapName)
                UpdateLastDonatedFishSign(e.NewLocation);
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            SetLastDonatedFish();
        }

        public void UpdateLastDonatedFish(Item i)
        {
            foreach (var flag in MasterPlayerMail.Where(flag => flag.StartsWith("AquariumLastDonated:")))
            {
                MasterPlayerMail.Remove(flag);
                break;
            }

            _monitor.Log($"The last donated fish is {i.Name}");
            LastDonatedFish = i.ParentSheetIndex;
            try
            {
                _helper.Multiplayer.SendMessage(LastDonatedFish, "FishDonated",
                    modIDs: new[] {"Cherry.StardewAquarium"});
            }
            catch
            {
                _monitor.Log("Something went wrong trying to sync data with other players. Not everyone may be able to see the sign outside the Aquarium updated with the current donation.");
            }

            MasterPlayerMail.Add($"AquariumLastDonated:{i.Name}");
        }

        public void SetLastDonatedFish()
        {
            string fish = MasterPlayerMail
                .Where(flag => flag
                    .StartsWith("AquariumLastDonated:"))
                .Select(flag => flag.Split(':')[1])
                .FirstOrDefault();

            if (fish == null)
            {
                LastDonatedFish = -1;
                return;
            }
                

            foreach (var kvp in Game1.objectInformation)
            {
                if (kvp.Value.GetNthChunk('/', 0).Equals(fish, StringComparison.Ordinal))
                {
                    LastDonatedFish = kvp.Key;
                    break;
                }
            }

            _monitor.Log($"The last donated fish on this file is {LastDonatedFish}");
        }

        internal void UpdateLastDonatedFishSign(GameLocation loc)
        {
            if (LastDonatedFish < 0)
                return;

            var position = new Vector2(_data.LastDonatedFishCoordinateX * 64f, _data.LastDonatedFishCoordinateY * 64f);
            var rect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.LastDonatedFish, 16, 16);

            var tas = new TemporaryAnimatedSprite
            {
                texture = Game1.objectSpriteSheet,
                sourceRect = rect,
                animationLength = 1,
                sourceRectStartingPos = new(rect.X, rect.Y),
                interval = 50000f,
                totalNumberOfLoops = 9999,
                position = position,
                scale = 4f,
                layerDepth = (((position.Y) * Game1.tileSize) / 10000f) + 0.01f, // a little offset so it doesn't show up on the floor.
                id = 777f,
            };

            loc.temporarySprites.Add(tas);
        }
    }
}