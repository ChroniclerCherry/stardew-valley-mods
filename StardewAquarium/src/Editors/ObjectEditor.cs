using StardewModdingAPI;
using StardewValley;
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
                if (data.TryGetValue(id, out ObjectData entry))
                    entry.Category = Object.baitCategory;

            }
        }
    }
}
