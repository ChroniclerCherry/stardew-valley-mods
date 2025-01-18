using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using TrainStation.Framework.ContentModels;

namespace TrainStation.Framework.LegacyContentModels;

/// <summary>A boat or train stop defined by a legacy Train Station content pack or the legacy mod API.</summary>
internal class LegacyStopModel : StopModel
{
    /*********
    ** Fields
    *********/
    /// <summary>The default display name to return if no translation is found, or <c>null</c> for a generic 'no translation' message.</summary>
    private string DisplayNameDefault;

    /// <summary>The display name translations for each language.</summary>
    private Dictionary<string, string> DisplayNameTranslations;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public override string DisplayName
    {
        get => this.Localize(this.DisplayNameTranslations, this.DisplayNameDefault);
        set { }
    }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance from a legacy content pack model.</summary>
    /// <param name="id"><inheritdoc cref="StopModel.Id" path="/summary"/></param>
    /// <param name="model">The content pack model to parse.</param>
    public static StopModel FromContentPack(string id, StopContentPackModel model)
    {
        return FromApi(
            id: id,
            toLocation: model.TargetMapName,
            toTile: new Point(model.TargetX, model.TargetY),
            toFacingDirection: model.FacingDirectionAfterWarp,
            cost: model.Cost,
            conditions: model.Conditions,
            displayNameTranslations: model.LocalizedDisplayName,
            displayNameDefault: null
        );
    }

    /// <summary>Construct an instance from a legacy API call.</summary>
    /// <param name="id"><inheritdoc cref="StopModel.Id" path="/summary" /></param>
    /// <param name="toLocation"><inheritdoc cref="StopModel.ToLocation" path="/summary" /></param>
    /// <param name="toTile"><inheritdoc cref="StopModel.ToTile" path="/summary" /></param>
    /// <param name="toFacingDirection"><inheritdoc cref="StopModel.ToFacingDirection" path="/summary" /></param>
    /// <param name="cost"><inheritdoc cref="StopModel.Cost" path="/summary" /></param>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    /// <param name="displayNameTranslations"><inheritdoc cref="DisplayNameTranslations" path="/summary" /></param>
    /// <param name="displayNameDefault"><inheritdoc cref="DisplayNameDefault" path="/summary" /></param>
    public static StopModel FromApi(string id, string toLocation, Point toTile, int toFacingDirection, int cost, string[] conditions, Dictionary<string, string> displayNameTranslations, string displayNameDefault)
    {
        return new LegacyStopModel
        {
            Id = id,
            DisplayName = null,
            ToLocation = toLocation,
            ToTile = toTile,
            ToFacingDirection = toFacingDirection.ToString(),
            Cost = cost,
            Conditions = BuildGameQueryForExpandedPreconditions(conditions),

            DisplayNameTranslations = displayNameTranslations,
            DisplayNameDefault = displayNameDefault
        };
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build a game state query equivalent to the provided Expanded Preconditions Utility conditions.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    private static string BuildGameQueryForExpandedPreconditions(string[] conditions)
    {
        const string expandedPreconditionsQuery = "Cherry.ExpandedPreconditionsUtility";

        switch (conditions?.Length)
        {
            case null:
            case < 1:
                return null;

            case 1:
                return $"{expandedPreconditionsQuery} {conditions[0]}";

            default:
                {
                    string[] queries = new string[conditions.Length];
                    for (int i = 0; i < conditions.Length; i++)
                        queries[i] = $"{expandedPreconditionsQuery} {conditions[i]}";

                    return "ANY \"" + string.Join("\" \"", queries) + "\"";
                }
        }
    }

    /// <summary>Get the localized text for a content pack dictionary.</summary>
    /// <param name="translations">The translation dictionary to read.</param>
    /// <param name="defaultName">The default text to return if no translation is found, or <c>null</c> for a generic 'no translation' message.</param>
    /// <returns>Returns the matching translation, else the English text, else the <paramref name="defaultName"/>, else the text 'No translation'.</returns>
    private string Localize(Dictionary<string, string> translations, string defaultName = null)
    {
        return
            translations?.GetValueOrDefault(LocalizedContentManager.CurrentLanguageCode.ToString())
            ?? translations?.GetValueOrDefault("en")
            ?? defaultName
            ?? "No translation";
    }
}
