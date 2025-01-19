namespace ShopTileFramework.Framework.Apis;

/// <summary>The interface for the Shop Tile Framework mod API.</summary>
public interface IShopTileFrameworkApi
{
    /// <summary>Open an item shop.</summary>
    /// <param name="shopName">The name of the shop.</param>
    /// <returns>Returns whether the shop UI was successfully opened.</returns>
    bool OpenItemShop(string shopName);
}
