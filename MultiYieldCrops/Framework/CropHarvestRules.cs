using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace MultiYieldCrops.Framework;

internal class CropHarvestRules
{
    /*********
    ** Accessors
    *********/
    public string? CropName;
    public List<Rule> HarvestRules = [];


    /*********
    ** Private methods
    *********/
    /// <summary>The method called after the config file is deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = SuppressReasons.UsedViaReflection)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.UsedViaReflection)]
    private void OnDeserializedMethod(StreamingContext context)
    {
        this.HarvestRules = DeserializationHelper.ToNonNullable(this.HarvestRules);
    }
}
