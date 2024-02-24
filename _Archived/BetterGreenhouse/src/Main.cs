using System;
using System.Collections.Generic;
using System.Linq;
using GreenhouseUpgrades.Data;
using GreenhouseUpgrades.Upgrades;
using StardewModdingAPI;
using StardewValley;

namespace GreenhouseUpgrades
{
    public class Main
    {
        public static Config Config;
        private static Data.Data ModData { get; set; }
        public static bool IsJojaRoute => Game1.MasterPlayer.mailReceived.Contains("JojaMember");

        public static List<Upgrade> Upgrades;

        public static int JunimoPoints { get; set; }

        public static bool JunimoOfferingMade { get; set; }

        public static string UpgradeForTonight { get; set; }
        public static bool IsThereUpgradeTonight => UpgradeForTonight != null;

        private static IModHelper _helper;
        private static IMonitor _monitor;

        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            Config = _helper.ReadConfig<Config>();

            _helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            _helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            _helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        }

        private static void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            JunimoOfferingMade = false;
            PerformEndOfDayUpdate();
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadData();
        }

        private static void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != Consts.ModUniqueID) return;

            switch (e.Type)
            {
                case Consts.MultiplayerLoadKey:
                    ModData = e.ReadAs<Data.Data>();
                    MapDataFromSave();
                    InitializeAllUpgrades();
                    break;
                case Consts.MultiplayerUpdate:
                    UpgradeForTonight = e.ReadAs<string>();
                    break;
                case Consts.MultiplayerJunimopointsKey:
                    JunimoPoints = e.ReadAs<int>();
                    break;
            }
        }

        public static void LoadData()
        {
            _monitor.Log("Loading mod data...");

            if (!Context.IsMainPlayer) return;

            try
            {
                ModData = _helper.Data.ReadSaveData<Data.Data>(Consts.SaveDataKey);
            }
            catch (Exception e)
            {
                ModData = new Data.Data();
                _helper.Data.WriteSaveData(Consts.SaveDataKey,ModData);
                _monitor.Log($"Better Greenhouse save data has been corrupted and removed: {e.Message} + {e.StackTrace}",LogLevel.Error);
            }
            
            if (ModData == null)
            {
                _monitor.Log("No previous mod data found on this save, generating new data", LogLevel.Info);
                ModData = new Data.Data();
            }

            MapDataFromSave();
            InitializeAllUpgrades();

            _helper.Multiplayer.SendMessage(ModData, Consts.MultiplayerLoadKey, new[] { Consts.ModUniqueID });

        }

        private static void MapDataFromSave()
        {
            JunimoPoints = ModData.JunimoPoints;
            var activeData = ModData.UpgradesStatus;
            Upgrades = new List<Upgrade>
            {
                new SizeUpgrade()
                {
                    Active = activeData.ContainsKey(UpgradeTypes.SizeUpgrade) &&
                             activeData[UpgradeTypes.SizeUpgrade].Active,
                                 Unlocked = activeData.ContainsKey(UpgradeTypes.SizeUpgrade) &&
                                            activeData[UpgradeTypes.SizeUpgrade].Unlocked
                },
                new AutoWaterUpgrade()
                {
                    Active = activeData.ContainsKey(UpgradeTypes.AutoWaterUpgrade) &&
                             activeData[UpgradeTypes.AutoWaterUpgrade].Active,
                    Unlocked = activeData.ContainsKey(UpgradeTypes.AutoWaterUpgrade) &&
                             activeData[UpgradeTypes.AutoWaterUpgrade].Unlocked
                },
                new AutoHarvestUpgrade()
                {
                    Active = activeData.ContainsKey(UpgradeTypes.AutoHarvestUpgrade) &&
                             activeData[UpgradeTypes.AutoHarvestUpgrade].Active,
                    Unlocked = activeData.ContainsKey(UpgradeTypes.AutoHarvestUpgrade) &&
                               activeData[UpgradeTypes.AutoHarvestUpgrade].Unlocked
                }
            };

        }

        private static void MapDataToSave()
        {
            ModData.JunimoPoints = JunimoPoints;
            ModData.UpgradesStatus.Clear();
            foreach (var upgrade in Upgrades)
            {
                ModData.UpgradesStatus.Add(upgrade.Type,new UpgradeData()
                {
                    Active = upgrade.Active, 
                    Unlocked = upgrade.Unlocked
                });
            }
        }

        private static void InitializeAllUpgrades()
        {
            foreach (var upgrade in Upgrades)
            {
                upgrade.Initialize(_helper, _monitor);
                upgrade.Start();
            }
        }

        public static void SaveData()
        {
            if (!Context.IsMainPlayer) return;

            _monitor.Log("Saving mod data...");
            MapDataToSave();
            _helper.Data.WriteSaveData(Consts.SaveDataKey, ModData);
        }

        public static void SetUpgradeForTonight(string upgradeName)
        {
            UpgradeForTonight = upgradeName;
            _helper.Multiplayer.SendMessage(upgradeName, Consts.MultiplayerUpdate, new[] { Consts.ModUniqueID });
        }

        public static void PerformEndOfDayUpdate(bool save = true)
        {
            if (UpgradeForTonight != null)
            {
                string translatedName = $"{_helper.Translation.Get(UpgradeForTonight+ ".Name")}";
                _monitor.Log($"Applying {translatedName}...", LogLevel.Info);

                Upgrade upgrade = Upgrades.FirstOrDefault(u => u.Name == UpgradeForTonight);
                if (upgrade == null)
                {
                    _monitor.Log($"{UpgradeForTonight} not found. Upgrade aborted", LogLevel.Error);
                    UpgradeForTonight = null;
                    return;
                }


                upgrade.Unlocked = true;
                upgrade.Start();
                UpgradeForTonight = null;
            }

            if (save)
                SaveData();
        }
    }
}