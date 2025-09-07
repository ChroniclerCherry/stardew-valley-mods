using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace MultiYieldCrops.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging</summary>
    private static IMonitor Monitor = null!; // set in Apply

    /// <summary>Apply the custom rules when a crop is harvested.</summary>
    private static SpawnHarvestDelegate SpawnHarvest = null!; // set in Apply

    /// <inheritdoc cref="ModEntry.SpawnHarvest" />
    public delegate void SpawnHarvestDelegate(Vector2 tileLocation, string cropName, int fertilizer, JunimoHarvester? junimo = null);


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging</param>
    /// <param name="spawnHarvest">Apply the custom rules when a crop is harvested.</param>
    public static void Apply(string modId, IMonitor monitor, SpawnHarvestDelegate spawnHarvest)
    {
        Monitor = monitor;
        SpawnHarvest = spawnHarvest;

        Harmony harmony = new(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
            prefix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.Before_Crop_Harvest)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.After_Crop_Harvest))
        );
    }


    /*********
    ** Private methods
    *********/
    public static void Before_Crop_Harvest(Crop __instance, out bool __state)
    {
        try
        {
            __state = CanHarvest(__instance);
        }
        catch (Exception ex)
        {
            __state = false;
            Monitor.Log($"Failed in {nameof(Before_Crop_Harvest)} patch:\n{ex}", LogLevel.Error);
        }
    }

    public static void After_Crop_Harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester? junimoHarvester, Crop __instance, bool __state, ref bool __result)
    {
        try
        {
            // wasn't marked harvestable
            if (!__state)
                return;

            // The vanilla function will either return true (for single-yield crops) or reset the instance (for
            // multi-yield crops). If neither is true, the crop didn't get harvested (e.g. because the player's
            // inventory was full).
            if (!__result && CanHarvest(__instance))
                return;

            string cropId = __instance.indexOfHarvest.Value;
            string cropName = ItemRegistry.GetDataOrErrorItem(cropId).InternalName;
            int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();

            SpawnHarvest(new Vector2(xTile, yTile), cropName, fertilizerQualityLevel, junimoHarvester);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_Crop_Harvest)} patch:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Get whether a crop can be harvested now.</summary>
    /// <param name="crop">The crop to check.</param>
    private static bool CanHarvest(Crop crop)
    {
        return
            crop.currentPhase.Value >= crop.phaseDays.Count - 1
            && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);
    }
}
