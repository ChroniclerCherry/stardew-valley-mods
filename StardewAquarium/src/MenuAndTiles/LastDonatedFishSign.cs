using System.IO;
using System.Linq;
using Netcode;
using StardewAquarium.Models;
using StardewModdingAPI;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace StardewAquarium.MenuAndTiles
{
    public class LastDonatedFishSign
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ModData _data { get => ModEntry.data; }
        public int LastDonatedFish { get; set; } = -1;

        private const string objTilesheetName = "z_objects";
        private TileSheet objectsTilesheet;
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.Player.Warped += Player_Warped;
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e?.NewLocation.Name == _data.ExteriorMapName)
                UpdateLastDonatedFishSign();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // This gets the asset key for a tilesheet.png file from your mod's folder. You can also load a game tilesheet like
            // this: helper.Content.GetActualAssetKey("spring_town", ContentSource.GameContent).
            string tilesheetPath = _helper.Content.GetActualAssetKey(@"Maps/springobjects", ContentSource.GameContent);

            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm() instead.
            GameLocation location = Game1.getLocationFromName(ModEntry.data.ExteriorMapName);

            // Add the tilesheet.
            objectsTilesheet = new TileSheet(
               id: objTilesheetName, // a unique ID for the tilesheet
               map: location.map,
               imageSource: tilesheetPath,
               sheetSize: new xTile.Dimensions.Size(24, 4000), // the tile size of your tilesheet image.
               tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
            );

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

            if (map.GetTileSheet(objTilesheetName) == null)
            {
                map.AddTileSheet(objectsTilesheet);
                map.LoadTileSheets(Game1.mapDisplayDevice);
            }  

            layer.Tiles[_data.LastDonatedFishCoordinateX, _data.LastDonatedFishCoordinateY] = new StaticTile(layer, objectsTilesheet, BlendMode.Alpha, tileIndex: LastDonatedFish);
        }
    }
}