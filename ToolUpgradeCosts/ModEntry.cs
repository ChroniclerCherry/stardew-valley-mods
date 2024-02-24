using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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
                AccessTools.Method(typeof(Utility), "priceForToolUpgradeLevel"), 
                new HarmonyMethod(typeof(ModEntry), nameof(Utility_priceForToolUpgradeLevel_prefix))
                );
			harmony.Patch(
                AccessTools.Method(typeof(Utility), "indexOfExtraMaterialForToolUpgrade"), 
                new HarmonyMethod(typeof(ModEntry), nameof(Utility_indexOfExtraMaterialForToolUpgrade_prefix))
                );
			harmony.Patch(
                AccessTools.Method(typeof(Utility), "getBlacksmithUpgradeStock"),
                postfix:new HarmonyMethod(typeof(ModEntry), nameof(Utility_getBlacksmithUpgradeStock_postfix))
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

		public static bool Utility_priceForToolUpgradeLevel_prefix(int level, ref int __result)
		{
			try
			{
				if (Enum.IsDefined(typeof(UpgradeMaterials), level))
				{
					__result = _instance._config.UpgradeCosts[(UpgradeMaterials)level].Cost;
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				_instance.Monitor.Log($"Failed in {nameof(Utility_priceForToolUpgradeLevel_prefix)}:\n{ex}", (LogLevel)4);
				return true;
			}
		}

		public static bool Utility_indexOfExtraMaterialForToolUpgrade_prefix(int level, ref int __result)
		{
			try
			{
				if (Enum.IsDefined(typeof(UpgradeMaterials), level))
				{
					__result = _instance._config.UpgradeCosts[(UpgradeMaterials)level].MaterialIndex;
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
                _instance.Monitor.Log($"Failed in {nameof(Utility_indexOfExtraMaterialForToolUpgrade_prefix)}:\n{ex}", (LogLevel)4);
				return true;
			}
		}

		public static void Utility_getBlacksmithUpgradeStock_postfix(ref Dictionary<ISalable, int[]> __result)
		{
			try
			{
				Dictionary<ISalable, int[]> editedStock = new Dictionary<ISalable, int[]>();
				foreach (KeyValuePair<ISalable, int[]> kvp in __result)
				{
					ISalable key = kvp.Key;
					Tool tool = key as Tool;
					if (tool != null && Enum.IsDefined(typeof(UpgradeMaterials), tool.UpgradeLevel))
					{
						int upgradeLvl = tool.UpgradeLevel;
						List<int> stockAndPrice = kvp.Value.ToList();
						if (tool is GenericTool)
						{
							upgradeLvl++;
						}
						stockAndPrice.Add(_instance._config.UpgradeCosts[(UpgradeMaterials)upgradeLvl].MaterialStack);
						editedStock.Add(kvp.Key, stockAndPrice.ToArray());
					}
					else
					{
						editedStock.Add(kvp);
					}
				}
				__result = editedStock;
			}
			catch (Exception ex)
			{
                _instance.Monitor.Log($"Failed in {nameof(Utility_indexOfExtraMaterialForToolUpgrade_prefix)}:\n{ex}", (LogLevel)4);
			}
		}
    }
}
