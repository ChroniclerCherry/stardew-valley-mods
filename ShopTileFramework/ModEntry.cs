using Microsoft.Xna.Framework;
using ShopTileFramework.Framework.API;
using ShopTileFramework.Framework.Shop;
using ShopTileFramework.Framework.Utility;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using xTile.ObjectModel;

namespace ShopTileFramework
{
    class ModEntry : Mod
    {
        internal static IModHelper helper;
        internal static IMonitor monitor;

        private bool ChangedMarnieStock = false;

        internal static GameLocation SourceLocation = null;
        internal static Vector2 PlayerPos = Vector2.Zero;

        /// <summary>
        /// the Mod entry point called by SMAPI
        /// </summary>
        /// <param name="h">the helper provided by SMAPI</param>
        public override void Entry(IModHelper h)
        {
            //make helper and monitor static so they can be accessed in other classes
            helper = h;
            monitor = Monitor;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

            //add console commands
            new ConsoleCommands().Register();

            //get all the info from content packs
            ShopManager.LoadContentPacks();

            /*
            //harmony black magic
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Utility), "getPurchaseAnimalStock"),
                postfix: new HarmonyMethod(typeof(PatchGetPurchaseAnimalStock), nameof(PatchGetPurchaseAnimalStock.PostFix))
                );
                */
        }
        /// <summary>
        /// Checks for warps from the buildings/animals menu 
        /// and ensures the player is returned to their original location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
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
        /// <summary>
        /// Stops Marnie's portrait from appearing in non-Marnie animal shops after animal purchasing
        /// And removes specified animals from Marnie's store
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            //this block fixes marnie's portrait popping up after purchasing an animal
            if (e.OldMenu is PurchaseAnimalsMenu && e.NewMenu is DialogueBox && SourceLocation != null)
            {
                var AnimalPurchaseMessage = ((DialogueBox)e.NewMenu).getCurrentString();

                //go away marnie we don't want you
                Game1.exitActiveMenu();

                //display the animal purchase message without Marnie's face
                Game1.activeClickableMenu = new DialogueBox(AnimalPurchaseMessage);
            }

            //this is the vanilla Marnie menu for us to exclude animals from
            if (e.NewMenu is PurchaseAnimalsMenu && SourceLocation == null &&
                !ChangedMarnieStock && AnimalShop.ExcludeFromMarnie.Count > 0)
            {
                //close the current menu to open our own	
                Game1.exitActiveMenu();
                var AllAnimalsStock = Utility.getPurchaseAnimalStock();
                ChangedMarnieStock = true;
                var newAnimalStock = (from animal in AllAnimalsStock
                                      where !AnimalShop.ExcludeFromMarnie.Contains(animal.Name)
                                      select animal).ToList();
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(newAnimalStock);
            }

            //idk why some menus have a habit of warping the player a tile to the left ocassionally
            //so im just gonna warp them back to their original location eh
            if (e.NewMenu == null && PlayerPos != Vector2.Zero)
            {
                Game1.player.position.Set(PlayerPos);
            }
        }
        /// <summary>
        /// Returns an instance of this mod's api
        /// </summary>
        /// <returns></returns>
        public override object GetApi()
        {
            return new STFApi();
        }
        /// <summary>
        /// refreshes the object information files on each save loaded in case of ids changing due to JA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Translations.UpdateSelectedLanguage();
            ItemsUtil.UpdateObjectInfoSource();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ShopManager.InitializeShops(); //things that need to be done after the game has launched
            APIs.RegisterJsonAssets();
            APIs.RegisterBFAV();
            APIs.RegisterFAVR();

        }

        /// <summary>
        /// Refresh the stock of every store at the start of each day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ShopManager.UpdateStock();
        }

        /// <summary>
        /// When input is received, check for shop tiles to open them as necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            //context and button check
            if (!Context.CanPlayerMove)
                return;

            //Resets the boolean I use to check if a menu used to move the player around came from my mod
            //and lets me return them to their original location
            SourceLocation = null;
            PlayerPos = Vector2.Zero;

            //checks if i've changed marnie's stock already after opening her menu
            ChangedMarnieStock = false;

            if (!e.Button.IsActionButton())
                return;

            OpenShop(e);
        }

        private void OpenShop(StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {

            Vector2 clickedTile = Helper.Input.GetCursorPosition().GrabTile;

            //check if there is a tile property on Buildings layer
            var tileProperty = TileUtility.GetTileProperty(Game1.currentLocation, "Buildings", clickedTile);
            if (tileProperty == null)
                return;

            //check if there is a Shop property on clicked tile
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);
            if (shopProperty != null)
            //everything in this block is for the shop property "Shop"
            {
                IClickableMenu menu = TileUtility.CheckVanillaShop(shopProperty, out bool warpingShop);
                if (menu != null) // checks for vanilla shop properties
                {
                    if (warpingShop)
                    {
                        SourceLocation = Game1.currentLocation;
                        PlayerPos = Game1.player.position.Get();
                    }

                    helper.Input.Suppress(e.Button);
                    Game1.activeClickableMenu = menu;

                }
                else
                {
                    //Extract the tile property value
                    string ShopName = shopProperty.ToString();
                    if (ShopManager.ItemShops.ContainsKey(ShopName))
                    {
                        helper.Input.Suppress(e.Button);
                        ShopManager.ItemShops[ShopName].DisplayShop();
                    }
                    else
                    {
                        Monitor.Log($"A Shop tile was clicked, but a shop by the name \"{ShopName}\" " +
                            $"was not found.", LogLevel.Debug);
                    }
                }
            }
            else
            {
                tileProperty.TryGetValue("AnimalShop", out shopProperty);
                if (shopProperty == null)
                {
                    return;
                }
                else
                {
                    string ShopName = shopProperty.ToString();
                    if (ShopManager.AnimalShops.ContainsKey(ShopName))
                    {

                        helper.Input.Suppress(e.Button);
                        ShopManager.AnimalShops[ShopName].DisplayShop();
                    }
                    else
                    {
                        Monitor.Log($"An Animal Shop tile was clicked, but a shop by the name \"{ShopName}\" " +
                            $"was not found.", LogLevel.Debug);
                    }
                }

            }
        }
    }
}
