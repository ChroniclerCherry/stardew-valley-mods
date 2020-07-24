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
        public abstract bool DisableOnFarmhand { get; set; }
        public abstract int Cost { get;}

        public abstract void Start();
        public abstract void Stop();

        public string translatedName => _helper.Translation.Get($"{Name}.Name");
        public string translatedDescription => _helper.Translation.Get($"{Name}.Description");


        public IModHelper _helper;
        public IMonitor _monitor;
        public virtual void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }
    }

}
