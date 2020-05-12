using ShopTileFramework.Framework.Data;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Shop
{
    class ShopManager
    {

        public static Dictionary<string, ItemShop> ItemShops = new Dictionary<string, ItemShop>();
        public static Dictionary<string, AnimalShop> AnimalShops = new Dictionary<string, AnimalShop>();
        public static void LoadContentPacks()
        {
            ModEntry.monitor.Log("Adding Content Packs...", LogLevel.Info);
            foreach (IContentPack contentPack in ModEntry.helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("shops.json"))
                {
                    ModEntry.monitor.Log($"No shops.json found from the mod {contentPack.Manifest.UniqueID}. " +
                        $"Skipping pack.", LogLevel.Warn);
                }
                else
                {
                    ContentPack data;
                    try
                    {
                        data = contentPack.ReadJsonFile<ContentPack>("shops.json");
                    }
                    catch (Exception ex)
                    {
                        ModEntry.monitor.Log($"Invalid JSON provided by {contentPack.Manifest.UniqueID}. Skipping pack. (More details in Trace)", LogLevel.Error);
                        ModEntry.monitor.Log(ex.Message + ex.StackTrace);
                        continue;
                    }

                    ModEntry.monitor.Log($"      {contentPack.Manifest.Name} by {contentPack.Manifest.Author} | " +
                        $"{contentPack.Manifest.Version} | {contentPack.Manifest.Description}", LogLevel.Info);

                    RegisterShops(data, contentPack);
                }
            }
        }
        public static void RegisterShops(ContentPack data, IContentPack contentPack)
        {
            if (data.Shops != null)
            {
                foreach (ItemShop shopPack in data.Shops)
                {
                    if (ItemShops.ContainsKey(shopPack.ShopName))
                    {
                        ModEntry.monitor.Log($"A mod is trying to add a Shop \"{shopPack.ShopName}\"," +
                            $" but a shop of this name has already been added. " +
                            $"It will not be added.", LogLevel.Warn);
                    }
                    else
                    {
                        shopPack.ContentPack = contentPack;
                        ItemShops.Add(shopPack.ShopName, shopPack);
                    }
                }
            }

            if (data.AnimalShops != null)
            {
                foreach (AnimalShop animalShopPack in data.AnimalShops)
                {
                    if (AnimalShops.ContainsKey(animalShopPack.ShopName))
                    {
                        ModEntry.monitor.Log($"A mod is trying to add an AnimalShop \"{animalShopPack.ShopName}\"," +
                            $" but a shop of this name has already been added. " +
                            $"It will not be added.", LogLevel.Warn);
                    }
                    else
                    {
                        AnimalShops.Add(animalShopPack.ShopName, animalShopPack);
                    }
                }
            }
                
        }

        public static void InitializeShops()
        {
            foreach (ItemShop itemShop in ItemShops.Values)
            {
                itemShop.Initialize();
            }

            foreach (AnimalShop animalShop in AnimalShops.Values)
            {
                animalShop.Initialize();
            }
        }

        internal static void UpdateStock()
        {
            ModEntry.monitor.Log($"Refreshing stock for all custom shops...", LogLevel.Debug);
            foreach (ItemShop Store in ItemShops.Values)
            {
                Store.UpdateItemPriceAndStock();
                Store.UpdatePortrait();
            }
        }

    }
}
