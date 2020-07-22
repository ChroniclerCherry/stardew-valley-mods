using System;
using System.Linq;
using Netcode;
using StardewAquarium.Models;
using StardewModdingAPI;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace StardewAquarium.TilesLogic
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
            if (!Utils.CheckAchievement())
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
                UpdateLastDonatedFishSign();
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
            _helper.Multiplayer.SendMessage(LastDonatedFish, "FishDonated", modIDs: new[] { "Cherry.StardewAquarium" });
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
                if (kvp.Value.Split('/')[0] != fish) continue;

                LastDonatedFish = kvp.Key;
                break;
            }

            _monitor.Log($"The last donated fish on this file is {LastDonatedFish}");
        }

        internal void UpdateLastDonatedFishSign()
        {
            if (LastDonatedFish < 0)
                return;

            var map = Game1.getLocationFromName(_data.ExteriorMapName)?.Map;

            if (map == null)
                return;

            Layer layer = map.GetLayer("Front");
            TileSheet objectsTilesheet = map.GetTileSheet(objTilesheetName);

            if (objectsTilesheet == null)
            {
                string tilesheetPath = _helper.Content.GetActualAssetKey(@"Maps/springobjects", ContentSource.GameContent);
                GameLocation location = Game1.getLocationFromName(ModEntry.Data.ExteriorMapName);

                // Add the tilesheet.
                objectsTilesheet = new TileSheet(
                   id: objTilesheetName, // a unique ID for the tilesheet
                   map: map,
                   imageSource: tilesheetPath,
                   sheetSize: new xTile.Dimensions.Size(24, 4000), // the tile size of your tilesheet image.
                   tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
                );

                map.AddTileSheet(objectsTilesheet);
                map.LoadTileSheets(Game1.mapDisplayDevice);
            }

            layer.Tiles[_data.LastDonatedFishCoordinateX, _data.LastDonatedFishCoordinateY] = new StaticTile(layer, objectsTilesheet, BlendMode.Alpha, tileIndex: LastDonatedFish);
        }
    }
}