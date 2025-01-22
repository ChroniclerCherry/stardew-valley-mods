using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;

namespace ToolUpgradeCosts.Framework;

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
            AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock), new[] { typeof(string), typeof(ShopData) }),
            postfix: new HarmonyMethod(typeof(GamePatcher), nameof(After_ShopBuilder_GetShopStock))
        );
    }


    /*********
    ** Private methods
    *********/
    private static void After_ShopBuilder_GetShopStock(string shopId, ref Dictionary<ISalable, ItemStockInformation> __result)
    {
        try
        {
            if (shopId != Game1.shop_blacksmithUpgrades)
                return;

            var config = Config();

            Dictionary<ISalable, ItemStockInformation> editedStock = new Dictionary<ISalable, ItemStockInformation>();
            foreach ((ISalable item, ItemStockInformation stockInfo) in __result)
            {
                if (item is Tool tool && Enum.IsDefined(typeof(UpgradeMaterials), tool.UpgradeLevel) && stockInfo is not null)
                {
                    UpgradeMaterials upgradeLevel = (UpgradeMaterials)tool.UpgradeLevel;

                    editedStock[tool] = new ItemStockInformation(
                        price: config.UpgradeCosts[upgradeLevel].Cost,
                        tradeItemCount: config.UpgradeCosts[upgradeLevel].MaterialStack,
                        tradeItem: config.UpgradeCosts[upgradeLevel].MaterialId,
                        stock: stockInfo.Stock,
                        stockMode: stockInfo.LimitedStockMode,
                        itemToSyncStack: stockInfo.ItemToSyncStack,
                        stackDrawType: stockInfo.StackDrawType,
                        actionsOnPurchase: stockInfo.ActionsOnPurchase
                    );
                }
                else
                {
                    editedStock[item] = stockInfo;
                }
            }
            __result = editedStock;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed in {nameof(After_ShopBuilder_GetShopStock)}:\n{ex}", LogLevel.Error);
        }
    }
}
