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
    /// <summary>
    /// This class holds all the information for each custom item shop
    /// </summary>
    class ItemShop : ItemShopModel
    {
        private Texture2D Portrait = null;
        public ItemPriceAndStockManager StockManager { get; set; }

        public IContentPack ContentPack { set; get; }

        /// <summary>
        /// This is used to make sure that JA only adds items to this shop the first time it is opened each day
        /// or else items will be added every time the shop is opened
        /// </summary>
        private bool shopOpenedToday;

        /// <summary>
        /// Initializes the stock manager, done at game loaded so that content packs have finished loading in
        /// </summary>
        public void Initialize()
        {
            StockManager = new ItemPriceAndStockManager(ItemStocks, this);
        }

        /// <summary>
        /// Loads the portrait, if it exists, and use the seasonal version if one is found for the current season
        /// </summary>
        public void UpdatePortrait()
        {
            if (PortraitPath == null)
                return;

            //construct seasonal path to the portrait
            string seasonalPath = PortraitPath.Insert(PortraitPath.IndexOf('.'), "_" + Game1.currentSeason);
            try
            {
                //if the seasonal version exists, load it
                if (ContentPack.HasFile(seasonalPath)) 
                {
                    Portrait = ContentPack.LoadAsset<Texture2D>(seasonalPath);
                }
                //if the seasonal version doesn't exist, try to load the default
                else if (ContentPack.HasFile(PortraitPath))
                {
                    Portrait = ContentPack.LoadAsset<Texture2D>(PortraitPath);
                }
            }
            catch (Exception ex) //couldn't load the image
            {
                ModEntry.monitor.Log(ex.Message, LogLevel.Warn);
            }
        }
        /// <summary>
        /// Refreshes the contents of all stores
        /// and sets the flag for if the store has been opened yet today to false
        /// </summary>
        public void UpdateItemPriceAndStock()
        {
            shopOpenedToday = false;
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            StockManager.Update();
        }

        /// <summary>
        /// Opens the shop if conditions are met. If not, display the closed message
        /// </summary>
        public void DisplayShop(bool debug = false)
        {
            ModEntry.monitor.Log($"Atempting to open the shop \"{ShopName}\"");

            //if conditions aren't met, display closed message if there is one
            //skips condition checking if debug mode
            if (!debug && !ConditionChecking.CheckConditions(When))
            {
                if (ClosedMessage != null)
                {
                    Game1.activeClickableMenu = new DialogueBox(ClosedMessage);
                }

                return;
            }

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

            var ShopMenu = new ShopMenu(StockManager.ItemPriceAndStock, currency: currency);

            if (CategoriesToSellHere != null)
                ShopMenu.categoriesToSellHere = CategoriesToSellHere;

            if (Portrait != null)
            {
                ShopMenu.portraitPerson = new NPC();
                //only add a shop name the first time store is open each day so that items added from JA's side are only added once
                if (!shopOpenedToday)
                    ShopMenu.portraitPerson.Name = "STF." + ShopName;

                ShopMenu.portraitPerson.Portrait = Portrait;
            }

            if (Quote != null)
            {
                ShopMenu.potraitPersonDialogue = Game1.parseText(Quote, Game1.dialogueFont, 304);
            }

            Game1.activeClickableMenu = ShopMenu;
            shopOpenedToday = true;
        }

        /// <summary>
        /// Translate what needs to be translated on game saved, in case of the language being changed
        /// </summary>
        internal void UpdateTranslations()
        {
            Quote = Translations.Localize(Quote, LocalizedQuote);
            ClosedMessage = Translations.Localize(ClosedMessage, LocalizedClosedMessage);
        }
    }
}
