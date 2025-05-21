using System;
using StardewModdingAPI;

namespace ExpandedPreconditionsUtility.Framework;

public class ConditionsChecker : IConditionsChecker
{
    /*********
    ** Fields
    *********/
    private ConditionChecker? ConditionChecker;

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
        if (this.ConditionChecker is null)
            throw new ArgumentException($"{nameof(this.Initialize)} must be called before {nameof(CheckConditions)}.");

        return this.ConditionChecker.CheckConditions(conditions);
    }

    public bool CheckConditions(string conditions)
    {
        if (this.ConditionChecker is null)
            throw new ArgumentException($"{nameof(this.Initialize)} must be called before {nameof(CheckConditions)}.");

        return this.ConditionChecker.CheckConditions(new[] { conditions });
    }
}
