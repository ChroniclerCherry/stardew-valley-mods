using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using ShopTileFramework.Framework.Apis;

namespace ShopTileFramework.Framework.Shop
{
    /// <summary>
    /// This class represents each animal store
    /// </summary>
    class AnimalShop : AnimalShopModel
    {

        private List<Object> _shopAnimalStock;
        private List<Object> _allAnimalsStock;

        internal static List<string> ExcludeFromMarnie = new List<string>();

        /// <summary>
        /// Translate what needs to be translated on game saved, in case of the language being changed
        /// </summary>
        public void UpdateTranslations()
        {
            this.ClosedMessage = Translations.Localize(this.ClosedMessage, this.LocalizedClosedMessage);
        }

        /// <summary>
        /// Updates the stock by grabbing the current data from getPurchaseAnimalStock and taking the info 
        /// for the animals that will be sold in this store
        /// </summary>
        private void UpdateShopAnimalStock()
        {
            //BFAV patches this anyways so it'll automatically work if installed
            this._allAnimalsStock = StardewValley.Utility.getPurchaseAnimalStock(Game1.getFarm());

            this._shopAnimalStock = new List<Object>();
            foreach (var animal in this._allAnimalsStock)
            {
                if (this.AnimalStock.Contains(animal.Name))
                {
                    this._shopAnimalStock.Add(animal);
                }
            }
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
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(this._shopAnimalStock);
            }
            else if (this.ClosedMessage != null)
            {
                Game1.activeClickableMenu = new DialogueBox(this.ClosedMessage);
            }
        }
    }
}