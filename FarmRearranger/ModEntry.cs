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
using System.Collections.Generic;

namespace FarmRearranger
{
    class ModEntry : Mod
    {
        private bool isArranging = false;

        private GameLocation loc = null;

        private ModConfig Config;

        private string FarmRearrangeId;
        private string FarmRearrangeQualifiedId;


        /// <summary>
        /// Entry function, the starting point of the mod called by SMAPI
        /// </summary>
        /// <param name="helper">provides useful apis for modding</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // init fields
            this.FarmRearrangeId = this.ModManifest.UniqueID + "_FarmRearranger";
            this.FarmRearrangeQualifiedId = ItemRegistry.type_bigCraftable + this.FarmRearrangeId;

            // hook events
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
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

            //check if the clicked tile contains a Farm Renderer
            Vector2 tile = Helper.Input.GetCursorPosition().Tile;
            if (Game1.currentLocation.Objects.TryGetValue(tile, out Object obj) && obj.QualifiedItemId == this.FarmRearrangeQualifiedId)
            {
                if (Game1.currentLocation.Name == "Farm" || Config.CanArrangeOutsideFarm)
                    RearrangeFarm();
                else
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("CantBuildOffFarm"));
            }
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

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
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

                    if (data.TryGetValue(Game1.shop_carpenter, out ShopData shop))
                    {
                        shop.Items.Add(new ShopItemData
                        {
                            Id = this.FarmRearrangeId,
                            ItemId = this.FarmRearrangeId,
                            Price = Config.Price,
                            Condition = "PLAYER_HAS_MAIL Current FarmRearrangerMail Received"
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
                    data["FarmRearrangerMail"] = Helper.Translation.Get("robinletter");
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

                    data[$"{this.FarmRearrangeId}_Name"] = this.Helper.Translation.Get("FarmRearranger_Name");
                    data[$"{this.FarmRearrangeId}_Description"] = this.Helper.Translation.Get("FarmRearranger_Description");
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
            isArranging = true;

            //remember the location, which should be Farm, but could be anywhere depending on configs
            loc = Game1.currentLocation;

            //for some reason the player gets watrped one tile to the left when using this menu on the farm
            //so i move them a tile to the right to prevent them getting warped into a solid tile
            if (loc.Name == "Farm")
            {
                var pos = Game1.player.Position;
                Game1.player.Position = new Vector2(pos.X + 64, pos.Y);
            }
            

            //open the carpenter menu then do everything that is normally done
            //when the move buildings option is clicked
            var menu = new CarpenterMenu();
            Game1.activeClickableMenu = menu;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(menu.setUpForBuildingPlacement), 0.02f);
            Game1.playSound("smallSelect");

            Helper.Reflection.GetField<bool>(menu, "onFarm").SetValue(true);
            Helper.Reflection.GetField<bool>(menu, "moving").SetValue(true);
        }
    }
}
