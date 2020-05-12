using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    public interface ISTFApi
    {
        bool RegisterShops(string dir);
        bool OpenShop(string ShopName);
        bool ResetShop(string ShopName);
        Dictionary<ISalable, int[]> GetItemPriceAndStock(string ShopName);
    }
}
