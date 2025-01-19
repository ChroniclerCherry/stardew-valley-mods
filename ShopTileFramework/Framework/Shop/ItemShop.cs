using System;
using Microsoft.Xna.Framework.Graphics;
using ShopTileFramework.Framework.Apis;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.ItemPriceAndStock;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ShopTileFramework.Framework.Shop;

/// <summary>
/// This class holds all the information for each custom item shop
/// </summary>
internal class ItemShop : ItemShopModel
{
    /*********
    ** Fields
    *********/
    private Texture2D Portrait;


    /*********
    ** Accessors
    *********/
    public ItemPriceAndStockManager StockManager { get; set; }

    public IContentPack ContentPack { set; get; }


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Initializes the stock manager, done at game loaded so that content packs have finished loading in
    /// </summary>
    public void Initialize()
    {
        this.StockManager = new ItemPriceAndStockManager(this);
    }

    /// <summary>
    /// Loads the portrait, if it exists, and use the seasonal version if one is found for the current season
    /// </summary>
    public void UpdatePortrait()
    {
        if (this.PortraitPath == null)
            return;

        //construct seasonal path to the portrait
        string seasonalPath = this.PortraitPath.Insert(this.PortraitPath.IndexOf('.'), "_" + Game1.currentSeason);
        try
        {
            //if the seasonal version exists, load it
            if (this.ContentPack.HasFile(seasonalPath))
            {
                this.Portrait = this.ContentPack.ModContent.Load<Texture2D>(seasonalPath);
            }
            //if the seasonal version doesn't exist, try to load the default
            else if (this.ContentPack.HasFile(this.PortraitPath))
            {
                this.Portrait = this.ContentPack.ModContent.Load<Texture2D>(this.PortraitPath);
            }
        }
        catch (Exception ex) //couldn't load the image
        {
            ModEntry.StaticMonitor.Log(ex.Message + ex.StackTrace, LogLevel.Error);
        }
    }

    /// <summary>
    /// Refreshes the contents of all stores
    /// and sets the flag for if the store has been opened yet today to false
    /// </summary>
    public void UpdateItemPriceAndStock()
    {
        ModEntry.StaticMonitor.Log($"Generating stock for {this.ShopName}", LogLevel.Debug);
        this.StockManager.Update();
    }

    /// <summary>
    /// Opens the shop if conditions are met. If not, display the closed message
    /// </summary>
    public void DisplayShop(bool debug = false)
    {
        ModEntry.StaticMonitor.Log($"Attempting to open the shop \"{this.ShopName}\"");

        //if conditions aren't met, display closed message if there is one
        //skips condition checking if debug mode
        if (!debug && !ApiManager.Conditions.CheckConditions(this.When))
        {
            if (this.ClosedMessage != null)
            {
                Game1.activeClickableMenu = new DialogueBox(this.ClosedMessage);
            }

            return;
        }

        int currency = 0;
        switch (this.StoreCurrency)
        {
            case "festivalScore":
                currency = 1;
                break;
            case "clubCoins":
                currency = 2;
                break;
        }

        string shopId = $"{this.ContentPack.Manifest.UniqueID}_{this.ShopName}";
        var shopMenu = new ShopMenu(shopId, this.StockManager.ItemPriceAndStock, currency: currency)
        {
            portraitTexture = this.Portrait
        };

        if (this.CategoriesToSellHere != null)
            shopMenu.categoriesToSellHere = this.CategoriesToSellHere;

        if (this.Quote != null)
            shopMenu.potraitPersonDialogue = Game1.parseText(this.Quote, Game1.dialogueFont, 304);

        Game1.activeClickableMenu = shopMenu;
    }

    /// <summary>
    /// Translate what needs to be translated on game saved, in case of the language being changed
    /// </summary>
    internal void UpdateTranslations()
    {
        this.Quote = Translations.Localize(this.Quote, this.LocalizedQuote);
        this.ClosedMessage = Translations.Localize(this.ClosedMessage, this.LocalizedClosedMessage);
    }
}
