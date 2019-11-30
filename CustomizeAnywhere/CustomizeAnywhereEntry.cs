using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CustomizeAnywhere
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if ((Game1.activeClickableMenu is CharacterCustomization) && (e.Button == SButton.Escape))
            {
                Game1.exitActiveMenu();
            }

            // ignore if player isn't free to move
            if (!Context.CanPlayerMove)
                return;

            this.Config = this.Helper.ReadConfig<ModConfig>();
            var input = this.Helper.Input;
            if (input.IsDown(this.Config.ActivateButton)) {
                if (input.IsDown(this.Config.customizeButton))
                {
                    Game1.activeClickableMenu = new CustomizeAnywhereMenu(); ;
                } 
                else if (input.IsDown(this.Config.dyeButton))
                {
                    Game1.activeClickableMenu = new DyeMenu();
                }
                else if (input.IsDown(this.Config.tailoringMenu))
                {
                    Game1.activeClickableMenu = new TailoringMenu();
                }

            }

        }//end buttonPressed event


    }//end ModEntry

    class ModConfig
    {
        public SButton ActivateButton { get; set; }
        public SButton customizeButton { get; set; }
        public SButton dyeButton { get; set; }
        public SButton tailoringMenu { get; set; }

        public ModConfig()
        {
            ActivateButton = SButton.LeftShift;
            customizeButton = SButton.D1;
            dyeButton = SButton.D2;
            tailoringMenu = SButton.D3;
        }
    }
    
}