using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewAquarium
{
    class AquariumMessage
    {
        private static ITranslationHelper _translation;
        private static IModHelper _helper;
        List<Response[]> _responsePages;
        private int _currentPage = 0;

        public static  void Initialize(IModHelper helper)
        {
            _helper = helper;
            _translation = helper.Translation;
        }

        public AquariumMessage(string[] args)
        {
            List<string> fishes = new List<string>();
            foreach (String str in args)
            {
                if (!Utils.IsUnDonatedFish(str))
                    fishes.Add(str);
            }

            if (fishes.Count == 0)
            {
                Game1.drawObjectDialogue(_translation.Get("EmptyTank"));
                return;
            }
            
            if (fishes.Count == 1)
            {
                Game1.drawObjectDialogue(_translation.Get($"Tank_{fishes[0]}"));
                return;
            }

            BuildResponse(fishes);
            Game1.currentLocation.createQuestionDialogue(_translation.Get("WhichFishInfo"), _responsePages[_currentPage], displayFishInfo);
        }

        private void BuildResponse(List<string> fishes)
        {
            _responsePages = new List<Response[]>();
            var responsesThisPage = new List<Response>();

            for (var index = 1; index < fishes.Count; index++)
            {
                responsesThisPage.Add(new Response(fishes[index], Utils.FishDisplayNames[fishes[index]]));

                //Max of 3 options per page, with more pages added as needed
                if (responsesThisPage.Count < 4) continue;
                if (index < fishes.Count - 1)
                    responsesThisPage.Add(new Response("More", _translation.Get("More")));
                responsesThisPage.Add(new Response("Exit", _translation.Get("Exit")));
                _responsePages.Add(responsesThisPage.ToArray());
                responsesThisPage = new List<Response>();
            }

            responsesThisPage.Add(new Response("Exit", _translation.Get("Exit")));
            _responsePages.Add(responsesThisPage.ToArray());

        }

        private void displayFishInfo(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "Exit")
            {
                return;
            }

            if (whichAnswer == "More")
            {
                Game1.activeClickableMenu = null;
                Game1.currentLocation.afterQuestion = null;
                _currentPage++;
                _helper.Events.GameLoop.UpdateTicked += OpenNextPage;
                return;
            }
            Game1.drawObjectDialogue(_translation.Get($"Tank_{whichAnswer}"));
        }

        private void OpenNextPage(object sender, UpdateTickedEventArgs e)
        {
            Game1.currentLocation.createQuestionDialogue(_translation.Get("WhichFishInfo"), _responsePages[_currentPage], displayFishInfo);
            _helper.Events.GameLoop.UpdateTicked -= OpenNextPage;
        }
    }
}
