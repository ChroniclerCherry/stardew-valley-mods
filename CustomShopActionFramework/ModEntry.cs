using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using xTile.ObjectModel;

namespace CustomShopActionFramework
{
    class ModEntry : Mod
    {
        public static IModHelper helper;
    private Dictionary<string, ShopPack> Shops { get; set; }
        public override void Entry(IModHelper h)
        {
            LoadContentPacks();
            helper = h;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove || e.Button != SButton.MouseRight)
                return;

            var grab = Helper.Input.GetCursorPosition().Tile;
            var tileProperty = GetTileProperty(Game1.currentLocation, "Buildings" ,Helper.Input.GetCursorPosition().Tile);
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);

            if (shopProperty == null)
                return;

            //openShopMenu(shopProperty.ToString());
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
                }
                else
                {
                    ContentModel data = contentPack.ReadJsonFile<ContentModel>("shops.json");
                    foreach (ShopPack s in data.Shops)
                    {
                        Shops.Add(s.ShopName, s);
                    }
                }

            }
        }
    }
}
