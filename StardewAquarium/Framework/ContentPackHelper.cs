using StardewValley;

namespace StardewAquarium.Framework;

/// <summary>Provides values from the Stardew Aquarium content pack component.</summary>
internal static class ContentPackHelper
{
    /*********
    ** Constants
    *********/
    /// <summary>The achievement ID unlocked when you complete the aquarium.</summary>
    public const int AchievementId = 637201;

    /// <summary>The mod ID for the Stardew Aquarium content pack.</summary>
    public const string ContentPackId = "Gervig91.StardewAquariumCP";

    /// <summary>The qualified item ID for the Legendary Bait item.</summary>
    public const string LegendaryBaitQualifiedId = $"{ItemRegistry.type_object}{ContentPackId}_LegendaryBait";

    /// <summary>The qualified item ID for the Pufferchick item.</summary>
    public const string PufferchickQualifiedId = $"{ItemRegistry.type_object}{ContentPackId}_Pufferchick";

    /// <summary>The internal name of the location outside the Stardew Aquarium.</summary>
    public const string ExteriorLocationName = $"{ContentPackId}_ExteriorMuseum";

    /// <summary>The internal name of the main location inside the Stardew Aquarium.</summary>
    public const string InteriorLocationName = $"{ContentPackId}_FishMuseum";


    /*********
    ** Methods
    *********/
    /// <summary>Load a translation string from <c>Strings\\UI</c> provided by the content pack.</summary>
    /// <param name="key">The translation key to load.</param>
    /// <param name="assetName">The asset name from which to load the string.</param>
    public static string LoadString(string key, string assetName = "Strings\\UI")
    {
        return Game1.content.LoadString($"{assetName}:{ContentPackHelper.ContentPackId}_{key}");
    }
}
