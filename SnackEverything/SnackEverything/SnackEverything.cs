using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SnackEverything
{
    class SnackEverything : Mod
    {
        ModConfig Config;
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Config = Helper.ReadConfig<ModConfig>();
            var ObjectInfo = new List<int>(Game1.objectInformation.Keys);
            foreach (var key in ObjectInfo)
            {
                var info = Game1.objectInformation[key].Split('/');

                if (int.Parse(info[2]) < 0 && (info[3] != "Arch" || Config.YummyArtefacts))
                        info[2] = Math.Max((int.Parse(info[1])/3),1).ToString();

                Game1.objectInformation[key] = string.Join("/", info);
            }
        }
    }

    class ModConfig
    {
       public bool YummyArtefacts { get; set; } = false;
    }
}
