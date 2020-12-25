using System.Collections.Generic;

namespace ShopTileFramework.API
{
    /// <summary>
    /// Interface for Custom Furniture API
    /// </summary>
    public interface ICustomFurnitureApi
    {
        List<string> GetAllFurnitureFromContentPack(string cp);
    }
}
