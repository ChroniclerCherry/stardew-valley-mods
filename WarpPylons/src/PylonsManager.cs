using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace WarpPylons
{
    static class PylonsManager
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;

        public static List<PylonData> Pylons { get; set; }

        public static readonly string SaveKey = "Pylons";
        public static readonly string WarpPylonName = "Warp Pylon";
        public static void  Initialize(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;

            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;


            if (Context.IsMainPlayer)
            {
                helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
                helper.Events.GameLoop.Saving += GameLoop_Saving;
            }
                
        }

        private static void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            _helper.Data.WriteSaveData<List<PylonData>>(SaveKey, Pylons);
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Pylons = _helper.Data.ReadSaveData<List<PylonData>>(SaveKey);

            Utils.IterateAllPylons(RegisterPylons);
        }

        private static void RegisterPylons(GameLocation loc, KeyValuePair<Vector2, Object> pylonCoordinates)
        {
            _monitor.Log($"Found {pylonCoordinates.Value.Name} in the map {loc.Name} at coordinates {pylonCoordinates.Key}");
        }

        private static void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (!e.FromModID.Equals("Cherry.WarpPylons") || e.Type != "PylonChanged") return;

            _monitor.Log($"Player {e.FromPlayerID} changed the pylons list");
            Pylons = e.ReadAs<List<PylonData>>();
        }
    }
}
