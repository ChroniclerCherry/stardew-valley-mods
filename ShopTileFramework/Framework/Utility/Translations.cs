using StardewValley;
using System.Collections.Generic;

namespace ShopTileFramework.Framework.Utility
{
    class Translations
    {
        private static LocalizedContentManager.LanguageCode selectedLanguage;
        public static string Localize(string english, Dictionary<string, string> translations)
        {
            if (selectedLanguage == LocalizedContentManager.LanguageCode.en)
                return english;
            if (translations == null || !translations.ContainsKey(selectedLanguage.ToString()))
                return english;
            return translations[selectedLanguage.ToString()];
        }

        internal static void UpdateSelectedLanguage()
        {
            selectedLanguage = LocalizedContentManager.CurrentLanguageCode;
        }
    }
}
