using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomCraftingStation
{
    public partial class ModEntry
    {
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

            var menu = new CustomCraftingMenu((int) centeringOnScreen.X, (int) centeringOnScreen.Y,
                800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, new List<Chest>(), station.CraftingRecipes, station.CookingRecipes);

            Game1.activeClickableMenu = menu;
        }

        
    }
}