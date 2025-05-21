using ShopTileFramework.Framework.Shop;

namespace ShopTileFramework.Framework.Apis;

/// <inheritdoc cref="IShopTileFrameworkApi" />
public class ShopTileFrameworkApi : IShopTileFrameworkApi
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public bool OpenItemShop(string shopName)
    {
        //opens up the shop of ShopName in-game
        ShopManager.ItemShops.TryGetValue(shopName, out ItemShop? shop);
        if (shop == null)
        {
            return false;
        }

        shop.DisplayShop();
        return true;
    }
}
