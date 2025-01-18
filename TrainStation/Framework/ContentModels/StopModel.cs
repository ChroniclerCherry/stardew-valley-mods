using Microsoft.Xna.Framework;
using StardewValley;

namespace TrainStation.Framework.ContentModels;

/// <summary>As part of <see cref="ContentModel"/>, a boat or train stop that can be visited by the player.</summary>
internal class StopModel
{
    /// <summary>A unique identifier for this stop.</summary>
    public string Id { get; set; }

    /// <summary>The translated name for the stop, shown in the bus or train menu.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    public string ToLocation { get; set; }

    /// <summary>The tile position to which the player should warp when they select this stop.</summary>
    public Point ToTile { get; set; }

    /// <summary>The direction the player should be facing after they warp, matching a value recognized by <see cref="Utility.TryParseDirection"/>.</summary>
    public string ToFacingDirection { get; set; } = "down";

    /// <summary>The gold price to go to that stop.</summary>
    public int Cost { get; set; } = 0;

    /// <summary>If set, a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</summary>
    public string Conditions { get; set; }
}
