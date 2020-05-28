using ShopTileFramework.ItemPriceAndStock;
using System.Collections.Generic;

namespace ShopTileFramework.Data
{
    abstract class VanillaShopModel
    {
        public string ShopName { get; set; }
        public bool ReplaceInsteadOfAdd { get; set; } = false;
        public bool AddStockAboveVanilla { get; set; } = false;
        public int ShopPrice { get; set; } = -1;
        public int MaxNumItemsSoldInStore { get; set; } = int.MaxValue;
        public double DefaultSellPriceMultipler { get; set; } = 1;
        public Dictionary<double, string[]> PriceMultiplierWhen { get; set; } = null;
        public ItemStock[] ItemStocks { get; set; }
    }
}
