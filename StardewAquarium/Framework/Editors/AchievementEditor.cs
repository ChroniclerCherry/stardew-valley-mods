using System.Collections.Generic;

using StardewModdingAPI;

namespace StardewAquarium.Framework.Editors;

internal static class AchievementEditor
{
    public const int AchievementId = 637201;

    public static void Edit(IAssetData asset)
    {
        IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
        data[AchievementId] = $"{I18n.AchievementName()}^{I18n.AchievementDescription()}^true^-1^-1";
    }
}
