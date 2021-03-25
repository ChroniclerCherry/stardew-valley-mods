using Harmony;
using ShopTileFramework.Shop;
using ShopTileFramework.Utility;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

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

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getHatStock)),
                postfix: new HarmonyMethod(typeof(VanillaShopStockPatches),
                    nameof(VanillaShopStockPatches.Utility_getHatStock))
            );
        }

        private static void EditShopStock(string shopName, ref Dictionary<ISalable, int[]> __result)
        {
            ModEntry.JustOpenedVanilla = true;

            Dictionary<string, VanillaShop> vanillaShops = ModEntry.helper.Content.Load<Dictionary<string, VanillaShop>>("Mods/ShopTileFramework/VanillaShops", ContentSource.GameContent);
            if (!vanillaShops.ContainsKey(shopName)) return;

            var customStock = vanillaShops[shopName].ItemPriceAndStock;
            ItemsUtil.RemoveSoldOutItems(customStock);
            if (vanillaShops[shopName].ReplaceInsteadOfAdd)
            {
                __result = customStock;
            }
            else
            {
                foreach (var key in customStock.Keys)
                {
                    if (__result.ContainsKey(key))
                        return;
                }

                if (vanillaShops[shopName].AddStockAboveVanilla)
                {
                    __result = customStock.Concat(__result).ToDictionary(x => x.Key, x => x.Value);
                }
                else
                {
                    __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }

        public static void SeedShop_shopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("PierreShop", ref __result);
        }
        public static void Utility_getJojaStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("JojaShop", ref __result);
        }
        public static void Utility_getCarpenterStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("RobinShop", ref __result);
        }
        public static void Utility_getBlacksmithStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("ClintShop", ref __result);
        }
        public static void Utility_getAdventureShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("MarlonShop", ref __result);
        }
        public static void Utility_getAnimalShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("MarnieShop", ref __result);
        }
        public static void Utility_getTravelingMerchantStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("TravellingMerchant", ref __result);
        }
        public static void Utility_getHospitalStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("HarveyShop", ref __result);
        }
        public static void GameLocation_sandyShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("SandyShop", ref __result);
        }
        public static void Desert_getDesertMerchantTradeStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("DesertTrader", ref __result);
        }
        public static void Sewer_getShadowShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("KrobusShop", ref __result);
        }
        public static void Utility_getDwarfShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("DwarfShop", ref __result);
        }
        public static void Utility_getSaloonStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("GusShop", ref __result);
        }

        public static void Utility_getFishShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("WillyShop", ref __result);
        }

        public static void Utility_getQiShopStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("QiShop", ref __result);
        }
        public static void Utility_getHatStock(ref Dictionary<ISalable, int[]> __result)
        {
            EditShopStock("HatMouse", ref __result);
        }
    }
}