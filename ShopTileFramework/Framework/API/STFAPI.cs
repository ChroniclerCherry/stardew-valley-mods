using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Shop;
using StardewValley;
using System;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
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
            ContentPack newShopModel = temp.ReadJsonFile<ContentPack>("shops.json");

            if (newShopModel == null)
            {
                return false;
            }

            ShopManager.RegisterShops(newShopModel, temp);
            return true;
        }

        /// <summary>
        /// Opens the item shop of a given name
        /// </summary>
        /// <param name="shopName">The name of the shop</param>
        /// <returns></returns>
        public bool OpenItemShop(string shopName)
        {
            //opens up the shop of ShopName in-game
            ShopManager.ItemShops.TryGetValue(shopName, out var shop);
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
        /// <param name="shopName">The name of the shop</param>
        /// <returns>true if the shop was successfully found, false if not</returns>
        public bool OpenAnimalShop(string shopName)
        {
            //opens up the shop of ShopName in-game
            ShopManager.AnimalShops.TryGetValue(shopName, out var shop);
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
        /// <param name="shopName">The name of the item shop</param>
        /// <returns>true if the shop was found and reset, false if not</returns>
        public bool ResetShopStock(string shopName)
        {
            //resets the stock of the given ShopName
            ShopManager.ItemShops.TryGetValue(shopName, out var shop);
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
        /// <param name="shopName">The name of the shop</param>
        /// <returns>The ItemPriceAndStock of the given store if found, null otherwise</returns>
        public Dictionary<ISalable, int[]> GetItemPriceAndStock(string shopName)
        {
            //gets the ItemStockAndPrice of the given ShopName
            ShopManager.ItemShops.TryGetValue(shopName, out var shop);
            if (shop == null)
            {
                return null;
            }

            return shop.StockManager.ItemPriceAndStock;
        }

    }
}
