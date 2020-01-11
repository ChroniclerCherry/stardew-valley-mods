using System.Collections.Generic;

namespace ShopTileFramework
{
    class ContentModel
    {
        public ShopPack[] Shops { get; set; } = null;
        public AnimalShopPack[] AnimalShops { get; set; } = null;
    }

    class AnimalShopPack
    {
        public string ShopName { get; set; }
        public List<string> AnimalStock { get; set; }
        public string[] ExcludeFromMarnies { get; set; } = null;
        public string[] When { get; set; } = null;

        public Dictionary<string,string>[] Conditions { get; set; } = null;
        public string ClosedMessage { get; set; } = null;

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
        public string[] When { get; set; } = null;
        public Dictionary<string, string>[] Conditions { get; set; } = null;
        public string ClosedMessage { get; set; } = null;
    }

    class ItemStock
    {
        public string ItemType { get; set; }
        public bool IsRecipe { get; set; } = false;
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
