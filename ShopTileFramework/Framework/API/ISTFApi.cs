using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    /// <summary>
    /// Interface for Shop Tile Framework
    /// </summary>
    public interface ISTFApi
    {
        bool RegisterShops(string dir);
        bool OpenItemShop(string shopName);
        bool ResetShopStock(string shopName);
        Dictionary<ISalable, int[]> GetItemPriceAndStock(string shopName);
    }
}
