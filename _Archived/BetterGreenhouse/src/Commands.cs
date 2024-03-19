using StardewModdingAPI;

namespace GreenhouseUpgrades
{
    public class Commands
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public Commands(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.ConsoleCommands.Add("bgsummary", "Lists all upgrades and their current status", SummarizeAllUpgrades);
            if (!Main.Config.EnableDebugging) return;
            _helper.ConsoleCommands.Add("bgupgrade", "Instantly applies an upgrade", AddUpgradeCommand);
            _helper.ConsoleCommands.Add("bgremove", "Stops a current upgrade", RemoveUpgradeCommand);
            _helper.ConsoleCommands.Add("bgstart", "Starts a current upgrade", StartUpgradeCommand);
            _helper.ConsoleCommands.Add("bgstop", "Stops a current upgrade", StopUpgradeCommand);
        }

        private void SummarizeAllUpgrades(string arg1, string[] arg2)
        {
            string activeUpgrades = "";
            string inactiveUpgrades = "";

            foreach (var upgrade in Main.Upgrades)
            {
                if (upgrade.Unlocked)
                {
                    activeUpgrades += $"- {upgrade.Name} : " +
                                      $"\n\tActive: {upgrade.Active}" +
                                      $"\n\tTranslated: {upgrade.TranslatedName} " +
                                      $"\n\tDescription: {upgrade.TranslatedDescription} " +
                                      $"\n\tCost: {upgrade.Cost}\n";
                }
                else
                {
                    inactiveUpgrades += $"- {upgrade.Name} : " +
                                        $"\n\tActive: {upgrade.Active}" +
                                        $"\n\tTranslated: {upgrade.TranslatedName} " +
                                        $"\n\tDescription: {upgrade.TranslatedDescription} " +
                                        $"\n\tCost: {upgrade.Cost}\n";

                    if (Main.UpgradeForTonight == upgrade.Name)
                        inactiveUpgrades += "\t**Will be upgraded tonight\n";
                }
            }

            var prtstr = "\n";
            if (Main.UpgradeForTonight != null)
                prtstr += $"Pending Upgrade tonight: {Main.UpgradeForTonight}\n";

            if (activeUpgrades.Length > 0)
            {
                prtstr += "Unlocked Upgrades\n---------------------\n" + activeUpgrades + "\n";
            }

            if (inactiveUpgrades.Length > 0)
            {
                prtstr += "Locked Upgrades\n---------------------\n" + inactiveUpgrades;
            }

            _monitor.Log(prtstr,LogLevel.Info);

        }

        public void StartUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                _monitor.Log("Upgrade not found", LogLevel.Warn);
                return;
            }

            if (!upgrade.Unlocked)
            {
                _monitor.Log("Upgrade not unlocked", LogLevel.Warn);
                return;
            }

            if (upgrade.Active)
            {
                _monitor.Log("Upgrade is already active", LogLevel.Warn);
                return;
            }

            upgrade.Start();
        }

        public void StopUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                _monitor.Log("Upgrade not found",LogLevel.Warn);
                return;
            }

            if (!upgrade.Active)
            {
                _monitor.Log("Upgrade is not active", LogLevel.Warn);
                return;
            }

            upgrade.Stop();
        }

        public void AddUpgradeCommand(string arg1, string[] arg2)
        {
            Main.SetUpgradeForTonight(arg2[0]);
            Main.PerformEndOfDayUpdate(false);
        }

        public void RemoveUpgradeCommand(string arg1, string[] arg2)
        {
            var upgrade = Utils.GetUpgradeByName(arg2[0]);

            if (upgrade == null)
            {
                _monitor.Log("Upgrade not found", LogLevel.Warn);
                return;
            }

            if (!upgrade.Unlocked)
            {
                _monitor.Log("Upgrade not unlocked", LogLevel.Warn);
                return;
            }

            upgrade.Stop();
            upgrade.Unlocked = false;
        }
    }
}