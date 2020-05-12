using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Shop;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    public class STFApi : ISTFApi
    {

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

        public bool OpenShop(string ShopName)
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

        public bool ResetShop(string ShopName)
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
