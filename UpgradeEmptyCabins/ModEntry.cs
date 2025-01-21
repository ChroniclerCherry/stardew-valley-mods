using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using UpgradeEmptyCabins.Framework;

namespace UpgradeEmptyCabins;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // init
        I18n.Init(helper.Translation);

        helper.ConsoleCommands.Add("upgrade_cabin", "If Robin is free, brings up the menu to upgrade cabins.", this.UpgradeCabinsCommand);
        helper.ConsoleCommands.Add("remove_seed_boxes", "Removes seed boxes from all unclaimed cabins.", this.RemoveSeedBoxesCommand);
        helper.ConsoleCommands.Add("remove_cabin_beds", "Removes beds from all unclaimed cabins.", this.RemoveCabinBedsCommand);
        helper.ConsoleCommands.Add("renovate_cabins", "Removes cribs and adds all the extra rooms to all unclaimed cabins.", this.RenovateCabinsCommand);
        helper.ConsoleCommands.Add("list_cabins", "Lists cabin names for toggle_renovate.", this.ListCabins);
        helper.ConsoleCommands.Add("list_renovations", "Lists renovation names for toggle_renovate.", this.ListRenovations);
        helper.ConsoleCommands.Add("toggle_renovate", "Toggles a renovation for an unclaimed cabin.", this.ToggleRenovateCommand);
        helper.ConsoleCommands.Add("set_crib_style", "Sets the crib style for an unclaimed cabin.", this.SetCribStyleCommand);

        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }


    /*********
    ** Private methods
    *********/
    private void SetCribStyleCommand(string arg1, string[] arg2)
    {
        string cabin = arg2[0] + " Cabin"; //"Plank","Stone","Log"
        int style = int.Parse(arg2[1]);

        foreach (Building cab in ModUtility.GetCabins())
        {
            if (((Cabin)cab.indoors.Value).owner.Name != "")
                continue;
            if (cab.buildingType.ToString() == cabin)
            {
                ((Cabin)cab.indoors.Value).cribStyle.Set(style);
                this.Monitor.Log("Cabin: " + cab.GetIndoorsName(), LogLevel.Info);
                this.Monitor.Log("Cabin Type: " + cab.buildingType.Value, LogLevel.Info);
                this.Monitor.Log("cribStyle: " + ((Cabin)cab.indoors.Value).cribStyle.Value, LogLevel.Info);
            }
        }
    }

    private void ListCabins(string arg1, string[] arg2)
    {
        this.Monitor.Log("Upgrade Level 2 required for renovations.", LogLevel.Info);
        foreach (Building cab in ModUtility.GetCabins())
        {
            if (((Cabin)cab.indoors.Value).owner.Name != "")
                continue;
            this.Monitor.Log("Cabin: " + cab.GetIndoorsName(), LogLevel.Info);
            this.Monitor.Log("    Upgrade Level: " + ((Cabin)cab.indoors.Value).upgradeLevel, LogLevel.Info);
        }
    }

    private void ListRenovations(string arg1, string[] arg2)
    {
        this.Monitor.Log("renovation_bedroom_open, renovation_southern_open, renovation_corner_open, renovation_extendedcorner_open, renovation_dining_open, renovation_diningroomwall_open, renovation_cubby_open, renovation_farupperroom_open", LogLevel.Info);
    }

    private void ToggleRenovateCommand(string arg1, string[] arg2)
    {
        string cabin = arg2[0]; //"Plank","Stone","Log"
        string reno = arg2[1]; //"renovation_bedroom_open", "renovation_southern_open", "renovation_corner_open", "renovation_extendedcorner_open", "renovation_dining_open", "renovation_diningroomwall_open", "renovation_cubby_open", "renovation_farupperroom_open"

        foreach (Building cab in ModUtility.GetCabins())
        {
            if (((Cabin)cab.indoors.Value).owner.Name != "")
                continue;
            if (cab.GetIndoorsName() == cabin)
            {
                ISet<string> mail = ((Cabin)cab.indoors.Value).owner.mailReceived;
                this.Monitor.Log("Cabin: " + cab.GetIndoorsName(), LogLevel.Info);

                if (reno == "renovation_diningroomwall_open")
                {
                    if (!mail.Contains("renovation_dining_open"))
                    {
                        mail.Add("renovation_dining_open");
                        mail.Remove(reno);
                        this.Monitor.Log("Renovation Added: renovation_dining_open" + reno, LogLevel.Info);
                        this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                    }
                    else if (mail.Contains("renovation_dining_open") && !mail.Contains("renovation_diningroomwall_open"))
                    {
                        mail.Add(reno);
                        this.Monitor.Log("Renovation Added: " + reno, LogLevel.Info);
                    }
                    else
                    {
                        mail.Remove(reno);
                        this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                    }
                }
                else if (mail.Contains(reno))
                {
                    if (reno == "renovation_corner_open")
                    {
                        mail.Remove("renovation_extendedcorner_open");
                        this.Monitor.Log("Renovation Removed: renovation_extendedcorner_open", LogLevel.Info);
                    }
                    if (reno == "renovation_dining_open" && mail.Contains("renovation_diningroomwall_open"))
                    {
                        mail.Remove("renovation_diningroomwall_open");
                        this.Monitor.Log("Renovation Removed: renovation_diningroomwall_open", LogLevel.Info);
                    }
                    mail.Remove(reno);
                    this.Monitor.Log("Renovation Removed: " + reno, LogLevel.Info);
                }
                else
                {
                    if (reno == "renovation_extendedcorner_open")
                    {
                        mail.Add("renovation_corner_open");
                        this.Monitor.Log("Renovation Added: renovation_corner_open", LogLevel.Info);
                    }
                    mail.Add(reno);
                    this.Monitor.Log("Renovation Added: " + reno, LogLevel.Info);
                }
            }
        }
    }

    private void RenovateCabinsCommand(string arg1, string[] arg2)
    {
        string[] renos = { "renovation_bedroom_open", "renovation_southern_open", "renovation_corner_open", "renovation_extendedcorner_open", "renovation_dining_open", "renovation_diningroomwall_open", "renovation_cubby_open", "renovation_farupperroom_open" };
        foreach (Building cab in ModUtility.GetCabins())
        {
            if (((Cabin)cab.indoors.Value).owner.Name != "")
                continue;
            this.Monitor.Log("Cabin: " + cab.GetIndoorsName(), LogLevel.Info);
            this.Monitor.Log("    Type: " + cab.buildingType.Value, LogLevel.Info);
            if (((Cabin)cab.indoors.Value).upgradeLevel < 2)
            {
                this.Monitor.Log("    Upgrade Level: " + ((Cabin)cab.indoors.Value).upgradeLevel, LogLevel.Info);
                this.Monitor.Log("    Upgrade Level 2 required for renovations. Not Renovated.", LogLevel.Info);
                continue;
            }
            ISet<string> mail = ((Cabin)cab.indoors.Value).owner.mailReceived;
            foreach (string reno in renos)
            {
                if (mail.Contains(reno))
                    this.Monitor.Log("Renovation already done: " + reno + " " + cab.GetIndoorsName(), LogLevel.Info);
                else
                {
                    if (reno == "renovation_diningroomwall_open")
                        mail.Remove(reno);
                    else
                        mail.Add(reno);
                }
            }

            ((Cabin)cab.indoors.Value).cribStyle.Set(0);
            this.Monitor.Log("    cribStyle:  " + ((Cabin)cab.indoors.Value).cribStyle.Value, LogLevel.Info);
            this.Monitor.Log("    flags: " + mail, LogLevel.Info);
        }
    }

    private void RemoveCabinBedsCommand(string arg1, string[] arg2)
    {
        foreach (Building cab in ModUtility.GetCabins())
        {
            BedFurniture bed = null;
            if (((Cabin)cab.indoors.Value).owner.Name != "")
                continue;
            foreach (Furniture furniture in ((Cabin)cab.indoors.Value).furniture)
            {
                if (furniture is BedFurniture b)
                {
                    bed = b;
                    break;
                }
            }

            if (bed != null)
            {
                ((Cabin)cab.indoors.Value).furniture.Remove(bed);
                this.Monitor.Log("Bed removed from " + cab.GetIndoorsName(), LogLevel.Info);
            }
        }
    }

    private void RemoveSeedBoxesCommand(string arg1, string[] arg2)
    {
        foreach (Building cab in ModUtility.GetCabins())
        {
            Cabin indoors = (Cabin)cab.indoors.Value;

            if (indoors.owner.Name != "")
                continue;
            foreach ((Vector2 tile, Object obj) in indoors.Objects.Pairs)
            {
                if (obj is not Chest chest || !chest.giftbox.Value || chest.bigCraftable.Value)
                {
                    continue;
                }

                indoors.Objects.Remove(tile);
                this.Monitor.Log("Seed box removed from " + cab.GetIndoorsName(), LogLevel.Info);
            }
        }
    }

    private void UpgradeCabinsCommand(string arg1, string[] arg2)
    {
        this.AskForUpgrade();
    }

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
