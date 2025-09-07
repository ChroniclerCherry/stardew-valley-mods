using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SnackAnything.Framework;

internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    public bool YummyArtefacts { get; set; } = false;

    public KeybindList HoldToActivate { get; set; } = KeybindList.ForSingle(SButton.LeftShift);


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
        this.HoldToActivate ??= new KeybindList();
    }
}
