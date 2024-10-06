using System;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Tools;

using SObject = StardewValley.Object;

namespace StardewAquarium.src;
internal static class AquariumGameStateQuery
{
    internal const string HasBaitQuery = "StardewValleyAquarium_PLAYER_HAS_BAIT";
    internal static void Init()
    {
        GameStateQuery.Register(HasBaitQuery, HasBait);
    }

    /// <summary>
    /// Checks to see if the player has the legendary bait.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool HasBait(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out string playerKey, out string error, false))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (target) =>
        {
            if (target?.CurrentTool is FishingRod rod && rod.GetBait() is SObject bait)
            {
                foreach (string candidate in query.AsSpan(2))
                {
                    if (bait.QualifiedItemId == candidate)
                    {
                        return true;
                    }
                }
            }

            return false;
        });
    }
}
