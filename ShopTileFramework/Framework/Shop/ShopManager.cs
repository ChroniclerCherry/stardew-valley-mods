using System;
using System.Collections.Generic;
using System.Linq;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;

namespace ShopTileFramework.Framework.Shop;

/// <summary>
/// This class holds and manages all the shops, loading content packs to create shops
/// And containing methods to update everything that needs to
/// </summary>
internal class ShopManager
{
    /*********
    ** Fields
    *********/
    private static readonly string[] VanillaShopNames = {
        "PierreShop",
        "JojaShop",
        "RobinShop",
        "ClintShop",
        "MarlonShop",
        "MarnieShop",
        "TravellingMerchant",
        "HarveyShop",
        "SandyShop",
        "DesertTrader",
        "KrobusShop",
        "DwarfShop",
        "GusShop",
        "QiShop",
        "WillyShop",
        "HatMouse"
    };


    /*********
    ** Accessors
    *********/
    public static Dictionary<string, ItemShop> ItemShops = new Dictionary<string, ItemShop>();
    public static Dictionary<string, AnimalShop> AnimalShops = new Dictionary<string, AnimalShop>();
    public static Dictionary<string, VanillaShop> VanillaShops = new Dictionary<string, VanillaShop>();


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Takes content packs and loads them as ItemShop and AnimalShop objects
    /// </summary>
    public static void LoadContentPacks()
    {
        ModEntry.StaticMonitor.Log("Adding Content Packs...", LogLevel.Info);
        foreach (IContentPack contentPack in ModEntry.StaticHelper.ContentPacks.GetOwned())
        {
            if (!contentPack.HasFile("shops.json"))
            {
                ModEntry.StaticMonitor.Log($"No shops.json found from the mod {contentPack.Manifest.UniqueID}. " +
                    $"Skipping pack.", LogLevel.Warn);
                continue;
            }

            ContentPack data;
            try
            {
                data = contentPack.ReadJsonFile<ContentPack>("shops.json");
            }
            catch (Exception ex)
            {
                ModEntry.StaticMonitor.Log($"Invalid JSON provided by {contentPack.Manifest.UniqueID}.", LogLevel.Error);
                ModEntry.StaticMonitor.Log(ex.Message + ex.StackTrace, LogLevel.Error);
                continue;
            }

            ModEntry.StaticMonitor.Log($"Loading: {contentPack.Manifest.Name} by {contentPack.Manifest.Author} | " +
                $"{contentPack.Manifest.Version} | {contentPack.Manifest.Description}", LogLevel.Info);

            RegisterShops(data, contentPack);
        }
    }

    /// <summary>
    /// Update all trans;ations for each shop when a save file is loaded
    /// </summary>
    public static void UpdateTranslations()
    {
        foreach (ItemShop itemShop in ItemShops.Values)
        {
            itemShop.UpdateTranslations();
        }

        foreach (AnimalShop animalShop in AnimalShops.Values)
        {
            animalShop.UpdateTranslations();
        }
    }

    /// <summary>
    /// Initializes all shops once the game is loaded
    /// </summary>
    public static void InitializeShops()
    {
        foreach (ItemShop itemShop in ItemShops.Values)
        {
            itemShop.Initialize();
        }
    }

    /// <summary>
    /// Initializes the stocks of each shop after the save file has loaded so that item IDs are available to generate items
    /// </summary>
    public static void InitializeItemStocks()
    {
        foreach (ItemShop itemShop in ItemShops.Values)
        {
            itemShop.StockManager.Initialize();
        }

        foreach (ItemPriceAndStockManager manager in VanillaShops.Values.SelectMany(vanillaShop => vanillaShop.StockManagers))
        {
            manager.Initialize();
        }
    }

    /// <summary>
    /// Updates the stock for all itemshops at the start of each day
    /// and updates their portraits too to match the current season
    /// </summary>
    internal static void UpdateStock()
    {
        if (ItemShops.Count > 0)
            ModEntry.StaticMonitor.Log("Refreshing stock for all custom shops...", LogLevel.Debug);

        foreach (ItemShop store in ItemShops.Values)
        {
            store.UpdateItemPriceAndStock();
            store.UpdatePortrait();
        }

        if (VanillaShops.Count > 0)
            ModEntry.StaticMonitor.Log("Refreshing stock for all Vanilla shops...", LogLevel.Debug);

        foreach (VanillaShop shop in VanillaShops.Values)
        {
            shop.UpdateItemPriceAndStock();
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Saves each shop as long as it has a unique name.</summary>
    public static void RegisterShops(ContentPack data, IContentPack contentPack)
    {
        ItemsUtil.RegisterPacksToRemove(data.RemovePacksFromVanilla, data.RemovePackRecipesFromVanilla, data.RemoveItemsFromVanilla);

        if (data.Shops != null)
        {
            foreach (ItemShop shopPack in data.Shops)
            {
                if (ItemShops.ContainsKey(shopPack.ShopName))
                {
                    ModEntry.StaticMonitor.Log($"{contentPack.Manifest.Name} is trying to add a Shop \"{shopPack.ShopName}\"," +
                        $" but a shop of this name has already been added. " +
                        $"It will not be added.", LogLevel.Warn);
                    continue;
                }
                shopPack.ContentPack = contentPack;
                ItemShops.Add(shopPack.ShopName, shopPack);
            }
        }

        if (data.AnimalShops != null)
        {
            foreach (AnimalShop animalShopPack in data.AnimalShops)
            {
                if (AnimalShops.ContainsKey(animalShopPack.ShopName))
                {
                    ModEntry.StaticMonitor.Log($"{contentPack.Manifest.Name} is trying to add an AnimalShop \"{animalShopPack.ShopName}\"," +
                        $" but a shop of this name has already been added. " +
                        $"It will not be added.", LogLevel.Warn);
                    continue;
                }
                AnimalShops.Add(animalShopPack.ShopName, animalShopPack);
            }
        }

        if (data.VanillaShops != null)
        {
            foreach (VanillaShop vanillaShopPack in data.VanillaShops)
            {
                if (!VanillaShopNames.Contains(vanillaShopPack.ShopName))
                {
                    ModEntry.StaticMonitor.Log($"{contentPack.Manifest.Name}" +
                        $" is trying to edit nonexistent vanilla store" +
                        $" \"{vanillaShopPack.ShopName}\"", LogLevel.Warn);
                    continue;
                }

                if (VanillaShops.ContainsKey(vanillaShopPack.ShopName))
                {
                    VanillaShops[vanillaShopPack.ShopName].StockManagers.Add(new ItemPriceAndStockManager(vanillaShopPack));

                    if (vanillaShopPack.ReplaceInsteadOfAdd)
                        VanillaShops[vanillaShopPack.ShopName].ReplaceInsteadOfAdd = true;

                    if (vanillaShopPack.AddStockAboveVanilla)
                        VanillaShops[vanillaShopPack.ShopName].AddStockAboveVanilla = true;

                }
                else
                {
                    vanillaShopPack.Initialize();
                    vanillaShopPack.StockManagers.Add(new ItemPriceAndStockManager(vanillaShopPack));
                    VanillaShops.Add(vanillaShopPack.ShopName, vanillaShopPack);
                }
            }
        }
    }
}
