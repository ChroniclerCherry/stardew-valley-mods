using System;
using Harmony;
using StardewAquarium.Menus;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Patches
{
    class AndroidShopMenuPatch
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;

        private static int PufferChickID { get => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1; }

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {

            _helper = helper;
            _monitor = monitor;

            HarmonyInstance harmony = ModEntry.harmony;
            harmony.Patch(original: AccessTools.Method(typeof(DonateFishMenuAndroid), "tryToPurchaseItem"),
                postfix: new HarmonyMethod(typeof(AndroidShopMenuPatch),nameof(tryToPurchaseItem_postfix))
            );
        }

        private static void tryToPurchaseItem_postfix(ref DonateFishMenuAndroid __instance, ref ISalable item)
        {
            try
            {
                if (!(item is Item donatedFish)) return; //this shouldn't happen but /shrug

                if (!Utils.DonateFish(donatedFish)) return; //this also shouldnt happen

                DonateFishMenuAndroid.Donated = true;
                Game1.player.removeItemsFromInventory(donatedFish.ParentSheetIndex,1);

                if (donatedFish.ParentSheetIndex == PufferChickID)
                {
                    Game1.playSound("openChest");
                    DonateFishMenuAndroid.PufferchickDonated = true;
                }

                DonateFishMenuAndroid.AchievementUnlock = Utils.CheckAchievement();

            }
            catch (Exception e)
            {
                _monitor.Log($"Failed in {nameof(tryToPurchaseItem_postfix)}: {e.Message} {e.StackTrace}",LogLevel.Error);
            }
            
        }
    }
}
