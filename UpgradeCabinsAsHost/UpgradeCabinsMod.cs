using StardewValley;
using StardewModdingAPI;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewValley.Objects;
using System.Linq;

namespace UpgradeCabinsAsHost
{
    public class UpgradeCabins : Mod
    {
        internal static IModHelper helper;
        public override void Entry(IModHelper h)
        {
                
            helper = h;
            helper.ConsoleCommands.Add("upgrade_cabin", "If Robin is free, brings up the menu to upgrade cabins.", UpgradeCabinsCommand);
            helper.ConsoleCommands.Add("remove_seed_boxes","Removes seed boxes from all unclaimed cabins.",RemoveSeedBoxesCommand);

            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
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
                    } ((Cabin)cab.indoors.Value).Objects.Remove(obj.Key);
                }
            }
        }

        private void UpgradeCabinsCommand(string arg1, string[] arg2)
        {
            AskForUpgrade();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {

            if (Context.IsWorldReady && e.Button == SButton.Escape && Game1.activeClickableMenu is CabinQuestionsBox)
            {
                Game1.playSound("smallSelect");
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = null;
            }

            if (!Context.CanPlayerMove)
                return;

            if (!e.Button.IsActionButton())
                return;

            if (Game1.currentLocation.Name != "ScienceHouse")
                return;

            if (helper.Input.GetCursorPosition().GrabTile != new Microsoft.Xna.Framework.Vector2(6, 19))
                return;

            AskForUpgrade();

        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            //so this doesn't end up happening for every single player online....
            if (!Context.IsMainPlayer)
                return;

            foreach (var cabin in ModUtility.GetCabins())
            {
                if (cabin.daysUntilUpgrade.Value == 1)
                {
                    var cabinIndoors = ((Cabin)cabin.indoors.Value);
                    cabin.daysUntilUpgrade.Value = -1;
                    cabinIndoors.upgradeLevel++;
                    cabinIndoors.moveObjectsForHouseUpgrade(cabinIndoors.upgradeLevel);
                    cabinIndoors.setMapForUpgradeLevel(cabinIndoors.upgradeLevel);
                }
            }
        }

        internal static void AskForUpgrade()
        {
            if (Game1.getFarm().isThereABuildingUnderConstruction())
            {
                Game1.drawObjectDialogue(helper.Translation.Get("robin.busy"));
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
                        displayInfo = $"{cabin.buildingType.Value} {helper.Translation.Get("robin.hu1_materials") }";
                        break;
                    case 1:
                        displayInfo = $"{cabin.buildingType.Value} {helper.Translation.Get("robin.hu2_materials") }";
                        break;
                    case 2:
                        displayInfo = $"{cabin.buildingType.Value} {helper.Translation.Get("robin.hu3_materials") }";
                        break;
                }
                if (displayInfo != null)
                    cabinNames.Add(new Response(cabin.nameOfIndoors, displayInfo));
            }
                    
            if (cabinNames.Count > 0)
            {
                cabinNames.Add(new Response("Cancel",helper.Translation.Get("menu.cancel_option")));
                Game1.activeClickableMenu = new CabinQuestionsBox("Which Cabin would you like to upgrade?", cabinNames);
            }
        }

        internal static void houseUpgradeAccept(Building cab)
        {
            if (cab == null)
            {
                Game1.playSound("smallSelect");
                Game1.activeClickableMenu = null;
                return;
            }

            var cabin = ((Cabin)cab.indoors.Value);

            switch (cabin.upgradeLevel)
            {
                case 0:
                    if (Game1.player.Money >= 10000 && Game1.player.hasItemInInventory(388, 450, 0))
                    {
                        cab.daysUntilUpgrade.Value = 3;
                        Game1.player.Money -= 10000;
                        Game1.player.removeItemsFromInventory(388, 450);
                        Game1.getCharacterFromName("Robin", true).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"), false, false);
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin", true));
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
                    if (Game1.player.Money >= 50000 && Game1.player.hasItemInInventory(709, 150, 0))
                    {
                        cab.daysUntilUpgrade.Value = 3;
                        Game1.player.Money -= 50000;
                        Game1.player.removeItemsFromInventory(709, 150);
                        Game1.getCharacterFromName("Robin", true).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"), false, false);
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin", true));
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
                        Game1.getCharacterFromName("Robin", true).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"), false, false);
                        Game1.drawDialogue(Game1.getCharacterFromName("Robin", true));
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
