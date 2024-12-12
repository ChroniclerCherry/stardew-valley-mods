using ShopTileFramework.Framework.Shop;

namespace ShopTileFramework.Framework.Data;

class ContentPack
{
    public string[] RemovePacksFromVanilla { get; set; }
    public string[] RemovePackRecipesFromVanilla { get; set; }

    public string[] RemoveItemsFromVanilla { get; set; }
    public ItemShop[] Shops { get; set; }
    public AnimalShop[] AnimalShops { get; set; }

    public VanillaShop[] VanillaShops { get; set; }
}
