using StardewModdingAPI;

namespace ExpandedPreconditionsUtility.Framework;

public class ConditionsChecker : IConditionsChecker
{
    /*********
    ** Fields
    *********/
    private ConditionChecker ConditionChecker;

    private readonly IModHelper Helper;
    private readonly IMonitor Monitor;


    /*********
    ** Public methods
    *********/
    internal ConditionsChecker(IMonitor monitor, IModHelper helper)
    {
        this.Helper = helper;
        this.Monitor = monitor;
    }

    public void Initialize(bool verbose, string uniqueId)
    {
        this.ConditionChecker = new ConditionChecker(this.Helper, this.Monitor, verbose, uniqueId);
    }

    public bool CheckConditions(string[] conditions)
    {
        return this.ConditionChecker.CheckConditions(conditions);
    }

    public bool CheckConditions(string conditions)
    {
        return this.ConditionChecker.CheckConditions(new[] { conditions });
    }
}
