using System.Collections.Generic;

namespace TrainStation.Framework.ContentModels;

/// <summary>The data loaded from a Train Station content pack.</summary>
internal class ContentPack
{
    /// <summary>The train stops to register.</summary>
    public List<ContentPackStopModel> TrainStops { get; set; }

    /// <summary>The boat stops to register.</summary>
    public List<ContentPackStopModel> BoatStops { get; set; }
}
