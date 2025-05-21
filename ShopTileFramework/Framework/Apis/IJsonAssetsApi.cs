#nullable disable

using System;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Apis;

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

    string GetCropId(string name);
    string GetFruitTreeId(string name);

    event EventHandler AddedItemsToShop;
}
