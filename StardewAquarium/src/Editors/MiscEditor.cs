using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class MiscEditor
    {
        private const string UIPath = "Strings/UI";
        private readonly IModHelper _helper;

        public MiscEditor(IModHelper helper)
        {
            this._helper = helper;
        }

        public bool CanEdit(IAssetName assetName)
        {
            return assetName.IsEquivalentTo(UIPath);
        }

        public void Edit(IAssetData asset)
        {
            if (asset.NameWithoutLocale.IsEquivalentTo(UIPath))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add("Chat_StardewAquarium.FishDonated", this._helper.Translation.Get("FishDonatedMP"));
                data.Add("Chat_StardewAquarium.AchievementUnlocked", this._helper.Translation.Get("AchievementUnlockedMP"));
            }
        }
    }
}
