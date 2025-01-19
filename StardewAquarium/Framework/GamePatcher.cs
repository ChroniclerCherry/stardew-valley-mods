using System;
using HarmonyLib;
using StardewAquarium.Framework.Menus;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewAquarium.Framework;

/// <summary>Applies Harmony patches to the game code for Android compatibility.</summary>
internal class GamePatcher
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private static IMonitor Monitor;

    /// <summary>Simplifies access to inaccessible code.</summary>
    private static IReflectionHelper Reflection;

    /// <summary>Provides translations stored in the mod's <c>i18n</c> folder.</summary>
    private static ITranslationHelper Translation;


    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="reflection">Simplifies access to inaccessible code.</param>
    /// <param name="translation">Provides translations stored in the mod's <c>i18n</c> folder.</param>
    public static void Apply(string modId, IMonitor monitor, IReflectionHelper reflection, ITranslationHelper translation)
    {
        Monitor = monitor;
        Reflection = reflection;
        Translation = translation;

        Harmony harmony = new(modId);
        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(tryToPurchaseItem_postfix))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(ShopMenu), "setCurrentItem"),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(setCurrentItem_postfix))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void setCurrentItem_postfix(ref ShopMenu __instance)
    {
        if (Game1.currentLocation?.Name != ContentPackHelper.InteriorLocationName || __instance is not DonateFishMenuAndroid) return;

        IReflectedField<string> nameItem = Reflection.GetField<string>(__instance, "nameItem");
        string nameItemString = nameItem.GetValue();
        nameItem.SetValue(Translation.Get("Donate") + nameItemString);

        Reflection.GetField<string>(__instance, "descItem").SetValue(ContentPackHelper.LoadString("DonateDescription"));
    }

    private static void tryToPurchaseItem_postfix(ref ShopMenu __instance, ref ISalable item)
    {
        if (Game1.currentLocation?.Name != ContentPackHelper.InteriorLocationName || __instance is not DonateFishMenuAndroid) return;
        try
        {
            if (!(item is Item donatedFish)) return; //this shouldn't happen but /shrug

            if (!Utils.DonateFish(donatedFish)) return; //this also shouldnt happen

            DonateFishMenuAndroid.Donated = true;
            Game1.player.removeItemFromInventory(donatedFish);

            if (donatedFish.QualifiedItemId == ContentPackHelper.PufferchickQualifiedId)
            {
                Game1.playSound("openChest");
                DonateFishMenuAndroid.PufferchickDonated = true;
            }
        }
        catch (Exception e)
        {
            Monitor.Log($"Failed in {nameof(tryToPurchaseItem_postfix)}: {e.Message} {e.StackTrace}", LogLevel.Error);
        }
    }
}
