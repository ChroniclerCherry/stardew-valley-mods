using CataloguesAnywhere.Framework;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

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
        if (!Context.IsWorldReady || !this.Config.Enabled)
            return;

        foreach ((string shopId, KeybindList keybind) in this.Config.Catalogues)
        {
            if (keybind.JustPressed())
            {
                if (this.CanOpenShopMenu(shopId))
                {
                    this.Monitor.Log($"Opening shop ID '{shopId}' per keybind '{keybind}'.");
                    Utility.TryOpenShopMenu(shopId, null as string);
                }

                break;
            }
        }
    }

    /// <summary>Get whether a given shop menu can be opened now.</summary>
    /// <param name="shopId">The shop ID being opened.</param>
    private bool CanOpenShopMenu(string shopId)
    {
        return
            // no menu open
            Context.CanPlayerMove

            // allow switching from one catalogue to another
            || (
                Game1.activeClickableMenu is ShopMenu { ShopId: not null } shopMenu
                && shopMenu.ShopId != shopId
                && this.Config.Catalogues.ContainsKey(shopMenu.ShopId)
                && shopMenu.readyToClose()
            );
    }
}
