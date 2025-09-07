using System.Collections.Generic;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using HayBalesAsSilos.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace HayBalesAsSilos;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry

    /// <summary>The unqualified item ID for the Hay Bale item.</summary>
    private const string HayBaleId = "45";

    /// <summary>The qualified item ID for the Hay Bale item.</summary>
    internal const string HayBaleQualifiedId = ItemRegistry.type_bigCraftable + HayBaleId;

    /// <summary>The cached number of hay bales in each location.</summary>
    /// <remarks>Most code should use <see cref="CountHayBalesIn"/> instead.</remarks>
    private readonly Dictionary<string, int> HayBalesByLocation = [];


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        this.Config = helper.ReadConfig<ModConfig>();
        I18n.Init(helper.Translation);
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor, () => this.Config, this.CountHayBalesIn);

        // hook events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.GameLoop.TimeChanged += this.GameLoopOnTimeChanged;
        helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }


    /*********
     ** Private methods
     *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForHayBaysAsSilos(),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        // handle click on hay bale
        if (Context.CanPlayerMove && (e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            GameLocation location = Game1.currentLocation;

            Vector2 tile = this.Helper.Input.GetCursorPosition().GrabTile;
            if (location.Objects.GetValueOrDefault(tile) is { QualifiedItemId: HayBaleQualifiedId })
            {
                if (this.Config.RequiresConstructedSilo && location.getBuildingByType("Silo") is null)
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
                    return;
                }

                if (e.Button.IsActionButton())
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay", location.piecesOfHay.Value, location.GetHayCapacity()));
                else if (e.Button.IsUseToolButton())
                {
                    //if holding hay, try to add it
                    if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Hay")
                    {
                        int stack = Game1.player.ActiveObject.Stack;
                        int tryToAddHay = location.tryToAddHay(Game1.player.ActiveObject.Stack);
                        Game1.player.ActiveObject.Stack = tryToAddHay;

                        if (Game1.player.ActiveObject.Stack < stack)
                        {
                            Game1.playSound("Ship");
                            DelayedAction.playSoundAfterDelay("grassyStep", 100);
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", stack - Game1.player.ActiveObject.Stack));
                        }
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                    }
                }
            }
        }
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // edit hay bale text
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                data["OrnamentalHayBale_Name"] = I18n.DisplayName();
                data["OrnamentalHayBale_Description"] = I18n.Description(capacity: this.Config.HayPerBale);
            });
        }

        // add to shop
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;

                if (data.TryGetValue(Game1.shop_animalSupplies, out ShopData? shop))
                {
                    foreach (ShopItemData item in shop.Items)
                    {
                        if (item?.ItemId is HayBaleId or HayBaleQualifiedId)
                            item.Price = this.Config.HayBalePrice;
                    }
                }
            });
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.TimeChanged" />
    private void GameLoopOnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        this.HayBalesByLocation.Clear();
    }

    /// <inheritdoc cref="IWorldEvents.ObjectListChanged" />
    private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
    {
        string locationKey = e.Location.NameOrUniqueName;

        if (locationKey != null)
            this.HayBalesByLocation.Remove(locationKey);
    }

    /// <summary>Get the number of hay bales currently placed in a given location.</summary>
    /// <param name="location">The location to check.</param>
    private int CountHayBalesIn(GameLocation? location)
    {
        if (location is null)
            return 0;

        string? locationKey = location.NameOrUniqueName;
        if (locationKey is null)
            return 0;

        if (!this.HayBalesByLocation.TryGetValue(locationKey, out int count))
            this.HayBalesByLocation[locationKey] = count = location.numberOfObjectsOfType(HayBaleId, bigCraftable: true);

        return count;
    }
}
