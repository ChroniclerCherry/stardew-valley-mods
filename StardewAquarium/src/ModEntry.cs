using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewAquarium.Menu;
using StardewAquarium.Tokens;
using StardewValley;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        private ModData data;
        public override void Entry(IModHelper helper)
        {
            //TODO: implement a way to obtain legendaries after being caught, but at 0 sell price so it's not profitable to catch. its only for missed donations

            helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);

            string dataPath = Path.Combine("data", "data.json");
            data = helper.Data.ReadJsonFile<ModData>(dataPath);

            LastDonatedFishCoordinates = new Vector2(data.LastDonatedFishCoordinateX,data.LastDonatedFishCoordinateY);

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
            public int LastDonatedFishCoordinateX { get; set; }
            public int LastDonatedFishCoordinateY { get; set; }

            public string ExteriorMapName { get; set; }
        }
    }
}
