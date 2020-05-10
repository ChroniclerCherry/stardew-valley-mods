using ShopTileFramework.Framework.Data;
using ShopTileFramework.Framework.Utility;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Shop
{


    class AnimalShop : AnimalShopData
    {

        private List<StardewValley.Object> ShopAnimalStock;
        private List<StardewValley.Object> AllAnimalsStock;

        public AnimalShop(string ShopName)
        {
            ClosedMessage = Translations.Localize(ClosedMessage, LocalizedClosedMessage);
        }

        private void UpdateShopAnimalStock()
        {
            //BFAV patches this anyways so it'll automatically work if installed
            AllAnimalsStock = StardewValley.Utility.getPurchaseAnimalStock();

            ShopAnimalStock = new List<StardewValley.Object>();
            foreach (var animal in AllAnimalsStock)
            {
                if (AnimalStock.Contains(animal.Name))
                {
                    ShopAnimalStock.Add(animal);
                }
            }
        }
        public void DisplayShop()
        {
            if (ConditionChecking.CheckConditions(When))
            {
                //get animal stock each time to refresh requirement checks
                UpdateShopAnimalStock();
                ModEntry.SourceLocation = Game1.currentLocation;
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(ShopAnimalStock);
            }
            else if (ClosedMessage != null)
            {
                Game1.activeClickableMenu = new DialogueBox(ClosedMessage);
            }
        }
    }

    public interface BFAVApi
    {
        bool IsEnabled();
        List<StardewValley.Object> GetAnimalShopStock(Farm farm);
        Dictionary<string, List<string>> GetFarmAnimalCategories();

    }
}