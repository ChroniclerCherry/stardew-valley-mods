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
    private IModHelper _helper;
    private IMonitor _monitor;
    private ITrainStationAPI TSApi;


    /*********
    ** Public methods
    *********/
    public ReturnTrain(IModHelper helper, IMonitor monitor)
    {
        this._helper = helper;
        this._monitor = monitor;

        this._helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.TSApi = this._helper.ModRegistry.GetApi<ITrainStationAPI>("Cherry.TrainStation");
        if (this.TSApi is null)
        {
            this._monitor.Log("The train station API was not found. Warps back to the Railroad will default to a map warp.", LogLevel.Warn);
            return;
        }

        this._helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
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
        this.TSApi.OpenTrainMenu();
    }
}
