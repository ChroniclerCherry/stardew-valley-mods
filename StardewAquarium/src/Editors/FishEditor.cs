using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Editors
{
    class FishEditor : IAssetEditor
    {
        private readonly IModHelper _helper;

        public FishEditor(IModHelper helper)
        {
            this._helper = helper;
        }
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return ModEntry.JsonAssets != null &&
                   (asset.AssetNameEquals("Data\\Fish")
                    || asset.AssetNameEquals("Data\\AquariumFish")
                    || asset.AssetNameEquals("LooseSprites\\AquariumFish")
                    || asset.AssetNameEquals("Data\\Locations"));
        }

        public void Edit<T>(IAssetData asset)
        {
            int id = ModEntry.JsonAssets.GetObjectId(ModEntry.PufferChickName);
            if (asset.AssetNameEquals("Data\\Fish"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                string localizedName = Game1.objectInformation[id].Split('/')[4];
                data.Add(id, $"{localizedName}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0");
            }
            else if (asset.AssetNameEquals("Data\\AquariumFish"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                data.Add(id, "20/float");
            }
            else if (asset.AssetNameEquals("LooseSprites\\AquariumFish"))
            {
                var editor = asset.AsImage();

                Texture2D sourceImage = this._helper.Content.Load<Texture2D>("data\\Objects\\Pufferchick\\object.png", ContentSource.ModFolder);
                editor.PatchImage(sourceImage, targetArea: new Rectangle(4, 52, 16, 16));
            }
            else if (asset.AssetNameEquals("Data\\Locations"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data[ModEntry.Data.ExteriorMapName] = data["Beach"];
            }

        }
    }
}
