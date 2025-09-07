using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace MultiYieldCrops.Framework;

internal class Rule
{
    /*********
    ** Accessors
    *********/
    public string ExtraYieldItemType = "Object";
    public string? ItemName;
    public int MinHarvest = 1;
    public int MaxHarvest = 1;
    public float MaxHarvestIncreasePerFarmingLevel = 0;
    public string[] DisableWithMods = [];


    /*********
    ** Private methods
    *********/
    /// <summary>The method called after the config file is deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.UsedViaReflection)]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = SuppressReasons.UsedViaReflection)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = SuppressReasons.UsedViaReflection)]
    private void OnDeserializedMethod(StreamingContext context)
    {
        this.ExtraYieldItemType ??= "Object";
        this.DisableWithMods = DeserializationHelper.ToNonNullable(this.DisableWithMods);
    }
}
