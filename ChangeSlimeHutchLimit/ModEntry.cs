using ChangeSlimeHutchLimit.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using Object = StardewValley.Object;

namespace ChangeSlimeHutchLimit;

public class ModEntry : Mod
{
    internal static Config Config { get; set; }
    internal static IModHelper ModHelper { get; private set; }

    public override void Entry(IModHelper helper)
    {
        // Store references to Config and ModHelper for use in other classes
        Config = this.Helper.ReadConfig<Config>();
        ModHelper = this.Helper;

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
        GMCMHandler.Setup(this.ModManifest);
    }

    private static void SlimeHutch_isFull_postfix(GameLocation __instance, ref bool __result)
    {
        __result = __instance.characters.Count >= (Config?.MaxSlimesInHutch ?? 20);
    }

    private static bool SlimeHutch_DayUpdate_Prefix(SlimeHutch __instance, int dayOfMonth)
    {
        if (!Config.EnableSlimeBallOverride) return true; // Skip custom logic if override is disabled

        // Count the number of water spots that are watered
        int total_wateredSpots = __instance.waterSpots.Count();
        int wateredSpots = __instance.waterSpots.Count(watered => watered);

        // Calculate slime-based limit, factoring in watered spots
        int slimeBasedLimit = (((__instance.characters.Count / total_wateredSpots) * wateredSpots) / 5);

        // Respect the user-defined maximum daily limit, if set
        int maxBallsToPlace = Config.MaxDailySlimeBalls > 0
            ? Math.Min(slimeBasedLimit, Config.MaxDailySlimeBalls)
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
                slimeBall.Fragility = 2;
                __instance.objects.Add(randomTile, slimeBall);
            }
        }

        return true;
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
