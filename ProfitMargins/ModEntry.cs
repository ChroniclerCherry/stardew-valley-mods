using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ProfitMargins
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration from the player.</summary>
        private ModConfig config;
        private float originalDifficulty;

        /*********
        ** Public methods
        *********/

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;
            return;

        }

        /*********
        ** Private methods
        *********/

        private void DayStarted(object sender, DayStartedEventArgs args)
        {
            if (checkContext()) { 
                originalDifficulty = Game1.player.difficultyModifier;
                Game1.player.difficultyModifier = config.ProfitMargin;
            }

        }

        private void OnSaving(object sender, SavingEventArgs args)
        {
            if (checkContext())
            {
                Game1.player.difficultyModifier = originalDifficulty;
                this.Monitor.Log("During save, DL:" + Game1.player.difficultyModifier.ToString(), LogLevel.Debug);
            }

        }

        private bool checkContext()
        {
            if (!Context.IsMainPlayer)
            {
                return false;
            } else if (Context.IsMultiplayer && !config.EnableInMultiplayer)
            {
                return false;
            }
            return true;
        }
    }

    class ModConfig
    {
        public bool EnableInMultiplayer { get; set; } = false;
        public float ProfitMargin { get; set; } = 1f;
    }
}