using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewAquarium.Editors
{
    internal class MiscEditor : IAssetManager
    {
        private const string UIPath = "Strings\\UI";
        private readonly IAssetName uiPath;
        private readonly ITranslationHelper translations;

        public MiscEditor(ITranslationHelper translations, IGameContentHelper parser)
        {
            this.uiPath = parser.ParseAssetName(UIPath);
            this.translations = translations;
        }

        public bool TryHandleAsset(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(uiPath))
            {
                e.Edit(this.EditImpl);
                return true;
            }
            return false;
        }

        private void EditImpl(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            data.Add("Chat_StardewAquarium.FishDonated", this.translations.Get("FishDonatedMP"));
            data.Add("Chat_StardewAquarium.AchievementUnlocked", this.translations.Get("AchievementUnlockedMP"));
        }
    }
}