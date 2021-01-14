using System.Linq;
using StardewAquarium.Patches;
using StardewModdingAPI;

namespace StardewAquarium.Editors
{
    class MiscEditor : IAssetEditor
    {
        private const string UIPath = "Strings\\UI";
        private const string NPCDispositions = "Data\\NPCDispositions";
        private readonly IModHelper _helper;

        public MiscEditor(IModHelper helper)
        {
            _helper = helper;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(UIPath);
        }

        public void Edit<T>(IAssetData asset)
        { 
            if (asset.AssetNameEquals(UIPath))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add("Chat_StardewAquarium.FishDonated", _helper.Translation.Get("FishDonatedMP"));
                data.Add("Chat_StardewAquarium.AchievementUnlocked", _helper.Translation.Get("AchievementUnlockedMP"));
            }
        }
    }
}