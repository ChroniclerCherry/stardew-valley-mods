using System.Collections.Generic;
using System.Linq;
using BetterGreenhouse.data;
using BetterGreenhouse.src;
using BetterGreenhouse.src.data;
using BetterGreenhouse.Upgrades;
using StardewModdingAPI;
using StardewValley;

namespace BetterGreenhouse
{
    public static class State
    {
        private static Data ModData { get; set; }
        private static bool IsJojaRoute => Game1.MasterPlayer.mailReceived.Contains("JojaMember");

        public static List<Upgrade> Upgrades
        {
            get => ModData.Upgrades;
            set => ModData.Upgrades = value;
        }

        public static int JunimoPoints
        {
            get => ModData.JunimoPoints;
            set => ModData.JunimoPoints = value;
        }

        public static Config Config;
        private static string _upgradeForTonight;
        public static bool IsThereUpgradeTonight => _upgradeForTonight != null;

        private static IModHelper _helper;
        private static IMonitor _monitor;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
        }

        private static void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != Consts.ModUniqueID) return;

            switch (e.Type)
            {
                case Consts.MultiplayerLoadKey:
                    ModData = e.ReadAs<Data>();
                    InitializeAllUpgrades();
                    break;
                case Consts.MultiplayerUpdate:
                    _upgradeForTonight = e.ReadAs<string>();
                    break;
                case Consts.MultiplayerJunimopointsKey:
                    JunimoPoints = e.ReadAs<int>();
                    break;
            }
        }

        public static void LoadData()
        {
            Config = _helper.ReadConfig<Config>();

            _monitor.Log("Loading mod data...");

            if (!Context.IsMainPlayer) return;

            ModData = _helper.Data.ReadSaveData<Data>(Consts.SaveDataKey);
            if (ModData == null)
            {
                _monitor.Log("No previous mod data found on this save, generating new data", LogLevel.Info);
                ModData = new Data();
            }

            AddMissingUpgrades();
            InitializeAllUpgrades();

            _helper.Multiplayer.SendMessage(ModData, Consts.MultiplayerLoadKey, modIDs: new[] { Consts.ModUniqueID });

        }

        private static void InitializeAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            {
                upgrade.Initialize(_helper, _monitor);
                upgrade.Start();
            }
        }

        private static void AddMissingUpgrades()
        {
            if (!Upgrades.Exists(u => u.UpgradeName == "SizeUpgrade"))
                Upgrades.Add(new SizeUpgrade());

            if (!Upgrades.Exists(u => u.UpgradeName == "AutoWaterUpgrade"))
                Upgrades.Add(new AutoWaterUpgrade());
        }

        public static void SaveData()
        {
            _monitor.Log("Saving mod data...");
            _helper.Data.WriteSaveData(Consts.SaveDataKey, ModData);
        }

        public static void SetUpgradeForTonight(string upgradeName)
        {
            _upgradeForTonight = upgradeName;
            _helper.Multiplayer.SendMessage(upgradeName, Consts.MultiplayerUpdate, modIDs: new[] { Consts.ModUniqueID });
        }

        public static void PerformEndOfDayUpdate()
        {
            if (_upgradeForTonight != null)
            {
                string translatedName = $"{_helper.Translation.Get(_upgradeForTonight)}.Name";
                _monitor.Log($"Applying {translatedName}...", LogLevel.Info);

                Upgrade upgrade = Upgrades.FirstOrDefault(u => u.UpgradeName == _upgradeForTonight);
                if (upgrade == null)
                {
                    _monitor.Log($"{_upgradeForTonight} not found. Upgrade aborted", LogLevel.Error);
                    _upgradeForTonight = null;
                    return;
                }

                upgrade.Active = true;
                upgrade.Start();
                _upgradeForTonight = null;
            }

            SaveData();
        }
    }
}