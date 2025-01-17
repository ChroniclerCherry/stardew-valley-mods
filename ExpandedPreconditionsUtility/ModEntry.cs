using ExpandedPreconditionsUtility.Framework;
using StardewModdingAPI;

namespace ExpandedPreconditionsUtility;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private IModHelper _helper;
    private IMonitor _monitor;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this._helper = helper;
        this._monitor = this.Monitor;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ConditionsChecker(this._monitor, this._helper);
    }
}
