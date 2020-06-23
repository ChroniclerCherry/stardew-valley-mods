using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewAquarium.Menu;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    public partial class ModEntry
    {

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

            if (tileProperty != "AquariumDonationMenu")
                return;

            TryToOpenDonationMenu();
        }


        private void TryToOpenDonationMenu()
        {
            if (!Utils.DoesPlayerHaveDonatableFish())
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("NothingToDonate"));
                return;
            }


            List<Response> options = new List<Response>
            {
                new Response("OptionYes", Helper.Translation.Get("OptionYes")),
                new Response("OptionNo", Helper.Translation.Get("OptionNo"))
            };

            Game1.currentLocation.createQuestionDialogue(Helper.Translation.Get("DonationQuestion"), options.ToArray(),
                HandleResponse);
        }

        private void HandleResponse(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "OptionNo")
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("DeclineToDonate"));
                return;
            }

            if (whichAnswer == "OptionYes")
            {
                Game1.activeClickableMenu = new DonateFishMenu();
            }
        }
    }
}