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

        public static void Initialize(IModHelper helper)
        {
            _helper = helper;
            _translation = helper.Translation;
        }

        public AquariumMessage(Span<string> args)
        {
            List<string> fishes = [];
            foreach (string str in args)
            {
                if (!Utils.IsUnDonatedFish(str))
                    fishes.Add(str);
            }

            if (fishes.Count == 0)
            {
                Game1.drawObjectDialogue(I18n.EmptyTank());
                return;
            }

            if (fishes.Count == 1)
            {
                Game1.drawObjectDialogue(I18n.GetByKey($"Tank_{fishes[0]}"));
                return;
            }

            this.BuildResponse(fishes);
            Game1.currentLocation.createQuestionDialogue(_translation.Get("WhichFishInfo"), this._responsePages[this._currentPage], this.displayFishInfo);
        }

        private void BuildResponse(List<string> fishes)
        {
            this._responsePages = [];
            List<Response> responsesThisPage = new();

            for (int index = 0; index < fishes.Count; index++)
            {
                responsesThisPage.Add(new Response(fishes[index], Utils.FishDisplayNames[fishes[index]]));

                //Max of 3 options per page, with more pages added as needed
                if (responsesThisPage.Count < 4) continue;
                if (index < fishes.Count - 1)
                    responsesThisPage.Add(new Response("More", _translation.Get("More")));
                responsesThisPage.Add(new Response("Exit", _translation.Get("Exit")));
                this._responsePages.Add(responsesThisPage.ToArray());
                responsesThisPage = [];
            }

            responsesThisPage.Add(new Response("Exit", _translation.Get("Exit")));
            this._responsePages.Add(responsesThisPage.ToArray());

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
                this._currentPage++;
                _helper.Events.GameLoop.UpdateTicked += this.OpenNextPage;
                return;
            }
            Game1.drawObjectDialogue(_translation.Get($"Tank_{whichAnswer}"));
        }

        private void OpenNextPage(object sender, UpdateTickedEventArgs e)
        {
            Game1.currentLocation.createQuestionDialogue(_translation.Get("WhichFishInfo"), this._responsePages[this._currentPage], this.displayFishInfo);
            _helper.Events.GameLoop.UpdateTicked -= this.OpenNextPage;
        }
    }
}
