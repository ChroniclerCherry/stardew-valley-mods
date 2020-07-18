using System;
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

        public static bool MenuOverride = true;

        private bool _openedNonCustomMenu;
        private void Display_RenderingActiveMenu(object sender, StardewModdingAPI.Events.RenderingActiveMenuEventArgs e)
        {
            if (!MenuOverride) return;

            if (!Context.IsWorldReady)
                return;

            if (_openedNonCustomMenu)
            {
                return;
            }

            _openedNonCustomMenu = true;

            var activeMenu = Game1.activeClickableMenu;
            if (activeMenu == null)
                return;

            IClickableMenu instance;

            

            if (activeMenu is CraftingPage)
                instance = activeMenu;
            else if (activeMenu is GameMenu gameMenu)
                instance = gameMenu.pages[GameMenu.craftingTab];
            else if (activeMenu.GetType() == CookingSkillMenu)
                instance = activeMenu;
            else
                return;

            

            OpenAndFixMenu(instance);
        }

        private void OpenAndFixMenu(IClickableMenu instance)
        {
            var isCooking = Helper.Reflection.GetField<bool>(instance, "cooking").GetValue();
            var layoutRecipes = Helper.Reflection.GetMethod(instance, "layoutRecipes");

            var pagesOfCraftingRecipes =
                Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(instance,
                    "pagesOfCraftingRecipes");
            pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

            List<string> knownCraftingRecipes =
                ReducedCraftingRecipes.Where(recipe => Game1.player.craftingRecipes.ContainsKey(recipe)).ToList();

            layoutRecipes.Invoke(isCooking ? ReducedCookingRecipes : knownCraftingRecipes);
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.OldMenu == null ||
                e.OldMenu is CustomCraftingMenu
                || e.OldMenu is CraftingPage
                || e.OldMenu is GameMenu
                || e.OldMenu.GetType() == CookingSkillMenu)
            {
                _openedNonCustomMenu = false;
            }
        }
    }
}