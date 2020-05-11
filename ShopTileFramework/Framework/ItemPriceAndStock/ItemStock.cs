using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    class ItemStock : ItemStockData
    {
        private int currencyObjectID;
        public ItemStock()
        {
            //sets the quality for this stock
            if (Quality < 0 || Quality == 3 || Quality > 4)
            {
                Quality = 0;
                ModEntry.monitor.Log("Item quality can only be 0,1,2, or 4. Defaulting to 0", LogLevel.Trace);
            }

            currencyObjectID = ItemsUtil.GetIndexByName(StockItemCurrency, Game1.objectInformation);

        }
        public Dictionary<ISalable, int[]> Update()
        {
            return null;
        }

    }
}
