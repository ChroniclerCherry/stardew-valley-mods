using StardewValley;

namespace TrainStation.Framework;

/// <summary>A boat or train stop that can be visited by the player.</summary>
public interface IStopModel
{
    /// <summary>A unique identifier for this stop.</summary>
    string Id { get; }

    /// <summary>The localized display name.</summary>
    string DisplayName { get; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    string? TargetMapName { get; }

    /// <summary>The tile X position to which the player should warp when they select this stop.</summary>
    int TargetX { get; }

    /// <summary>The tile Y position to which the player should warp when they select this stop.</summary>
    int TargetY { get; }

    /// <summary>The direction the player should be facing after they warp, matching a value recognized by <see cref="Utility.TryParseDirection"/>.</summary>
    int FacingDirectionAfterWarp { get; }

    /// <summary>The gold price to go to that stop.</summary>
    int Cost { get; }

    /// <summary>Whether this is a boat stop; else it's a train stop.</summary>
    bool IsBoat { get; }

    /// <summary>If set, the Expanded Precondition Utility conditions which indicate whether this stop should appear in the menu at a given time.</summary>
    string[]? Conditions { get; }
}
