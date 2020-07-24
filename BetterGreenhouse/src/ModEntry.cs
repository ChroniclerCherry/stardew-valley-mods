using StardewModdingAPI;
using System;
using System.Linq;
using BetterGreenhouse.Interaction;
using BetterGreenhouse.src;

namespace BetterGreenhouse
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            State.Initialize(Helper,Monitor);
            Consts.ModUniqueID = ModManifest.UniqueID;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            new InteractionDetection(Helper, Monitor);

            Helper.ConsoleCommands.Add("bgsummary", "Lists all upgrades and their current status", SummarizeAllUpgrades);
            if (!State.Config.EnableDebugging) return;
            Helper.ConsoleCommands.Add("bgupgrade", "Instantly applies an upgrade", AddUpgradeCommand);
            Helper.ConsoleCommands.Add("bgremove", "Stops a current upgrade", RemoveUpgradeCommand);
            Helper.ConsoleCommands.Add("bgstart", "Starts a current upgrade", StartUpgradeCommand);
            Helper.ConsoleCommands.Add("bgstop", "Stops a current upgrade", StopUpgradeCommand);

        }

        private void SummarizeAllUpgrades(string arg1, string[] arg2)
        {
            string activeUpgrades = "";
            string inactiveUpgrades = "";
            foreach (var upgrade in State.Upgrades)
            {
                if (upgrade.Unlocked)
                {
                    activeUpgrades += $"- {upgrade.Name} : " +
                                      $"\n\tActive: {upgrade.Active}" +
                                      $"\n\tTranslated: {upgrade.translatedName} " +
                                      $"\n\tDescription: {upgrade.translatedDescription} " +
                                      $"\n\tCost: {upgrade.Cost}\n";
                }
                else
                {
                    inactiveUpgrades += $"- {upgrade.Name} : " +
                                        $"\n\tActive: {upgrade.Active}" +
                                        $"\n\tTranslated: {upgrade.translatedName} " +
                                        $"\n\tDescription: {upgrade.translatedDescription} " +
                                        $"\n\tCost: {upgrade.Cost}\n";

                    if (State.UpgradeForTonight == upgrade.Name)
                        inactiveUpgrades += "\t**Will be upgraded tonight\n";
                }
            }

            var prtstr = "\n";

            if (activeUpgrades.Length > 0)
            {
                prtstr += "Unlocked Upgrades\n---------------------\n" + activeUpgrades + "\n";
            }

            if (inactiveUpgrades.Length > 0)
            {
                prtstr += "Locked Upgrades\n---------------------\n" + inactiveUpgrades;
            }

            Monitor.Log(prtstr,LogLevel.Info);

        }

        private void StartUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                Monitor.Log("Upgrade not found", LogLevel.Warn);
                return;
            }

            if (!upgrade.Unlocked)
            {
                Monitor.Log("Upgrade not unlocked", LogLevel.Warn);
                return;
            }

            if (upgrade.Active)
            {
                Monitor.Log("Upgrade is already active", LogLevel.Warn);
                return;
            }

            upgrade.Start();
        }
        private void StopUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                Monitor.Log("Upgrade not found",LogLevel.Warn);
                return;
            }

            if (!upgrade.Active)
            {
                Monitor.Log("Upgrade is not active", LogLevel.Warn);
                return;
            }

            upgrade.Stop();
        }

        private void AddUpgradeCommand(string arg1, string[] arg2)
        {
            State.SetUpgradeForTonight(arg2[0]);
            State.PerformEndOfDayUpdate(false);
        }

        private void RemoveUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                Monitor.Log("Upgrade not found", LogLevel.Warn);
                return;
            }

            if (!upgrade.Unlocked)
            {
                Monitor.Log("Upgrade not unlocked", LogLevel.Warn);
                return;
            }

            upgrade.Stop();
            upgrade.Unlocked = false;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            State.LoadData();
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            State.PerformEndOfDayUpdate();
        }
    }
}
