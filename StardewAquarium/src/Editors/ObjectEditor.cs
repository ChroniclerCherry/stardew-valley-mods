using StardewModdingAPI;
using StardewValley.GameData.Objects;

namespace StardewAquarium.Editors
{
    class ObjectEditor
    {
        private const string ObjInfoPath = "Data/Objects";

        public bool CanEdit(IAssetName assetName)
        {
            return ModEntry.JsonAssets != null && assetName.IsEquivalentTo(ObjInfoPath);
        }

        public void Edit(IAssetData asset)
        {
            if (asset.NameWithoutLocale.IsEquivalentTo(ObjInfoPath))
            {
                string id = ModEntry.JsonAssets.GetObjectId(ModEntry.LegendaryBaitName);
                var data = asset.AsDictionary<string, ObjectData>().Data;
                if (data.ContainsKey(id))
                {
                    data[id].Category = -21;
                    /*string[] fields = data[id].Split('/');
                    fields[3] = "Basic -21";
                    data[id] = string.Join("/", fields);*/
                }

            }
        }
    }
}
