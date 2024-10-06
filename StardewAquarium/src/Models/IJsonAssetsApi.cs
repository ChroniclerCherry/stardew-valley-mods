namespace StardewAquarium.Models
{
    public interface IJsonAssetsApi
    {
        string GetObjectId(string name);
        string GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
}
