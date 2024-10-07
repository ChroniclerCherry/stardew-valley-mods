using System.Collections.Generic;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;

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
                );
        }

        public void Edit(IAssetData asset)
        {
            string id = ModEntry.JsonAssets.GetObjectId(ModEntry.PufferChickName);
            if (asset.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data.Add(id, $"{Game1.objectData[id].Name}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0/false");
            }
            else if (asset.NameWithoutLocale.IsEquivalentTo("Data/AquariumFish"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data.Add(id, "20/float");
            }
            else if (asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/AquariumFish"))
            {
                IAssetDataForImage editor = asset.AsImage();

                IRawTextureData sourceImage = this._helper.ModContent.Load<IRawTextureData>("data/Objects/Pufferchick/object.png");
                editor.PatchImage(sourceImage, targetArea: new Rectangle(4, 52, 16, 16));
            }

        }
    }
}
