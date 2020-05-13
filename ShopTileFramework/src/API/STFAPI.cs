using ShopTileFramework.Data;
using ShopTileFramework.Shop;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ShopTileFramework.API
{
    /// <summary>
    /// API for Shop Tile Framework. 
    /// </summary>
    [Obsolete("This class is WIP and not tested yet.")]
    public class STFApi : ISTFApi
    {
        /// <summary>
        /// Registers a mod directory as a content pack
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool RegisterShops(string dir)
        {
            //registers a shops.json inside the given dir
            var temp = ModEntry.helper.ContentPacks.CreateFake(dir);
            ContentPack NewShopModel = temp.ReadJsonFile<ContentPack>("shops.json");

            if (NewShopModel == null)
            {
                return false;
            }

            ShopManager.RegisterShops(NewShopModel, temp);
            return true;
        }

        /// <summary>
        /// Opens the item shop of a given name
        /// </summary>
        /// <param name="ShopName">The name of the shop</param>
        /// <returns></returns>
        public bool OpenItemShop(string ShopName)
        {
            //opens up the shop of ShopName in-game
            ShopManager.ItemShops.TryGetValue(ShopName, out var shop);
            if (shop == null)
            {
                return false;
            }

            shop.DisplayShop();
            return true;
        }

        /// <summary>
        /// Opens the animal shop of a given name
        /// </summary>
        /// <param name="ShopName">The name of the shop</param>
        /// <returns>true if the shop was successfully found, false if not</returns>
        public bool OpenAnimalShop(string ShopName)
        {
            //opens up the shop of ShopName in-game
            ShopManager.AnimalShops.TryGetValue(ShopName, out var shop);
            if (shop == null)
            {
                return false;
            }

            shop.DisplayShop();
            return true;
        }

        /// <summary>
        /// Resets the stock of the shop, checking all conditions and randomizations as if it's a new day
        /// </summary>
        /// <param name="ShopName">The name of the item shop</param>
        /// <returns>true if the shop was found and reset, false if not</returns>
        public bool ResetShopStock(string ShopName)
        {
            //resets the stock of the given ShopName
            ShopManager.ItemShops.TryGetValue(ShopName, out var shop);
            if (shop == null)
            {
                return false;
            }

            shop.UpdateItemPriceAndStock();
            return true;
        }

        /// <summary>
        /// Rettrieves the ItemPriceAndStock of the shop of a given name
        /// </summary>
        /// <param name="ShopName">The name of the shop</param>
        /// <returns>The ItemPriceAndStock of the given store if found, null otherwise</returns>
        public Dictionary<ISalable, int[]> GetItemPriceAndStock(string ShopName)
        {
            //gets the ItemStockAndPrice of the given ShopName
            ShopManager.ItemShops.TryGetValue(ShopName, out var shop);
            if (shop == null)
            {
                return null;
            }

            return shop.StockManager.ItemPriceAndStock;
        }

    }
}
