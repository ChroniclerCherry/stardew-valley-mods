using System.Collections.Generic;
using StardewValley;

namespace TrainStation.Framework.ContentModels;

/// <inheritdoc cref="IStopModel" />
internal class StopModel : IStopModel
{
    /*********
    ** Private fields
    *********/
    /// <summary>The default display name to return if no translation is found, or <c>null</c> for a generic 'no translation' message.</summary>
    private string DisplayNameDefault { get; init; }

    /// <summary>The display name translations for each language.</summary>
    private Dictionary<string, string> DisplayNameTranslations { get; init; }


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Id { get; init; }

    /// <summary>The localized display name.</summary>
    string IStopModel.DisplayName => this.GetDisplayName();

    /// <inheritdoc />
    public string TargetMapName { get; init; }

    /// <inheritdoc />
    public int TargetX { get; init; }

    /// <inheritdoc />
    public int TargetY { get; init; }

    /// <inheritdoc />
    public int FacingDirectionAfterWarp { get; init; }

    /// <inheritdoc />
    public int Cost { get; init; }

    /// <inheritdoc />
    public bool IsBoat { get; init; }

    /// <inheritdoc />
    public string[] Conditions { get; init; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance from a legacy content pack model.</summary>
    /// <param name="id"><inheritdoc cref="StopModel.Id" path="/summary"/></param>
    /// <param name="model">The content pack model to parse.</param>
    /// <param name="isBoat"><inheritdoc cref="IsBoat" path="/summary" /></param>
    public static StopModel FromContentPack(string id, ContentPackStopModel model, bool isBoat)
    {
        return FromData(
            id: id,
            targetMapName: model.TargetMapName,
            targetX: model.TargetX,
            targetY: model.TargetY,
            facingDirectionAfterWarp: model.FacingDirectionAfterWarp,
            cost: model.Cost,
            conditions: model.Conditions,
            isBoat: isBoat,
            displayNameTranslations: model.LocalizedDisplayName,
            displayNameDefault: null
        );
    }

    /// <summary>Construct an instance from a legacy API call.</summary>
    /// <param name="model">The stop model to copy.</param>
    public static StopModel FromData(StopModel model)
    {
        return FromData(
            id: model.Id,
            targetMapName: model.TargetMapName,
            targetX: model.TargetX,
            targetY: model.TargetY,
            facingDirectionAfterWarp: model.FacingDirectionAfterWarp,
            cost: model.Cost,
            conditions: model.Conditions,
            isBoat: model.IsBoat,
            displayNameTranslations: model.DisplayNameTranslations,
            displayNameDefault: model.DisplayNameDefault
        );
    }

    /// <summary>Construct an instance from a legacy API call.</summary>
    /// <param name="id"><inheritdoc cref="Id" path="/summary" /></param>
    /// <param name="targetMapName"><inheritdoc cref="TargetMapName" path="/summary" /></param>
    /// <param name="targetX"><inheritdoc cref="TargetX" path="/summary" /></param>
    /// <param name="targetY"><inheritdoc cref="TargetY" path="/summary" /></param>
    /// <param name="facingDirectionAfterWarp"><inheritdoc cref="FacingDirectionAfterWarp" path="/summary" /></param>
    /// <param name="cost"><inheritdoc cref="Cost" path="/summary" /></param>
    /// <param name="conditions"><inheritdoc cref="Conditions" path="/summary" /></param>
    /// <param name="isBoat"><inheritdoc cref="IsBoat" path="/summary" /></param>
    /// <param name="displayNameTranslations"><inheritdoc cref="DisplayNameTranslations" path="/summary" /></param>
    /// <param name="displayNameDefault"><inheritdoc cref="DisplayNameDefault" path="/summary" /></param>
    public static StopModel FromData(string id, string targetMapName, int targetX, int targetY, int facingDirectionAfterWarp, int cost, string[] conditions, bool isBoat, Dictionary<string, string> displayNameTranslations, string displayNameDefault)
    {
        return new StopModel
        {
            Id = id,
            TargetMapName = targetMapName,
            TargetX = targetX,
            TargetY = targetY,
            FacingDirectionAfterWarp = facingDirectionAfterWarp,
            Cost = cost,
            Conditions = conditions,
            IsBoat = isBoat,

            DisplayNameTranslations = displayNameTranslations,
            DisplayNameDefault = displayNameDefault
        };
    }

    /// <summary>Get the localized display name.</summary>
    public string GetDisplayName()
    {
        return
            this.DisplayNameTranslations?.GetValueOrDefault(LocalizedContentManager.CurrentLanguageCode.ToString())
            ?? this.DisplayNameTranslations?.GetValueOrDefault("en")
            ?? this.DisplayNameDefault
            ?? "No translation";
    }
}
