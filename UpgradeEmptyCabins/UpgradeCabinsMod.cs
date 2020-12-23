using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

namespace UpgradeEmptyCabins
{
    public partial class UpgradeCabins : Mod
    {
        private Config _config;
        public override void Entry(IModHelper h)
        {
            _config = Helper.ReadConfig<Config>();

            Helper.ConsoleCommands.Add("upgrade_cabin", "If Robin is free, brings up the menu to upgrade cabins.", UpgradeCabinsCommand);
            Helper.ConsoleCommands.Add("remove_seed_boxes", "Removes seed boxes from all unclaimed cabins.", RemoveSeedBoxesCommand);
            Helper.ConsoleCommands.Add("remove_cabin_beds", "Removes beds from all unclaimed cabins.", RemoveCabinBedsCommand);

            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void RemoveCabinBedsCommand(string arg1, string[] arg2)
        {
            foreach (var cab in ModUtility.GetCabins())
            {
                BedFurniture bed = null;
                if (((Cabin)cab.indoors.Value).owner.Name != "")
                    continue;
                foreach (var furniture in ((Cabin)cab.indoors.Value).furniture)
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
                    Monitor.Log("Bed removed from " + cab.nameOfIndoors, LogLevel.Info);
                }
                    
            }
        }

        private void RemoveSeedBoxesCommand(string arg1, string[] arg2)
        {
            foreach (var cab in ModUtility.GetCabins())
            {
                if (((Cabin)cab.indoors.Value).owner.Name != "")
                    continue;
                foreach (var obj in
                    ((Cabin)cab.indoors.Value).Objects.SelectMany(objs =>
                    objs.Where(obj => obj.Value is Chest).Select(obj => obj)))
                {
                    Chest chest = (Chest)obj.Value;
                    if (!chest.giftbox.Value || chest.bigCraftable.Value)
                    {
                        continue;
                    }
                    
                    ((Cabin)cab.indoors.Value).Objects.Remove(obj.Key);
                    Monitor.Log("Seed box removed from " + cab.nameOfIndoors, LogLevel.Info);
                }
            }
        }

        private void UpgradeCabinsCommand(string arg1, string[] arg2)
        {
            AskForUpgrade();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
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

            if (Helper.Input.GetCursorPosition().GrabTile != new Vector2(6, 19))
                return;

            AskForUpgrade();

        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            //so this doesn't end up happening for every single player online....
            if (!Context.IsMainPlayer)
                return;

            foreach (var cabin in ModUtility.GetCabins())
            {
                if (cabin.daysUntilUpgrade.Value == 1)
                {
                    FinalUpgrade(cabin);
                }
            }
            
        }

        private static void FinalUpgrade(Building cabin)
        {
            var cabinIndoors = ((Cabin)cabin.indoors.Value);
                cabin.daysUntilUpgrade.Value = -1;
                cabinIndoors.moveObjectsForHouseUpgrade(cabinIndoors.upgradeLevel);
                cabinIndoors.setMapForUpgradeLevel(cabinIndoors.upgradeLevel);
                cabinIndoors.upgradeLevel++;
        }

        internal void AskForUpgrade()
        {
            if (Game1.getFarm().isThereABuildingUnderConstruction())
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("robin.busy"));
                return;
            }

            List<Response> cabinNames = new List<Response>();
            foreach (var cabin in ModUtility.GetCabins())
            {
                string displayInfo = null;
                var cabinIndoors = ((Cabin)cabin.indoors.Value);

                //if the cabin is occupied, we ignore it
                if (cabinIndoors.owner.Name != "")
                    continue;

                switch (cabinIndoors.upgradeLevel)
                {
                    case 0:
                        displayInfo = $"{cabin.buildingType.Value} {Helper.Translation.Get("robin.hu1_materials") }";
                        break;
                    case 1:
                        displayInfo = $"{cabin.buildingType.Value} {Helper.Translation.Get("robin.hu2_materials") }";
                        break;
                    case 2:
                        displayInfo = $"{cabin.buildingType.Value} {Helper.Translation.Get("robin.hu3_materials") }";
                        break;
                }
                if (displayInfo != null)
                    cabinNames.Add(new Response(cabin.nameOfIndoors, displayInfo));
            }

            if (cabinNames.Count > 0)
            {
                cabinNames.Add(new Response("Cancel", Helper.Translation.Get("menu.cancel_option")));
                //Game1.activeClickableMenu = new CabinQuestionsBox("Which Cabin would you like to upgrade?", cabinNames);
                Game1.currentLocation.createQuestionDialogue(
                                Helper.Translation.Get("robin.whichcabin_question"),
                                cabinNames.ToArray(),
                                delegate (Farmer who, string answer)
                                {
                                    Game1.activeClickableMenu = null;
                                    HouseUpgradeAccept(ModUtility.GetCabin(answer));
                                }
                                );
            }
        }

        internal void HouseUpgradeAccept(Building cab)
        {
            Game1.activeClickableMenu = null;
            Game1.player.canMove = true;
            if (cab == null)
            {
                Game1.playSound("smallSelect");
                return;
            }

            if (_config.InstantBuild)
            {
                FinalUpgrade(cab);
                Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
                Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                return;
            }

            var cabin = ((Cabin)cab.indoors.Value);


            switch (cabin.upgradeLevel)
            {
                case 0:
                    if (Game1.player.Money >= 10000 && Game1.player.hasItemInInventory(388, 450))
                    {
                        cab.daysUntilUpgrade.Value = 3;
                        Game1.player.Money -= 10000;
                        Game1.player.removeItemsFromInventory(388, 450);
                        Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                        break;
                    }
                    if (Game1.player.Money < 10000)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                        break;
                    }
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood1"));
                    break;
                case 1:
                    if (Game1.player.Money >= 50000 && Game1.player.hasItemInInventory(709, 150))
                    {
                        cab.daysUntilUpgrade.Value = 3;
                        Game1.player.Money -= 50000;
                        Game1.player.removeItemsFromInventory(709, 150);
                        Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                        break;
                    }
                    if (Game1.player.Money < 50000)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                        break;
                    }
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood2"));
                    break;
                case 2:
                    if (Game1.player.Money >= 100000)
                    {
                        cab.daysUntilUpgrade.Value = 3;
                        Game1.player.Money -= 100000;
                        Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
                        break;
                    }
                    if (Game1.player.Money >= 100000)
                        break;
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
                    break;
            }
        }
    }
}
