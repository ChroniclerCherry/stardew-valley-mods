using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using xTile.ObjectModel;

namespace ShopTileFramework
{
    class ModEntry : Mod
    {
        public static IModHelper helper;
        public static IMonitor monitor;
        private Dictionary<string, Shop> Shops { get; set; }
        public override void Entry(IModHelper h)
        {
            //make helper and monitor static so they can be accessed in other classes
            helper = h;
            monitor = Monitor;
            Shops = new Dictionary<string, Shop>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            LoadContentPacks();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            //refresh the stock of each store every day
            foreach (Shop Store in Shops.Values)
            {
                Monitor.Log($"Updating stock for {Store.ShopName}");
                Store.UpdateItemPriceAndStock();
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            //context and button check
            if (!Context.CanPlayerMove || e.Button.IsActionButton() || e.Button == SButton.MouseRight)
                return;

            //check if clicked tile is near the player
            var clickedTile = Helper.Input.GetCursorPosition().Tile;
            if (!IsClickWithinReach(clickedTile))
                return;

            //check if there is a tile property on Buildings layer
            var tileProperty = GetTileProperty(Game1.currentLocation, "Buildings" ,clickedTile);
            if (tileProperty == null)
                return;

            //check if there is a Shop property on clicked tile
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);
            if (shopProperty == null)
                return;

            //Extract the tile property value
            string ShopName = shopProperty.ToString();
            if (Shops.ContainsKey(ShopName))
            {
                helper.Input.Suppress(e.Button);
                Shops[ShopName].DisplayStore();
            } else
            {
                Monitor.Log($"A shop tile was clicked, but a shop by the name \"{ShopName}\" was not found.",LogLevel.Debug);
            }

        }
        private IPropertyCollection GetTileProperty(GameLocation map, string layer, Vector2 tile)
        {
            if (map == null)
                return null;

            var checkTile = map.Map.GetLayer(layer).Tiles[(int)tile.X, (int)tile.Y];

            if (checkTile == null)
                return null;

            return checkTile.Properties;
        }

        private void LoadContentPacks()
        {
            monitor.Log("Adding Content Packs...", LogLevel.Info);
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("shops.json"))
                {
                    Monitor.Log($"No shops.json found from the mod {contentPack.Manifest.UniqueID}. Skipping pack.", LogLevel.Warn);
                }
                else
                {
                    ContentModel data = contentPack.ReadJsonFile<ContentModel>("shops.json");
                    Monitor.Log($"{contentPack.Manifest.Name} | {contentPack.Manifest.Description}", LogLevel.Info);
                    foreach (ShopPack s in data.Shops)
                    {
                        if (Shops.ContainsKey(s.ShopName))
                        {
                            Monitor.Log($"The shop \"{s.ShopName}\" has already been added. Ignoring {contentPack.Manifest.UniqueID}", LogLevel.Warn);

                        } else
                        {
                            Shops.Add(s.ShopName, new Shop(s, contentPack));
                        }
                    }
                }

            }
        }

        private bool IsClickWithinReach(Vector2 tile)
        {
            var playerPosition = Game1.player.Position;
            var playerTile = new Vector2(playerPosition.X / 64, playerPosition.Y / 64);

            if (tile.X < (playerTile.X - 1.5) || tile.X > (playerTile.X + 1.5))
                return false;

            if (tile.Y < (playerTile.Y - 1.5) || tile.Y > (playerTile.Y + 1.5))
                return false;

            return true;
        }
    }
}
