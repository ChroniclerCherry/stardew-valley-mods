using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTileFramework.Framework.Data
{
    abstract class ItemStockModel
    {
        public string ItemType { get; set; }
        public bool IsRecipe { get; set; } = false;
        public int StockPrice { get; set; } = -1;
        public string StockItemCurrency { get; set; } = "Money";
        public int StockCurrencyStack { get; set; } = 1;
        public int Quality { get; set; } = 0;
        public int[] ItemIDs { get; set; } = null;
        public string[] JAPacks { get; set; } = null;
        public string[] ItemNames { get; set; } = null;
        public int Stock { get; set; } = int.MaxValue;
        public int MaxNumItemsSoldInItemStock { get; set; } = int.MaxValue;
        public string[] When { get; set; } = null;
    }
}
