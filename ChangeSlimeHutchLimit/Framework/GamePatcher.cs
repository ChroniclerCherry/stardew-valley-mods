using System;
using HarmonyLib;
using StardewValley;

namespace ChangeSlimeHutchLimit.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Get the mod config.</summary>
    private static Func<ModConfig> Config;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="config">Get the mod config.</param>
    public static void Apply(string modId, Func<ModConfig> config)
    {
        Config = config;

        Harmony harmony = new Harmony(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.isFull)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.SlimeHutch_isFull_postfix))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void SlimeHutch_isFull_postfix(GameLocation __instance, ref bool __result)
    {
        __result = __instance.characters.Count >= Config().MaxSlimesInHutch;
    }
}
