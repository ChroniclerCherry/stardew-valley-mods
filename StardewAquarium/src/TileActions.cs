using System;

using Microsoft.Xna.Framework;

using StardewAquarium.Editors;
using StardewAquarium.Menus;

using StardewModdingAPI;

using StardewValley;

namespace StardewAquarium.src;

internal static class TileActions
{
    private static IMonitor Monitor;
    private static IModHelper Helper;

    internal static void Init(IModHelper helper, IMonitor monitor)
    {
        Helper = helper;
        Monitor = monitor;
        GameLocation.RegisterTileAction("AquariumDonationMenu", DonationMenu);
        GameLocation.RegisterTileAction("AquariumSign", AquariumSign);
        GameLocation.RegisterTileAction("AquariumString", AquariumString);
        GameLocation.RegisterTileAction("AquariumCollectionMenu", ShowAquariumCollectionMenu)
    }

    private static bool ShowAquariumCollectionMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)
    {
        Game1.activeClickableMenu = new AquariumCollectionMenu(I18n.CollectionsMenu());
        return true;
    }

    private static bool AquariumString(GameLocation location, string[] actions, Farmer farmer, Point point)
    {
        if (!ArgUtility.TryGet(actions, 1, out string key, out string error, allowBlank: false))
        {
            location.LogTileActionError(actions, point.X, point.Y, error);
            return false;
        }

        Game1.drawObjectDialogue(I18n.GetByKey(key));

        return true;
    }

    private static bool AquariumSign(GameLocation location, string[] actions, Farmer farmer, Point point)
    {
        new AquariumMessage(actions.AsSpan(1));
        return true;
    }

    private static bool DonationMenu(GameLocation location, string[] actions, Farmer farmer, Point point)
    {
        Monitor.Log("AquariumDonationMenu tile detected, opening donation menu...");
        if (!Utils.DoesPlayerHaveDonatableFish())
        {
            if (Game1.MasterPlayer.achievements.Contains(AchievementEditor.AchievementId))
            {
                Game1.drawObjectDialogue(I18n.AquariumWelcome());
                return true;
            }

            Game1.drawObjectDialogue(I18n.NothingToDonate());
            return true;
        }

        Response[] options = [
            new Response("OptionYes", I18n.OptionYes()),
            new Response("OptionNo", I18n.OptionNo())
        ];

        Game1.currentLocation.createQuestionDialogue(I18n.DonationQuestion(), options, HandleResponse);
        return true;
    }

    private static void HandleResponse(Farmer who, string whichAnswer)
    {
        switch (whichAnswer)
        {
            case "OptionNo":
                Game1.drawObjectDialogue(I18n.DeclineToDonate() );
                return;
            case "OptionYes" when Constants.TargetPlatform == GamePlatform.Android:
                Game1.activeClickableMenu = new DonateFishMenuAndroid(Helper, Monitor);
                break;
            case "OptionYes":
                Game1.activeClickableMenu = new DonateFishMenu(Helper, Monitor);
                break;
        }
    }
}
