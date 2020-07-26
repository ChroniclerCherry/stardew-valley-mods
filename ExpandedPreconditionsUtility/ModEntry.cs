using StardewModdingAPI;

namespace ExpandedPreconditionsUtility
{
    public class ModEntry : Mod
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
        }

        public override object GetApi()
        {
            return new ConditionsChecker(_monitor,_helper);
        }
    }
}
