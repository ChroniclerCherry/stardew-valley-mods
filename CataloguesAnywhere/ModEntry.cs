using CataloguesAnywhere.Framework;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace CataloguesAnywhere;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForCataloguesAnywhere(() => this.Config),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.CanPlayerMove || !this.Config.Enabled)
            return;

        foreach ((string shopId, KeybindList keybind) in this.Config.Catalogues)
        {
            if (keybind.JustPressed())
            {
                this.Monitor.Log($"Opening shop ID '{shopId}' per keybind '{keybind}'.");
                Utility.TryOpenShopMenu(shopId, null as string);
                break;
            }
        }
    }
}
