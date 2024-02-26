using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class AchievementEditor

    {
        private IModHelper _helper;

        public const int AchievementId = 637201;

        public AchievementEditor(IModHelper helper)
        {
            this._helper = helper;
        }

        public bool CanEdit(IAssetName assetName)
        {
            return assetName.IsEquivalentTo("Data/Achievements");
        }

        public void Edit(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            data[AchievementId] = $"{this._helper.Translation.Get("AchievementName")}^{this._helper.Translation.Get("AchievementDescription")}^true^-1^-1";
        }
    }
}
