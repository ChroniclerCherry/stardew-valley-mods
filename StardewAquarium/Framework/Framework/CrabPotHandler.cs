using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace StardewAquarium.Framework.Framework;

internal static class CrabPotHandler
{
    private static IMonitor Monitor = null!;

    internal static void Init(IGameLoopEvents events, IMonitor monitor)
    {
        Monitor = monitor;
        events.DayStarted += OnDayStart;
    }

    /// <summary>Updates crab pots</summary>
    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private static void OnDayStart(object sender, DayStartedEventArgs e)
    {
        GameLocation loc = Game1.getLocationFromName(ContentPackHelper.ExteriorLocationName);
        if (loc is null)
            return;

        // the original method actually flat out re-implemented the code for crab pots. We're...not going to do that anymore.
        // it was meant to mimic crab pot behavior on the beach, which is now in the data.

        // HOWEVER that wasn't actually what it did. Instead, it just...caught something. Even if not baited.
        // We will mimic this by baiting the crab pots ourselves.

        // This map also apparently will remove trash if that's what's rolled.

        foreach (SObject obj in loc.objects.Values)
        {
            if (obj is not CrabPot pot || (pot.heldObject.Value is not null && pot.heldObject.Value.Category != SObject.junkCategory))
            {
                continue;
            }

            try
            {
                pot.heldObject.Value = null;
                pot.bait.Value ??= new SObject("685", 1); // normal bait.
                pot.DayUpdate();
            }
            catch (Exception ex)
            {
                Monitor.Log(ex.ToString());
            }
        }
    }
}
