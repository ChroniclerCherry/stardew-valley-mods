using System.Collections.Generic;

namespace TrainStation.Framework.ContentModels;

/// <summary>The data model for the data asset containing bus and train stops.</summary>
internal class ContentModel
{
    /// <summary>The boat stops that can be visited by the player.</summary>
    public List<StopModel> BoatStops { get; set; } = new();

    /// <summary>The train stops that can be visited by the player.</summary>
    public List<StopModel> TrainStops { get; set; } = new();
}
