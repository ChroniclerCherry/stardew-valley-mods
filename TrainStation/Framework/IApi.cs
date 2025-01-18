using System;
using System.Collections.Generic;
using StardewValley;

namespace TrainStation.Framework;

/// <summary>The API interface for the Train Station mod.</summary>
public interface IApi
{
    /// <summary>Open the menu to choose a train destination.</summary>
    void OpenTrainMenu();

    /// <summary>Open the menu to choose a boat destination.</summary>
    void OpenBoatMenu();

    /// <summary>Add a stop to the train network, overwriting it by ID if needed.</summary>
    /// <param name="stopId">A unique identifier for this stop. This should be prefixed with your mod's unique ID, like <c>YourModId_StopId</c>.</param>
    /// <param name="targetMapName">The internal name of the location to which the player should warp when they select this stop.</param>
    /// <param name="localizedDisplayName">The display name translations for each language.</param>
    /// <param name="targetX">The X tile position to which the player should warp when they select this stop.</param>
    /// <param name="targetY">The Y tile position to which the player should warp when they select this stop.</param>
    /// <param name="cost">The gold price to go to that stop.</param>
    /// <param name="facingDirectionAfterWarp">The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</param>
    /// <param name="conditions">If set, the Expanded P a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</param>
    /// <param name="translatedName">The default display name for the stop if not overridden by <paramref name="localizedDisplayName"/>, shown in the bus or train menu.</param>
    [Obsolete("Registering stations through the mod API is deprecated. Consider registering them through the standard data asset instead. See the Train Station README for more info.")]
    void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);

    /// <summary>Add a stop to the boat network, overwriting it by ID if needed.</summary>
    /// <param name="stopId">A unique identifier for this stop. This should be prefixed with your mod's unique ID, like <c>YourModId_StopId</c>.</param>
    /// <param name="targetMapName">The internal name of the location to which the player should warp when they select this stop.</param>
    /// <param name="localizedDisplayName">The display name translations for each language.</param>
    /// <param name="targetX">The X tile position to which the player should warp when they select this stop.</param>
    /// <param name="targetY">The Y tile position to which the player should warp when they select this stop.</param>
    /// <param name="cost">The gold price to go to that stop.</param>
    /// <param name="facingDirectionAfterWarp">The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</param>
    /// <param name="conditions">If set, the Expanded P a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</param>
    /// <param name="translatedName">The default display name for the stop if not overridden by <paramref name="localizedDisplayName"/>, shown in the bus or train menu.</param>
    [Obsolete("Registering stations through the mod API is deprecated. Consider registering them through the standard data asset instead. See the Train Station README for more info.")]
    void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);
}
