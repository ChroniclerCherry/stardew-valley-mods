using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;
using UpgradeEmptyCabins.Framework;

namespace UpgradeEmptyCabins;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles console commands registered by the mod.</summary>
    private CommandHandler CommandHandler;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        I18n.Init(helper.Translation);
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor);
        this.CommandHandler = new CommandHandler(this.Monitor, this.AskForUpgrade);

        this.CommandHandler.Register(helper.ConsoleCommands);

        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.CanPlayerMove)
            return;

        if (Constants.TargetPlatform == GamePlatform.Android)
        {
            if (e.Button != SButton.MouseLeft)
                return;
            if (e.Cursor.GrabTile != e.Cursor.Tile)
                return;
        }
        else if (!e.Button.IsActionButton())
            return;

        if (Game1.currentLocation.Name != "ScienceHouse")
            return;

        if (this.Helper.Input.GetCursorPosition().GrabTile != new Vector2(6, 19))
            return;

        this.AskForUpgrade();

    }

    /// <summary>Upgrade a cabin to its next level.</summary>
    /// <param name="cabin">The cabin building to upgrade.</param>
    /// <remarks>Derived from <see cref="Farmer.dayupdate"/>.</remarks>
    private void PerformUpgrade(Building cabin)
    {
        Cabin indoors = (Cabin)cabin.indoors.Value;
        indoors.moveObjectsForHouseUpgrade(indoors.upgradeLevel + 1);
        indoors.upgradeLevel++;
        indoors.setMapForUpgradeLevel(indoors.upgradeLevel);
    }

    /// <summary>Show the UI to choose a cabin to upgrade.</summary>
    private void AskForUpgrade()
    {
        if (Game1.getFarm().isThereABuildingUnderConstruction())
        {
            Game1.drawObjectDialogue(I18n.Robin_Busy());
            return;
        }

        List<Response> cabinNames = new List<Response>();
        foreach (Building cabin in ModUtility.GetCabins())
        {
            string displayInfo = null;
            Cabin cabinIndoors = ((Cabin)cabin.indoors.Value);

            //if the cabin is occupied, we ignore it
            if (cabinIndoors.owner.Name != "")
                continue;

            switch (cabinIndoors.upgradeLevel)
            {
                case 0:
                    displayInfo = $"{cabin.buildingType.Value} {I18n.Robin_Hu1Materials()}";
                    break;
                case 1:
                    displayInfo = $"{cabin.buildingType.Value} {I18n.Robin_Hu2Materials()}";
                    break;
                case 2:
                    displayInfo = $"{cabin.buildingType.Value} {I18n.Robin_Hu3Materials()}";
                    break;
            }
            if (displayInfo != null)
                cabinNames.Add(new Response(cabin.GetIndoorsName(), displayInfo));
        }

        if (cabinNames.Count > 0)
        {
            cabinNames.Add(new Response("Cancel", I18n.Menu_CancelOption()));
            //Game1.activeClickableMenu = new CabinQuestionsBox("Which Cabin would you like to upgrade?", cabinNames);
            Game1.currentLocation.createQuestionDialogue(
                I18n.Robin_WhichCabinQuestion(),
                cabinNames.ToArray(),
                delegate (Farmer _, string answer)
                {
                    Game1.activeClickableMenu = null;
                    this.OnSelectedHouseUpgrade(ModUtility.GetCabin(answer));
                }
            );
        }
    }

    /// <summary>Handle the player accepting a cabin upgrade.</summary>
    /// <param name="cabin">The cabin to upgrade.</param>
    /// <remarks>Derived from <see cref="GameLocation.houseUpgradeAccept"/>, modified to work for cabins not owned by the current player and build instantly (since the game doesn't track upgrades for non-active players).</remarks>
    private void OnSelectedHouseUpgrade(Building cabin)
    {
        Game1.activeClickableMenu = null;
        Game1.player.canMove = true;
        if (cabin == null)
        {
            Game1.playSound("smallSelect");
            return;
        }

        NPC robin = Game1.RequireCharacter("Robin");
        Cabin indoors = (Cabin)cabin.indoors.Value;
        string displayName = TokenParser.ParseText(cabin.GetData()?.Name);

        switch (indoors.upgradeLevel)
        {
            case 0:
                if (Game1.player.Money >= 10000 && Game1.player.Items.ContainsId(Object.woodQID, 450))
                {
                    Game1.player.Money -= 10000;
                    Game1.player.Items.ReduceId(Object.woodQID, 450);
                    Game1.DrawDialogue(robin, "Data\\ExtraDialogue:Robin_Instant", displayName.ToLower(), displayName);
                    this.PerformUpgrade(cabin);
                }
                else if (Game1.player.Money < 10000)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", 450));
                break;

            case 1:
                if (Game1.player.Money >= 65000 && Game1.player.Items.ContainsId("(O)709"/* hardwood */, 100))
                {
                    Game1.player.Money -= 65000;
                    Game1.player.Items.ReduceId("(O)709", 100);
                    Game1.DrawDialogue(robin, "Data\\ExtraDialogue:Robin_Instant", displayName.ToLower(), displayName);
                    this.PerformUpgrade(cabin);
                }
                else if (Game1.player.Money < 65000)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughHardwood", 100));
                break;

            case 2:
                if (Game1.player.Money >= 100000)
                {
                    Game1.player.Money -= 100000;
                    Game1.DrawDialogue(robin, "Data\\ExtraDialogue:Robin_Instant", displayName.ToLower(), displayName);
                    this.PerformUpgrade(cabin);
                }
                else if (Game1.player.Money < 100000)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                break;
        }
    }
}
