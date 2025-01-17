using System;
using System.Collections.Generic;
using StardewValley;

namespace StardewAquarium.Framework;

internal sealed class AquariumMessage
{
    /*********
    ** Fields
    *********/
    private List<Response[]>? _responsePages;
    private int _currentPage;


    /*********
    ** Public methods
    *********/
    public AquariumMessage(Span<string> args)
    {
        List<string> fishes = new(args.Length);
        foreach (string str in args)
        {
            if (Utils.HasDonatedFishKey(str))
                fishes.Add(str);
        }

        if (fishes.Count == 0)
        {
            Game1.drawObjectDialogue(ContentPackHelper.LoadString("EmptyTank"));
            return;
        }

        if (fishes.Count == 1)
        {
            Game1.drawObjectDialogue(GetDescription(fishes[0]));
            return;
        }

        this.BuildResponse(fishes);
        Game1.currentLocation.createQuestionDialogue(ContentPackHelper.LoadString("WhichFishInfo"), this._responsePages[this._currentPage], this.DisplayFishInfo);
    }


    /*********
    ** Private methods
    *********/
    private void BuildResponse(List<string> fishes)
    {
        this._responsePages = [];
        this._currentPage = 0;
        List<Response> responsesThisPage = [];

        for (int index = 0; index < fishes.Count; index++)
        {
            if (!Utils.FishDisplayNames.TryGetValue(fishes[index], out string translated))
            {
                continue;
            }
            responsesThisPage.Add(new Response(fishes[index], translated));

            //Max of 3 options per page, with more pages added as needed
            if (responsesThisPage.Count < 4)
                continue;
            if (index < fishes.Count - 1)
                responsesThisPage.Add(new Response("More", ContentPackHelper.LoadString("More")));
            responsesThisPage.Add(new Response("Exit", ContentPackHelper.LoadString("Exit")));
            this._responsePages.Add(responsesThisPage.ToArray());
            responsesThisPage = [];
        }

        responsesThisPage.Add(new Response("Exit", ContentPackHelper.LoadString("Exit")));
        this._responsePages.Add(responsesThisPage.ToArray());
    }

    private void DisplayFishInfo(Farmer who, string whichAnswer)
    {
        switch (whichAnswer)
        {
            case "Exit":
                break;

            case "More":
                Game1.activeClickableMenu = null;
                Game1.currentLocation.afterQuestion = null;
                this._currentPage++;

                // delays until the next tick.
                DelayedAction.functionAfterDelay(
                    () => Game1.currentLocation.createQuestionDialogue(ContentPackHelper.LoadString("WhichFishInfo"), this._responsePages[this._currentPage], this.DisplayFishInfo),
                    10
                );
                break;

            default:
                Game1.drawObjectDialogue(GetDescription(whichAnswer));
                break;
        }
    }

    private static string GetDescription(string key)
    {
        return Game1.content
            .Load<Dictionary<string, string>>($"Mods/{ContentPackHelper.ContentPackId}/FishTankDescriptions")
            .GetValueOrDefault(key, key);
    }
}
