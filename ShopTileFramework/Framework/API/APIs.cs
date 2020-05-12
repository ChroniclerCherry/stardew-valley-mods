using ShopTileFramework.Framework.Shop;
using StardewModdingAPI;

namespace ShopTileFramework.Framework.API
{
    class APIs
    {
        internal static IJsonAssetsApi JsonAssets;
        internal static IBFAVApi BFAV;

        public static void RegisterJsonAssets()
        {
            JsonAssets = ModEntry.helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            if (JsonAssets == null)
            {
                ModEntry.monitor.Log("Json Assets API not detected. Custom JA items will not be added to shops.",
                    LogLevel.Info);
            }

        }

        public static void RegisterBFAV()
        {
            BFAV = ModEntry.helper.ModRegistry.GetApi<IBFAVApi>("Paritee.BetterFarmAnimalVariety");

            if (BFAV == null)
            {
                ModEntry.monitor.Log("BFAV API not detected. Custom farm animals will not be added to animal shops.",
                    LogLevel.Info);
            }
            else if (!BFAV.IsEnabled())
            {
                BFAV = null;
                ModEntry.monitor.Log("BFAV is installed but not enabled. Custom farm animals will not be added to animal shops.",
                    LogLevel.Info);
            }
        }

        public static void RegisterFAVR()
        {
            //TODO: when FAVR is released, start deprecating support for BFAV
        }

    }
}
