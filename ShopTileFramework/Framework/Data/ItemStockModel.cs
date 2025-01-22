namespace ShopTileFramework.Framework.Data;

internal abstract class ItemStockModel
{
    public string ItemType { get; set; }
    public bool IsRecipe { get; set; } = false;
    public int StockPrice { get; set; } = -1;
    public string StockItemCurrency { get; set; } = "Money";
    public int StockCurrencyStack { get; set; } = 1;
    public int Quality { get; set; } = 0;
    public string[] ItemIds { get; set; } = null;
    public string[] JaPacks { get; set; } = null;
    public string[] ExcludeFromJaPacks { get; set; } = null;
    public string[] ItemNames { get; set; } = null;
    public bool FilterSeedsBySeason { get; set; } = true;
    public int Stock { get; set; } = int.MaxValue;
    public int MaxNumItemsSoldInItemStock { get; set; } = int.MaxValue;
    public string[] When { get; set; } = null;
}
