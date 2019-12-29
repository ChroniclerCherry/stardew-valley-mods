using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework
{
    public interface IAPI
    {
        bool RegisterShops(string dir);
        bool OpenShop(string ShopName);
        bool ResetShop(string ShopName);
        Dictionary<ISalable, int[]> GetItemPriceAndStock(string ShopName);
    }
    class API : IAPI
    {

        public bool RegisterShops(string dir)
        {
            //registers a shops.json inside the given dir
            var temp = ModEntry.helper.ContentPacks.CreateFake(dir);
            ContentModel NewShopModel = temp.ReadJsonFile<ContentModel>("shops.json");

            if (NewShopModel == null)
            {
                return false;
            }

            ModEntry.RegisterShops(NewShopModel, temp);
            return true;
        }

        public bool OpenShop(string ShopName)
        {
            //opens up the shop of ShopName in-game
            ModEntry.Shops.TryGetValue(ShopName, out var shop);
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
            ModEntry.Shops.TryGetValue(ShopName, out var shop);
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
            ModEntry.Shops.TryGetValue(ShopName, out var shop);
            if (shop == null)
            {
                return null;
            }

            return shop.ItemPriceAndStock;
        }

    }
}
