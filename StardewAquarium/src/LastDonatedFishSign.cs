using System.IO;
using System.Linq;
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
        private ModData _data;

        public static int LastDonatedFish { get; set; } = 435;
        private static NetStringList MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            string dataPath = Path.Combine("data", "data.json");
            _data = helper.Data.ReadJsonFile<ModData>(dataPath);

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
                return;

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
            var map = Game1.getLocationFromName(_data.ExteriorMapName)?.Map;

            if (map == null)
                return;

            Layer layer = map.GetLayer("Front");
            TileSheet tilesheet = map.GetTileSheet("springobjects");
            layer.Tiles[_data.LastDonatedFishCoordinateX, _data.LastDonatedFishCoordinateY] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: LastDonatedFish);
        }
    }
}