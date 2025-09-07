using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace CustomCraftingStations.Framework;

internal class CraftingStationConfig
{
    /// <summary>The name of the big craftable which can be clicked to open the menu.</summary>
    public string? BigCraftable { get; set; }

    /// <summary>An ID which can be used with the <c>Action CraftingStation {id}</c> tile property to open the menu.</summary>
    public string? TileData { get; set; }

    /// <summary>Whether the recipes for this station should be excluded from the vanilla cooking/crafting menus.</summary>
    public bool ExclusiveRecipes { get; set; } = true;

    /// <summary>The crafting recipe names to show for this station.</summary>
    /// <remarks>You should specify either <see cref="CraftingRecipes"/> or <see cref="CookingRecipes"/>, not both.</remarks>
    public List<string> CraftingRecipes { get; set; } = [];

    /// <summary>The cooking recipe names to show for this station.</summary>
    /// <inheritdoc cref="CraftingRecipes" path="/remarks" />
    public List<string> CookingRecipes { get; set; } = [];


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
        this.CraftingRecipes = DeserializationHelper.ToNonNullable(this.CraftingRecipes);
        this.CookingRecipes = DeserializationHelper.ToNonNullable(this.CookingRecipes);
    }
}
