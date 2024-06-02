using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;

namespace StardewAquarium.Editors
{
    class FishEditor
    {
        private readonly IModHelper _helper;

        public FishEditor(IModHelper helper)
        {
            this._helper = helper;
        }
        public bool CanEdit(IAssetName assetName)
        {
            return
                ModEntry.JsonAssets != null
                && (
                    assetName.IsEquivalentTo("Data/Fish")
                    || assetName.IsEquivalentTo("Data/AquariumFish")
                    || assetName.IsEquivalentTo("LooseSprites/AquariumFish")
                    || assetName.IsEquivalentTo("Data/Locations")
                );
        }

        public void Edit(IAssetData asset)
        {
            string id = ModEntry.JsonAssets.GetObjectId(ModEntry.PufferChickName);
            if (asset.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                string localizedName = Game1.objectData[id].DisplayName;
                data.Add(id, $"{localizedName}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0");
            }
            else if (asset.NameWithoutLocale.IsEquivalentTo("Data/AquariumFish"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add(id, "20/float");
            }
            else if (asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/AquariumFish"))
            {
                var editor = asset.AsImage();

                Texture2D sourceImage = this._helper.GameContent.Load<Texture2D>("data/Objects/Pufferchick/object.png");
                editor.PatchImage(sourceImage, targetArea: new Rectangle(4, 52, 16, 16));
            }
            else if (asset.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                var data = asset.AsDictionary<string, LocationData>().Data;
                data[ModEntry.Data.ExteriorMapName] = data["Beach"];
            }

        }
    }
}
