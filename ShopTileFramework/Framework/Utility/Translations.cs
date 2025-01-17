using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace ShopTileFramework.Framework.Utility;

/// <summary>
/// This class stores the current language and handles translation work
/// </summary>
internal class Translations
{
    /*********
    ** Fields
    *********/
    private static LocalizedContentManager.LanguageCode SelectedLanguage;


    /*********
    ** Public methods
    *********/
    /// <summary>
    /// Given the english string, and then a dictionary of localized versions of the string,
    /// return the string of the current selected language
    /// Pretty much copy&pasted from Json Assets
    /// </summary>
    /// <param name="english">the english string</param>
    /// <param name="translations">each key is a language code with the value being the translated string</param>
    /// <returns>The string of the current language if available, english as a default</returns>
    public static string Localize(string english, Dictionary<string, string> translations)
    {
        if (SelectedLanguage == LocalizedContentManager.LanguageCode.en)
            return english;
        if (translations == null || !translations.ContainsKey(SelectedLanguage.ToString()))
            return english;
        return translations[SelectedLanguage.ToString()];
    }

    /// <summary>
    /// Update the selectedLanguage to the current language on save loaded, in case it has been changed
    /// </summary>
    public static void UpdateSelectedLanguage()
    {
        SelectedLanguage = LocalizedContentManager.CurrentLanguageCode;
        ModEntry.StaticMonitor.Log($"Updating current language settings: {SelectedLanguage}", LogLevel.Trace);
    }
}
