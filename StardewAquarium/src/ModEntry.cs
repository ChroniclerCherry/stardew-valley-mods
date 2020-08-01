using System.IO;
using StardewModdingAPI;
using StardewAquarium.Models;
using StardewValley;
using Harmony;
using StardewAquarium.Patches;
using StardewAquarium.Editors;
using StardewAquarium.Menus;
using StardewAquarium.TilesLogic;
using StardewValley.Menus;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        public static ModConfig Config;
        public static bool RecatchLegends;
        public static ModData Data;
        public const string PufferChickName = "Pufferchick";
        private readonly bool _isAndroid = Constants.TargetPlatform == GamePlatform.Android;
        public static HarmonyInstance harmony { get; } = HarmonyInstance.Create("Cherry.StardewAquarium");

        public static IJsonAssetsApi JsonAssets { get; set; }
        public override void Entry(IModHelper helper)
        {
            Utils.Initialize(Helper, Monitor,ModManifest);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            InitializeEditors();

            if (_isAndroid)
            {
                AndroidShopMenuPatch.Initialize(Helper, Monitor);
                Helper.Events.Display.MenuChanged += AndroidPlsHaveMercyOnMe;
            }

            new ReturnTrain(Helper, Monitor);
            new InteractionHandler(Helper,Monitor);

            Config = Helper.ReadConfig<ModConfig>();

            string dataPath = Path.Combine("data", "data.json");
            Data = helper.Data.ReadJsonFile<ModData>(dataPath);

            //disable if recatch legendary fish is installed
            if (Config.EnableRecatchWorthlessUndonatedLegends &&
                !Helper.ModRegistry.IsLoaded("cantorsdust.RecatchLegendaryFish"))
            {
                Monitor.Log("Enabling the recatch of legendaries...");
                RecatchLegends = true;
            }
            else
            {
                Monitor.Log("Disabling the recatch of legendaries from this mod. (if cantorsdust.RecatchLegendaryFish is installed, behaviour will default to that mod's)");
                RecatchLegends = false;
            }

            LegendaryFishPatches.Initialize(Helper, Monitor);


            if (Config.EnableDebugCommands)
            {
                if (_isAndroid)
                    Helper.ConsoleCommands.Add("donatefish", "", AndroidDonateFish);
                else
                    Helper.ConsoleCommands.Add("donatefish", "", OpenDonationMenuCommand);

                Helper.ConsoleCommands.Add("aquariumprogress", "", OpenAquariumCollectionMenu);
                Helper.ConsoleCommands.Add("removedonatedfish", "", RemoveDonatedFish);
            }
        }
        private void AndroidPlsHaveMercyOnMe(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            //don't ask me what the heck is going on here but its the only way to get it to work
            if (!(e.OldMenu is DonateFishMenuAndroid androidMenu)) return;
            //80% sure this is a DonateFishMenuAndroid but it won't work if i check for that but the harmony patch seems to work on it so idk
            if (!(e.NewMenu is ShopMenu menu)) return;

            menu.exitFunction = androidMenu.OnExit;
        }

        private void AndroidDonateFish(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new DonateFishMenuAndroid(Helper, Monitor);
        }

        private void InitializeEditors()
        {
            Helper.Content.AssetEditors.Add(new AchievementEditor(Helper, Monitor));
            Helper.Content.AssetEditors.Add(new MiscEditor(Helper));
            Helper.Content.AssetEditors.Add(new FishEditor());
            Helper.Content.AssetEditors.Add(new MailEditor(Helper));
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
            Game1.activeClickableMenu = new DonateFishMenu(Helper,Monitor);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "data"));
        }
    }
}
