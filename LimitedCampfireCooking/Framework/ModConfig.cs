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
}
