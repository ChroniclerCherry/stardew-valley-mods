using Microsoft.Xna.Framework;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    class ItemBuilder
    {
        private readonly string itemType;
        private readonly bool isRecipe;
        private int StockPrice = -1;

        private string itemName = null;
        private int itemID = -1;
        private int quality = 0;


        private int currencyItemID = -1;
        private int currencyItemStack = int.MaxValue;
        private int stock = int.MaxValue;

        public ItemBuilder(string itemType, bool isRecipe, int price) {
            this.itemType = itemType;
            this.isRecipe = isRecipe;
            this.StockPrice = price;
        }

        public ItemBuilder SetItemName(string itemName)
        {
            int id = ItemsUtil.GetIndexByName(itemName, ItemsUtil.ObjectInfoSource[itemType]);
            SetItemID(id);

            return this;
        }

        public ItemBuilder SetItemID(int itemID)
        {
            this.itemID = itemID;

            return this;
        }

        public ItemBuilder SetQuality(int quality)
        {
            this.quality = quality;
            return this;
        }

        public ItemBuilder SetCurrencyItemID(int currencyItemID)
        {
            this.currencyItemID = currencyItemID;
            return this;
        }

        public ItemBuilder SetCurrencyItemStack(int currencyItemStack)
        {
            this.currencyItemStack = currencyItemStack;
            return this;
        }

        public ItemBuilder SetStock(int stock)
        {
            this.stock = stock;
            return this;
        }

        public bool Build(Dictionary<ISalable, int[]> ItemPriceAndStock)
        {
            if (itemID < 0)
            {
                ModEntry.monitor.Log((itemName == null) ?
                    $"{itemType} named \"{itemName}\" could not be added":
                    $"{itemType} of ID {itemID} could not be added", LogLevel.Debug);

                clear();
                return false;
            }

            var item = GetItem();
            if (item == null)
            {
                clear();
                return false;
            }

            var priceStockCurrency = getPriceStockAndCurrency(item);

            ItemPriceAndStock.Add(item, priceStockCurrency);

            clear();
            return true;       
        }

        private ISalable GetItem()
        {
            Item item = null;

            if (itemID == -1)
            {
                return null;
            }

            switch (itemType)
            {
                case "Object":
                    item = new Object(itemID, stock, isRecipe, quality: quality);
                    break;
                case "BigCraftable":
                    item = new Object(Vector2.Zero, itemID) { Stack = stock, IsRecipe = isRecipe };
                    break;
                case "Clothing":
                    item = new Clothing(itemID);
                    break;
                case "Ring":
                    item = new Ring(itemID);
                    break;
                case "Hat":
                    item = new Hat(itemID);
                    break;
                case "Boot":
                    item = new Boots(itemID);
                    break;
                case "Furniture":
                    item = new Furniture(itemID, Vector2.Zero);
                    break;
                case "Weapon":
                    item = new MeleeWeapon(itemID);
                    break;
            }
            return item;
        }

        private int[] getPriceStockAndCurrency(ISalable i)
        {
            int[] PriceStockCurrency;
            var price = (StockPrice == -1) ? i.salePrice() : StockPrice;
            if (currencyItemID == -1)
            {
                PriceStockCurrency = new int[] { price, stock };
            }
            else if (currencyItemStack == -1)
            {
                PriceStockCurrency = new int[] { price, stock, currencyItemID };
            }
            else
            {
                PriceStockCurrency = new int[] { price, stock, currencyItemID, currencyItemStack };
            }

            return PriceStockCurrency;
        }


        private void clear()
        {
            itemName = null;
            itemID = -1;
            quality = 0;
            currencyItemID = -1;
            currencyItemStack = int.MaxValue;
            stock = int.MaxValue;
        }

    }
}
