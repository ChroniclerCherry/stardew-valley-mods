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
        private ItemBuilder builder;

        internal void Initialize(ItemShopData data)
        {
            if (Quality < 0 || Quality == 3 || Quality > 4)
            {
                Quality = 0;
                ModEntry.monitor.Log("Item quality can only be 0,1,2, or 4. Defaulting to 0", LogLevel.Trace);
            }

            currencyObjectID = ItemsUtil.GetIndexByName(StockItemCurrency, Game1.objectInformation);

            if (StockPrice == -1)
            {
                StockPrice = data.ShopPrice;
            }

            builder = new ItemBuilder(ItemType, IsRecipe, StockPrice);

        }

        public Dictionary<ISalable, int[]> Update()
        {

            if (When != null && !ConditionChecking.CheckConditions(When))
                return null;

            if (!ItemsUtil.CheckItemType(ItemType))
            {
                ModEntry.monitor.Log($" \"{ItemType}\" is not a valid ItemType. Some items will not be added.", LogLevel.Warn);
                return null;
            }

            var ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            AddByID(ItemPriceAndStock);
            AddByName(ItemPriceAndStock);
            AddByJAPack(ItemPriceAndStock);

            ItemsUtil.RandomizeStock(ItemPriceAndStock, MaxNumItemsSoldInItemStock);
            return ItemPriceAndStock;
        }

        private void AddByID(Dictionary<ISalable, int[]> itemPriceAndStock)
        {
            if (ItemType == "Seed")
            {
                ModEntry.monitor.Log($" Itemtype \"Seed\" is only applicable to JAPacks, not ItemIDs", LogLevel.Warn);
                return;
            }

            if (ItemIDs == null)
                return;

                foreach (var ItemID in ItemIDs)
                {
                builder.SetItemID(ItemID).SetQuality(Quality).SetStock(Stock).SetCurrencyItemID(currencyObjectID).SetCurrencyItemStack(StockCurrencyStack).Build(itemPriceAndStock);
                }
        }

        private void AddByName(Dictionary<ISalable, int[]> itemPriceAndStock)
        {
            if (ItemType == "Seed")
            {
                ModEntry.monitor.Log($" Itemtype \"Seed\" is only applicable to JAPacks, not ItemNames", LogLevel.Warn);
                return;
            }


            if (ItemNames == null)
                return;

            foreach (var ItemName in ItemNames)
            {
                builder.SetItemName(ItemName).SetQuality(Quality).SetStock(Stock).SetCurrencyItemID(currencyObjectID).SetCurrencyItemStack(StockCurrencyStack).Build(itemPriceAndStock);
            }

        }

        private void AddByJAPack(Dictionary<ISalable, int[]> itemPriceAndStock)
        {
            if (JAPacks == null)
                return;

            foreach (var JAPack in JAPacks)
            {
                //TODO
            }
        }

    }
}
