using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;

namespace CustomizeAnywhere
{
    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }

    class DresserAndMirror
    {
        private IModHelper helper;
        private IJsonAssetsApi JsonAssets;
        private int ClothingCatalogueID = -1;
        private int CustomizationMirrorID = -1;

        public DresserAndMirror(IModHelper helper)
        {
            this.helper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
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

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady &&
                Game1.currentLocation != null &&
                Game1.activeClickableMenu == null &&
                e.Button.IsActionButton())
            {
                GameLocation loc = Game1.currentLocation;
                var clickedTile = ModEntry.helper.Input.GetCursorPosition().Tile;
                if (!IsClickWithinReach(clickedTile))
                    return;

                Vector2 tile = e.Cursor.Tile;
                loc.Objects.TryGetValue(tile, out StardewValley.Object obj);
                if (obj != null && obj.bigCraftable.Value)
                {
                    if (obj.ParentSheetIndex.Equals(ClothingCatalogueID))
                    {
                        Game1.activeClickableMenu = new DresserMenu();
                        helper.Input.Suppress(e.Button);
                    }
                    else if (obj.ParentSheetIndex.Equals(CustomizationMirrorID))
                    {
                        Game1.activeClickableMenu = new CustomizeAnywhereMenu();
                        helper.Input.Suppress(e.Button);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (JsonAssets != null)
            {
                ClothingCatalogueID = JsonAssets.GetBigCraftableId("Clothing Catalogue");
                CustomizationMirrorID = JsonAssets.GetBigCraftableId("Customization Mirror");

                if (ClothingCatalogueID == -1)
                {
                    ModEntry.monitor.Log("Could not get the ID for Clothing catalogue",LogLevel.Warn);
                }
                if (CustomizationMirrorID == -1)
                {
                    ModEntry.monitor.Log("Could not get the ID for customization mirror", LogLevel.Warn);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                ModEntry.monitor.Log("Json Assets API not detected: mirror and catalogue items not added", LogLevel.Info);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(helper.DirectoryPath, "assets"));
            }
        }

    }
}
