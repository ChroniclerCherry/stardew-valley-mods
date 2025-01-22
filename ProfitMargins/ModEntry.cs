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
    /// <summary>The mod settings.</summary>
    private ModConfig Config;

    private float OriginalDifficulty;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.Saving += this.OnSaving;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        if (this.CheckContext())
        {
            this.OriginalDifficulty = Game1.player.difficultyModifier;
            Game1.player.difficultyModifier = this.Config.ProfitMargin;
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.Saving" />
    private void OnSaving(object sender, SavingEventArgs e)
    {
        if (this.CheckContext())
        {
            Game1.player.difficultyModifier = this.OriginalDifficulty;
            this.Monitor.Log("During save, DL:" + Game1.player.difficultyModifier, LogLevel.Debug);
        }
    }

    private bool CheckContext()
    {
        return
            Context.IsMainPlayer
            && (!Context.IsMultiplayer || this.Config.EnableInMultiplayer);
    }
}
