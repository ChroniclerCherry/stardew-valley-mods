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
    private static IMonitor Monitor = null!; // set in Apply


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

        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.dumpContents)),
            prefix: new HarmonyMethod(typeof(GamePatcher), nameof(Before_Chest_DumpContents)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_Chest_DumpContents))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void After_BedFurniture_CanModifyBed(Farmer? who, ref bool __result)
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

    private static void Before_Chest_DumpContents(Chest __instance, out bool __state)
    {
        __state = false; // whether we changed the gift box

        try
        {
            if (__instance.giftbox.Value && __instance.giftboxIsStarterGift.Value && __instance.Location is Cabin { IsOwnerActivated: false })
            {
                // let player open box, and disable starting quest
                __instance.giftboxIsStarterGift.Value = false;
                __state = true;
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(Before_Chest_DumpContents)} patch:\n{ex}", LogLevel.Error);
        }
    }

    private static void After_Chest_DumpContents(Chest __instance, bool __state)
    {
        try
        {
            if (__state)
                __instance.giftboxIsStarterGift.Value = true;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_Chest_DumpContents)} patch:\n{ex}", LogLevel.Error);
        }
    }
}
