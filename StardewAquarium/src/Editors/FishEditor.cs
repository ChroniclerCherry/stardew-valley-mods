using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Editors
{
    class FishEditor : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\Fish") && ModEntry.JsonAssets != null; 
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            int id = ModEntry.JsonAssets.GetObjectId(ModEntry.PufferChickName);
            string localizedName = Game1.objectInformation[id].Split('/')[4];
            data.Add(id, $"{localizedName}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0");
        }
    }
}
