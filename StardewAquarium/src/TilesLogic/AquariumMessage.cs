using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.TilesLogic
{
    class AquariumMessage
    {
        private static ITranslationHelper _translation;

        public static  void Initialize(ITranslationHelper translation)
        {
            _translation = translation;
        }

        public AquariumMessage(string[] args)
        {
            List<string> fishes = new List<string>();
            foreach (var str in args)
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

            List<Response> responses = new List<Response>();

            foreach (var fish in fishes)
            {
                responses.Add(new Response(fish,Utils.FishDisplayNames[fish]));
            }

            Game1.currentLocation.createQuestionDialogue(_translation.Get("WhichFishInfo"),
                responses.ToArray(),displayFishInfo);
        }

        private void displayFishInfo(Farmer who, string whichAnswer)
        {
            Game1.drawObjectDialogue(_translation.Get($"Tank_{whichAnswer}"));
        }
    }
}
