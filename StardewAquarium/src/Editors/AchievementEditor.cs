using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class AchievementEditor : IAssetEditor

    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public const int AchievementId = 637201;

        public AchievementEditor(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\Achievements");
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            data[AchievementId]
                = $"{_helper.Translation.Get("AchievementName")}^{_helper.Translation.Get("AchievementDescription")}^true^-1^-1";
        }
    }
}
