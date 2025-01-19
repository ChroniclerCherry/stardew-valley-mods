using StardewAquarium.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewAquarium.Framework;

internal class ReturnTrain
{
    /*********
    ** Fields
    *********/
    private readonly IModHelper Helper;
    private readonly IMonitor Monitor;
    private ITrainStationApi TrainStationApi;


    /*********
    ** Public methods
    *********/
    public ReturnTrain(IModHelper helper, IMonitor monitor)
    {
        this.Helper = helper;
        this.Monitor = monitor;

        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.TrainStationApi = this.Helper.ModRegistry.GetApi<ITrainStationApi>("Cherry.TrainStation");
        if (this.TrainStationApi is null)
        {
            this.Monitor.Log("The train station API was not found. Warps back to the Railroad will default to a map warp.", LogLevel.Warn);
            return;
        }

        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicked" />
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (Game1.player.Position.Y > 32)
            return;

        if (Game1.currentLocation?.Name != ContentPackHelper.ExteriorLocationName)
            return;

        Game1.player.position.Y += 32;
        this.TrainStationApi.OpenTrainMenu();
    }
}
