using System;
using System.Collections.Generic;
using BetterGreenhouse.src;
using BetterGreenhouse.Upgrades;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace BetterGreenhouse.Interaction
{
    class UpgradeMenu
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private bool _isJoja;

        List<Response[]> ResponsePages;
        private int _currentPage = 0;
        public UpgradeMenu(IModHelper helper, IMonitor monitor, bool isJoja)
        {
            _helper = helper;
            _monitor = monitor;
            _isJoja = isJoja;

            BuildResponses();
            Game1.currentLocation.createQuestionDialogue("", ResponsePages[_currentPage],ButtonSelected);
        }

        private void ButtonSelected(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "Exit")
            {
                return;
            }

            if (whichAnswer == "More")
            {
                Game1.activeClickableMenu = null;
                _currentPage++;
                Game1.currentLocation.createQuestionDialogue("", ResponsePages[_currentPage], ButtonSelected);
                return;
            }

            State.UpgradeForTonight = whichAnswer;
            if (_isJoja)
            {
                Game1.drawObjectDialogue(_helper.Translation.Get("JojaUpgradeAccept"));
            }
            else
            {
                Game1.drawObjectDialogue(_helper.Translation.Get("JunimoUpgradeAccept"));
            }

        }

        private void BuildResponses()
        {
            ResponsePages = new List<Response[]>();
            List<Response> ResponsesThisPage = new List<Response>();

            for (var index = 0; index < State.Upgrades.Count; index++)
            {
                var upgrade = State.Upgrades[index];
                if (upgrade.Unlocked) continue;
                string text;
                
                text = _isJoja ? $"{upgrade.translatedName} : {upgrade.Cost}$" 
                    : $"{upgrade.translatedName} : {upgrade.Cost}";

                ResponsesThisPage.Add(new Response(upgrade.Name, text));

                if (ResponsesThisPage.Count >= 3)
                {
                    //if there's more options
                    if (index < State.Upgrades.Count - 1)
                        ResponsesThisPage.Add(new Response("More", "More"));
                    ResponsesThisPage.Add(new Response("Exit", "More"));
                    ResponsePages.Add(ResponsesThisPage.ToArray());
                    ResponsesThisPage = new List<Response>();
                }
            }
        }
    }
}
