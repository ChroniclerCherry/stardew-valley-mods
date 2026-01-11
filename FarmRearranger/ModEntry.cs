using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using FarmRearranger.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace FarmRearranger;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mail ID for the farm rearranger intro letter.</summary>
    private const string MailId = "FarmRearrangerMail";

    /// <summary>The unqualified item ID for the Farm Rearranger item.</summary>
    private const string FarmRearrangeId = "cel10e.Cherry.FarmRearranger_FarmRearranger";

    /// <summary>The qualified item ID for the Farm Rearranger item.</summary>
    private const string FarmRearrangeQualifiedId = ItemRegistry.type_bigCraftable + FarmRearrangeId;

    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry

    /// <summary>Whether we're currently showing the move-buildings UI.</summary>
    private readonly PerScreen<bool> IsArranging = new();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        I18n.Init(helper.Translation);

        // read config
        this.Config = helper.ReadConfig<ModConfig>();

        // hook events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForFarmRearranger(),
            get: () => this.Config,
            set: config =>
            {
                this.Config = config;
                this.Helper.GameContent.InvalidateCache("Data/BigCraftables");
                this.Helper.GameContent.InvalidateCache("Data/Shops");
            });
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    /// <remarks>This checks for friendship with robin at the start of the day.</remarks>
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        // if friendship is higher enough, add the letter to the mailbox
        if (Game1.player.getFriendshipLevelForNPC("Robin") >= this.Config.FriendshipPointsRequired)
            Game1.addMail(MailId);
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    /// <remarks>This checks if the farm rearranger was clicked, then opens the menu if applicable.</remarks>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        // ignore input if world not loaded, they're in an event, a menu is up, etc
        if (!Context.CanPlayerMove || Game1.locationRequest is not null)
            return;

        // activated a Farm Rearranger
        if (e.Button.IsActionButton() && !this.IsArranging.Value)
        {
            bool hasCursor = Constants.TargetPlatform != GamePlatform.Android && Game1.wasMouseVisibleThisFrame; // note: only reliable when a menu isn't open
            Vector2 tile = hasCursor
                ? this.Helper.Input.GetCursorPosition().Tile
                : this.GetFacingTile(Game1.player);

            if (Game1.currentLocation.Objects.GetValueOrDefault(tile) is { QualifiedItemId: FarmRearrangeQualifiedId })
            {
                GameLocation? location = this.GetTargetLocation(Game1.currentLocation);

                if (location != null)
                    this.StartRearranging(location);
                else
                    Game1.activeClickableMenu = new DialogueBox(I18n.NoBuildingsToMove());
            }
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicking" />
    /// <remarks>When move buildings is exited, by default it returns the player to Robin's house and the menu becomes the menu to choose buildings. This detects when that happens and returns the player to their original location and closes the menu.</remarks>
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (this.IsArranging.Value)
        {
            if (Game1.activeClickableMenu is not CarpenterMenu { onFarm: true })
                this.StopRearranging();
        }
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        string modId = this.ModManifest.UniqueID;

        // add item data
        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, BigCraftableData>().Data;

                data[FarmRearrangeId] = new BigCraftableData
                {
                    Name = FarmRearrangeId,
                    DisplayName = TokenStringBuilder.LocalizedText($"Strings\\BigCraftables:{FarmRearrangeId}_Name"),
                    Description = TokenStringBuilder.LocalizedText($"Strings\\BigCraftables:{FarmRearrangeId}_Description"),
                    Price = 1,
                    Texture = $"LooseSprites/{modId}"
                };
            });
        }

        // add to shop
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;

                if (data.TryGetValue(Game1.shop_carpenter, out ShopData? shop))
                {
                    shop.Items.Add(new ShopItemData
                    {
                        Id = FarmRearrangeId,
                        ItemId = FarmRearrangeId,
                        Price = this.Config.Price,
                        Condition = $"PLAYER_HAS_MAIL Current {MailId} Received"
                    });
                }
            });
        }

        // add mail
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;
                data[MailId] = I18n.RobinLetter();
            });
        }

        // add texture
        else if (e.NameWithoutLocale.IsEquivalentTo($"LooseSprites/{modId}"))
            e.LoadFromModFile<Texture2D>("assets/farm-rearranger.png", AssetLoadPriority.Exclusive);

        // add translation text
        else if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                data[$"{FarmRearrangeId}_Name"] = I18n.FarmRearrangerName();
                data[$"{FarmRearrangeId}_Description"] = I18n.FarmRearrangerDescription();
            });
        }
    }

    /// <summary>Switch the game to the move-building UI.</summary>
    /// <param name="targetLocation">The location whose buildings to manage.</param>
    private void StartRearranging(GameLocation targetLocation)
    {
        //our boolean to keep track that we are currently in a Farm rearranger menu
        //so we don't mess with any other vanilla warps to robin's house
        this.IsArranging.Value = true;

        //open the carpenter menu then do everything that is normally done
        //when the move buildings option is clicked
        CarpenterMenu menu = new(Game1.builder_robin, targetLocation);

        Game1.activeClickableMenu = menu;
        Game1.globalFadeToBlack(menu.setUpForBuildingPlacement);
        Game1.playSound("smallSelect");

        menu.onFarm = true;
        menu.Action = CarpenterMenu.CarpentryAction.Move;
    }

    /// <summary>Reset the game to the normal mode.</summary>
    private void StopRearranging()
    {
        this.IsArranging.Value = false;

        if (Game1.activeClickableMenu is CarpenterMenu)
            Game1.exitActiveMenu();

        if (Game1.locationRequest != null)
            Game1.locationRequest.OnWarp += ResetPlayer;

        ResetPlayer();

        void ResetPlayer()
        {
            Game1.player.viewingLocation.Value = null;
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.displayFarmer = true;
        }
    }

    /// <summary>Get the tile the player is facing.</summary>
    /// <param name="player">The player to check.</param>
    private Vector2 GetFacingTile(Farmer player)
    {
        Vector2 tile = player.Tile;
        return player.FacingDirection switch
        {
            Game1.up => tile + new Vector2(0, -1),
            Game1.right => tile + new Vector2(1, 0),
            Game1.left => tile + new Vector2(-1, 0),
            _ => tile + new Vector2(0, 1)
        };
    }

    /// <summary>Get the location whose buildings can be moved by a Farm Rearranger in a given location.</summary>
    /// <param name="currentLocation">The location containing the Farm Rearranger.</param>
    private GameLocation? GetTargetLocation(GameLocation currentLocation)
    {
        for (GameLocation location = currentLocation; location != null; location = location.GetParentLocation())
        {
            if (location.buildings.Count > 0 && location.IsActiveLocation())
                return location;
        }

        return null;
    }
}
