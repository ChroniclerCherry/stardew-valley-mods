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
        //These are set for each stock
        private readonly string itemType;
        private readonly bool isRecipe;
        private readonly int StockPrice = -1;
        private readonly int quality = 0;
        private readonly int currencyItemID = -1;
        private readonly int currencyItemStack = int.MaxValue;
        private readonly int stock = int.MaxValue;
        private readonly string shopName;
        private Dictionary<ISalable, int[]> itemPriceAndStock;

        public ItemBuilder(string itemType,
                           bool isRecipe,
                           int price,
                           int currencyItemID,
                           int currencyItemStack,
                           int stock,
                           int quality,
                           string shopName) {
            this.itemType = itemType;
            this.isRecipe = isRecipe;
            this.StockPrice = price;
            this.currencyItemID = currencyItemID;
            this.currencyItemStack = currencyItemStack;
            this.stock = stock;
            this.quality = quality;
            this.shopName = shopName;
        }

        public void setItemPriceAndStock(Dictionary<ISalable, int[]> ItemPriceAndStock)
        {
            this.itemPriceAndStock = ItemPriceAndStock;
        }

        public bool GetItem(string itemName)
        {
            int id = ItemsUtil.GetIndexByName(itemName, ItemsUtil.ObjectInfoSource[itemType]);
            if (id == -1)
            {
                ModEntry.monitor.Log($"{itemType} named \"{itemName}\" could not be added to the Shop {shopName}", LogLevel.Debug);
                return false;
            }

            return GetItem(id);
        }

        public bool GetItem(int itemID)
        {
            if (itemID < 0)
            {
                ModEntry.monitor.Log($"{itemType} of ID {itemID} could not be added to the Shop {shopName}", LogLevel.Debug);
                return false;
            }

            var item = CreateItem(itemID);
            if (item == null)
            {
                return false;
            }

            if (isRecipe)
            {
                if (!ItemsUtil.RecipesList.Contains(item.Name))
                {
                    ModEntry.monitor.Log($"{item.Name} is not a valid recipe and won't be added.");
                    return false;
                }
            }

            var priceStockCurrency = getPriceStockAndCurrency(item);
            itemPriceAndStock.Add(item, priceStockCurrency);

            return true;       
        }

        private ISalable CreateItem(int itemID)
        {
            switch (itemType)
            {
                case "Object":
                    return new Object(itemID, isRecipe? 1 : stock, isRecipe, quality: quality);
                case "Seed":
                    return new Object(itemID, isRecipe ? 1 : stock, isRecipe, quality: quality);
                case "BigCraftable":
                    return new Object(Vector2.Zero, itemID) { Stack = isRecipe ? 1 : stock, IsRecipe = isRecipe };
                case "Clothing":
                    return new Clothing(itemID);
                case "Ring":
                    return new Ring(itemID);
                case "Hat":
                    return new Hat(itemID);
                case "Boot":
                    return new Boots(itemID);
                case "Furniture":
                    return new Furniture(itemID, Vector2.Zero);
                case "Weapon":
                    return new MeleeWeapon(itemID);
                default: return null;
            }
        }

        private int[] getPriceStockAndCurrency(ISalable item)
        {
            int[] PriceStockCurrency;
            var price = (StockPrice == -1) ? item.salePrice() : StockPrice;
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
    }
}
