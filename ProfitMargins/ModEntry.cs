using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
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
    private ModConfig Config = null!; // set in Entry

    private float OriginalDifficulty;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.Saving += this.OnSaving;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForProfitMargins(),
            get: () => this.Config,
            set: config =>
            {
                this.Config = config;
                if (Context.IsWorldReady && this.CheckContext())
                    Game1.player.difficultyModifier = config.ProfitMargin;
            }
        );
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (this.CheckContext())
        {
            this.OriginalDifficulty = Game1.player.difficultyModifier;
            Game1.player.difficultyModifier = this.Config.ProfitMargin;
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.Saving" />
    private void OnSaving(object? sender, SavingEventArgs e)
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
