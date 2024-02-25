using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Apis
{
    /// <summary>
    /// Interface for Shop Tile Framework
    /// </summary>
    public interface IShopTileFrameworkApi
    {
        bool RegisterShops(string dir);
        bool OpenItemShop(string shopName);
        bool ResetShopStock(string shopName);
        Dictionary<ISalable, ItemStockInformation> GetItemPriceAndStock(string shopName);
    }
}
