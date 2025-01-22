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
    /// <summary>The mod settings.</summary>
    private ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.CanPlayerMove || !this.Config.Enabled)
            return;

        if (this.Config.FurnitureKey.JustPressed())
            Utility.TryOpenShopMenu("Furniture Catalogue", null as string);
        else if (this.Config.WallpaperKey.JustPressed())
            Utility.TryOpenShopMenu("Catalogue", null as string);
    }
}
