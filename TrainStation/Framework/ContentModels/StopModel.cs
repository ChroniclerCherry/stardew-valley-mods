using System.Collections.Generic;
using StardewValley;

namespace TrainStation.Framework.ContentModels;

/// <summary>A boat or train stop that can be visited by the player.</summary>
internal class StopModel
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
    /// <summary>A unique identifier for this stop.</summary>
    public string Id { get; init; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    public string TargetMapName { get; init; }

    /// <summary>The tile X position to which the player should warp when they select this stop.</summary>
    public int TargetX { get; set; }

    /// <summary>The tile Y position to which the player should warp when they select this stop.</summary>
    public int TargetY { get; set; }

    /// <summary>The direction the player should be facing after they warp, matching a value recognized by <see cref="Utility.TryParseDirection"/>.</summary>
    public int FacingDirectionAfterWarp { get; init; }

    /// <summary>The gold price to go to that stop.</summary>
    public int Cost { get; init; }

    /// <summary>Whether this is a boat stop; else it's a train stop.</summary>
    public bool IsBoat { get; init; }

    /// <summary>If set, the Expanded Precondition Utility conditions which indicate whether this stop should appear in the menu at a given time.</summary>
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
