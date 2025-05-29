#nullable disable

using System.Collections.Generic;
using ShopTileFramework.Framework.Apis;
using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using StardewValley.Menus;

namespace ShopTileFramework.Framework.Shop;

/// <summary>
/// This class represents each animal store
/// </summary>
internal class AnimalShop : AnimalShopModel
{
    /*********
    ** Fields
    *********/
    private List<Object> ShopAnimalStock;
    private List<Object> AllAnimalsStock;


    /*********
    ** Accessors
    *********/
    internal static List<string> ExcludeFromMarnie = [];


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Translate what needs to be translated on game saved, in case of the language being changed
    /// </summary>
    public void UpdateTranslations()
    {
        this.ClosedMessage = Translations.Localize(this.ClosedMessage, this.LocalizedClosedMessage);
    }

    public void DisplayShop(bool debug = false)
    {
        //skip condition checking if called from console commands
        if (debug || ApiManager.Conditions.CheckConditions(this.When))
        {
            //get animal stock each time to refresh requirement checks
            this.UpdateShopAnimalStock();

            //sets variables I use to control hardcoded warps
            ModEntry.SourceLocation = Game1.currentLocation;
            Game1.activeClickableMenu = new PurchaseAnimalsMenu(this.ShopAnimalStock);
        }
        else if (this.ClosedMessage != null)
        {
            Game1.activeClickableMenu = new DialogueBox(this.ClosedMessage);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>
    /// Updates the stock by grabbing the current data from getPurchaseAnimalStock and taking the info 
    /// for the animals that will be sold in this store
    /// </summary>
    private void UpdateShopAnimalStock()
    {
        //BFAV patches this anyways so it'll automatically work if installed
        this.AllAnimalsStock = StardewValley.Utility.getPurchaseAnimalStock(Game1.getFarm());

        this.ShopAnimalStock = [];
        foreach (Object animal in this.AllAnimalsStock)
        {
            if (this.AnimalStock.Contains(animal.Name))
            {
                this.ShopAnimalStock.Add(animal);
            }
        }
    }
}
