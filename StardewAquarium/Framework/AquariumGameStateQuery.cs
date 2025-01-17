using System;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace StardewAquarium.Framework;

internal static class AquariumGameStateQuery
{
    /*********
    ** Fields
    *********/
    private const string RandomChanceForPuffer = "StardewValleyAquarium_RANDOM_CHANCE_FOR_PUFFER";


    /*********
    ** Accessors
    *********/
    public const string HasBaitQuery = "StardewValleyAquarium_PLAYER_HAS_BAIT";


    /*********
    ** Public methods
    *********/
    public static void Init()
    {
        GameStateQuery.Register(HasBaitQuery, HasBait);
        GameStateQuery.Register(RandomChanceForPuffer, PufferChance);
    }


    /*********
    ** Private methods
    *********/
    private static bool PufferChance(string[] query, GameStateQueryContext context)
    {
        double pufferChance = 0.01 + 0.005 * Utils.GetNumDonatedFish();
        return Random.Shared.NextBool(pufferChance);
    }

    /// <summary>Get whether a player has the legendary bait.</summary>
    /// <inheritdoc cref="GameStateQueryDelegate" />
    private static bool HasBait(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out string playerKey, out string error, false))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        return GameStateQuery.Helpers.WithPlayer(context.Player, playerKey, (target) =>
        {
            SObject bait = (target?.CurrentTool as FishingRod)?.GetBait();

            if (bait is not null)
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
