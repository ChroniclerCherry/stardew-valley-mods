using CataloguesAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CataloguesAnywhere;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.CanPlayerMove || !this.Config.Enabled)
            return;

        var input = this.Helper.Input;
        if (input.IsDown(this.Config.ActivateButton))
        {
            if (input.IsDown(this.Config.FurnitureButton))
                Utility.TryOpenShopMenu("Furniture Catalogue", null as string);
            else if (input.IsDown(this.Config.WallpaperButton))
                Utility.TryOpenShopMenu("Catalogue", null as string);
        }
    }
}
