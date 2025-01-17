using System;
using ChangeSlimeHutchLimit.Framework;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ChangeSlimeHutchLimit;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private static ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        Config = helper.ReadConfig<ModConfig>();
        I18n.Init(helper.Translation);

        // add console commands
        helper.ConsoleCommands.Add("SetSlimeHutchLimit", "Changes the max number of slimes that can inhabit a slime hutch.\n\nUsage: SetSlimeHutchLimit <value>\n- value: the number of slimes", this.ChangeMaxSlimes);

        // add patches
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)), postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SlimeHutch_isFull_postfix)));

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
            get: () => ModEntry.Config,
            set: config => ModEntry.Config = config
        );
    }

    private static void SlimeHutch_isFull_postfix(GameLocation __instance, ref bool __result)
    {
        __result = __instance.characters.Count >= (Config?.MaxSlimesInHutch ?? 20);
    }

    private void ChangeMaxSlimes(string arg1, string[] arg2)
    {
        if (int.TryParse(arg2[0], out int newLimit))
        {
            Config.MaxSlimesInHutch = newLimit;
            this.Helper.WriteConfig(Config);
            this.Monitor.Log($"The new Slime limit is: {Config.MaxSlimesInHutch}");
        }
        else
        {
            this.Monitor.Log("Invalid input.", LogLevel.Error);
        }
    }
}
