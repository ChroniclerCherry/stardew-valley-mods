using System.Collections.Generic;
using StardewValley;

namespace TrainStation.Framework.ContentModels;

/// <summary>A boat or train stop that can be visited by the player, as provided by a Train Station content pack or through the API.</summary>
internal class ContentPackStopModel
{
    /// <summary>The display name translations for each language.</summary>
    public Dictionary<string, string> LocalizedDisplayName { get; set; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    public string TargetMapName { get; set; }

    /// <summary>The tile X position to which the player should warp when they select this stop.</summary>
    public int TargetX { get; set; }

    /// <summary>The tile Y position to which the player should warp when they select this stop.</summary>
    public int TargetY { get; set; }

    /// <summary>The gold price to go to that stop.</summary>
    public int Cost { get; set; } = 0;

    /// <summary>The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</summary>
    public int FacingDirectionAfterWarp { get; set; } = Game1.down;

    /// <summary>If set, the Expanded Precondition Utility conditions which indicate whether this stop should appear in the menu at a given time.</summary>
    public string[] Conditions { get; set; }
}
