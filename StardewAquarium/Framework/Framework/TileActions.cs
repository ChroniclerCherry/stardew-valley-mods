using Microsoft.Xna.Framework;
using StardewAquarium.Framework.Menus;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Framework.Framework;

internal static class TileActions
{
    /*********
    ** Fields
    *********/
    private static IMonitor Monitor;


    /*********
    ** Public methods
    *********/
    public static void Init(IMonitor monitor)
    {
        Monitor = monitor;

        GameLocation.RegisterTileAction("AquariumDonationMenu", DonationMenu);
        GameLocation.RegisterTileAction("AquariumSign", AquariumSign);
        GameLocation.RegisterTileAction("AquariumCollectionMenu", ShowAquariumCollectionMenu);
    }


    /*********
    ** Private methods
    *********/
    private static bool ShowAquariumCollectionMenu(GameLocation location, string[] arg2, Farmer farmer, Point point)
    {
        Game1.activeClickableMenu = new AquariumCollectionMenu(ContentPackHelper.LoadString("CollectionsMenu"));
        return true;
    }

    private static bool AquariumSign(GameLocation location, string[] actions, Farmer farmer, Point point)
    {
        _ = new AquariumMessage(actions);
        return true;
    }

    private static bool DonationMenu(GameLocation location, string[] actions, Farmer farmer, Point point)
    {
        Monitor.Log("AquariumDonationMenu tile detected, opening donation menu...");
        if (!Utils.DoesPlayerHaveDonatableFish(farmer))
        {
            if (Game1.MasterPlayer.achievements.Contains(ContentPackHelper.AchievementId))
            {
                Game1.drawObjectDialogue(ContentPackHelper.LoadString("AquariumWelcome"));
                return true;
            }

            Game1.drawObjectDialogue(ContentPackHelper.LoadString("NothingToDonate"));
            return true;
        }

        Response[] options = [
            new Response("OptionYes", ContentPackHelper.LoadString("OptionYes")),
            new Response("OptionNo", ContentPackHelper.LoadString("OptionNo"))
        ];

        Game1.currentLocation.createQuestionDialogue(ContentPackHelper.LoadString("DonationQuestion"), options, HandleResponse);
        return true;
    }

    private static void HandleResponse(Farmer who, string whichAnswer)
    {
        switch (whichAnswer)
        {
            case "OptionNo":
                Game1.drawObjectDialogue(ContentPackHelper.LoadString("DeclineToDonate"));
                return;

            case "OptionYes" when Constants.TargetPlatform == GamePlatform.Android:
                Game1.activeClickableMenu = new DonateFishMenuAndroid();
                break;

            case "OptionYes":
                Game1.activeClickableMenu = new DonateFishMenu();
                break;
        }
    }
}
