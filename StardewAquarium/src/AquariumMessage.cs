using System;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;

namespace StardewAquarium;

internal sealed class AquariumMessage
{
    private PerScreen<List<Response[]>?> _responsePages = new();
    private PerScreen<int> _currentPage = new();

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
            Game1.drawObjectDialogue(GetDescription(fishes[0]));
            return;
        }

        this.BuildResponse(fishes);
        Game1.currentLocation.createQuestionDialogue(I18n.WhichFishInfo(), this._responsePages.Value[this._currentPage.Value], this.displayFishInfo);
    }

    private void BuildResponse(List<string> fishes)
    {
        this._responsePages.Value = [];
        this._currentPage.Value = 0;
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
                responsesThisPage.Add(new Response("More", I18n.More()));
            responsesThisPage.Add(new Response("Exit", I18n.Exit()));
            this._responsePages.Value.Add(responsesThisPage.ToArray());
            responsesThisPage = [];
        }

        responsesThisPage.Add(new Response("Exit", I18n.Exit()));
        this._responsePages.Value.Add(responsesThisPage.ToArray());
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
            this._currentPage.Value++;

            // delays until the next tick.
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.currentLocation.createQuestionDialogue(I18n.WhichFishInfo(), this._responsePages.Value[this._currentPage.Value], this.displayFishInfo);
            }, 10);
            return;
        }
        Game1.drawObjectDialogue(GetDescription(whichAnswer));
    }

    private static string GetDescription(string key)
    {
        var overrideContent = Game1.content.Load<Dictionary<string, string>>("Mods/StardewAquarium/FishDescriptions");
        if (overrideContent.TryGetValue(key, out string value))
            return value;

        return I18n.GetByKey($"Tank_{key}");
    }
}
