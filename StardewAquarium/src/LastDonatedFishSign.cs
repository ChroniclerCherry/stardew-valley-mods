using System;
using System.Linq;
using Netcode;
using StardewAquarium.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
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
        public string LastDonatedFish { get; set; } = null;

        private const string objTilesheetName = "z_objects";
        private static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;

            this._helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this._helper.Events.Player.Warped += this.Player_Warped;
            this._helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;

            this._helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!MasterPlayerMail.Contains("AquariumCompleted"))
                return;

            var random = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
            int i = random.Next(0, Utils.FishIDs.Count - 1);
            this.LastDonatedFish = Utils.FishIDs[i];
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == "Cherry.StardewAquarium" && e.Type == "FishDonated")
            {
                this.LastDonatedFish = e.ReadAs<string>();
                this._monitor.Log($"The player {e.FromPlayerID} donated the fish of ID {this.LastDonatedFish}");
            }
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e?.NewLocation.Name == this._data.ExteriorMapName)
                this.UpdateLastDonatedFishSign();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.SetLastDonatedFish();
        }

        public void UpdateLastDonatedFish(Item i)
        {
            foreach (string flag in MasterPlayerMail.Where(flag => flag.StartsWith("AquariumLastDonated:")))
            {
                MasterPlayerMail.Remove(flag);
                break;
            }

            this._monitor.Log($"The last donated fish is {i.Name}");
            this.LastDonatedFish = i.ItemId;
            try
            {
                this._helper.Multiplayer.SendMessage(this.LastDonatedFish, "FishDonated",
                    modIDs: new[] { "Cherry.StardewAquarium" });
            }
            catch
            {
                this._monitor.Log("Something went wrong trying to sync data with other players. Not everyone may be able to see the sign outside the Aquarium updated with the current donation.");
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
                this.LastDonatedFish = null;
                return;
            }

            foreach (var kvp in Game1.objectData)
            {
                if (kvp.Value.Name != fish)
                    continue;

                this.LastDonatedFish = kvp.Key;
                break;
            }

            this._monitor.Log($"The last donated fish on this file is {this.LastDonatedFish}");
        }

        internal void UpdateLastDonatedFishSign()
        {
            if (this.LastDonatedFish is null)
                return;

            var map = Game1.getLocationFromName(this._data.ExteriorMapName)?.Map;

            if (map == null)
                return;

            Layer layer = map.GetLayer("Front");
            TileSheet objectsTilesheet = map.GetTileSheet(objTilesheetName);

            if (objectsTilesheet == null)
            {
                string tilesheetPath = PathUtilities.NormalizeAssetName("Maps/springobjects");
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

            layer.Tiles[this._data.LastDonatedFishCoordinateX, this._data.LastDonatedFishCoordinateY] = new StaticTile(layer, objectsTilesheet, BlendMode.Alpha, tileIndex: ItemRegistry.GetDataOrErrorItem(this.LastDonatedFish).SpriteIndex);
        }
    }
}
