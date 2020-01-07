using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.IO;

namespace FarmRearranger
{
    class ModEntry : Mod, IAssetEditor
    {
        private bool isArranging = false;
        private GameLocation loc = null;
        private IJsonAssetsApi JsonAssets;
        private ModConfig Config;
        private int FarmRearrangerID;

        /// <summary>
        /// Entry function, the starting point of the mod called by SMAPI
        /// </summary>
        /// <param name="helper">provides useful apis for modding</param>
        public override void Entry(IModHelper helper)
        {
            //read the config and save it
            this.Config = this.Helper.ReadConfig<ModConfig>();

            //all the events
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        /// <summary>
        /// Checks for when Robin's store is open and adds the Farm Renderer to the stock as necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            //we don't care if it's not a shop menu
            if (!(e.NewMenu is ShopMenu))
                return;

            var shop = (ShopMenu)e.NewMenu;

            //we don't care if it's not robin's store
            if (shop.portraitPerson == null || !(shop.portraitPerson.Name == "Robin"))
                return;

            //ignore if player hasn't seen the mail yet
            if (!Game1.player.mailReceived.Contains("FarmRearrangerMail"))
                return;

            //create the farm renderer object and add it to robin's stock
            var itemStock = shop.itemPriceAndStock;
            var obj = new StardewValley.Object(Vector2.Zero, FarmRearrangerID);
            itemStock.Add(obj, new int[] { Config.Price, int.MaxValue });
            shop.setItemPriceAndStock(itemStock);

        }

        /// <summary>
        /// Check for friendship with robin at the end of the day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            //if friendship is higher enough, send the mail tomorrow
            if (Game1.player.getFriendshipLevelForNPC("Robin") >= Config.FriendshipPointsRequired)
            {
                Game1.addMailForTomorrow("FarmRearrangerMail");
            }
        }

        /// <summary>
        /// Get the ID for our Farm Rearranger, on save loaded as that's when JA loads stuff in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (JsonAssets != null)
            {
                FarmRearrangerID = JsonAssets.GetBigCraftableId("Farm Rearranger");

                if (FarmRearrangerID == -1)
                {
                    Monitor.Log("Could not get the ID for the Farm Rearranger item", LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// This checks if the farm rearranger was clicked then open the menu if applicable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //ignore input if the player isnt free to move aka world not loaded,
            //they're in an event, a menu is up, etc
            if (!Context.CanPlayerMove)
                return;

            //action button works for right click on mouse and action button for controllers
            if (!e.Button.IsActionButton())
                return;

            //checks if the clicked tile is actually adjacent to the player
            var clickedTile = Helper.Input.GetCursorPosition().Tile;
            if (!IsClickWithinReach(clickedTile))
                return;

            //check if the clicked tile contains a Farm Renderer
            Vector2 tile = e.Cursor.Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (obj.ParentSheetIndex.Equals(FarmRearrangerID))
                {
                    if (Game1.currentLocation.Name == "Farm" && !Config.CanArrangeOutsideFarm)
                    {
                        RearrangeFarm();
                    }
                    else
                    {
                        Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("CantBuildOffFarm"));
                    }
                }
            }
        }

        /// <summary>
        /// Checks if tile is adjacent to player
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>True if tile is adjacent to player, false if not</returns>
        private bool IsClickWithinReach(Vector2 tile)
        {
            var playerPosition = Game1.player.Position;
            var playerTile = new Vector2(playerPosition.X / 64, playerPosition.Y / 64);

            if (tile.X < (playerTile.X - 1.5) || tile.X > (playerTile.X + 1.5) ||
                tile.Y < (playerTile.Y - 1.5) || tile.Y > (playerTile.Y + 1.5))
                return false;

            return true;
        }

        /// <summary>
        /// Load the JA api and give it our content pack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        /// <summary>
        /// When move buildings is exited, on default it returns the player to Robin's house
        /// and the menu becomes the menu to choose buildings
        /// This detects when that happens and returns the player to their original location and closs the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            //if we're not currently moving buildings from our mod, ignore
            if (!isArranging)
                return;


            if (Game1.locationRequest?.Name == "ScienceHouse")
            {
                //change location to source location
                Game1.locationRequest.Location = loc;

                //no longer arranging so set our trackers to defaults
                isArranging = false;
                loc = null;

                //close the menu so players aren't gonna be asked to keep buying buildings
                Game1.exitActiveMenu();
            }
        }

        /// <summary>
        /// Brings up the menu to move the building
        /// </summary>
        private void RearrangeFarm()
        {
            //our boolean to keep track that we are currently in a Farm rearranger menu
            //so we don't mess with any other vanilla warps to robin's house
            isArranging = true;

            //remember the location, which should be Farm, but could be anywhere depending on configs
            loc = Game1.currentLocation;

            //open the carpenter menu then do everything that is normally done
            //when the move buildings option is clicked
            var menu = new CarpenterMenu();
            Game1.activeClickableMenu = menu;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(menu.setUpForBuildingPlacement), 0.02f);
            Game1.playSound("smallSelect");

            Helper.Reflection.GetField<bool>(menu, "onFarm").SetValue(true);
            Helper.Reflection.GetField<bool>(menu, "moving").SetValue(true);
        }

        /// <summary>
        /// We ony edit the mail
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        /// <returns></returns>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        /// <summary>
        /// Adds the letter to mail.xnb from the i18n translations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data["FarmRearrangerMail"]
                = Helper.Translation.Get("robinletter"); ;
        }
    }

    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }

    class ModConfig
    {
        //players can set the farm rearranger to work anywhere, just with a warning that it could behave oddly
        public bool CanArrangeOutsideFarm { get; set; } = false;

        //the price farm rearranger is sold for
        public int Price { get; set; } = 25000;

        //the friendship points before robin sends the letter
        public int FriendshipPointsRequired { get; set; } = 2000;
    }
}
