using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class LocationsEditor : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\Locations");
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            data[ModEntry.data.ExteriorMapName] = data["Beach"];

        }
    }
}
