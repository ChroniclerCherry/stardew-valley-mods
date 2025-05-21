#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ShopTileFramework.Framework;
using ShopTileFramework.Framework.Apis;
using ShopTileFramework.Framework.Shop;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using xTile.ObjectModel;
using SObject = StardewValley.Object;

namespace ShopTileFramework;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    //The following variables are to help revert hardcoded warps done by the carpenter and
    //animal shop menus
    private bool ChangedMarnieStock;
    private static Vector2 PlayerPos = Vector2.Zero;


    /*********
    ** Accessors
    *********/
    public static IModHelper StaticHelper;
    public static IMonitor StaticMonitor;
    public static GameLocation SourceLocation;

    public static bool VerboseLogging;

    public static bool JustOpenedVanilla;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor);

        //make helper and monitor static so they can be accessed in other classes
        ModEntry.StaticHelper = helper;
        ModEntry.StaticMonitor = this.Monitor;

        //set verbose logging
        VerboseLogging = helper.ReadConfig<ModConfig>().VerboseLogging;

        if (VerboseLogging)
            this.Monitor.Log("Verbose logging has been turned on. More information will be printed to the console.", LogLevel.Info);

        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;

        //add console commands
        new ConsoleCommands().Register(helper);

        //get all the info from content packs
        ShopManager.LoadContentPacks();
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        //TODO: Test this
        return new ShopTileFrameworkApi();
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.UpdateTicking" />
    /// <remarks>This checks for warps from the buildings/animals menu, and ensures the player is returned to their original location.</remarks>
    private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
    {
        //Fixes the game warping the player to places we don't want them to warp
        //if buildings/animal purchase menus are brought up from a custom tile
        if (SourceLocation != null && (
            Game1.locationRequest?.Name == "AnimalShop" ||
            Game1.locationRequest?.Name == "WizardHouse" ||
            Game1.locationRequest?.Name == "ScienceHouse"))
        {
            Game1.locationRequest.Location = SourceLocation;
            Game1.locationRequest.IsStructure = SourceLocation.isStructure.Value;
        }
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    /// <remarks>This stops Marnie's portrait from appearing in non-Marnie animal shops after animal purchasing, and removes specified animals from Marnie's store.</remarks>
    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        //this block fixes marnie's portrait popping up after purchasing an animal
        if (e.OldMenu is PurchaseAnimalsMenu && e.NewMenu is DialogueBox && SourceLocation != null)
        {
            string animalPurchaseMessage = ((DialogueBox)e.NewMenu).getCurrentString();

            //go away marnie we don't want you
            Game1.exitActiveMenu();

            //display the animal purchase message without Marnie's face
            Game1.activeClickableMenu = new DialogueBox(animalPurchaseMessage);
        }

        //TODO: deprecate this once FAVR is out
        //this is the vanilla Marnie menu for us to exclude animals from
        if (e.NewMenu is PurchaseAnimalsMenu && SourceLocation == null &&
            !this.ChangedMarnieStock && AnimalShop.ExcludeFromMarnie.Count > 0)
        {
            //close the current menu to open our own	
            Game1.exitActiveMenu();
            List<SObject> allAnimalsStock = Utility.getPurchaseAnimalStock(Game1.getFarm());
            this.ChangedMarnieStock = true;

            //removes all animals on the exclusion list
            List<SObject> newAnimalStock = allAnimalsStock
                .Where(animal => !AnimalShop.ExcludeFromMarnie.Contains(animal.Name))
                .ToList();
            Game1.activeClickableMenu = new PurchaseAnimalsMenu(newAnimalStock);
        }

        //idk why some menus have a habit of warping the player a tile to the left ocassionally
        //so im just gonna warp them back to their original location eh
        if (e.NewMenu == null && PlayerPos != Vector2.Zero)
        {
            Game1.player.position.Set(PlayerPos);
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    /// <remarks>On save loaded, store the language for translation purposes. Done on save loaded in case it's changed between saves. This also retrieve all object information, done on save loaded because that's when JA adds custom items.</remarks>
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        Translations.UpdateSelectedLanguage();
        ShopManager.UpdateTranslations();

        ItemsUtil.UpdateObjectInfoSource();
        ShopManager.InitializeItemStocks();

        ItemsUtil.RegisterItemsToRemove();
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    /// <remarks>This initializes all the shops and registers all external APIs.</remarks>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        ShopManager.InitializeShops();

        ApiManager.RegisterJsonAssets();
        if (ApiManager.JsonAssets != null)
            ApiManager.JsonAssets.AddedItemsToShop += this.OnJsonAssetsAddedItemsToShop;

        ApiManager.RegisterExpandedPreconditionsUtility();
    }

    private void OnJsonAssetsAddedItemsToShop(object sender, EventArgs e)
    {
        //make sure we only remove all objects if we camew from a vanilla store
        //this stops us from removing all packs from custom TMXL or STF stores
        if (!JustOpenedVanilla)
            return;

        if (Game1.activeClickableMenu is ShopMenu shop)
        {
            shop.setItemPriceAndStock(ItemsUtil.RemoveSpecifiedJsonAssetsPacks(shop.itemPriceAndStock));
        }

        JustOpenedVanilla = false;
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    /// <remarks>This refreshes the stock of every store at the start of each day.</remarks>
    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        ShopManager.UpdateStock();
    }

    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    /// <remarks>When input is received, check that the player is free and used an action button. If so, attempt open the shop if it exists.</remarks>
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        //context and button check
        if (!Context.CanPlayerMove)
            return;

        //Resets the boolean I use to check if a menu used to move the player around came from my mod
        //and lets me return them to their original location
        SourceLocation = null;
        PlayerPos = Vector2.Zero;
        //checks if i've changed marnie's stock already after opening her menu
        this.ChangedMarnieStock = false;

        if (Constants.TargetPlatform == GamePlatform.Android)
        {
            if (e.Button != SButton.MouseLeft)
                return;
            if (e.Cursor.GrabTile != e.Cursor.Tile)
                return;

            if (VerboseLogging)
                this.Monitor.Log("Input detected!");
        }
        else if (!e.Button.IsActionButton())
            return;

        Vector2 clickedTile = this.Helper.Input.GetCursorPosition().GrabTile;

        //check if there is a tile property on Buildings layer
        IPropertyCollection tileProperty = TileUtility.GetTileProperty(Game1.currentLocation, "Buildings", clickedTile);

        if (tileProperty == null)
            return;

        //if there is a tile property, attempt to open shop if it exists
        this.CheckForShopToOpen(tileProperty, e);
    }

    /// <summary>
    /// Checks the tile property for shops, and open them
    /// </summary>
    /// <param name="tileProperty"></param>
    /// <param name="e"></param>
    private void CheckForShopToOpen(IPropertyCollection tileProperty, ButtonPressedEventArgs e)
    {
        //check if there is a Shop property on clicked tile
        tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);
        if (VerboseLogging)
            this.Monitor.Log($"Shop Property value is: {shopProperty}");
        if (shopProperty != null) //There was a `Shop` property so attempt to open shop
        {
            //check if the property is for a vanilla shop, and gets the shopmenu for that shop if it exists
            if (TileUtility.TryOpenVanillaShop(shopProperty, out bool warpingShop))
            {
                if (warpingShop)
                {
                    SourceLocation = Game1.currentLocation;
                    PlayerPos = Game1.player.position.Get();
                }

                //stop the click action from going through after the menu has been opened
                this.Helper.Input.Suppress(e.Button);
            }
            else //no vanilla shop found
            {
                //Extract the tile property value
                string shopName = shopProperty.ToString();

                if (ShopManager.ItemShops.ContainsKey(shopName))
                {
                    //stop the click action from going through after the menu has been opened
                    this.Helper.Input.Suppress(e.Button);
                    ShopManager.ItemShops[shopName].DisplayShop();
                }
                else
                {
                    this.Monitor.Log($"A Shop tile was clicked, but a shop by the name \"{shopName}\" could not be opened.", LogLevel.Debug);
                }
            }
        }
        else //no shop property found
        {
            tileProperty.TryGetValue("AnimalShop", out shopProperty); //see if there's an AnimalShop property
            if (shopProperty != null) //no animal shop found
            {
                string shopName = shopProperty.ToString();
                if (ShopManager.AnimalShops.ContainsKey(shopName))
                {
                    //stop the click action from going through after the menu has been opened
                    this.Helper.Input.Suppress(e.Button);
                    ShopManager.AnimalShops[shopName].DisplayShop();
                }
                else
                {
                    this.Monitor.Log($"An Animal Shop tile was clicked, but a shop by the name \"{shopName}\" " +
                        $"was not found.", LogLevel.Debug);
                }
            }
        } //end shopProperty null check
    }
}
