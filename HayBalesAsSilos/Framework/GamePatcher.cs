using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace HayBalesAsSilos.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal class GamePatcher
{
    /*********
    ** Public methods
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private static IMonitor Monitor;

    /// <summary>Get the mod config.</summary>
    private static Func<ModConfig> Config;

    /// <summary>Get the number of hay bales currently placed in a given location.</summary>
    private static Func<GameLocation, int> CountHayBalesIn;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="config">Get the mod config.</param>
    /// <param name="countHayBalesIn">Get the number of hay bales currently placed in a given location.</param>
    public static void Apply(string modId, IMonitor monitor, Func<ModConfig> config, Func<GameLocation, int> countHayBalesIn)
    {
        Monitor = monitor;
        Config = config;
        CountHayBalesIn = countHayBalesIn;

        Harmony harmony = new(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.GetHayCapacity)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_GameLocation_GetHayCapacity))
        );
    }


    /*********
    ** Private methods
    *********/
    public static void After_GameLocation_GetHayCapacity(ref GameLocation __instance, ref int __result)
    {
        try
        {
            ModConfig config = Config();

            if (__result > 0 || !config.RequiresConstructedSilo)
                __result += CountHayBalesIn(__instance) * config.HayPerBale;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_GameLocation_GetHayCapacity)} patch:\n{ex}", LogLevel.Error);
        }
    }
}
