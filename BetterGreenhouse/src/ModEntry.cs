using StardewModdingAPI;
using System;
using System.Linq;

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

            Helper.ConsoleCommands.Add("bgsummary", "Lists all upgrades and their current status", SummarizeAllUpgrades);
            Helper.ConsoleCommands.Add("bgupgrade", "Instantly applies an upgrade", ApplyUpgradeCommand);
            Helper.ConsoleCommands.Add("bgremove", "Removes a current upgrade", RemoveUpgradeCommand);
        }

        private void SummarizeAllUpgrades(string arg1, string[] arg2)
        {
            string activeUpgrades = "";
            string inactiveUpgrades = "";
            foreach (var upgrade in State.Upgrades)
            {
                if (upgrade.Active)
                {
                    activeUpgrades += $"- {upgrade.Name} : " +
                                      $"\n\tTranslated: {upgrade.translatedName} " +
                                      $"\n\tDescription: {upgrade.translatedDescription} " +
                                      $"\n\tCost: {upgrade.Cost}\n";
                }
                else
                {
                    inactiveUpgrades += $"- {upgrade.Name} : " +
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
                prtstr += "Upgrades applied\n---------------------\n" + activeUpgrades + "\n";
            }

            if (inactiveUpgrades.Length > 0)
            {
                prtstr += "Upgrades not applied\n---------------------\n" + inactiveUpgrades;
            }

            Monitor.Log(prtstr,LogLevel.Info);

        }

        private void RemoveUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = State.Upgrades.FirstOrDefault(u => u.Name == arg2[0]);

            if (upgrade == null)
            {
                Monitor.Log("Upgrade not found",LogLevel.Warn);
                return;
            }

            upgrade.Stop();
        }

        private void ApplyUpgradeCommand(string arg1, string[] arg2)
        {
            State.SetUpgradeForTonight(arg2[0]);
            State.PerformEndOfDayUpdate(false);
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
