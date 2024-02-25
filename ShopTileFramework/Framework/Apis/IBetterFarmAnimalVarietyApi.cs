using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Apis
{
    /// <summary>
    /// Interface for Better Farm Animal Variety API
    /// </summary>
    public interface IBetterFarmAnimalVarietyApi
    {
        bool IsEnabled();
        List<Object> GetAnimalShopStock(Farm farm);
        Dictionary<string, List<string>> GetFarmAnimalCategories();

    }
}