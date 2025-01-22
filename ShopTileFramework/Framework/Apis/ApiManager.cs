using StardewModdingAPI;

namespace ShopTileFramework.Framework.Apis;

/// <summary>
/// This class is used to register external APIs and hold the instances of those APIs to be accessed
/// by the rest of the mod
/// </summary>
internal class ApiManager
{
    /*********
    ** Accessors
    *********/
    public static IJsonAssetsApi JsonAssets { get; private set; }
    public static IConditionsApi Conditions { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Register the API for Json Assets
    /// </summary>
    public static void RegisterJsonAssets()
    {
        JsonAssets = ModEntry.StaticHelper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

        if (JsonAssets == null)
        {
            ModEntry.StaticMonitor.Log("Json Assets API not detected. This is only an issue if you're using cystom Json Assets items and shops trying to sell them, as custom items will not appear in shops.",
                LogLevel.Info);
        }
    }

    /// <summary>
    /// Register the API for Expanded Preconditions Utility
    /// </summary>
    public static void RegisterExpandedPreconditionsUtility()
    {
        Conditions = ModEntry.StaticHelper.ModRegistry.GetApi<IConditionsApi>("Cherry.ExpandedPreconditionsUtility");

        if (Conditions == null)
        {
            ModEntry.StaticMonitor.Log("Expanded Preconditions Utility API not detected. Something went wrong, please check that your installation of Expanded Preconditions Utility is valid",
                LogLevel.Error);
            return;
        }

        Conditions.Initialize(ModEntry.VerboseLogging, "Cherry.ShopTileFramework");
    }
}
