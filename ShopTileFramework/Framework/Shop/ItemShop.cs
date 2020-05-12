using Microsoft.Xna.Framework.Graphics;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ShopTileFramework.Framework.Shop
{
    class ItemShop : ItemShopModel
    {
        private Texture2D Portrait = null;
        public ItemPriceAndStockManager StockManager { get; set; }

        public IContentPack ContentPack { set; get; }

        private bool shopOpenedToday;

        public void Initialize()
        {
            StockManager = new ItemPriceAndStockManager(ItemStocks, this);
        }

        public void UpdatePortrait()
        {
            if (PortraitPath == null)
                return;

            string seasonalPath = PortraitPath.Insert(PortraitPath.IndexOf('.'), "_" + Game1.currentSeason);
            try
            {
                if (ContentPack.HasFile(seasonalPath))
                {
                    Portrait = ContentPack.LoadAsset<Texture2D>(seasonalPath);
                }
                else if (ContentPack.HasFile(PortraitPath))
                {
                    Portrait = ContentPack.LoadAsset<Texture2D>(PortraitPath);
                }
            }
            catch (Exception ex)
            {
                ModEntry.monitor.Log(ex.Message, LogLevel.Warn);
            }
        }


        public void UpdateItemPriceAndStock()
        {
            shopOpenedToday = false;
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            StockManager.Update();
        }

        public void DisplayShop()
        {
            ModEntry.monitor.Log($"Atempting to open the shop \"{ShopName}\"");
            if (ConditionChecking.CheckConditions(When))
            {
                int currency = 0;
                switch (StoreCurrency)
                {
                    case "festivalScore":
                        currency = 1;
                        break;
                    case "clubCoins":
                        currency = 2;
                        break;
                }

                var ShopMenu = new ShopMenu(StockManager.ItemPriceAndStock,
                    currency: currency);
                if (CategoriesToSellHere != null)
                    ShopMenu.categoriesToSellHere = CategoriesToSellHere;

                if (Portrait != null)
                {
                    ShopMenu.portraitPerson = new NPC();
                    if (!shopOpenedToday) //only add a shop name the first time store is open each day so that items added from JA's side are only added once
                        ShopMenu.portraitPerson.Name = "STF." + ShopName;
                    ShopMenu.portraitPerson.Portrait = Portrait;
                }

                if (Quote != null)
                {
                    ShopMenu.potraitPersonDialogue = Game1.parseText(Translations.Localize(Quote, LocalizedQuote), Game1.dialogueFont, 304);
                }

                Game1.activeClickableMenu = ShopMenu;
                shopOpenedToday = true;
            }
            else if (ClosedMessage != null)
            {
                Game1.activeClickableMenu = new DialogueBox(Translations.Localize(ClosedMessage, LocalizedClosedMessage));
            }

        }
    }
}
