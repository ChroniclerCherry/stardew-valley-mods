using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using FarmRearranger.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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

    private bool IsArranging;

    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry

    private string FarmRearrangeId = null!; // set in Entry
    private string FarmRearrangeQualifiedId = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        I18n.Init(helper.Translation);
        this.FarmRearrangeId = this.ModManifest.UniqueID + "_FarmRearranger";
        this.FarmRearrangeQualifiedId = ItemRegistry.type_bigCraftable + this.FarmRearrangeId;

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
        //ignore input if the player isnt free to move aka world not loaded,
        //they're in an event, a menu is up, etc
        if (!Context.CanPlayerMove)
            return;

        //action button works for right click on mouse and action button for controllers
        if (!e.Button.IsActionButton())
            return;

        // check if the clicked tile contains a Farm Rearranger
        bool hasCursor = Constants.TargetPlatform != GamePlatform.Android && Game1.wasMouseVisibleThisFrame; // note: only reliable when a menu isn't open
        Vector2 tile = hasCursor
            ? this.Helper.Input.GetCursorPosition().Tile
            : this.GetFacingTile(Game1.player);
        if (Game1.currentLocation.Objects.TryGetValue(tile, out Object? obj) && obj.QualifiedItemId == this.FarmRearrangeQualifiedId)
        {
            if (Game1.currentLocation.Name == "Farm" || this.Config.CanArrangeOutsideFarm)
                this.RearrangeFarm();
            else
                Game1.activeClickableMenu = new DialogueBox(I18n.CantBuildOffFarm());
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicking" />
    /// <remarks>When move buildings is exited, by default it returns the player to Robin's house and the menu becomes the menu to choose buildings. This detects when that happens and returns the player to their original location and closes the menu.</remarks>
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (this.IsArranging)
        {
            if (Game1.activeClickableMenu is not CarpenterMenu menu)
                this.IsArranging = false;
            else if (!menu.onFarm)
            {
                this.IsArranging = false;
                Game1.exitActiveMenu();
            }
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

                data[this.FarmRearrangeId] = new BigCraftableData
                {
                    Name = this.FarmRearrangeId,
                    DisplayName = TokenStringBuilder.LocalizedText($"Strings\\BigCraftables:{this.FarmRearrangeId}_Name"),
                    Description = TokenStringBuilder.LocalizedText($"Strings\\BigCraftables:{this.FarmRearrangeId}_Description"),
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
                        Id = this.FarmRearrangeId,
                        ItemId = this.FarmRearrangeId,
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

                data[$"{this.FarmRearrangeId}_Name"] = I18n.FarmRearrangerName();
                data[$"{this.FarmRearrangeId}_Description"] = I18n.FarmRearrangerDescription();
            });
        }
    }

    /// <summary>
    /// Brings up the menu to move the building
    /// </summary>
    private void RearrangeFarm()
    {
        //our boolean to keep track that we are currently in a Farm rearranger menu
        //so we don't mess with any other vanilla warps to robin's house
        this.IsArranging = true;

        //open the carpenter menu then do everything that is normally done
        //when the move buildings option is clicked
        CarpenterMenu menu = new(Game1.builder_robin);
        Game1.activeClickableMenu = menu;
        Game1.globalFadeToBlack(menu.setUpForBuildingPlacement);
        Game1.playSound("smallSelect");

        menu.onFarm = true;
        menu.Action = CarpenterMenu.CarpentryAction.Move;
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
}
