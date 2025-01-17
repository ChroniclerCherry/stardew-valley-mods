using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ShopTileFramework.Framework.Shop;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace ShopTileFramework.Framework.Patches;

internal class VanillaShopStockPatches
{
    /*********
    ** Public methods
    *********/
    public static void Apply(Harmony harmony)
    {
        harmony.Patch(
           original: AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock), new[] { typeof(string), typeof(ShopData) }),
           postfix: new HarmonyMethod(typeof(VanillaShopStockPatches), nameof(VanillaShopStockPatches.ShopBuilder_GetShopStock))
        );
    }

    public static void ShopBuilder_GetShopStock(string shopId, ShopData shop, ref Dictionary<ISalable, ItemStockInformation> __result)
    {
        // get STF shop ID
        string internalShopId = shopId switch
        {
            Game1.shop_adventurersGuild => "MarlonShop",
            Game1.shop_animalSupplies => "MarnieShop",
            Game1.shop_blacksmith => "ClintShop",
            Game1.shop_dwarf => "DwarfShop",
            Game1.shop_carpenter => "RobinShop",
            Game1.shop_desertTrader => "DesertTrader",
            Game1.shop_fish => "WillyShop",
            Game1.shop_generalStore => "PierreShop",
            Game1.shop_hatMouse => "HatMouse",
            Game1.shop_hospital => "HarveyShop",
            Game1.shop_jojaMart => "JojaShop",
            Game1.shop_krobus => "KrobusShop",
            Game1.shop_qiGemShop => "QiShop",
            Game1.shop_saloon => "GusShop",
            Game1.shop_sandy => "SandyShop",
            Game1.shop_travelingCart => "TravellingMerchant",

            _ => null
        };

        if (internalShopId != null)
            EditShopStock(internalShopId, ref __result);
    }


    /*********
    ** Private methods
    *********/
    private static void EditShopStock(string shopName, ref Dictionary<ISalable, ItemStockInformation> __result)
    {
        ModEntry.JustOpenedVanilla = true;

        if (!ShopManager.VanillaShops.ContainsKey(shopName)) return;

        var customStock = ShopManager.VanillaShops[shopName].ItemPriceAndStock;
        ItemsUtil.RemoveSoldOutItems(customStock);
        if (ShopManager.VanillaShops[shopName].ReplaceInsteadOfAdd)
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

            if (ShopManager.VanillaShops[shopName].AddStockAboveVanilla)
            {
                __result = customStock.Concat(__result).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                __result = __result.Concat(customStock).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}
