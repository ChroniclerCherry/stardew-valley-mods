using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace StardewAquarium.Editors
{
    class MailEditor : IAssetManager
    {
        private const string AquariumOpenAfterLandslide = "StardewAquarium.Open";
        private const string AquariumOpenLater = "StardewAquarium.OpenLater";

        private readonly ITranslationHelper translations;
        private readonly IAssetName mail;

        public MailEditor(IModHelper helper)
        {
            this.translations = helper.Translation;
            this.mail = helper.GameContent.ParseAssetName("Data\\mail");
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {

            if (Game1.player.hasOrWillReceiveMail(AquariumOpenAfterLandslide) || Game1.player.hasOrWillReceiveMail(AquariumOpenLater))
                return;

            if (Game1.Date.TotalDays == 30)
                Game1.player.mailbox.Add(AquariumOpenAfterLandslide);

            if (Game1.Date.TotalDays > 30)
                Game1.player.mailbox.Add(AquariumOpenLater);
        }

        public bool TryHandleAsset(AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(this.mail))
            {
                e.Edit(this.EditImpl);
                return true;
            }
            return false;
        }

        public void EditImpl(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            data[AquariumOpenAfterLandslide] = this.translations.Get("AquariumOpenLandslide");
            data[AquariumOpenLater] = this.translations.Get("AquariumOPenLater");
        }


    }
}
