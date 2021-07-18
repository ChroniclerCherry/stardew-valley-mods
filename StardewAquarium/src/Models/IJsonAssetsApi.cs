namespace StardewAquarium.Models
{
    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
}
