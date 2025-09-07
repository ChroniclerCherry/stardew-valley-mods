using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using ChroniclerCherry.Common;

namespace LimitedCampfireCooking.Framework;

internal class ModConfig
{
    public bool EnableAllCookingRecipes { get; set; } = false;
    public string[] Recipes { get; set; } = [
        "Fried Egg",
        "Baked Fish",
        "Parsnip Soup",
        "Vegetable Stew",
        "Bean Hotpot",
        "Glazed Yams",
        "Carp Surprise",
        "Fish Taco",
        "Tom Kha Soup",
        "Trout Soup",
        "Pumpkin Soup",
        "Algae Soup",
        "Pale Broth",
        "Roasted Hazelnuts",
        "Chowder",
        "Lobster Bisque",
        "Fish Stew"
    ];



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
        this.Recipes = DeserializationHelper.ToNonNullable(this.Recipes);
    }
}
