using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewAquarium.Menu;
using StardewAquarium.Tokens;
using StardewValley;

namespace StardewAquarium
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);
            Utils.Initialize(Helper, Monitor);
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            new TokenHandler(Helper,Monitor,ModManifest).RegisterTokens();
        }

        private void TryToOpenDonationMenu()
        {
            if (!Utils.DoesPlayerHaveDonatableFish())
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("NothingToDonate"));
                return;
            }


            List<Response> options = new List<Response>
            {   new Response("OptionYes", Helper.Translation.Get("OptionYes")),
                new Response("OptionNo", Helper.Translation.Get("OptionNo"))
            };

            Game1.currentLocation.createQuestionDialogue(Helper.Translation.Get("DonationQuestion"), options.ToArray(), HandleResponse);

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

        private void OpenDonationMenuCommand(string arg1, string[] arg2)
        {
            TryToOpenDonationMenu();
        }
    }
}
