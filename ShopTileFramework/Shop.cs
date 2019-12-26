using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace ShopTileFramework
{
    class Shop
    {
        public string ShopName { get; set; }
        private Texture2D Portrait = null;
        private string Quote { get; set; }
        private int ShopPrice { get; set; }
        private ItemStock[] ItemStocks { get; set; }
        private int MaxNumItemsSoldInStore { get; set; } = int.MaxValue;
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; }
        public Shop(ShopPack pack, IContentPack contentPack)
        {
            ShopName = pack.ShopName;
            ItemStocks = pack.ItemStocks;
            Quote = pack.Quote;

            //try and load in the portrait
            if (pack.PortraitPath != null)
            {
                try
                {
                    Portrait = contentPack.LoadAsset<Texture2D>(pack.PortraitPath);
                } catch (Exception ex)
                {
                    ModEntry.monitor.Log(ex.Message, LogLevel.Warn);
                }
                
            }

        }

        public void UpdateItemPriceAndStock()
        {
            ModEntry.monitor.Log($"Generating stock for {ShopName}",LogLevel.Debug);
            ItemPriceAndStock = new Dictionary<ISalable, int[]>();

            //TODO: add in pricing/stock/and maximum number randomization

            foreach (ItemStock Inventory in ItemStocks)
            {

                int Price = Inventory.StockPrice;
                if (Price == -1)
                {
                    Price = ShopPrice;
                }

                if (Inventory.ItemIDs != null)
                {
                    foreach (var ItemID in Inventory.ItemIDs)
                    {
                        var i = GetItem(Inventory.ItemType, ItemID);
                        if (i != null)
                        {
                            ItemPriceAndStock.Add(i, new int[] { (Price == -1) ? i.salePrice() : Price, Inventory.Stock });
                        } else
                        {
                            ModEntry.monitor.Log($"{Inventory.ItemType} of ID {ItemID} could not be added to {ShopName}", LogLevel.Warn);
                        }
                            
                    }
                }

                if (Inventory.ItemNames != null)
                {
                    foreach (var ItemName in Inventory.ItemNames)
                    {
                        var i = GetItem(Inventory.ItemType, ItemName);
                        if (i != null)
                        {
                            ItemPriceAndStock.Add(i, new int[] { (Price == -1) ? i.salePrice() : Price, Inventory.Stock });
                        } else
                        {
                            ModEntry.monitor.Log($"{Inventory.ItemType} named \"{ItemName}\" could not be added to {ShopName}", LogLevel.Warn);
                        }    
                    }
                }

                if (Inventory.JAPacks != null)
                {
                    foreach (var Item in Inventory.JAPacks)
                    {
                        ModEntry.monitor.Log($"JA Packs are not yet supported, \"{Item}\" not added.", LogLevel.Warn);
                    }
                }

            }
        }

        private Item GetItem(string objectType, int index)
        {
            Item item = null;
            if (objectType == "Object")
            {
                if (index != -1)
                    item = new StardewValley.Object(index, 1);
            }
            else if (objectType == "BigCraftable")
            {
                if (index != -1)
                    item = new StardewValley.Object(Vector2.Zero, index);
            }
            else if (objectType == "Clothing")
            {
                if (index != -1)
                    item = new Clothing(index);
            }
            else if (objectType == "Ring")
            {
                if (index != -1)
                    item = new Ring(index);
            }
            else if (objectType == "Hat")
            {
                if (index != -1)
                    item = new Hat(index);
            }
            else if (objectType == "Boots")
            {
                if (index != -1)
                    item = new Boots(index);
            }
            else if (objectType == "TV")
            {
                if (index != -1)
                    item = new TV(index, Vector2.Zero);
            }
            else if (objectType == "IndoorPot")
                item = new IndoorPot(Vector2.Zero);
            else if (objectType == "CrabPot")
                item = new CrabPot(Vector2.Zero);
            else if (objectType == "Chest")
                item = new Chest(true);
            else if (objectType == "Cask")
                item = new Cask(Vector2.Zero);
            else if (objectType == "Furniture")
            {
                if (index != -1)
                    item = new Furniture(index, Vector2.Zero);
            }
            else if (objectType == "Sign")
                item = new Sign(Vector2.Zero, index);
            else if (objectType == "Wallpaper")
                item = new Wallpaper(Math.Abs(index), false);
            else if (objectType == "Floors")
                item = new Wallpaper(Math.Abs(index), true);
            else if (objectType == "Weapon")
            {
                if (index != -1)
                    item = new MeleeWeapon(index);
            }

            return item;
        }

        private Item GetItem(string objectType, string name)
        {
            Item item = null;
            int index = -1;

            if (name != null)
            {
                if (objectType == "Object")
                {
                    //includes rings
                    index = GetIndexByName(name, Game1.objectInformation);
                }
                else if (objectType == "BigCraftable")
                {
                    index = GetIndexByName(name, Game1.bigCraftablesInformation);
                }
                else if (objectType == "Clothing")
                {
                    index = GetIndexByName(name, Game1.clothingInformation);
                }
                else if (objectType == "Hat")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/hats", ContentSource.GameContent));
                }
                else if (objectType == "Boots")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/Boots", ContentSource.GameContent));

                }
                else if (objectType == "TV")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent));
                }
                else if (objectType == "IndoorPot")
                    return new StardewValley.Objects.IndoorPot(Vector2.Zero);
                else if (objectType == "CrabPot")
                    return  new StardewValley.Objects.CrabPot(Vector2.Zero);
                else if (objectType == "Chest")
                    return new StardewValley.Objects.Chest(true);
                else if (objectType == "Cask")
                    return new StardewValley.Objects.Cask(Vector2.Zero);
                else if (objectType == "Furniture")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/Furniture", ContentSource.GameContent));
                }
                else if (objectType == "Weapon")
                {
                    index = GetIndexByName(name, ModEntry.helper.Content.Load<Dictionary<int, string>>(@"Data/weapons", ContentSource.GameContent));
                }

                return GetItem(objectType, index);
            }

            return item;
        }

        private int GetIndexByName (string name, IDictionary<int,string> ObjectInfo)
        {
            foreach(KeyValuePair<int,string> kvp in ObjectInfo)
            {
                if (kvp.Value.StartsWith(name))
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public void DisplayStore()
        {
            Game1.activeClickableMenu = new ShopMenu(ItemPriceAndStock);
            
        }


    }
}
