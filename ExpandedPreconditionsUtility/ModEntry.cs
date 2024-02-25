using ExpandedPreconditionsUtility.Framework;
using StardewModdingAPI;

namespace ExpandedPreconditionsUtility
{
    public class ModEntry : Mod
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        public override void Entry(IModHelper helper)
        {
            this._helper = this.Helper;
            this._monitor = this.Monitor;
        }

        public override object GetApi()
        {
            return new ConditionsChecker(this._monitor, this._helper);
        }
    }
}
