using System;
using ChangeSlimeHutchLimit.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace ChangeSlimeHutchLimit;

public class ModEntry : Mod
{
    internal static Config Config { get; set; }
    internal static IModHelper ModHelper { get; private set; }

    public override void Entry(IModHelper helper)
    {
        // store references for use in other classes
        Config = this.Helper.ReadConfig<Config>();
        ModHelper = this.Helper;

        // add console commands
        this.Helper.ConsoleCommands.Add("SetSlimeHutchLimit", "Changes the max number of slimes that can inhabit a slime hutch.\n\nUsage: SetSlimeHutchLimit <value>\n- value: the number of slimes", this.ChangeMaxSlimes);

        // set up patches
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)), postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SlimeHutch_isFull_postfix)));

        // Delay GMCM setup until all mods are initialized
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object sender, EventArgs e)
    {
        GenericModConfigMenuIntegrationForChangeSlimeHutchLimit.Setup(this.ModManifest);
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
