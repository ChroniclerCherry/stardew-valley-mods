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
            LoadContentPacks();
            helper = h;
            monitor = Monitor;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            foreach (Shop Store in Shops.Values)
            {
                Monitor.Log($"Updating stock for {Store.ShopName}");
                Store.UpdateItemPriceAndStock();
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove || e.Button != SButton.MouseRight)
                return;

            var grab = Helper.Input.GetCursorPosition().Tile;

            if (!IsClickWithinReach(grab))
                return;

            var tileProperty = GetTileProperty(Game1.currentLocation, "Buildings" ,grab);
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);

            if (shopProperty == null)
                return;

            string ShopName = shopProperty.ToString();

            if (Shops.ContainsKey(ShopName))
            {
                Shops[ShopName].DisplayStore();
            } else
            {
                Monitor.Log($"A shop by the name {ShopName} was not found.",LogLevel.Debug);
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
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("shops.json"))
                {
                    Monitor.Log($"No shops.json found for {contentPack.Manifest.UniqueID}. No shop will be added.", LogLevel.Warn);
                    ContentModel data = new ContentModel();
                    data.Shops = new ShopPack[] { 
                        new ShopPack() { ShopName = "test1"}
                        , new ShopPack() { ShopName = "test2"} 
                    };
                    contentPack.WriteJsonFile("shops.json", data);
                }
                else
                {
                    ContentModel data = contentPack.ReadJsonFile<ContentModel>("shops.json");
                    foreach (ShopPack s in data.Shops)
                    {
                        Shops.Add(s.ShopName, new Shop(s, contentPack));
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
