using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace SnackAnything.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private static IMonitor Monitor;

    /// <summary>Get the mod config.</summary>
    private static Func<ModConfig> Config;


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
            original: AccessTools.Method(typeof(Game1), nameof(Game1.pressActionButton)),
            prefix: new HarmonyMethod(typeof(GamePatcher), nameof(Before_Game1_PressActionButton)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_Game1_PressActionButton))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
            prefix: new HarmonyMethod(typeof(GamePatcher), nameof(Before_Farmer_EatObject)),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_Farmer_EatObject))
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Mark the held object edible when pressing the action button, if applicable.</summary>
    /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
    private static void Before_Game1_PressActionButton(out int? __state)
    {
        try
        {
            if (Config().HoldToActivate.IsDown())
                TryReplaceEdibility(Game1.player.ActiveObject, out __state);
            else
                __state = null;
        }
        catch (Exception ex)
        {
            __state = null;
            Monitor.Log($"Failed in {nameof(Before_Game1_PressActionButton)} patch:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Restore the held object's original edibility after handling the action button, if applicable.</summary>
    /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
    private static void After_Game1_PressActionButton(int? __state)
    {
        try
        {
            if (Config().HoldToActivate.IsDown())
                TryRestoreEdibility(Game1.player.ActiveObject, __state);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_Game1_PressActionButton)} patch:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Mark the held object edible when it's eaten, if applicable.</summary>
    /// <param name="o">The item being eaten.</param>
    /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
    private static void Before_Farmer_EatObject(SObject o, out int? __state)
    {
        try
        {
            TryReplaceEdibility(o, out __state);
        }
        catch (Exception ex)
        {
            __state = null;
            Monitor.Log($"Failed in {nameof(Before_Farmer_EatObject)} patch:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Restore the held object's original edibility after the eat handler, and mark the item-to-eat edible if needed.</summary>
    /// <param name="o">The item being eaten.</param>
    /// <param name="__state">The previous object edibility, or <c>null</c> if it didn't change.</param>
    private static void After_Farmer_EatObject(SObject o, int? __state)
    {
        try
        {
            TryRestoreEdibility(o, __state);

            Game1.player.itemToEat = Game1.player.itemToEat?.getOne();
            TryReplaceEdibility(Game1.player.itemToEat as SObject, out _);
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_Farmer_EatObject)} patch:\n{ex}", LogLevel.Error);
        }
    }

    /// <summary>Make the item snackable if needed.</summary>
    /// <param name="obj">The object to change.</param>
    /// <param name="previousEdibility">The object's previous edibility, or <c>null</c> if it didn't change.</param>
    private static void TryReplaceEdibility(SObject obj, out int? previousEdibility)
    {
        if (obj?.Edibility < 0 && (obj.Type != "Arch" || Config().YummyArtefacts))
        {
            previousEdibility = obj.Edibility;
            obj.Edibility = Math.Max(obj.Price / 3, 1);
        }
        else
            previousEdibility = null;
    }

    /// <summary>Restore the object's original edibility if applicable.</summary>
    /// <param name="obj">The object to change.</param>
    /// <param name="originalEdibility">The object's previous edibility, or <c>null</c> it wasn't changed.</param>
    private static void TryRestoreEdibility(SObject obj, int? originalEdibility)
    {
        if (obj != null && originalEdibility != null)
            obj.Edibility = originalEdibility.Value;
    }
}
