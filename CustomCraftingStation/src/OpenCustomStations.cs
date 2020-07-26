using System.Collections.Generic;
using CustomCraftingStation.src;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
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
                    OpenCraftingMenu(_craftableCraftingStations[obj.Name], e.Cursor.GrabTile);
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
                OpenCraftingMenu(_tileCraftingStations[properties[1]], e.Cursor.GrabTile);
                Helper.Input.Suppress(e.Button);
            }
        }

        public void OpenCraftingMenu(CraftingStation station,Vector2 grabTile)
        {
            List<Chest> Chests = GetChests(grabTile);

            Vector2 centeringOnScreen =
                Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2,
                    600 + IClickableMenu.borderWidth * 2);

            var menu = new CustomCraftingMenu((int) centeringOnScreen.X, (int) centeringOnScreen.Y,
                800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, Chests, station.CraftingRecipes, station.CookingRecipes);

            Game1.activeClickableMenu = menu;
        }

        public readonly NetMutex mutex = new NetMutex();

        public List<Chest> GetChests(Vector2 grabTile)
        {
            List<Chest> chests = new List<Chest>();
            int radius = _config.CraftingFromChestsRadius;
            if (radius == 0 && !_config.GlobalCraftFromChest)
                return chests;

            IEnumerable<GameLocation> locs;
            locs = Context.IsMainPlayer ? Game1.locations : Helper.Multiplayer.GetActiveLocations();

            if (_config.CraftFromFridgeWhenInHouse)
                if (Game1.currentLocation is FarmHouse house)
                    chests.Add(house.fridge.Value);

            if (_config.GlobalCraftFromChest)
            {
                if (!_config.CraftFromFridgeWhenInHouse) //so we dont add this twice
                    chests.Add((Game1.getLocationFromName("FarmHouse") as FarmHouse)?.fridge.Value);

                foreach (var location in locs)
                {
                    foreach (var objs in location.objects)
                    {
                        foreach (var obj in objs)
                        {
                            if (obj.Value is Chest chest)
                                chests.Add(chest);
                        }
                    }
                }
            }
            else
            {
                var loc = Game1.currentLocation;

                for (int i = -radius; i < radius; i++)
                {
                    for (int j = -radius; j < radius; j++)
                    {
                        var tile = new Vector2(grabTile.X + i, grabTile.Y + j);
                        if (!loc.objects.ContainsKey(tile)) continue;

                        var obj = loc.objects[tile];
                        if (obj != null && obj is Chest chest)
                            chests.Add(chest);
                    }
                }
            }

            return chests;

        }
    }
}