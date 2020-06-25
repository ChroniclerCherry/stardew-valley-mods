using System.Linq;
using Netcode;
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
        private ModEntry.ModData _data;

        public static int LastDonatedFish { get; set; } = -1;
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor, ModEntry.ModData data)
        {
            _helper = helper;
            _monitor = monitor;
            _data = data;

            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            SetLastDonatedFish();
            var map = Game1.getLocationFromName(_data.ExteriorMapName).Map;
            string tilesheetPath = _helper.Content.GetActualAssetKey("Maps/springobjects", ContentSource.GameContent);

            TileSheet tilesheet = new TileSheet(
                "z_springobjects", // a unique ID for the tilesheet
                map,
                tilesheetPath,
                new xTile.Dimensions.Size(24, 4096), // the tile size of your tilesheet image.
                new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
            );
            map.AddTileSheet(tilesheet);
            map.LoadTileSheets(Game1.mapDisplayDevice);

            UpdateLastDonatedFishSign();
        }

        public void UpdateLastDonatedFish(Item i)
        {
            foreach (var flag in MasterPlayerMail.Where(flag => flag.StartsWith("AquariumLastDonated:")))
            {
                MasterPlayerMail.Remove(flag);
                break;
            }

            LastDonatedFish = i.ParentSheetIndex;
            UpdateLastDonatedFishSign();
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
                return;

            foreach (var kvp in Game1.objectInformation)
            {
                if (kvp.Value.Split('/')[0] != fish) continue;

                LastDonatedFish = kvp.Key;
                break;

            }
        }

        internal void UpdateLastDonatedFishSign()
        {
            if (LastDonatedFish == -1) return;

            var map = Game1.getLocationFromName(_data.ExteriorMapName).Map;
            Layer layer = map.GetLayer("Front");
            TileSheet tilesheet = map.GetTileSheet("z_springobjects");
            layer.Tiles[_data.LastDonatedFishCoordinateX, _data.LastDonatedFishCoordinateY] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: LastDonatedFish);
        }

                /*
        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //ExteriorMuseum
            if (Game1.currentLocation.Name != data.ExteriorMapName)
                return;

            if (Utils.LastDonatedFish == null)
                return;

            var loc = Game1.GlobalToLocal(new Vector2(data.LastDonatedFishCoordinateX, data.LastDonatedFishCoordinateY));


            Utils.LastDonatedFish.drawInMenu(e.SpriteBatch, loc,1);
        } */
    }
}