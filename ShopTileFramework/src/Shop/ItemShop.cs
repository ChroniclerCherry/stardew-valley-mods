using Microsoft.Xna.Framework.Graphics;
using ShopTileFramework.Data;
using ShopTileFramework.ItemPriceAndStock;
using ShopTileFramework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using ShopTileFramework.API;

namespace ShopTileFramework.Shop
{
    /// <summary>
    /// This class holds all the information for each custom item shop
    /// </summary>
    class ItemShop : ItemShopModel
    {
        private Texture2D _portrait;
        public ItemPriceAndStockManager StockManager { get; set; }

        public IContentPack ContentPack { set; get; }

        /// <summary>
        /// This is used to make sure that JA only adds items to this shop the first time it is opened each day
        /// or else items will be added every time the shop is opened
        /// </summary>
        private bool _shopOpenedToday;

        /// <summary>
        /// Initializes the stock manager, done at game loaded so that content packs have finished loading in
        /// </summary>
        public void Initialize()
        {
            StockManager = new ItemPriceAndStockManager(this);
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
                    _portrait = ContentPack.LoadAsset<Texture2D>(seasonalPath);
                }
                //if the seasonal version doesn't exist, try to load the default
                else if (ContentPack.HasFile(PortraitPath))
                {
                    _portrait = ContentPack.LoadAsset<Texture2D>(PortraitPath);
                }
            }
            catch (Exception ex) //couldn't load the image
            {
                ModEntry.monitor.Log(ex.Message+ex.StackTrace, LogLevel.Error);
            }
        }
        /// <summary>
        /// Refreshes the contents of all stores
        /// and sets the flag for if the store has been opened yet today to false
        /// </summary>
        public void UpdateItemPriceAndStock()
        {
            _shopOpenedToday = false;
            ModEntry.monitor.Log($"Generating stock for {ShopName}", LogLevel.Debug);
            StockManager.Update();
        }

        /// <summary>
        /// Opens the shop if conditions are met. If not, display the closed message
        /// </summary>
        public void DisplayShop(bool debug = false)
        {
            ModEntry.monitor.Log($"Attempting to open the shop \"{ShopName}\"", LogLevel.Trace);

            //if conditions aren't met, display closed message if there is one
            //skips condition checking if debug mode
            if (!debug && !APIs.Conditions.CheckConditions(When))
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

            var shopMenu = new ShopMenu(StockManager.ItemPriceAndStock, currency: currency);

            if (CategoriesToSellHere != null)
                shopMenu.categoriesToSellHere = CategoriesToSellHere;

            if (_portrait != null)
            {
                shopMenu.portraitPerson = new NPC();
                //only add a shop name the first time store is open each day so that items added from JA's side are only added once
                if (!_shopOpenedToday)
                    shopMenu.portraitPerson.Name = "STF." + ShopName;

                shopMenu.portraitPerson.Portrait = _portrait;
            }

            if (Quote != null)
            {
                shopMenu.potraitPersonDialogue = Game1.parseText(Quote, Game1.dialogueFont, 304);
            }

            Game1.activeClickableMenu = shopMenu;
            _shopOpenedToday = true;
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
