using ExpandedPreconditionsUtility.Framework;
using StardewModdingAPI;

namespace ExpandedPreconditionsUtility;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper) { }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ConditionsChecker(this.Monitor, this.Helper);
    }
}
