using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace StardewAquarium.Editors
{
    class FishEditor : IAssetManager
    {
        private readonly ITranslationHelper translations;
        private readonly IAssetName dataFish;
        private readonly IAssetName dataAquarium;
        private readonly IAssetName looseSprites;
        private readonly IAssetName dataLocations;

        private readonly IRawTextureData fishTex;

        public FishEditor(IModHelper helper)
        {
            this.translations = helper.Translation;

            this.dataFish = helper.GameContent.ParseAssetName("Data\\Fish");
            this.dataAquarium = helper.GameContent.ParseAssetName("Data\\AquariumFish");
            this.looseSprites = helper.GameContent.ParseAssetName("LooseSprites\\AquariumFish");
            this.dataLocations = helper.GameContent.ParseAssetName("Data\\Locations");

            this.fishTex = helper.ModContent.Load<IRawTextureData>("data\\Objects\\Pufferchick\\object.png");
        }

        public bool TryHandleAsset(AssetRequestedEventArgs e)
        {
            if (ModEntry.JsonAssets is null)
                return false;

            int id = ModEntry.JsonAssets.GetObjectId(ModEntry.PufferChickName);
            if (id == -1)
                return false;

            if (e.NameWithoutLocale.IsEquivalentTo(this.dataFish))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<int, string>().Data;
                    string localizedName = Game1.objectInformation[id].Split('/')[4];
                    data.Add(id, $"{localizedName}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0");
                });
                return true;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(this.dataAquarium))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<int, string>().Data;
                    data.Add(id, "20/float");
                });
                return true;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(this.looseSprites))
            {
                e.Edit((asset) =>
                {
                    var editor = asset.AsImage();
                    editor.PatchImage(this.fishTex, targetArea: new Rectangle(4, 52, 16, 16));
                });
                return true;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(this.dataLocations))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data[ModEntry.Data.ExteriorMapName] = data["Beach"];
                });
                return true;
            }

            return false;
        }
    }
}
