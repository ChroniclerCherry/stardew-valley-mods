using System;
using ChangeSlimeHutchLimit.Framework;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ChangeSlimeHutchLimit;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        this.Config = helper.ReadConfig<ModConfig>();
        I18n.Init(helper.Translation);
        GamePatcher.Apply(this.ModManifest.UniqueID, () => this.Config);

        // add console commands
        helper.ConsoleCommands.Add("SetSlimeHutchLimit", "Changes the max number of slimes that can inhabit a slime hutch.\n\nUsage: SetSlimeHutchLimit <value>\n- value: the number of slimes", this.ChangeMaxSlimes);

        // hook events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, EventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForChangeSlimeHutchLimit(),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    private void ChangeMaxSlimes(string arg1, string[] arg2)
    {
        if (int.TryParse(arg2[0], out int newLimit))
        {
            this.Config.MaxSlimesInHutch = newLimit;
            this.Helper.WriteConfig(this.Config);
            this.Monitor.Log($"The new Slime limit is: {this.Config.MaxSlimesInHutch}");
        }
        else
        {
            this.Monitor.Log("Invalid input.", LogLevel.Error);
        }
    }
}
