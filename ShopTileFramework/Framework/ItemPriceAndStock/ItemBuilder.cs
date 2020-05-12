using Microsoft.Xna.Framework;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    /// <summary>
    /// This class stores the global data for each itemstock, in order to generate and add items by ID or name
    /// to the stock
    /// </summary>
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

        /// <summary>
        /// Sets the global data for this item stock
        /// </summary>
        /// <param name="itemType">The string name of the item type</param>
        /// <param name="isRecipe">If items are recipes or not</param>
        /// <param name="price">The price of </param>
        /// <param name="currencyItemID">the object ID of the currency item</param>
        /// <param name="currencyItemStack">How many of the currency item is needed</param>
        /// <param name="stock">how much of the item is available</param>
        /// <param name="quality">The quality of the items</param>
        /// <param name="shopName">Name of the shop, kept for logging purposes</param>
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

        /// <param name="ItemPriceAndStock">the ItemPriceAndStock this builder will add items to</param>
        public void setItemPriceAndStock(Dictionary<ISalable, int[]> ItemPriceAndStock)
        {
            this.itemPriceAndStock = ItemPriceAndStock;
        }

        /// <summary>
        /// Takes an item name, and adds that item to the stock
        /// </summary>
        /// <param name="itemName">name of the item</param>
        /// <returns></returns>
        public bool AddItemToStock(string itemName)
        {
            int id = ItemsUtil.GetIndexByName(itemName, ItemsUtil.ObjectInfoSource[itemType]);
            if (id < 0)
            {
                ModEntry.monitor.Log($"{itemType} named \"{itemName}\" could not be added to the Shop {shopName}", LogLevel.Debug);
                return false;
            }

            return AddItemToStock(id);
        }

        /// <summary>
        /// Takes an item id, and adds that item to the stock
        /// </summary>
        /// <param name="itemID">the id of the item</param>
        /// <returns></returns>
        public bool AddItemToStock(int itemID)
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

        /// <summary>
        /// Given an itemID, return an instance of that item with the parameters saved in this builder
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates the second parameter in ItemStockAndPrice, an array that holds info on the price, stock,
        /// and if it exists, the item currency it takes
        /// </summary>
        /// <param name="item">An instance of the item</param>
        /// <returns>The array that's the second parameter in ItemPriceAndStock</returns>
        private int[] getPriceStockAndCurrency(ISalable item)
        {
            int[] PriceStockCurrency;
            //if no price is provided, use the item's sale price
            var price = (StockPrice == -1) ? item.salePrice() : StockPrice; 
            if (currencyItemID == -1) // no currency item
            {
                PriceStockCurrency = new int[] { price, stock };
            }
            else if (currencyItemStack == -1) //no stack provided for currency item so defaults to 1
            {
                PriceStockCurrency = new int[] { price, stock, currencyItemID };
            }
            else //both currency item and stack provided
            {
                PriceStockCurrency = new int[] { price, stock, currencyItemID, currencyItemStack };
            }

            return PriceStockCurrency;
        }
    }
}
