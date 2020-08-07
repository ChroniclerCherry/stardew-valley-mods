using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewModdingAPI;

namespace BetterGreenhouse.Upgrades
{
    public enum UpgradeTypes
    {
        AutoWaterUpgrade, SizeUpgrade, AutoHarvestUpgrade
    }
    public abstract class Upgrade
    {
        public abstract UpgradeTypes Type { get; }
        public virtual string Name => Type.ToString();
        public abstract bool Active { get; set; }
        public abstract bool Unlocked { get; set; }
        public virtual int Cost => State.Config.UpgradeCosts[Type];

        public virtual bool DisableOnFarmhand { get; } = false;
        public virtual void Start()
        {
            if (!Context.IsMainPlayer && DisableOnFarmhand) return;
            if (!Unlocked) return;
            Active = true;
        }
        public abstract void Stop();

        public string TranslatedName => _helper.Translation.Get($"{Name}.Name");
        public string TranslatedDescription => _helper.Translation.Get($"{Name}.Description");


        public IModHelper _helper;
        public IMonitor _monitor;
        public virtual void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }
    }

}
