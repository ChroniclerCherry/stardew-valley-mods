using System;
using System.IO;
using StardewModdingAPI;
using StardewAquarium.Models;
using StardewAquarium.src.Pufferchick;
using StardewAquarium.Tokens;
using StardewValley;
using Harmony;
using StardewAquarium.Patches;
using StardewAquarium.MenuAndTiles;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        public static ModConfig config;
        public static bool RecatchLegends;
        public static ModData data;
        public const string PufferChickName = "Pufferchick";
        public static HarmonyInstance harmony { get; } = HarmonyInstance.Create("Cherry.StardewAquarium");

        public static IJsonAssetsApi JsonAssets { get; set; }

        public override void Entry(IModHelper helper)
        {
            Utils.Initialize(Helper, Monitor,ModManifest);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            helper.Content.AssetEditors.Add(new AchievementEditor(Helper,Monitor));
            helper.Content.AssetEditors.Add(new FishEditor());

            LegendaryFishPatches.Initialize(Helper, Monitor);

            new ReturnTrain(Helper, Monitor);
            new InteractionHandler(Helper,Monitor);

            config = Helper.ReadConfig<ModConfig>();

            string dataPath = Path.Combine("data", "data.json");
            data = helper.Data.ReadJsonFile<ModData>(dataPath);

            //disable if recatch legendary fish is installed
            if (config.EnableRecatchWorthlessUndonatedLegends &&
                !Helper.ModRegistry.IsLoaded("cantorsdust.RecatchLegendaryFish"))
            {
                Monitor.Log("Enabling the recatch of legendaries...");
                RecatchLegends = true;
                //new LegendaryRecatch(Helper, Monitor);
            }
            else
            {
                Monitor.Log("Disabling the recatch of legendaries from this mod. ( if cantorsdust.RecatchLegendaryFish is installed, behaviour will default to that mod's )");
                RecatchLegends = false;
            }
                
            
            if (config.EnableDebugCommands)
            {
                Helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);
                Helper.ConsoleCommands.Add("aquariumprogress", "", OpenAquariumCollectionMenu);
                Helper.ConsoleCommands.Add("removedonatedfish", "", RemoveDonatedFish);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            AquariumMessage.Initialize(Helper.Translation);
        }

        private void RemoveDonatedFish(string arg1, string[] arg2)
        {
            var mail = Game1.MasterPlayer.mailReceived;
            for (int i = mail.Count - 1; i >= 0; i--)
            {
                if (mail[i].StartsWith("AquariumDonated:") || mail[i].StartsWith("AquariumFishDonated:"))
                    mail.RemoveAt(i);
            }
        }

        private void OpenAquariumCollectionMenu(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new AquariumCollectionMenu(Helper.Translation.Get("CollectionsMenu"));
        }

        private void OpenDonationMenuCommand(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new DonateFishMenu(Helper.Translation);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "data"));
            new TokenHandler(Helper,ModManifest).RegisterTokens();
        }
    }
}
