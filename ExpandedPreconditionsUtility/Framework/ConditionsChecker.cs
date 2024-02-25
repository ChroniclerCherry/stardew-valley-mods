using StardewModdingAPI;

namespace ExpandedPreconditionsUtility.Framework
{
    public class ConditionsChecker : IConditionsChecker
    {
        private ConditionChecker _conditionChecker;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        internal ConditionsChecker(IMonitor monitor, IModHelper helper)
        {
            this._helper = helper;
            this._monitor = monitor;
        }

        public void Initialize(bool verbose, string uniqueId)
        {
            this._conditionChecker = new ConditionChecker(this._helper, this._monitor, verbose, uniqueId);
        }
        public bool CheckConditions(string[] conditions)
        {
            return this._conditionChecker.CheckConditions(conditions);
        }

        public bool CheckConditions(string conditions)
        {
            return this._conditionChecker.CheckConditions(new[] { conditions });
        }
    }
}
