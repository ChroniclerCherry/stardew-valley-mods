using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    public interface IBFAVApi
    {
        bool IsEnabled();
        List<Object> GetAnimalShopStock(Farm farm);
        Dictionary<string, List<string>> GetFarmAnimalCategories();

    }
}