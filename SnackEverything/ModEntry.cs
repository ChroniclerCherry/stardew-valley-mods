using SnackEverything.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using System;

namespace SnackEverything
{
    class ModEntry : Mod
    {
        ModConfig Config;
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Config = Helper.ReadConfig<ModConfig>();

            foreach ((string id, ObjectData data) in Game1.objectData)
            {
                if (data.Edibility < 0 && (data.Type != "Arch" || Config.YummyArtefacts))
                    data.Edibility = Math.Max(data.Price / 3, 1);
            }
        }
    }
}
