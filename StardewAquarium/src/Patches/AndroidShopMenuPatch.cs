using System;
using HarmonyLib;
using StardewAquarium.Menus;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewAquarium.Patches
{
    class AndroidShopMenuPatch
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static string? PufferChickID => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName);

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {

            _helper = helper;
            _monitor = monitor;

            Harmony harmony = ModEntry.Harmony;
            harmony.Patch(original: AccessTools.Method(typeof(ShopMenu), "tryToPurchaseItem"),
                postfix: new HarmonyMethod(typeof(AndroidShopMenuPatch), nameof(tryToPurchaseItem_postfix))
            );

            harmony.Patch(original: AccessTools.Method(typeof(ShopMenu), "setCurrentItem"),
                postfix: new HarmonyMethod(typeof(AndroidShopMenuPatch), nameof(setCurrentItem_postfix))
            );
        }

        private static void setCurrentItem_postfix(ref ShopMenu __instance)
        {
            if (Game1.currentLocation?.Name != "FishMuseum" || __instance is not DonateFishMenuAndroid) return;

            IReflectedField<string> nameItem = _helper.Reflection.GetField<string>(__instance, "nameItem");
            string nameItemString = nameItem.GetValue();
            nameItem.SetValue(_helper.Translation.Get("Donate") + nameItemString);

            _helper.Reflection.GetField<string>(__instance, "descItem").SetValue(_helper.Translation.Get("DonateDescription"));
        }

        private static void tryToPurchaseItem_postfix(ref ShopMenu __instance, ref ISalable item)
        {
            if (Game1.currentLocation?.Name != "FishMuseum" || __instance is not DonateFishMenuAndroid) return;
            try
            {
                if (!(item is Item donatedFish)) return; //this shouldn't happen but /shrug

                if (!Utils.DonateFish(donatedFish)) return; //this also shouldnt happen

                DonateFishMenuAndroid.Donated = true;
                Game1.player.removeItemFromInventory(donatedFish);

                if (donatedFish.ItemId == PufferChickID)
                {
                    Game1.playSound("openChest");
                    DonateFishMenuAndroid.PufferchickDonated = true;
                }

            }
            catch (Exception e)
            {
                _monitor.Log($"Failed in {nameof(tryToPurchaseItem_postfix)}: {e.Message} {e.StackTrace}", LogLevel.Error);
            }
        }
    }
}
