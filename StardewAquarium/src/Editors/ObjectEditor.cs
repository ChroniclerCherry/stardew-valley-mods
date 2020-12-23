using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class ObjectEditor : IAssetEditor
    {
        private readonly IModHelper _helper;
        private const string ObjInfoPath = "Data\\ObjectInformation";

        public ObjectEditor(IModHelper helper)
        {
            _helper = helper;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return ModEntry.JsonAssets != null 
                   && asset.AssetNameEquals(ObjInfoPath);
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals(ObjInfoPath))
            {

                int id = ModEntry.JsonAssets.GetObjectId(ModEntry.LegendaryBaitName);
                var data = asset.AsDictionary<int, string>().Data;
                if (data.ContainsKey(id))
                {
                    var fields = data[id].Split('/');
                    fields[3] = "Basic -21";
                    data[id] = string.Join("/", fields);
                }

            }
        }
    }
}
