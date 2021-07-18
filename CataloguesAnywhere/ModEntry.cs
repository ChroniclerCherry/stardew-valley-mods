using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CataloguesAnywhere
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove || !Config.Enabled)
                return;

            var input = this.Helper.Input;
            if (input.IsDown(this.Config.ActivateButton))
            {
                if (input.IsDown(this.Config.furnitureButton))
                {
                    Game1.activeClickableMenu = (IClickableMenu)new ShopMenu(Utility.getAllFurnituresForFree(), 0, (string)null, (Func<ISalable, Farmer, int, bool>)null, (Func<ISalable, bool>)null, "Furniture Catalogue");
                }
                else if (input.IsDown(this.Config.WallpaperButton))
                {
                    Game1.activeClickableMenu = (IClickableMenu)new ShopMenu(Utility.getAllWallpapersAndFloorsForFree(), 0, (string)null, (Func<ISalable, Farmer, int, bool>)null, (Func<ISalable, bool>)null, "Catalogue");
                }
            }
        }

        class ModConfig
        {
            public bool Enabled { get; set; } = true;
            public SButton ActivateButton { get; set; } = SButton.LeftControl;
            public SButton furnitureButton { get; set; } = SButton.D1;
            public SButton WallpaperButton { get; set; } = SButton.D2;
        }
    }
}