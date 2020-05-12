using System.Collections.Generic;

namespace ShopTileFramework.Framework.API
{
    public interface IJsonAssetsApi
    {
        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);

        int GetObjectId(string name);
        int GetCropId(string name);
        int GetFruitTreeId(string name);
        int GetBigCraftableId(string name);
        int GetHatId(string name);
        int GetWeaponId(string name);
        int GetClothingId(string name);
    }
}
