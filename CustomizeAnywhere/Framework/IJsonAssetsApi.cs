namespace CustomizeAnywhere.Framework
{
    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
}
