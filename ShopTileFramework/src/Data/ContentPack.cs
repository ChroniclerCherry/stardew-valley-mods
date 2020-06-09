using ShopTileFramework.Shop;
using System.Collections.Generic;

namespace ShopTileFramework.Data
{
    class ContentPack
    {
        public string[] RemovePacksFromVanilla { get; set; }
        public string[] RemovePackRecipesFromVanilla { get; set; }
        public ItemShop[] Shops { get; set; }
        public AnimalShop[] AnimalShops { get; set; }

        public VanillaShop[] VanillaShops { get; set; }
    }
}
