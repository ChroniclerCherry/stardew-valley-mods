using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace BetterGreenhouse.Interaction
{
    class InteractionDetection
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        public InteractionDetection(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.Player.Warped += Player_Warped;
            _helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (Game1.currentLocation.Name == "JojaMart")
            {
                Game1.currentLocation.setTileProperty((int)State.Config.JojaMartUpgradeCoordinates.X,
                    (int)State.Config.JojaMartUpgradeCoordinates.Y, "Buildings", "Action", "");
            }

            if (Game1.currentLocation.Name == "CommunityCenter")
            {
                Game1.currentLocation.setTileProperty((int)State.Config.CommunityCenterUpgradeCoordinates.X,
                    (int)State.Config.CommunityCenterUpgradeCoordinates.Y, "Buildings", "Action", "");
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
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

            if (!Game1.MasterPlayer.mailReceived.Contains("ccPantry")) return;

            Vector2 grabTile = e.Cursor.GrabTile;

            if (Game1.currentLocation.Name == "JojaMart" && State.IsJojaRoute)
            {
                if (grabTile != State.Config.JojaMartUpgradeCoordinates) return;
                if (State.IsThereUpgradeTonight)
                {
                    Game1.drawObjectDialogue(_helper.Translation.Get("JojaUpgradeInProgress"));
                    return;
                }
                new UpgradeMenu(_helper, _monitor,true);

            } else if (Game1.currentLocation.Name == "CommunityCenter" && !State.IsJojaRoute)
            {
                if (grabTile != State.Config.CommunityCenterUpgradeCoordinates) return;

                if (Game1.player.ActiveObject != null)
                {
                    GiveJunimoOffering();
                }
                else
                {
                    if (State.IsThereUpgradeTonight)
                    {
                        Game1.drawObjectDialogue(_helper.Translation.Get("JunimoUpgradeInProgress"));
                        return;
                    }
                    new UpgradeMenu(_helper,_monitor,false);
                }

            }
        }

        private void GiveJunimoOffering()
        {
            if (State.JunimoOfferingMade)
            {
                Game1.drawObjectDialogue(_helper.Translation.Get("JunimoOfferingMade"));
                return;
            }

            int points = CalculateJunimoPoints(Game1.player.ActiveObject);
            State.JunimoPoints += points;
            State.JunimoOfferingMade = true;
            _helper.Multiplayer.SendMessage(State.JunimoPoints, Consts.MultiplayerJunimopointsKey,
                new[] { Consts.ModUniqueID });
            Game1.drawObjectDialogue(_helper.Translation.Get("JunimoOffering", new { JunimoPoints = points }));

        }

        private int CalculateJunimoPoints(Object obj)
        {
            int points = obj.Price; //base points is its price
            if (obj.Edibility > 0)
                points += obj.Edibility; //add edibility
            points *= (obj.Quality+1); //multiply by quality

            //multiply by 3 if it's a cooked dish
            if (CraftingRecipe.cookingRecipes.ContainsKey(obj.Name))
                points *= 3;

            if (obj.Stack > 1)
                obj.Stack--;
            else
                Game1.player.removeItemFromInventory(obj);

            return points;
        }
    }
}
