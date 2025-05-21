using System.Collections.Generic;
using System.Linq;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace CustomCraftingStations.Framework;

internal class CustomCraftingMenu : CraftingPage
{
    /*********
    ** Fields
    *********/
    /// <summary>The custom recipe names to show.</summary>
    private readonly List<string>? Recipes; // nullable because it can be accessed from the base constructor before it's set


    /*********
    ** Public methods
    *********/
    public CustomCraftingMenu(int x, int y, int width, int height, List<IInventory> materialContainers, List<string> recipes, bool cooking)
        : base(x, y, width, height, standaloneMenu: true, materialContainers: materialContainers, cooking: cooking)
    {
        this.Recipes = recipes;

        this.RepositionElements();
    }

    /// <summary>Get the recipes to display in the menu.</summary>
    protected override List<string> GetRecipesToDisplay()
    {
        return
            this.Recipes?.ToList()
            ?? base.GetRecipesToDisplay();
    }
}
