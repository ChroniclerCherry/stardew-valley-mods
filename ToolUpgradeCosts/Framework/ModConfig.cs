using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace ToolUpgradeCosts.Framework;

internal class ModConfig
{
    public Dictionary<UpgradeMaterials, Upgrade> UpgradeCosts { get; set; } = new()
    {
        [UpgradeMaterials.Copper] = new Upgrade
        {
            Cost = 2000,
            MaterialName = "Copper Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Steel] = new Upgrade
        {
            Cost = 5000,
            MaterialName = "Iron Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Gold] = new Upgrade
        {
            Cost = 10000,
            MaterialName = "Gold Bar",
            MaterialStack = 5
        },
        [UpgradeMaterials.Iridium] = new Upgrade
        {
            Cost = 25000,
            MaterialName = "Iridium Bar",
            MaterialStack = 5
        }
    };

    /// <summary>Whether to halve the price of a trash can upgrade.</summary>
    public bool TrashCanHalfPrice { get; set; } = true;


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
        this.UpgradeCosts ??= [];
    }
}
