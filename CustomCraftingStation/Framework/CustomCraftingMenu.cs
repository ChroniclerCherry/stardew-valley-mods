using System.Collections.Generic;
using System.Linq;
using StardewValley.Inventories;

namespace StardewValley.Menus
{
    public class CustomCraftingMenu : CraftingPage
    {
        private readonly List<string> CraftingRecipes;
        private readonly List<string> CookingRecipes;

        public CustomCraftingMenu(int x, int y, int width, int height, List<IInventory> materialContainers, List<string> craftingRecipes, List<string> cookingRecipes)
            : base(x, y, width, height, standaloneMenu: true, materialContainers: materialContainers)
        {
            this.CraftingRecipes = craftingRecipes;
            this.CookingRecipes = cookingRecipes;

            this.RepositionElements();
        }

        /// <summary>Get the recipes to display in the menu.</summary>
        protected override List<string> GetRecipesToDisplay()
        {
            return
                this.CraftingRecipes?.Union(this.CookingRecipes).ToList()
                ?? base.GetRecipesToDisplay(); // not initialized yet
        }
    }
}