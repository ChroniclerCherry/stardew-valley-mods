using System.Collections.Generic;

using Microsoft.Xna.Framework;

using StardewAquarium.src.Editors;

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
            if (asset.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
            {

            }
        }
    }
}
