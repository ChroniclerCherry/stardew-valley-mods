using System;
using System.Collections.Generic;

namespace ShopTileFramework.API
{
    /// <summary>
    /// Interface for Json Assets API
    /// </summary>
    public interface IJsonAssetsApi
    {
        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);

        int GetCropId(string name);
        int GetFruitTreeId(string name);

        event EventHandler AddedItemsToShop;
    }

    /// <summary>
    /// Boots added to JA API in Dec 2020.
    /// </summary>
    public interface IJsonAssetsApiWithBoots : IJsonAssetsApi
    {
        List<string> GetAllBootsFromContentPack(string cp);
    }
}
