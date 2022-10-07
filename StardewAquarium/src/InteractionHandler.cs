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
            _helper = helper;
            _monitor = monitor;

            _helper.Events.Input.ButtonPressed += Input_ButtonPressed;

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
                _monitor.Log("AquariumDonationMenu tile detected, opening donation menu...");
                TryToOpenDonationMenu();
            }
            else if (tileProperty == "AquariumCollectionMenu")
            {
                _monitor.Log("AquariumCollectionMenu tile detected, opening collections menu...");
                Game1.activeClickableMenu = new AquariumCollectionMenu(_helper.Translation.Get("CollectionsMenu"));
            }
            else if (tileProperty.StartsWith("AquariumSign"))
            {
                new AquariumMessage(tileProperty.Split(' '));
            }
            else if (tileProperty.StartsWith("AquariumString"))
            {
                string strKey = tileProperty.Split(' ')[1];
                Game1.drawObjectDialogue(_helper.Translation.Get(strKey));
            }
        }

        private void TryToOpenDonationMenu()
        {

            if (!Utils.DoesPlayerHaveDonatableFish())
            {
                if (Game1.MasterPlayer.achievements.Contains(AchievementEditor.AchievementId))
                {
                    Game1.drawObjectDialogue(_helper.Translation.Get("AquariumWelcome"));
                    return;
                }

                Game1.drawObjectDialogue(_helper.Translation.Get("NothingToDonate"));
                return;
            }

            List<Response> options = new List<Response>
            {
                new Response("OptionYes", _helper.Translation.Get("OptionYes")),
                new Response("OptionNo", _helper.Translation.Get("OptionNo"))
            };

            Game1.currentLocation.createQuestionDialogue(_helper.Translation.Get("DonationQuestion"), options.ToArray(),
                HandleResponse);
        }

        private void HandleResponse(Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "OptionNo":
                    Game1.drawObjectDialogue(_helper.Translation.Get("DeclineToDonate"));
                    return;
                case "OptionYes" when Constants.TargetPlatform == GamePlatform.Android:
                    Game1.activeClickableMenu = new DonateFishMenuAndroid(_helper, _monitor);
                    break;
                case "OptionYes":
                    Game1.activeClickableMenu = new DonateFishMenu(_helper, _monitor);
                    break;
            }
        }
    }

}
