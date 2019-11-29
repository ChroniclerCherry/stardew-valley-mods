using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CustomizeAnywhere
{
    public class ModEntry : Mod
    {
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {

            // ignore if player hasn't loaded a save yet
            if (!Context.CanPlayerMove)
                return;

            this.Config = this.Helper.ReadConfig<ModConfig>();
            var input = this.Helper.Input;
            if (input.IsDown(this.Config.ActivateButton)) {
                if (input.IsDown(this.Config.customizeButton))
                {
                    Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard); ;
                } 
                else if (input.IsDown(this.Config.changeButton))
                {
                    Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Dresser);
                }

            }

        }//end buttonPressed event


    }//end ModEntry

    class ModConfig
    {
        public SButton ActivateButton { get; set; }
        public SButton customizeButton { get; set; }
        public SButton changeButton { get; set; }

        public ModConfig()
        {
            ActivateButton = SButton.LeftShift;
            customizeButton = SButton.D1;
            changeButton = SButton.D2;
        }
    }
    
}