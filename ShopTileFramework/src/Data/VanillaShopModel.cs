using ShopTileFramework.ItemPriceAndStock;

namespace ShopTileFramework.Data
{
    abstract class VanillaShopModel
    {
        public string ShopName { get; set; }
        public bool ReplaceInsteadOfAdd { get; set; } = false;
        public int ShopPrice { get; set; } = -1;
        public int MaxNumItemsSoldInStore { get; set; } = int.MaxValue;
        public ItemStock[] ItemStocks { get; set; }
    }
}
