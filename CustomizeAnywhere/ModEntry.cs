using CustomizeAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CustomizeAnywhere;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private ModConfig Config;
    internal static IModHelper StaticHelper;

    private DresserAndMirror DresserAndMirror;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModEntry.StaticHelper = helper;

        this.DresserAndMirror = new DresserAndMirror(helper, this.ModManifest.UniqueID);

        this.Config = helper.ReadConfig<ModConfig>();
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        // ignore if player isn't free to move or direct access is turned off in the config
        if (!Context.CanPlayerMove || !this.Config.CanAccessMenusAnywhere)
            return;

        var input = this.Helper.Input;
        if (input.IsDown(this.Config.ActivateButton))
        {
            if (input.IsDown(this.Config.CustomizeButton))
                Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
            else if (input.IsDown(this.Config.DresserButton))
                this.DresserAndMirror.OpenDresser();

            if (this.Config.CanTailorWithoutEvent || Game1.player.eventsSeen.Contains("992559"))
            {
                if (input.IsDown(this.Config.DyeButton))
                {
                    Game1.activeClickableMenu = new DyeMenu();
                }
                else if (input.IsDown(this.Config.TailoringButton))
                {
                    Game1.activeClickableMenu = new TailoringMenu();
                }
            }
        }
    }
}
