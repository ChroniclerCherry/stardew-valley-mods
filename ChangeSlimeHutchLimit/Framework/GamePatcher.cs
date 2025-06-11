using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace ChangeSlimeHutchLimit.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private static IMonitor Monitor = null!; // set in Apply

    /// <summary>Get the mod config.</summary>
    private static Func<ModConfig> Config = null!; // set in Apply


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="config">Get the mod config.</param>
    public static void Apply(string modId, IMonitor monitor, Func<ModConfig> config)
    {
        Monitor = monitor;
        Config = config;

        Harmony harmony = new(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.After_SlimeHutch_IsFull))
        );

        harmony.Patch(
            AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.SlimeHutch_DayUpdate_Postfix))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void After_SlimeHutch_IsFull(GameLocation __instance, ref bool __result)
    {
        try
        {
            __result = __instance.characters.Count >= Config().MaxSlimesInHutch;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_SlimeHutch_IsFull)} patch:\n{ex}", LogLevel.Error);
        }
    }

    private static void SlimeHutch_DayUpdate_Postfix(SlimeHutch __instance, int dayOfMonth)
    {

        // Check if the slime ball override is enabled
        if (!Config().EnableSlimeBallOverride) return;

        int numberSlimes = __instance.characters.Count;

        // Ensure there are enough slimes for slime balls to spawn
        if (numberSlimes < 20) return;

        // Calculate the number of usable water spots
        int waters = 0;
        int startIndex = Game1.random.Next(__instance.waterSpots.Length);
        int totalWaters = __instance.waterSpots.Length;

        for (int i = 0; i < __instance.waterSpots.Length; i++)
        {
            // Check if the current water spot is watered and meets the slime-to-water ratio
            if (__instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] && waters * 5 < numberSlimes)
            {
                waters++; // Increment the count of usable water spots
                __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false; // Mark the water spot as used
            }
        }

        // Calculate slime-based limit, factoring in watered spots
        int slimeBasedLimit = waters > 0
            ? ((((numberSlimes - 20) / totalWaters) * waters) / 5)
            : 0;

        // Respect the user-defined maximum daily limit, if set
        int maxBallsToPlace = Config().MaxDailySlimeBalls > 0
            ? Math.Min(slimeBasedLimit, (Config().MaxDailySlimeBalls))
            : slimeBasedLimit;

        // Spawn slime balls
        float mapHeight = (float)(Game1.currentLocation.map.Layers[0].LayerHeight - 2);
        for (int i = maxBallsToPlace; i > 0; i--)
        {
            int attemptsLeft = 50;
            Vector2 randomTile = __instance.getRandomTile();
            while ((!__instance.CanItemBePlacedHere(randomTile, itemIsPassable: false, CollisionMask.All, CollisionMask.None) ||
                    __instance.doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "NPCBarrier", "Back") != null ||
                    randomTile.Y >= mapHeight) && attemptsLeft > 0)
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
    }
}
