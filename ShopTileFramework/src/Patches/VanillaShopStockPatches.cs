using Harmony;
using ShopTileFramework.Shop;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace ShopTileFramework.Patches
{
    public class VanillaShopStockPatches
    {
        public static void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
               original: AccessTools.Method(typeof(SeedShop), nameof(SeedShop.shopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.SeedShop_shopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getJojaStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getJojaStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getCarpenterStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getCarpenterStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getBlacksmithStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getBlacksmithStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getAdventureShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getAdventureShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getAnimalShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getAnimalShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getAnimalShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getAnimalShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getTravelingMerchantStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getTravelingMerchantStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getHospitalStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getHospitalStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "sandyShopStock"),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.GameLocation_sandyShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Desert), nameof(Desert.getDesertMerchantTradeStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Desert_getDesertMerchantTradeStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Sewer), nameof(Sewer.getShadowShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Sewer_getShadowShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getDwarfShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getDwarfShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getSaloonStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getSaloonStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getFishShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getFishShopStock))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getQiShopStock)),
               postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.Utility_getQiShopStock))
            );
        }

        public static void SeedShop_shopStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getJojaStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getCarpenterStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getBlacksmithStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getAdventureShopStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getAnimalShopStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getTravelingMerchantStock(Dictionary<ISalable, int[]> __result)
        {
            //TODO: only add once a day
        }
        public static void Utility_getHospitalStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void GameLocation_sandyShopStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Desert_getDesertMerchantTradeStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Sewer_getShadowShopStock(Dictionary<ISalable, int[]> __result)
        {
            //TODO: only add once a day
        }
        public static void Utility_getDwarfShopStock(Dictionary<ISalable, int[]> __result)
        {

        }
        public static void Utility_getSaloonStock(Dictionary<ISalable, int[]> __result)
        {

        }

        public static void Utility_getFishShopStock(Dictionary<ISalable, int[]> __result)
        {

        }

        public static void Utility_getQiShopStock(Dictionary<ISalable, int[]> __result)
        {

        }

    }
}
