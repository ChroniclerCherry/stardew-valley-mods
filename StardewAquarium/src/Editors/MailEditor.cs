using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium.Editors
{
    class MailEditor : IAssetEditor
    {
        private IModHelper _helper;
        private const string AquariumOpenAfterLandslide = "StardewAquarium.Open";
        private const string AquariumOpenLater = "StardewAquarium.OpenLater";
        public MailEditor(IModHelper helper)
        {
            _helper = helper;
            _helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (Game1.player.hasOrWillReceiveMail(AquariumOpenAfterLandslide) || Game1.player.hasOrWillReceiveMail(AquariumOpenLater))
                return;

            if (Game1.Date.TotalDays == 30)
                Game1.player.mailbox.Add(AquariumOpenAfterLandslide);

            if (Game1.Date.TotalDays > 30)
                Game1.player.mailbox.Add(AquariumOpenLater);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\mail");
        }

        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            data[AquariumOpenAfterLandslide] = _helper.Translation.Get("AquariumOpenLandslide");
            data[AquariumOpenLater] = _helper.Translation.Get("AquariumOPenLater");
        }
    }
}
