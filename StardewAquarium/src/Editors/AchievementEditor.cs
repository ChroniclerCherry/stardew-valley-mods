using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewAquarium.Editors
{
    class AchievementEditor : IAssetManager
    {
        private readonly ITranslationHelper _translations;

        private readonly IAssetName _achievements;

        public const int AchievementId = 637201;

        public AchievementEditor(ITranslationHelper translations, IGameContentHelper parser)
        {
            this._translations = translations;
            this._achievements = parser.ParseAssetName("Data\\Achievements");
        }

        public bool TryHandleAsset(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(_achievements))
            {
                e.Edit(this.EditImpl);
                return true;
            }
            return false;
        }

        private void EditImpl(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            data[AchievementId]
                = $"{_translations.Get("AchievementName")}^{_translations.Get("AchievementDescription")}^true^-1^-1";
        }

        
    }
}
