using StardewAquarium.Models;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    class ReturnTrain
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ITrainStationAPI TSApi;
        public ReturnTrain(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            TSApi = _helper.ModRegistry.GetApi<ITrainStationAPI>("Cherry.TrainStation");
            if (TSApi == null)
            {
                _monitor.Log("The train station API was not found. Warps back to the Railroad will default to a map warp.", LogLevel.Warn);
                return;
            }

            _helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation?.Name != ModEntry.Data.ExteriorMapName)
                return;

            if (Game1.player.Position.Y > 32)
                return;

            Game1.player.position.Y += 32;
            TSApi.OpenTrainMenu();
        }
    }
}
