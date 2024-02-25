using System.Collections.Generic;
using System.Linq;
using ShopTileFramework.Framework.Apis;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    /// <summary>
    /// This class stores the data for each stock, with a stock being a list of items of the same itemtype
    /// and sharing the same store parameters such as price
    /// </summary>
    class ItemStock : ItemStockModel
    {
        internal string CurrencyObjectId;
        internal double DefaultSellPriceMultiplier;
        internal Dictionary<double, string[]> PriceMultiplierWhen;
        internal string ShopName;

        private Dictionary<double, string[]> _priceMultiplierWhen;

        private ItemBuilder _builder;
        private Dictionary<ISalable, ItemStockInformation> _itemPriceAndStock;

        /// <summary>
        /// Initialize the ItemStock, doing error checking on the quality, and setting the price to the store price
        /// if none is given specifically for this stock.
        /// Creates the builder
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="price"></param>
        /// <param name="defaultSellPriceMultiplier"></param>
        /// <param name="priceMultiplierWhen"></param>
        internal void Initialize(string shopName, int price, double defaultSellPriceMultiplier, Dictionary<double, string[]> priceMultiplierWhen)
        {
            ShopName = shopName;
            DefaultSellPriceMultiplier = defaultSellPriceMultiplier;
            PriceMultiplierWhen = priceMultiplierWhen;

            if (Quality < 0 || Quality == 3 || Quality > 4)
            {
                Quality = 0;
                ModEntry.monitor.Log("Item quality can only be 0,1,2, or 4. Defaulting to 0", LogLevel.Warn);
            }

            CurrencyObjectId = ItemsUtil.GetItemIdByName(StockItemCurrency);

            //sets price to the store price if no stock price is given
            if (StockPrice == -1)
            {
                StockPrice = price;
            }
            this._priceMultiplierWhen = priceMultiplierWhen;

            if (IsRecipe)
                Stock = 1;

            _builder = new ItemBuilder(this);
        }

        /// <summary>
        /// Resets the items of this item stock, with condition checks and randomization
        /// </summary>
        /// <returns></returns>
        public Dictionary<ISalable, ItemStockInformation> Update()
        {
            if (When != null && !ApiManager.Conditions.CheckConditions(When))
                return null; //did not pass conditions

            if (!ItemsUtil.CheckItemType(ItemType)) //check that itemtype is valid
            {
                ModEntry.monitor.Log($"\t\"{ItemType}\" is not a valid ItemType. No items from this stock will be added."
                    , LogLevel.Warn);
                return null;
            }

            _itemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
            _builder.SetItemPriceAndStock(_itemPriceAndStock);

            double pricemultiplier = 1;
            if (_priceMultiplierWhen != null)
            {
                foreach (KeyValuePair<double,string[]> kvp in _priceMultiplierWhen)
                {
                    if (ApiManager.Conditions.CheckConditions(kvp.Value))
                    {
                        pricemultiplier = kvp.Key;
                        break;
                    }
                }
            }

            if (ItemType != "Seed")
            {
                AddById(pricemultiplier);
                AddByName(pricemultiplier);
            }
            else
            {
                if (ItemIds != null)
                    ModEntry.monitor.Log(
                        "ItemType of \"Seed\" is a special itemtype used for parsing Seeds from JA Pack crops and trees and does not support input via ID. If adding seeds via ID, please use the ItemType \"Object\" instead to directly sell the seeds/saplings");
                if (ItemNames != null)
                    ModEntry.monitor.Log(
                        "ItemType of \"Seed\" is a special itemtype used for parsing Seeds from JA Pack crops and trees and does not support input via Name. If adding seeds via Name, please use the ItemType \"Object\" instead to directly sell the seeds/saplings");
            }

            AddByJAPack(pricemultiplier);

            ItemsUtil.RandomizeStock(_itemPriceAndStock, MaxNumItemsSoldInItemStock);
            return _itemPriceAndStock;
        }

        /// <summary>
        /// Add all items listed in the ItemIDs section
        /// </summary>
        private void AddById(double pricemultiplier)
        {
            if (ItemIds == null)
                return;

            foreach (string itemId in ItemIds)
            {
                _builder.AddItemToStock(itemId, pricemultiplier);
            }
        }

        /// <summary>
        /// Add all items listed in the ItemNames section
        /// </summary>
        private void AddByName(double pricemultiplier)
        {
            if (ItemNames == null)
                return;

            foreach (var itemName in ItemNames)
            {
                _builder.AddItemToStock(itemName, pricemultiplier);
            }

        }

        /// <summary>
        /// Add all items from the JA Packs listed in the JAPacks section
        /// </summary>
        private void AddByJAPack(double pricemultiplier)
        {
            if (JaPacks == null)
                return;

            if (ApiManager.JsonAssets == null)
                return;

            foreach (var JAPack in JaPacks)
            {
                ModEntry.monitor.Log($"Adding all {ItemType}s from {JAPack}", LogLevel.Debug);

                if (ItemType == "Seed")
                {
                    var crops = ApiManager.JsonAssets.GetAllCropsFromContentPack(JAPack);
                    var trees = ApiManager.JsonAssets.GetAllFruitTreesFromContentPack(JAPack);


                    if (crops != null)
                    {

                        foreach (string crop in crops)
                        {
                            if (ExcludeFromJaPacks != null && ExcludeFromJaPacks.Contains(crop)) continue;
                            string id = ItemsUtil.GetSeedId(crop);
                            if (id is not null)
                                _builder.AddItemToStock(id, pricemultiplier);
                        }
                    }

                    if (trees != null)
                    {

                        foreach (string tree in trees)
                        {
                            if (ExcludeFromJaPacks != null && ExcludeFromJaPacks.Contains(tree)) continue;
                            string id = ItemsUtil.GetSaplingId(tree);
                            if (id is not null)
                                _builder.AddItemToStock(id, pricemultiplier);
                        }
                    }

                    continue; //skip the rest of the loop so we don't also add the none-seed version
                }

                var packs = GetJaItems(JAPack);
                if (packs == null)
                {
                    ModEntry.monitor.Log($"No {ItemType} from {JAPack} could be found", LogLevel.Trace);
                    continue;
                }

                foreach (string itemName in packs)
                {
                    if (ExcludeFromJaPacks != null && ExcludeFromJaPacks.Contains(itemName)) continue;
                    _builder.AddItemToStock(itemName, pricemultiplier);
                }
            }
        }

        /// <summary>
        /// Depending on the itemtype, returns a list of the names of all items of that type in a JA pack
        /// </summary>
        /// <param name="JAPack">Unique ID of the pack</param>
        /// <returns>A list of all the names of the items of the right item type in that pack</returns>
        private List<string> GetJaItems(string JAPack)
        {
            switch (ItemType)
            {
                case "Object":
                    return ApiManager.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "BigCraftable":
                    return ApiManager.JsonAssets.GetAllBigCraftablesFromContentPack(JAPack);
                case "Clothing":
                    return ApiManager.JsonAssets.GetAllClothingFromContentPack(JAPack);
                case "Ring":
                    return ApiManager.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "Hat":
                    return ApiManager.JsonAssets.GetAllHatsFromContentPack(JAPack);
                case "Weapon":
                    return ApiManager.JsonAssets.GetAllWeaponsFromContentPack(JAPack);
                default:
                    return null;
            }
        }

    }
}
