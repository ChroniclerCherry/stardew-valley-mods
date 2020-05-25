using Harmony;
using ShopTileFramework.Shop;
using ShopTileFramework.Utility;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

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

        public static void SeedShop_shopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("PierreShop"))
            {
                var customStock = ShopManager.VanillaShops["PierreShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["PierreShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                } else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getJojaStock(ref Dictionary<ISalable, int[]> __result)
        {
            if (ShopManager.VanillaShops.ContainsKey("JojaShop"))
            {
                var customStock = ShopManager.VanillaShops["JojaShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["JojaShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getCarpenterStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("RobinShop"))
            {
                var customStock = ShopManager.VanillaShops["RobinShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["RobinShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getBlacksmithStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("ClintShop"))
            {
                var customStock = ShopManager.VanillaShops["ClintShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["ClintShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getAdventureShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("MarlonShop"))
            {
                var customStock = ShopManager.VanillaShops["MarlonShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["MarlonShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getAnimalShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("MarnieShop"))
            {
                var customStock = ShopManager.VanillaShops["MarnieShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["MarnieShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getTravelingMerchantStock(ref Dictionary<ISalable, int[]> __result)
        {
            if (ShopManager.VanillaShops.ContainsKey("TravellingMerchant"))
            {
                var customStock = ShopManager.VanillaShops["TravellingMerchant"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["TravellingMerchant"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getHospitalStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("HarveyShop"))
            {
                var customStock = ShopManager.VanillaShops["HarveyShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["HarveyShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void GameLocation_sandyShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("SandyShop"))
            {
                var customStock = ShopManager.VanillaShops["SandyShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["SandyShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Desert_getDesertMerchantTradeStock(ref Dictionary<ISalable, int[]> __result)
        {
            if (ShopManager.VanillaShops.ContainsKey("DesertTrader"))
            {
                var customStock = ShopManager.VanillaShops["DesertTrader"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["DesertTrader"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Sewer_getShadowShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("KrobusShop"))
            {
                var customStock = ShopManager.VanillaShops["KrobusShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["KrobusShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getDwarfShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("DwarfShop"))
            {
                var customStock = ShopManager.VanillaShops["DwarfShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["DwarfShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
        public static void Utility_getSaloonStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("GusShop"))
            {
                var customStock = ShopManager.VanillaShops["GusShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["GusShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }

        public static void Utility_getFishShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.justOpenedVanilla = true;
            if (ShopManager.VanillaShops.ContainsKey("WillyShop"))
            {
                var customStock = ShopManager.VanillaShops["WillyShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["WillyShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }

        public static void Utility_getQiShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            if (ShopManager.VanillaShops.ContainsKey("QiShop"))
            {
                var customStock = ShopManager.VanillaShops["QiShop"].ItemPriceAndStock;
                ItemsUtil.RemoveSoldOutItems(customStock);
                if (ShopManager.VanillaShops["QiShop"].ReplaceInsteadOfAdd)
                {
                    __result = customStock;
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }

    }
}