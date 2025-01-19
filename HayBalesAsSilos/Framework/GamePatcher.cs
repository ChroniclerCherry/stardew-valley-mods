using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewValley;

namespace HayBalesAsSilos.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal class GamePatcher
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get the mod config.</summary>
    private static Func<ModConfig> Config;

    /// <summary>Get the locations whose hay storage should be affected.</summary>
    private static Func<IEnumerable<GameLocation>> GetAllAffectedMaps;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="config">Get the mod config.</param>
    /// <param name="getAllAffectedMaps">Get the locations whose hay storage should be affected.</param>
    public static void Apply(string modId, Func<ModConfig> config, Func<IEnumerable<GameLocation>> getAllAffectedMaps)
    {
        Config = config;
        GetAllAffectedMaps = getAllAffectedMaps;

        Harmony harmony = new(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.GetHayCapacity)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_GetHayCapacity))
        );
    }


    /*********
    ** Private methods
    *********/
    public static void After_GetHayCapacity(ref GameLocation __instance, ref int __result)
    {
        if (!GetAllAffectedMaps().Contains(Game1.currentLocation))
            return;

        ModConfig config = Config();

        if (__result > 0 || !config.RequiresConstructedSilo)
        {
            int hayBales = __instance.Objects.Values.Count(p => p.QualifiedItemId == ModEntry.HayBaleQualifiedId);
            if (hayBales == 0)
                return;

            __result += hayBales * config.HayPerBale;
        }
    }
}
