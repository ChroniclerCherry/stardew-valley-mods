using System.Collections.Generic;

namespace ShopTileFramework
{
    class ContentModel
    {
        public ShopPack[] Shops { get; set; }
    }

    class ShopPack
    {
        public string ShopName { get; set; }
        public string StoreCurrency { get; set; } = "Money";
        public List<int> CategoriesToSellHere { get; set; } = null;
        public string PortraitPath { get; set; } = null;
        public string Quote { get; set; } = null;
        public int ShopPrice { get; set; } = -1;
        public int MaxNumItemsSoldInStore { get; set; } = int.MaxValue;
        public ItemStock[] ItemStocks { get; set; }
    }

    class ItemStock
    {
        public string ItemType { get; set; }
        public int StockPrice { get; set; } = -1;
        public string StockItemCurrency { get; set; } = "Money";
        public int StockCurrencyStack { get; set; } = 1;
        public int[] ItemIDs { get; set; } = null;
        public string[] JAPacks { get; set; } = null;
        public string[] ItemNames { get; set; } = null;
        public int Stock { get; set; } = int.MaxValue;
        public int MaxNumItemsSoldInItemStock { get; set; } = int.MaxValue;
        public string[] When { get; set; } = null;
    }
}
