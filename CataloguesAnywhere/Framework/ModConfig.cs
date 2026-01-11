using System;
using System.Collections.Generic;
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

    /// <summary>The key binds which open each shop ID.</summary>
    public Dictionary<string, KeybindList> Catalogues = new(StringComparer.OrdinalIgnoreCase)
    {
        [ShopIds.WallpaperCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D1),

        [ShopIds.FurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D2),
        [ShopIds.JojaFurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D3),
        [ShopIds.JunimoFurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D4),
        [ShopIds.RetroFurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D5),
        [ShopIds.TrashFurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D6),
        [ShopIds.WizardFurnitureCatalogueId] = KeybindList.ForSingle(SButton.LeftControl, SButton.D7)
    };


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
        this.Catalogues = new Dictionary<string, KeybindList>(this.Catalogues ?? [], StringComparer.OrdinalIgnoreCase);
    }
}
