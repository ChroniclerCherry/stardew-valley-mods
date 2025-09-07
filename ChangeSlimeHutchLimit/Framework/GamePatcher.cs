using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

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
}
