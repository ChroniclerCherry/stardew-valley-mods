using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CataloguesAnywhere.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether the mod should be enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The key bindings which open the furniture catalogue.</summary>
    public KeybindList FurnitureKey { get; set; } = KeybindList.ForSingle(SButton.LeftControl, SButton.D1);

    /// <summary>The key bindings which open the wallpaper catalogue.</summary>
    public KeybindList WallpaperKey { get; set; } = KeybindList.ForSingle(SButton.LeftControl, SButton.D2);


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
        this.FurnitureKey ??= new KeybindList();
        this.WallpaperKey ??= new KeybindList();
    }
}
