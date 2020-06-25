using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewAquarium.Menu;
using StardewAquarium.Tokens;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        private ModEntry.ModData data;

        public override void Entry(IModHelper helper)
        {
            //TODO: implement a way to obtain legendaries after being caught, but at 0 sell price so it's not profitable to catch. its only for missed donations
            //TODO: add a message after donation to let users know it'll take a day
            //TODO: add museum donation collection menu

            string dataPath = Path.Combine("data", "data.json");
            data = helper.Data.ReadJsonFile<ModEntry.ModData>(dataPath);

            Utils.Initialize(Helper, Monitor,data);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            new MenuInteractionHandler(Helper,Monitor);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            new TokenHandler(Helper,ModManifest).RegisterTokens();
        }

        public class ModData
        {
            public int LastDonatedFishCoordinateX { get; set; }
            public int LastDonatedFishCoordinateY { get; set; }

            public string ExteriorMapName { get; set; }
        }
    }
}
