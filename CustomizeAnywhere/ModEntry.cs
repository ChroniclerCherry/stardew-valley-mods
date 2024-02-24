using CustomizeAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CustomizeAnywhere
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        internal static IMonitor monitor;
        internal static IModHelper helper;

        private DresserAndMirror DresserAndMirror;

        public override void Entry(IModHelper h)
        {
            helper = h;
            monitor = Monitor;

            this.DresserAndMirror = new DresserAndMirror(helper, this.ModManifest.UniqueID);
            
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player isn't free to move or direct access is turned off in the config
            if (!Context.CanPlayerMove || !Config.canAccessMenusAnywhere)
                return;

            var input = this.Helper.Input;
            if (input.IsDown(this.Config.ActivateButton))
            {
                if (input.IsDown(this.Config.customizeButton))
                    Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
                else if (input.IsDown(this.Config.dresserButton))
                    this.DresserAndMirror.OpenDresser();

                if (this.Config.canTailorWithoutEvent || Game1.player.eventsSeen.Contains("992559"))
                {
                    if (input.IsDown(this.Config.dyeButton))
                    {
                        Game1.activeClickableMenu = new DyeMenu();
                    }
                    else if (input.IsDown(this.Config.tailoringButton))
                    {
                        Game1.activeClickableMenu = new TailoringMenu();
                    }
                }
            }
        }
    }
}
