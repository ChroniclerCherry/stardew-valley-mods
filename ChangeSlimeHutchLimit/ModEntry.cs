using ChangeSlimeHutchLimit.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using GenericModConfigMenu;
using System;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;
using System.Linq;

namespace ChangeSlimeHutchLimit;

public class ModEntry : Mod
{
    private static Config _config;

    public override void Entry(IModHelper helper)
    {
        // Load the configuration
        _config = this.Helper.ReadConfig<Config>();

        // Add console commands
        this.Helper.ConsoleCommands.Add("SetSlimeHutchLimit",
            "Changes the max number of slimes that can inhabit a slime hutch.\n\nUsage: SetSlimeHutchLimit <value>\n- value: the number of slimes",
            this.ChangeMaxSlimes);

        // Set up Harmony patches
        Harmony harmony = new Harmony(this.ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(SlimeHutch_isFull_postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(SlimeHutch_DayUpdate_Prefix))
        );

        // Delay GMCM setup until all mods are initialized
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object sender, EventArgs e)
    {
        this.SetupGMCM();
    }

    private void SetupGMCM()
    {
        var api = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api == null) return;

        // Register the mod for GMCM
        api.Register(
            mod: this.ModManifest,
            reset: () => _config = new Config(), // Reset to default values
            save: () => this.Helper.WriteConfig(_config) // Save to file
        );

        // Add sections and options to the GMCM menu
        api.AddSectionTitle(
            mod: this.ModManifest,
            text: () => "Slime Hutch Settings"
        );

        api.AddNumberOption(
            mod: this.ModManifest,
            name: () => "Max # of Slimes in Hutch",
            tooltip: () => "Set the maximum number of slimes allowed in the slime hutch.",
            getValue: () => _config.MaxSlimesInHutch,
            setValue: value => _config.MaxSlimesInHutch = value,
            min: 20,
            interval: 5
        );

        api.AddSectionTitle(
            mod: this.ModManifest,
            text: () => "Slime Ball Settings"
        );

        api.AddBoolOption(
            mod: this.ModManifest,
            name: () => "Enable Slime Ball Override",
            tooltip: () => "Enable or disable the override for slime ball placement.",
            getValue: () => _config.EnableSlimeBallOverride,
            setValue: value => _config.EnableSlimeBallOverride = value
        );

        api.AddNumberOption(
            mod: this.ModManifest,
            name: () => "Max Daily Slime Balls",
            tooltip: () => "Set the maximum number of slime balls that can spawn daily (set to 0 for unlimited).",
            getValue: () => _config.MaxDailySlimeBalls,
            setValue: value => _config.MaxDailySlimeBalls = value,
            min: 0,
            max: 100,
            interval: 1
        );
    }

    private static void SlimeHutch_isFull_postfix(GameLocation __instance, ref bool __result)
    {
        __result = __instance.characters.Count >= (_config?.MaxSlimesInHutch ?? 20);
    }

    private static bool SlimeHutch_DayUpdate_Prefix(SlimeHutch __instance, int dayOfMonth)
    {
        if (!_config.EnableSlimeBallOverride) return true; // Skip custom logic if override is disabled

        // Count the number of water spots that are watered
        int wateredSpots = __instance.waterSpots.Count(watered => watered);

        // Calculate slime-based limit, factoring in watered spots
        int slimeBasedLimit = (((__instance.characters.Count / 4) * wateredSpots) / 5);

        // Respect the user-defined maximum daily limit, if set
        int maxBallsToPlace = _config.MaxDailySlimeBalls > 0
            ? Math.Min(slimeBasedLimit, _config.MaxDailySlimeBalls)
            : slimeBasedLimit;

        for (int i = maxBallsToPlace; i > 0; i--)
        {
            int attemptsLeft = 50;
            Vector2 randomTile = __instance.getRandomTile();
            while ((!__instance.CanItemBePlacedHere(randomTile, itemIsPassable: false, CollisionMask.All, CollisionMask.None) ||
                    __instance.doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "NPCBarrier", "Back") != null ||
                    randomTile.Y >= 12f) && attemptsLeft > 0)
            {
                randomTile = __instance.getRandomTile();
                attemptsLeft--;
            }

            if (attemptsLeft > 0)
            {
                Object slimeBall = ItemRegistry.Create<Object>("(BC)56"); // Slime Ball ID
#pragma warning disable AvoidNetField // Avoid Netcode types when possible
                slimeBall.fragility.Value = 2;
#pragma warning restore AvoidNetField // Avoid Netcode types when possible
                __instance.objects.Add(randomTile, slimeBall);
            }
        }

        return true;
    }

    private void ChangeMaxSlimes(string arg1, string[] arg2)
    {
        if (int.TryParse(arg2[0], out int newLimit))
        {
            _config.MaxSlimesInHutch = newLimit;
            this.Helper.WriteConfig(_config);
            this.Monitor.Log($"The new Slime limit is: {_config.MaxSlimesInHutch}");
        }
        else
        {
            this.Monitor.Log("Invalid input.", LogLevel.Error);
        }
    }
}
