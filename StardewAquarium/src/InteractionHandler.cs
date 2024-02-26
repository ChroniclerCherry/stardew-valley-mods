using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewAquarium.Editors;
using StardewAquarium.Menus;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    public class InteractionHandler
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public InteractionHandler(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;

            this._helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                if (e.Button != SButton.MouseLeft)
                    return;
                if (e.Cursor.GrabTile != e.Cursor.Tile)
                    return;
            }
            else if (!e.Button.IsActionButton())
                return;

            Vector2 grabTile = e.Cursor.GrabTile;

            string tileProperty = Game1.currentLocation.doesTileHaveProperty((int)grabTile.X, (int)grabTile.Y, "Action", "Buildings");

            if (tileProperty == null)
                return;

            if (tileProperty == "AquariumDonationMenu")
            {
                this._monitor.Log("AquariumDonationMenu tile detected, opening donation menu...");
                this.TryToOpenDonationMenu();
            }
            else if (tileProperty == "AquariumCollectionMenu")
            {
                this._monitor.Log("AquariumCollectionMenu tile detected, opening collections menu...");
                Game1.activeClickableMenu = new AquariumCollectionMenu(this._helper.Translation.Get("CollectionsMenu"));
            }
            else if (tileProperty.StartsWith("AquariumSign"))
            {
                new AquariumMessage(tileProperty.Split(' '));
            }
            else if (tileProperty.StartsWith("AquariumString"))
            {
                string strKey = tileProperty.Split(' ')[1];
                Game1.drawObjectDialogue(this._helper.Translation.Get(strKey));
            }
        }

        private void TryToOpenDonationMenu()
        {
            if (!Utils.DoesPlayerHaveDonatableFish())
            {
                if (Game1.MasterPlayer.achievements.Contains(AchievementEditor.AchievementId))
                {
                    Game1.drawObjectDialogue(this._helper.Translation.Get("AquariumWelcome"));
                    return;
                }

                Game1.drawObjectDialogue(this._helper.Translation.Get("NothingToDonate"));
                return;
            }

            List<Response> options = new List<Response>
            {
                new Response("OptionYes", this._helper.Translation.Get("OptionYes")),
                new Response("OptionNo", this._helper.Translation.Get("OptionNo"))
            };

            Game1.currentLocation.createQuestionDialogue(this._helper.Translation.Get("DonationQuestion"), options.ToArray(), this.HandleResponse);
        }

        private void HandleResponse(Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "OptionNo":
                    Game1.drawObjectDialogue(this._helper.Translation.Get("DeclineToDonate"));
                    return;
                case "OptionYes" when Constants.TargetPlatform == GamePlatform.Android:
                    Game1.activeClickableMenu = new DonateFishMenuAndroid(this._helper, this._monitor);
                    break;
                case "OptionYes":
                    Game1.activeClickableMenu = new DonateFishMenu(this._helper, this._monitor);
                    break;
            }
        }
    }

}
