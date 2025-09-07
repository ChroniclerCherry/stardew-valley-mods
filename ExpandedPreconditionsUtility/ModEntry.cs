using ExpandedPreconditionsUtility.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace ExpandedPreconditionsUtility;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The conditions checker.</summary>
    private ConditionsChecker ConditionsChecker = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.ConditionsChecker = new ConditionsChecker(this.Monitor, this.Helper);

        GameStateQuery.Register($"{this.ModManifest.UniqueID}", this.HandleGameStateQuery);
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return this.ConditionsChecker;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Handle the game state query that checks an Expanded Preconditions Utility condition.</summary>
    /// <inheritdoc cref="GameStateQueryDelegate" />
    private bool HandleGameStateQuery(string[] query, GameStateQueryContext context)
    {
        string queryStr = string.Join(" ", ArgUtility.GetSubsetOf(query, 1));

        return this.ConditionsChecker.CheckConditions(queryStr);
    }
}
