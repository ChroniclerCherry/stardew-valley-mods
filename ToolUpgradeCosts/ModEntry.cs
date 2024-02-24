using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Tools;
using ToolUpgradeCosts.Framework;

namespace ToolUpgradeCosts
{
	public class ModEntry : Mod
	{
		private static ModEntry _instance;

		private readonly Dictionary<UpgradeMaterials, int> _defaultMaterials = new Dictionary<UpgradeMaterials, int>
		{
			{UpgradeMaterials.Copper,334},
			{UpgradeMaterials.Steel, 335},
			{UpgradeMaterials.Gold, 336},
			{UpgradeMaterials.Iridium, 337}
		};

		private Config _config;

		public override void Entry(IModHelper helper)
		{
            _instance = this;
			_config = helper.ReadConfig<Config>();
            Helper.Events.GameLoop.SaveLoaded += GetIndexes;

            Harmony harmony = new Harmony(ModManifest.UniqueID);

			harmony.Patch(
                AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock)),
                postfix:new HarmonyMethod(typeof(ModEntry), nameof(ShopBuilder_GetShopStock_Postfix))
            );
		}

		private void GetIndexes(object sender, SaveLoadedEventArgs e)
		{
			foreach (KeyValuePair<UpgradeMaterials, Upgrade> upgrade in _config.UpgradeCosts)
			{
				string name = upgrade.Value.MaterialName;
				int index = Game1.objectInformation.FirstOrDefault(kvp => kvp.Value.Split('/')[0] == name).Key;
				if (index == 0)
				{
					Monitor.Log($"Object named \"{name}\" not found for the tool upgrade level of {upgrade.Key}. Vanilla upgrade item will be used", LogLevel.Error);
					index = _defaultMaterials[upgrade.Key];
				}
				upgrade.Value.MaterialIndex = index;
			}
		}

		public static void ShopBuilder_GetShopStock_Postfix(string shopId, ref Dictionary<ISalable, ItemStockInformation> __result)
		{
			if (shopId != Game1.shop_blacksmithUpgrades)
				return;

			try
			{
				Dictionary<ISalable, ItemStockInformation> editedStock = new Dictionary<ISalable, ItemStockInformation>();
				foreach ((ISalable item, ItemStockInformation stockInfo) in __result)
				{
					if (item is Tool tool && Enum.IsDefined(typeof(UpgradeMaterials), tool.UpgradeLevel))
					{
						UpgradeMaterials upgradeLevel = (UpgradeMaterials)tool.UpgradeLevel;
						if (tool is GenericTool)
						{
							upgradeLevel++;
						}
						editedStock[tool] = stockInfo with
						{
							Price = _instance._config.UpgradeCosts[upgradeLevel].Cost,
							TradeItem = _instance._config.UpgradeCosts[upgradeLevel].MaterialIndex,
							TradeItemCount = _instance._config.UpgradeCosts[upgradeLevel].MaterialStack
						};
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
}
