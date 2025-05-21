using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace TrainStation.Framework.ContentModels;

/// <summary>The data loaded from a Train Station content pack.</summary>
internal class ContentPack
{
    /// <summary>The train stops to register.</summary>
    public List<ContentPackStopModel> TrainStops { get; set; } = [];

    /// <summary>The boat stops to register.</summary>
    public List<ContentPackStopModel> BoatStops { get; set; } = [];


    /*********
    ** Private methods
    *********/
    /// <summary>The method called after the config file is deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.ValidatesNullability)]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = SuppressReasons.UsedViaReflection)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.UsedViaReflection)]
    private void OnDeserializedMethod(StreamingContext context)
    {
        this.TrainStops = DeserializationHelper.ToNonNullable(this.TrainStops);
        this.BoatStops = DeserializationHelper.ToNonNullable(this.BoatStops);
    }
}
