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
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        // ignore if player isn't free to move or direct access is turned off in the config
        if (!Context.CanPlayerMove || !this.Config.CanAccessMenusAnywhere)
            return;

        if (this.Config.CustomizeKey.JustPressed())
            Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
        else if (this.Config.DresserKey.JustPressed())
            this.DresserAndMirror.OpenDresser();

        else if (this.Config.CanTailorWithoutEvent || Game1.player.eventsSeen.Contains("992559"))
        {
            if (this.Config.DyeKey.JustPressed())
                Game1.activeClickableMenu = new DyeMenu();
            else if (this.Config.TailoringKey.JustPressed())
                Game1.activeClickableMenu = new TailoringMenu();
        }
    }
}
