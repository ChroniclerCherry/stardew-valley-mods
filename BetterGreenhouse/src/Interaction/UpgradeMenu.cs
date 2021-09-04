using System.Collections.Generic;
using GreenhouseUpgrades.Upgrades;
using StardewModdingAPI;
using StardewValley;

namespace GreenhouseUpgrades.Interaction
{
    class UpgradeMenu
    {
        private readonly IModHelper _helper;
        private IMonitor _monitor;
        private readonly bool _isJoja;

        List<Response[]> _responsePages;
        private int _currentPage = 0;
        public UpgradeMenu(IModHelper helper, IMonitor monitor, bool isJoja)
        {
            _helper = helper;
            _monitor = monitor;
            _isJoja = isJoja;

            BuildResponses();
            string dialogue = _helper.Translation.Get(_isJoja ? "JojaUpgradeMenu" : "JunimoUpgradeMenu", new { JunimoPoints = Main.JunimoPoints });
            Game1.currentLocation.createQuestionDialogue(dialogue, _responsePages[_currentPage],ButtonSelected);
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
                string dialogue = _helper.Translation.Get(_isJoja ? "JojaUpgradeMenu" : "JunimoUpgradeMenu", new { JunimoPoints = Main.JunimoPoints });
                Game1.currentLocation.createQuestionDialogue(dialogue, _responsePages[_currentPage], ButtonSelected);
                return;
            }

            ApplyUpgrades(who,Utils.GetUpgradeByName(whichAnswer));

        }

        private void ApplyUpgrades(Farmer who,Upgrade upgrade)
        {
            int cost = upgrade.Cost;
            if (_isJoja)
            {
                if (who.Money < cost)
                {
                    Game1.drawObjectDialogue(_helper.Translation.Get("JojaNotEnoughMoney"));
                    return;
                }

                Game1.drawObjectDialogue(_helper.Translation.Get("JojaUpgradeAccept"));
                who.Money -= cost;
            }
            else
            {
                if (Main.JunimoPoints < cost)
                {
                    Game1.drawObjectDialogue(_helper.Translation.Get("JunimoNotEnoughPoints"));
                    return;
                }

                Game1.drawObjectDialogue(_helper.Translation.Get("JunimoUpgradeAccept"));
                Main.JunimoPoints -= cost;
                _helper.Multiplayer.SendMessage(Main.JunimoPoints, Consts.MultiplayerJunimopointsKey, new[] { Consts.ModUniqueID });
            }

            Main.UpgradeForTonight = upgrade.Name;
            _helper.Multiplayer.SendMessage(upgrade.Name, Consts.MultiplayerUpdate, new[] { Consts.ModUniqueID });
        }

        private void BuildResponses()
        {
            _responsePages = new List<Response[]>();
            var responsesThisPage = new List<Response>();

            for (var index = 0; index < Main.Upgrades.Count; index++)
            {
                var upgrade = Main.Upgrades[index];
                if (upgrade.Unlocked) continue;

                string text = _isJoja ? $"{upgrade.TranslatedName} : {upgrade.Cost}$" 
                    : $"{upgrade.TranslatedName} : {upgrade.Cost}";

                responsesThisPage.Add(new Response(upgrade.Name, text));

                //Max of 3 options per page, with more pages added as needed
                if (responsesThisPage.Count < 3) continue;
                if (index < Main.Upgrades.Count - 1)
                    responsesThisPage.Add(new Response("More", "More"));
                responsesThisPage.Add(new Response("Exit", "Exit"));
                _responsePages.Add(responsesThisPage.ToArray());
                responsesThisPage = new List<Response>();
            }

            responsesThisPage.Add(new Response("Exit", "Exit"));
            _responsePages.Add(responsesThisPage.ToArray());
        }
    }
}
