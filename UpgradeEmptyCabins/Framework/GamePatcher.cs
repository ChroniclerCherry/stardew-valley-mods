using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace UpgradeEmptyCabins.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private static IMonitor Monitor;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public static void Apply(string modId, IMonitor monitor)
    {
        Monitor = monitor;

        Harmony harmony = new(modId);

        harmony.Patch(
            AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.CanModifyBed)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_BedFurniture_CanModifyBed))
        );

        harmony.Patch(
            AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.HasPermissionsToPaint)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_CarpenterMenu_HasPermissionToPaint))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void After_BedFurniture_CanModifyBed(BedFurniture __instance, Farmer who, ref bool __result)
    {
        try
        {
            if (__result || who is null)
                return;

            __result = who.currentLocation is FarmHouse { IsOwnerActivated: false }; // can move bed in any unowned cabin
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_BedFurniture_CanModifyBed)}:\n{ex}", LogLevel.Error);
        }
    }

    private static void After_CarpenterMenu_HasPermissionToPaint(Building b, ref bool __result)
    {
        try
        {
            if (__result)
                return;

            __result =
                b is { isCabin: true }
                && b.GetIndoors() is Cabin { IsOwnerActivated: false }; // can paint any unowned cabin
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_CarpenterMenu_HasPermissionToPaint)}:\n{ex}", LogLevel.Error);
        }
    }
}
