using StardewModdingAPI;

namespace BetterGreenhouse.Upgrades
{
    public abstract class Upgrade
    {
        public abstract string UpgradeName { get; }
        public abstract bool Active { get; set; }
        public abstract bool DisableOnFarmhand { get; set; }
        public abstract int Cost { get;}

        public abstract void Start();

        public string translatedName => _helper.Translation.Get($"{UpgradeName}.Name");
        public string translatedDescription => _helper.Translation.Get($"{UpgradeName}.Name");


        public IModHelper _helper;
        public IMonitor _monitor;
        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        /// <summary>
        /// Honestly I can't think of a state where we'd need to stop and remove an upgrade
        /// But this is added in for safety when I need to
        /// </summary>
        public void Stop()
        {
            Active = false;
        }
    }
}
