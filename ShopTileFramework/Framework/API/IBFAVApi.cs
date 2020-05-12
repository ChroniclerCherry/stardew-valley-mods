using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    /// <summary>
    /// Interface for Better Farm Animal Variety API
    /// </summary>
    public interface IBFAVApi
    {
        bool IsEnabled();
        List<Object> GetAnimalShopStock(Farm farm);
        Dictionary<string, List<string>> GetFarmAnimalCategories();

    }
}