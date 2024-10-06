using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewAquarium.Editors;
using StardewAquarium.Menus;
using StardewAquarium.Models;
using StardewAquarium.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewAquarium
{
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static ModData Data;
        public const string PufferChickName = "Pufferchick";
        public const string LegendaryBaitName = "Legendary Bait";
        private readonly bool _isAndroid = Constants.TargetPlatform == GamePlatform.Android;
        public static Harmony Harmony { get; } = new Harmony("Cherry.StardewAquarium");

        private AchievementEditor AchievementEditor;
        private FishEditor FishEditor;
        private MailEditor MailEditor;
        private ObjectEditor ObjectEditor;
        private MiscEditor MiscEditor;

        public static IJsonAssetsApi JsonAssets { get; set; }
        public override void Entry(IModHelper helper)
        {
            Utils.Initialize(this.Helper, this.Monitor, this.ModManifest);

            this.AchievementEditor = new(this.Helper);
            this.FishEditor = new(this.Helper);
            this.MailEditor = new(this.Helper);
            this.ObjectEditor = new();
            this.MiscEditor = new(this.Helper);

            this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
            this.Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            this.Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

            if (this._isAndroid)
            {
                AndroidShopMenuPatch.Initialize(this.Helper, this.Monitor);
                this.Helper.Events.Display.MenuChanged += this.AndroidPlsHaveMercyOnMe;
            }

            new ReturnTrain(this.Helper, this.Monitor);
            new InteractionHandler(this.Helper, this.Monitor);

            Config = this.Helper.ReadConfig<ModConfig>();

            string dataPath = Path.Combine("data", "data.json");
            Data = helper.Data.ReadJsonFile<ModData>(dataPath);

            LegendaryFishPatches.Initialize();


            if (Config.EnableDebugCommands)
            {
                if (this._isAndroid)
                    this.Helper.ConsoleCommands.Add("donatefish", "", this.AndroidDonateFish);
                else
                    this.Helper.ConsoleCommands.Add("donatefish", "", this.OpenDonationMenuCommand);

                this.Helper.ConsoleCommands.Add("aquariumprogress", "", this.OpenAquariumCollectionMenu);
                this.Helper.ConsoleCommands.Add("removedonatedfish", "", this.RemoveDonatedFish);
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (this.AchievementEditor.CanEdit(e.NameWithoutLocale))
                e.Edit(this.AchievementEditor.Edit);
            else if (this.FishEditor.CanEdit(e.NameWithoutLocale))
                e.Edit(this.FishEditor.Edit);
            else if (this.MailEditor.CanEdit(e.NameWithoutLocale))
                e.Edit(this.MailEditor.Edit);
            else if (this.ObjectEditor.CanEdit(e.NameWithoutLocale))
                e.Edit(this.ObjectEditor.Edit);
            else if (this.MiscEditor.CanEdit(e.NameWithoutLocale))
                e.Edit(this.MiscEditor.Edit);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.CanPlayerMove && Config.CheckDonationCollection == e.Button)
            {
                Game1.activeClickableMenu = new AquariumCollectionMenu(this.Helper.Translation.Get("CollectionsMenu"));
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer) return; //we don't want this running for every single logged in player, so just the host can handle the logic

            //all credit to kdau, i lifted this code from East Scarpe
            var loc = Game1.getLocationFromName(Data.ExteriorMapName);
            foreach (Object obj in loc.objects.Values)
            {
                // Must be a Crab Pot.
                if (!(obj is CrabPot pot))
                    continue;

                // Must have a non-trash catch already.
                if (pot.heldObject.Value == null ||
                    pot.heldObject.Value.Category == Object.junkCategory)
                    continue;

                // Check for the Mariner profession.
                Farmer player = (pot.owner.Value != 0L)
                    ? Game1.getFarmer(pot.owner.Value) : Game1.player;
                bool mariner = player?.professions?.Contains(Farmer.mariner) ?? false;

                // Seed the RNG.
                Random rng = new Random((int)Game1.stats.DaysPlayed +
                                        (int)Game1.uniqueIDForThisGame / 2 +
                                        (int)pot.TileLocation.X * 1000 +
                                        (int)pot.TileLocation.Y);

                // Search for suitable fish.
                Dictionary<string, string> fishes = this.Helper.GameContent.Load<Dictionary<string, string>>("Data/Fish");
                List<string> candidates = new List<string>();
                foreach (KeyValuePair<string, string> fish in fishes)
                {
                    if (!fish.Value.Contains("trap"))
                        continue;

                    string[] fields = fish.Value.Split('/');
                    if (fields[4].Equals("freshwater"))
                        continue;

                    candidates.Add(fish.Key);

                    if (!mariner && rng.NextDouble() < Convert.ToDouble(fields[2]))
                    {
                        pot.heldObject.Value = new Object(fish.Key, 1);
                        candidates.Clear();
                        break;
                    }
                }

                if (candidates.Count > 0)
                {
                    pot.heldObject.Value = new Object
                        (candidates[rng.Next(candidates.Count)], 1);
                }
            }

        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.currentLocation?.Name != Data.ExteriorMapName) return;
            if (Game1.isTimePaused) return;

            //This code was borrowed from East Scarpe

            // Very rarely show the Sea Monster.
            if (Game1.eventUp || !(Game1.random.NextDouble() < Data.DolphinChance))
                return;

            // Randomly find a starting position within the range.
            Vector2 position = 64f * new Vector2
            (Game1.random.Next(Data.DolphinRange.Left,
                    Data.DolphinRange.Right + 1),
                Game1.random.Next(Data.DolphinRange.Top,
                    Data.DolphinRange.Bottom + 1));

            var loc = Game1.currentLocation;

            bool foundPosition = true;

            // Confirm there is water tiles in the 3x2 area the dolphin spawns in
            Vector2[] tiles = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 0),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, 1) };
            foreach (var tile in tiles)
            {
                if (loc.doesTileHaveProperty((int)((position.X / 64) + tile.X), (int)((position.Y / 64) + tile.Y), "Water", "Back") == null)
                {
                    foundPosition = false;
                    break;
                }
            }

            // Spawn if possible.
            if (foundPosition)
            {
                loc.temporarySprites.Add(new DolphinAnimatedSprite(position, this.Helper.ModContent.Load<Texture2D>("data/dolphin.png")));
            }

        }

        private void AndroidPlsHaveMercyOnMe(object sender, MenuChangedEventArgs e)
        {
            //don't ask me what the heck is going on here but its the only way to get it to work
            if (!(e.OldMenu is DonateFishMenuAndroid androidMenu)) return;
            //80% sure this is a DonateFishMenuAndroid but it won't work if i check for that but the harmony patch seems to work on it so idk
            if (!(e.NewMenu is ShopMenu menu)) return;

            menu.exitFunction = androidMenu.OnExit;
        }

        private void AndroidDonateFish(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new DonateFishMenuAndroid(this.Helper, this.Monitor);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            AquariumMessage.Initialize(this.Helper);
            if (Utils.CheckAchievement())
                Utils.UnlockAchievement();

        }

        private void RemoveDonatedFish(string arg1, string[] arg2)
        {
            Game1.MasterPlayer.mailReceived.RemoveWhere(id => id.StartsWith("AquariumDonated:") || id.StartsWith("AquariumFishDonated:"));
        }

        private void OpenAquariumCollectionMenu(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new AquariumCollectionMenu(this.Helper.Translation.Get("CollectionsMenu"));
        }

        private void OpenDonationMenuCommand(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new DonateFishMenu(this.Helper, this.Monitor);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JsonAssets.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "data"));

            Event.RegisterCommand("GiveAquariumTrophy1", GiveAquariumTrophy1);
            Event.RegisterCommand("GiveAquariumTrophy2", GiveAquariumTrophy2);
        }

        public static void GiveAquariumTrophy1(Event e, string[] args, EventContext context)
        {
            string id = JsonAssets.GetBigCraftableId("Stardew Aquarium Trophy");
            Object trophy = ItemRegistry.Create<Object>(id);
            e.farmer.holdUpItemThenMessage(trophy, true);
            ++e.CurrentCommand;
        }
        public static void GiveAquariumTrophy2(Event e, string[] args, EventContext context)
        {
            string id = JsonAssets.GetBigCraftableId("Stardew Aquarium Trophy");
            Object trophy = new Object(Vector2.Zero, id);
            e.farmer.addItemByMenuIfNecessary(trophy);
            if (Game1.activeClickableMenu == null)
                ++e.CurrentCommand;
            ++e.CurrentCommand;
        }
    }
}
