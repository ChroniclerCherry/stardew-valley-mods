using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewAquarium.Menu;
using StardewAquarium.Tokens;
using StardewValley;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            //TODO: implement a way to obtain legendaries after being caught, but at 0 sell price so it's not profitable to catch. its only for missed donations

            //TODO: save the last donated fish and display on sign

            helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);

            Utils.Initialize(Helper, Monitor);
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            new TokenHandler(Helper,ModManifest).RegisterTokens();
        }
        private void OpenDonationMenuCommand(string arg1, string[] arg2)
        {
            TryToOpenDonationMenu();
        }

        public class ModData
        {
            private Vector2 LastDonatedFishCoordinates { get; set; }
        }
    }
}
