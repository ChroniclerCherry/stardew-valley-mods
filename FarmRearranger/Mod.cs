using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmRearranger
{
    class ModEntry : Mod, IAssetEditor
    {

        private bool isArranging = false;
        private GameLocation loc = null;
        private IJsonAssetsApi JsonAssets;
        private ModConfig Config;
        private int FarmRearrangerID;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.ConsoleCommands.Add("rearrange_farm", "Brings up the move buildings menu", RearrangeFarmCommand);

            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu))
                return;

            var shop = (ShopMenu)e.NewMenu;

            //ignore if not robin's store
            if (!(shop.portraitPerson.Name == "Robin"))
                return;

            //ignore if player hasn't seen the mail yet
            if (!Game1.player.mailReceived.Contains("FarmRearrangerMail"))
                return;

            var itemStock = shop.itemPriceAndStock;
            var obj = new StardewValley.Object(Vector2.Zero, FarmRearrangerID);
            itemStock.Add(obj, new int[]{ Config.Price, int.MaxValue });
            shop.setItemPriceAndStock(itemStock);

        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            var a = Game1.player.getFriendshipLevelForNPC("Robin");

            if (Game1.player.getFriendshipLevelForNPC("Robin") >= Config.FriendshipPointsRequired )
            {
                Game1.addMailForTomorrow("FarmRearrangerMail");
            }
        }

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

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (!e.Button.IsActionButton())
                return;

            var clickedTile = Helper.Input.GetCursorPosition().Tile;
            if (!IsClickWithinReach(clickedTile))
                return;

            Vector2 tile = e.Cursor.Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (obj.ParentSheetIndex.Equals(FarmRearrangerID))
                {
                    if (Game1.currentLocation.Name == "Farm" && !Config.CanArrangeOutsideFarm)
                    {
                        RearrangeFarm();
                    } else
                    {
                        Game1.activeClickableMenu = new DialogueBox("You can only rearrange your buildings from your farm");
                    }
                    
                }
            }

        }
        private bool IsClickWithinReach(Vector2 tile)
        {
            var playerPosition = Game1.player.Position;
            var playerTile = new Vector2(playerPosition.X / 64, playerPosition.Y / 64);

            if (tile.X < (playerTile.X - 1.5) || tile.X > (playerTile.X + 1.5))
                return false;

            if (tile.Y < (playerTile.Y - 1.5) || tile.Y > (playerTile.Y + 1.5))
                return false;

            return true;
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!isArranging)
                return;

            if (Game1.locationRequest?.Name == "ScienceHouse")
            {
                Game1.locationRequest.Location = loc;
                isArranging = false;
                loc = null;
                Game1.exitActiveMenu();
            }
        }


        private void RearrangeFarm()
        {
            isArranging = true;
            loc = Game1.currentLocation;
            var menu = new CarpenterMenu();
            Game1.activeClickableMenu = menu;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(menu.setUpForBuildingPlacement), 0.02f);
            Game1.playSound("smallSelect");

            Helper.Reflection.GetField<bool>(menu, "onFarm").SetValue(true);
            Helper.Reflection.GetField<bool>(menu, "moving").SetValue(true);
        }

        private void RearrangeFarmCommand(string command, string[] args)
        {
            if (!Context.CanPlayerMove)
                return;

            if (Game1.currentLocation.Name != "Farm")
                return;

            RearrangeFarm();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data["FarmRearrangerMail"] = Helper.Translation.Get("robinletter"); ;
        }
    }

    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }

    class ModConfig
    {
        public bool CanArrangeOutsideFarm { get; set; } = false;
        public int Price { get; set; } = 25000;
        public int FriendshipPointsRequired { get; set; } = 2000;
    }
}
