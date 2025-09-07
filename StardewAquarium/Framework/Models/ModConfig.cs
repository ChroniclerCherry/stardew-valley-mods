using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;
using StardewModdingAPI.Utilities;

namespace StardewAquarium.Framework.Models;

internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    public bool EnableDebugCommands { get; set; } = false;
    public KeybindList CheckDonationCollection { get; set; } = new();


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
        this.CheckDonationCollection ??= new KeybindList();
    }
}
