using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Menu
{
    public class MenuInteractionHandler
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public MenuInteractionHandler(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);
            helper.ConsoleCommands.Add("aquariumprogress", "", OpenAquariumCollectionMenu);

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

            if (tileProperty == "AquariumDonationMenu")
            {
                TryToOpenDonationMenu();
            } else if (tileProperty == "AquariumCollectionMenu")
            {
                Game1.activeClickableMenu = new AquariumCollectionMenu(_helper.Translation.Get("CollectionsMenu"));
            }


        }

        private void TryToOpenDonationMenu()
        {
            if (!Utils.DoesPlayerHaveDonatableFish())
            {
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
            if (whichAnswer == "OptionNo")
            {
                Game1.drawObjectDialogue(_helper.Translation.Get("DeclineToDonate"));
                return;
            }

            if (whichAnswer == "OptionYes")
            {
                Game1.activeClickableMenu = new DonateFishMenu(_helper.Translation);
            }
        }

        private void OpenAquariumCollectionMenu(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new AquariumCollectionMenu(_helper.Translation.Get("CollectionsMenu"));
        }

        private void OpenDonationMenuCommand(string arg1, string[] arg2)
        {
            TryToOpenDonationMenu();
        }
    }

}
