using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTileFramework.Framework.Utility
{
    class Translations
    {
        private static LocalizedContentManager.LanguageCode currLang;
        public static string Localize(string english, Dictionary<string, string> translations)
        {
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return english;
            if (translations == null || !translations.ContainsKey(currLang.ToString()))
                return english;
            return translations[currLang.ToString()];
        }

        internal static void LoadCurrentLang()
        {
            currLang = LocalizedContentManager.CurrentLanguageCode;
        }
    }
}
