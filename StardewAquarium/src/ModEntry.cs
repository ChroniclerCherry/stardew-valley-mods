﻿using System;
using StardewModdingAPI;
using StardewAquarium.Menu;
using StardewAquarium.Models;
using StardewAquarium.Tokens;
using StardewValley;

namespace StardewAquarium
{
    public partial class ModEntry : Mod
    {
        private static ModConfig config;

        public override void Entry(IModHelper helper)
        {
            Utils.Initialize(Helper, Monitor);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;


            new InteractionHandler(Helper,Monitor);

            config = Helper.ReadConfig<ModConfig>();

            //disable if recatch legendary fish is installed
            if (config.EnableRecatchWorthlessUndonatedLegends &&
                !Helper.ModRegistry.IsLoaded("cantorsdust.RecatchLegendaryFish"))
                new LegendaryRecatch(Helper, Monitor);
            
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
            new TokenHandler(Helper,ModManifest).RegisterTokens();
        }
    }
}