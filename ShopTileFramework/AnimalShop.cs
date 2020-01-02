using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ShopTileFramework
{
    class AnimalShop
    {
        private List<StardewValley.Object> ShopAnimalStock;
        private List<string> AnimalNames;
        private string ShopName;
        public AnimalShop(List<string> AnimalNames, string ShopName)
        {
            this.AnimalNames = AnimalNames;
        }

        private void UpdateShopAnimalStock()
        {
            List<StardewValley.Object> AllAnimalsStock = new List<StardewValley.Object>();
            if (ModEntry.BFAV == null)
            {
                AllAnimalsStock = Utility.getPurchaseAnimalStock();
            }
            else
            {
                AllAnimalsStock = ModEntry.BFAV.GetAnimalShopStock(Game1.getFarm());
            }

            ShopAnimalStock = new List<StardewValley.Object>();
            foreach (var animal in AllAnimalsStock)
            {
                if (AnimalNames.Contains(animal.Name))
                {
                    ShopAnimalStock.Add(animal);
                }
            }
        }

        internal void DisplayShop()
        {
            //get animal stock each time to refresh requirement checks
            UpdateShopAnimalStock();
            ModEntry.SourceLocation = Game1.currentLocation;
            Game1.activeClickableMenu = new PurchaseAnimalsMenu(ShopAnimalStock);
        }
    }

    public interface BFAVApi
    {
        bool IsEnabled();
        List<StardewValley.Object> GetAnimalShopStock(StardewValley.Farm farm);

    }
}