using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewModdingAPI;

namespace BetterGreenhouse.Upgrades
{
    public enum UpgradeTypes
    {
        AutoWaterUpgrade, SizeUpgrade
    }
    public abstract class Upgrade
    {
        public abstract UpgradeTypes Type { get; }
        public abstract string Name { get; }
        public abstract bool Active { get; set; }
        public abstract bool Unlocked { get; set; }
        public virtual bool DisableOnFarmhand { get; set; } = false;
        public abstract int Cost { get;}
        public abstract void Start();
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
