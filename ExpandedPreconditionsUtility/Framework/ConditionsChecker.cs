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
            _helper = helper;
            _monitor = monitor;
        }

        public void Initialize(bool verbose, string uniqueId)
        {
            _conditionChecker = new ConditionChecker(_helper, _monitor, verbose, uniqueId);
        }
        public bool CheckConditions(string[] conditions)
        {
            return _conditionChecker.CheckConditions(conditions);
        }

        public bool CheckConditions(string conditions)
        {
            return _conditionChecker.CheckConditions(new[] { conditions });
        }
    }
}
