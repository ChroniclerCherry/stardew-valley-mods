using ProfitMargins.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ProfitMargins;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod configuration from the player.</summary>
    private ModConfig config;
    private float originalDifficulty;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.Saving += this.OnSaving;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        if (this.checkContext())
        {
            this.originalDifficulty = Game1.player.difficultyModifier;
            Game1.player.difficultyModifier = this.config.ProfitMargin;
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.Saving" />
    private void OnSaving(object sender, SavingEventArgs e)
    {
        if (this.checkContext())
        {
            Game1.player.difficultyModifier = this.originalDifficulty;
            this.Monitor.Log("During save, DL:" + Game1.player.difficultyModifier.ToString(), LogLevel.Debug);
        }
    }

    private bool checkContext()
    {
        if (!Context.IsMainPlayer)
        {
            return false;
        }
        else if (Context.IsMultiplayer && !this.config.EnableInMultiplayer)
        {
            return false;
        }
        return true;
    }
}
