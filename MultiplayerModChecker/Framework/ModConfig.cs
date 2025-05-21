using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace MultiplayerModChecker.Framework;

internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    public string[] IgnoredMods { get; set; } = ["Cherry.MultiplayerModChecker"];
    public bool HideReportInTrace { get; set; } = false;


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
        this.IgnoredMods = DeserializationHelper.ToNonNullable(this.IgnoredMods);
    }
}
