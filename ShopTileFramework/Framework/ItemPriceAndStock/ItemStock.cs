using ShopTileFramework.Framework.API;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.ItemPriceAndStock
{
    class ItemStock : ItemStockModel
    {
        private int currencyObjectID;
        private ItemBuilder builder;
        private Dictionary<ISalable, int[]> ItemPriceAndStock;

        internal void Initialize(ItemShopModel data)
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

            //initializes the builder with all the parameters of this itemstock
            builder = new ItemBuilder(itemType: ItemType,
                                      isRecipe: IsRecipe,
                                      price: StockPrice,
                                      currencyItemID: currencyObjectID,
                                      currencyItemStack: StockCurrencyStack,
                                      stock: Stock,
                                      quality: Quality,
                                      shopName: data.ShopName);
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

            ItemPriceAndStock = new Dictionary<ISalable, int[]>();
            builder.setItemPriceAndStock(ItemPriceAndStock);

            AddByID();
            AddByName();
            AddByJAPack();

            ItemsUtil.RandomizeStock(ItemPriceAndStock, MaxNumItemsSoldInItemStock);
            return ItemPriceAndStock;
        }

        private void AddByID()
        {

            if (ItemIDs == null)
                return;

            if (ItemType == "Seed")
            {
                ModEntry.monitor.Log($" Itemtype \"Seed\" is only applicable to JAPacks, not ItemIDs", LogLevel.Warn);
                return;
            }

            foreach (var ItemID in ItemIDs)
            {
                builder.GetItem(ItemID);
            }
        }

        private void AddByName()
        {
            if (ItemNames == null)
                return;

            if (ItemType == "Seed")
            {
                ModEntry.monitor.Log($" Itemtype \"Seed\" is only applicable to JAPacks, not ItemNames", LogLevel.Warn);
                return;
            }

            foreach (var ItemName in ItemNames)
            {
                builder.GetItem(ItemName);
            }

        }

        private void AddByJAPack()
        {
            if (JAPacks == null)
                return;

            if (APIs.JsonAssets == null)
                return;

            foreach (var JAPack in JAPacks)
            {
                ModEntry.monitor.Log($"Adding objects from JA pack {JAPack}", LogLevel.Debug);

                if (ItemType == "Seed")
                {
                    var crops = APIs.JsonAssets.GetAllCropsFromContentPack(JAPack);
                    var trees = APIs.JsonAssets.GetAllFruitTreesFromContentPack(JAPack);

                    if (crops != null)
                    {
                        foreach(string crop in crops)
                        {
                            int id = ItemsUtil.GetSeedID(crop);
                            if (id >0)
                                builder.GetItem(id);
                        }
                    }

                    if (trees != null)
                    {
                        foreach (string tree in trees)
                        {
                            int id = ItemsUtil.GetSaplingID(tree);
                            if (id > 0)
                                builder.GetItem(id);
                        }
                    }

                    continue;
                }

                var packs = getJAItems(JAPack);
                if (packs == null)
                {
                    ModEntry.monitor.Log($"No {ItemType} from {JAPack} could be found", LogLevel.Trace);
                    continue;
                }

                foreach (string itemName in packs)
                {
                    builder.GetItem(itemName);
                }
            }
        }

        private List<string> getJAItems(string JAPack)
        {
            switch (ItemType)
            {
                case "Object":
                    return APIs.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "BigCraftable":
                    return APIs.JsonAssets.GetAllBigCraftablesFromContentPack(JAPack);
                case "Clothing":
                    return APIs.JsonAssets.GetAllClothingFromContentPack(JAPack);
                case "Ring":
                    return APIs.JsonAssets.GetAllObjectsFromContentPack(JAPack);
                case "Hat":
                    return APIs.JsonAssets.GetAllHatsFromContentPack(JAPack);
                case "Weapon":
                    return APIs.JsonAssets.GetAllWeaponsFromContentPack(JAPack);
                default:
                    return null;
            }
        }

    }
}
