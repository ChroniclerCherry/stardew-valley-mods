using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomCraftingStation
{
    public partial class ModEntry
    {
        public bool OpenedModdedStation { get; set; }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return; 
            
            if (!e.Button.IsActionButton())
                return;

            Vector2 grabTile = e.Cursor.GrabTile;

            Game1.currentLocation.Objects.TryGetValue(grabTile, out var obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (_craftableCraftingStations.ContainsKey(obj.Name))
                {
                    OpenCraftingMenu(_craftableCraftingStations[obj.Name]);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
            }

            //No relevant BigCraftable found so check for tiledata
            string tileProperty =
                Game1.currentLocation.doesTileHaveProperty((int) grabTile.X, (int) grabTile.Y, "Action", "Buildings");
            if (tileProperty == null)
                return;

            string[] properties = tileProperty.Split(' ');
            if (properties[0] != "CraftingStation")
                return;

            if (_tileCraftingStations.ContainsKey(properties[1]))
            {
                OpenCraftingMenu(_tileCraftingStations[properties[1]]);
                Helper.Input.Suppress(e.Button);
            }
        }

        public void OpenCraftingMenu(CraftingStation station)
        {
            Vector2 centeringOnScreen =
                Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2,
                    600 + IClickableMenu.borderWidth * 2);

            OpenedModdedStation = true;
            var menu = new CraftingPage((int) centeringOnScreen.X, (int) centeringOnScreen.Y,
                800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);

            LayoutRecipe(menu, station.CraftingRecipes, station.CookingRecipes);
            Game1.activeClickableMenu = menu;
        }

        private void LayoutRecipe(CraftingPage menu, List<string> craftingRecipes, List<string> cookingingRecipes)
        {
            var pagesOfCraftingRecipes = Helper.Reflection.GetField<List<Dictionary<ClickableTextureComponent, CraftingRecipe>>>(menu, "pagesOfCraftingRecipes");
            pagesOfCraftingRecipes.SetValue(new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>());

            var createNewPage = Helper.Reflection.GetMethod(menu, "createNewPage");
            var createNewPageLayout = Helper.Reflection.GetMethod(menu, "createNewPageLayout");
            var spaceOccupied = Helper.Reflection.GetMethod(menu, "spaceOccupied");
            var craftingPageY = Helper.Reflection.GetMethod(menu, "craftingPageY");

            int craftingPageX = menu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
            int spaceBetweenCraftingIcons = 8;
            var newPage = createNewPage.Invoke<Dictionary<ClickableTextureComponent, CraftingRecipe>>();
            int x = 0;
            int y = 0;
            int numRecipes = 0;
            ClickableTextureComponent[,] newPageLayout = createNewPageLayout.Invoke<ClickableTextureComponent[,]>();
            List<ClickableTextureComponent[,]> textureComponentArrayList = new List<ClickableTextureComponent[,]>();
            textureComponentArrayList.Add(newPageLayout);
            foreach (string playerRecipe in craftingRecipes)
            {
                ++numRecipes;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, false);
                while (spaceOccupied.Invoke<bool>(newPageLayout, x, y, recipe))
                {
                    ++x;
                    if (x >= 10)
                    {
                        x = 0;
                        ++y;
                        if (y >= 4)
                        {
                            newPage = createNewPage.Invoke<Dictionary<ClickableTextureComponent, CraftingRecipe>>();
                            newPageLayout = createNewPageLayout.Invoke<ClickableTextureComponent[,]>();
                            textureComponentArrayList.Add(newPageLayout);
                            x = 0;
                            y = 0;
                        }
                    }
                }

                int id = 200 + numRecipes;
                var textureComponent = new ClickableTextureComponent("",
                    new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY.Invoke<int>() + y * 72, 64,
                        recipe.bigCraftable ? 128 : 64), null,
                    Game1.player.craftingRecipes.ContainsKey(recipe.name) ? "" : "ghosted",
                    recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet,
                    recipe.bigCraftable
                        ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32,
                            recipe.getIndexOfMenuView())
                        : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(),
                            16, 16), 4f)
                {
                    myID = id,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                ClickableTextureComponent component = textureComponent;
                newPage.Add(component, recipe);
                newPageLayout[x, y] = component;
                if (recipe.bigCraftable)
                    newPageLayout[x, y + 1] = component;
            }

            foreach (string playerRecipe in cookingingRecipes)
            {
                ++numRecipes;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, true);
                while (spaceOccupied.Invoke<bool>(newPageLayout, x, y, recipe))
                {
                    ++x;
                    if (x >= 10)
                    {
                        x = 0;
                        ++y;
                        if (y >= 4)
                        {
                            newPage = createNewPage.Invoke<Dictionary<ClickableTextureComponent, CraftingRecipe>>();
                            newPageLayout = createNewPageLayout.Invoke<ClickableTextureComponent[,]>();
                            textureComponentArrayList.Add(newPageLayout);
                            x = 0;
                            y = 0;
                        }
                    }
                }

                int num5 = 200 + numRecipes;
                var textureComponent = new ClickableTextureComponent("",
                    new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY.Invoke<int>() + y * 72, 64,
                        recipe.bigCraftable ? 128 : 64), null,
                    Game1.player.cookingRecipes.ContainsKey(recipe.name) ? "" : "ghosted",
                    recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet,
                    recipe.bigCraftable
                        ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32,
                            recipe.getIndexOfMenuView())
                        : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(),
                            16, 16), 4f)
                {
                    myID = num5,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                ClickableTextureComponent key = textureComponent;
                newPage.Add(key, recipe);
                newPageLayout[x, y] = key;
                if (recipe.bigCraftable)
                    newPageLayout[x, y + 1] = key;
            }

        }
    }
}