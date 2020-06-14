using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomCraftingStation
{

    public partial class ModEntry
    {
        public List<string> ReducedCookingRecipes { get; set; }
        public List<string> ReducedCraftingRecipes { get; set; }

        private bool _openedNonCustomMenu;
        private void Display_RenderingActiveMenu(object sender, StardewModdingAPI.Events.RenderingActiveMenuEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (_openedNonCustomMenu)
            {
                return;
            }

            _openedNonCustomMenu = true;


            if (OpenedModdedStation)
            {
                OpenedModdedStation = false;
                return;
            }

            var activeMenu = Game1.activeClickableMenu;
            if (activeMenu == null)
                return;

            CraftingPage instance;

            switch (activeMenu)
            {
                case CraftingPage craftingPage:
                    instance = craftingPage;
                    break;
                case GameMenu gameMenu:
                    instance = (CraftingPage)gameMenu.pages[GameMenu.craftingTab];
                    break;
                default:
                    return;
            }

            var isCooking = Helper.Reflection.GetField<bool>(instance, "cooking").GetValue();
            var layoutRecipes = Helper.Reflection.GetMethod(instance, "layoutRecipes");

            var pagesOfCraftingRecipes = Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(instance, "pagesOfCraftingRecipes");
            pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

            List<string> knownCraftingRecipes = ReducedCraftingRecipes.Where(recipe => Game1.player.craftingRecipes.ContainsKey(recipe)).ToList();

            layoutRecipes.Invoke(isCooking ? ReducedCookingRecipes : knownCraftingRecipes);
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.OldMenu is CraftingPage || e.OldMenu is GameMenu)
                _openedNonCustomMenu = false;
        }
    }
}