using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Tools;
using ToolUpgradeCosts.Framework;

namespace ToolUpgradeCosts;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private static ModEntry _instance;

    private readonly Dictionary<UpgradeMaterials, string> _defaultMaterials = new()
    {
        {UpgradeMaterials.Copper, "334"},
        {UpgradeMaterials.Steel, "335"},
        {UpgradeMaterials.Gold, "336"},
        {UpgradeMaterials.Iridium, "337"}
    };

    private ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        _instance = this;
        this.Config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

        Harmony harmony = new Harmony(this.ModManifest.UniqueID);

        harmony.Patch(
            AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock), new[] { typeof(string), typeof(ShopData) }),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ShopBuilder_GetShopStock_Postfix))
        );
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        foreach (KeyValuePair<UpgradeMaterials, Upgrade> upgrade in this.Config.UpgradeCosts)
        {
            string name = upgrade.Value.MaterialName;

            string id = Game1.objectData.FirstOrDefault(kvp => kvp.Value.Name == name).Key;
            if (id is null)
            {
                this.Monitor.Log($"Object named \"{name}\" not found for the tool upgrade level of {upgrade.Key}. Vanilla upgrade item will be used", LogLevel.Error);
                id = this._defaultMaterials[upgrade.Key];
            }
            upgrade.Value.MaterialId = id;
        }
    }

    private static void ShopBuilder_GetShopStock_Postfix(string shopId, ref Dictionary<ISalable, ItemStockInformation> __result)
    {
        if (shopId != Game1.shop_blacksmithUpgrades)
            return;

        try
        {
            Dictionary<ISalable, ItemStockInformation> editedStock = new Dictionary<ISalable, ItemStockInformation>();
            foreach ((ISalable item, ItemStockInformation stockInfo) in __result)
            {
                if (item is Tool tool && Enum.IsDefined(typeof(UpgradeMaterials), tool.UpgradeLevel) && stockInfo is not null)
                {
                    UpgradeMaterials upgradeLevel = (UpgradeMaterials)tool.UpgradeLevel;
                    if (tool is GenericTool)
                    {
                        upgradeLevel++;
                    }
                    editedStock[tool] = new ItemStockInformation(
                        price: _instance.Config.UpgradeCosts[upgradeLevel].Cost,
                        tradeItemCount: _instance.Config.UpgradeCosts[upgradeLevel].MaterialStack,
                        tradeItem: _instance.Config.UpgradeCosts[upgradeLevel].MaterialId,
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
            _instance.Monitor.Log($"Failed in {nameof(ShopBuilder_GetShopStock_Postfix)}:\n{ex}", LogLevel.Error);
        }
    }
}
