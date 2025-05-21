using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using TrainStation.Framework.ContentModels;

namespace TrainStation.Framework;

/// <summary>Manages the available boat and train stops.</summary>
internal class StopManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The unique mod ID for Train Station.</summary>
    private readonly string ModId;

    /// <summary>Get the current mod config.</summary>
    private readonly Func<ModConfig> Config;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Whether the Expanded Preconditions Utility mod is installed.</summary>
    private readonly bool HasExpandedPreconditionsUtility;

    /// <summary>The Expanded Preconditions Utility API, if available.</summary>
    /// <remarks>This becomes available after <see cref="IGameLoopEvents.GameLaunched"/>, so it shouldn't be used for initial validation.</remarks>
    private readonly Func<IConditionsChecker> ConditionsApi;

    /// <summary>The defined boat and train stops, including both boat and train stops.</summary>
    private List<StopModel>? Stops;


    /*********
    ** Accessors
    *********/
    /// <summary>The stops registered by Train Station packs or through the API.</summary>
    public List<StopModel> CustomStops { get; } = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modId">The unique mod ID for Train Station.</param>
    /// <param name="config">Get the current mod config.</param>
    /// <param name="hasExpandedPreconditionsUtility">Whether the Expanded Preconditions Utility mod is installed.</param>
    /// <param name="conditionsApi">The Expanded Preconditions Utility API, if available.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public StopManager(string modId, Func<ModConfig> config, bool hasExpandedPreconditionsUtility, Func<IConditionsChecker> conditionsApi, IMonitor monitor)
    {
        this.ModId = modId;
        this.Config = config;
        this.HasExpandedPreconditionsUtility = hasExpandedPreconditionsUtility;
        this.ConditionsApi = conditionsApi;
        this.Monitor = monitor;
    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="isBoat">Whether this is a boat stop; else it's a train stop.</param>
    public IEnumerable<StopModel> GetAvailableStops(bool isBoat)
    {
        this.Stops ??= this.BuildStopsList();

        foreach (StopModel stop in this.Stops)
        {
            if (stop.IsBoat != isBoat || stop.TargetMapName == Game1.currentLocation.Name || Game1.getLocationFromName(stop.TargetMapName) is null)
                continue;

            if (stop.Conditions?.Length > 0 && this.HasExpandedPreconditionsUtility && !this.ConditionsApi().CheckConditions(stop.Conditions))
                continue;

            yield return stop;
        }
    }

    /// <summary>Reload the data asset, so it'll be reloaded next time it's accessed.</summary>
    public void ResetData()
    {
        this.Stops = null;
    }

    /// <summary>Load the boat and train stops from loaded content packs.</summary>
    /// <param name="contentPacks">The loaded content packs to read.</param>
    public void LoadContentPacks(IEnumerable<IContentPack> contentPacks)
    {
        foreach (IContentPack pack in contentPacks)
        {
            if (!pack.HasFile("TrainStops.json"))
            {
                this.Monitor.Log($"{pack.Manifest.UniqueID} is missing a \"TrainStops.json\"", LogLevel.Error);
                continue;
            }

            ContentPack cp = pack.ModContent.Load<ContentPack>("TrainStops.json");
            for (int i = 0; i < cp.TrainStops.Count; i++)
                this.LoadStop(pack, cp.TrainStops[i], false, i);

            for (int i = 0; i < cp.BoatStops.Count; i++)
                this.LoadStop(pack, cp.BoatStops[i], true, i);
        }
    }

    /// <summary>Load a single boat or train stop from a content pack.</summary>
    /// <param name="contentPack">The content pack being loaded.</param>
    /// <param name="rawModel">The raw stop data from the content pack.</param>
    /// <param name="isBoat"><inheritdoc cref="StopModel.IsBoat" path="/summary" /></param>
    /// <param name="index">The index of this stop in the content pack's list.</param>
    private void LoadStop(IContentPack contentPack, ContentPackStopModel rawModel, bool isBoat, int index)
    {
        this.ValidateExpandedPreconditionsInstalledIfNeeded(rawModel.Conditions, contentPack.Manifest.Name);

        this.CustomStops.Add(
            StopModel.FromContentPack($"{contentPack.Manifest.UniqueID}_{(isBoat ? "Boat" : "Train")}_{index}", rawModel, isBoat)
        );
    }

    /// <summary>Log an error message if a mod uses Expanded Preconditions Utility conditions, but it isn't installed.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    /// <param name="fromModName">The name of the mod for which the conditions are being parsed.</param>
    /// <returns>Returns the input <paramref name="conditions" /> for chaining.</returns>
    public string[]? ValidateExpandedPreconditionsInstalledIfNeeded(string[]? conditions, string fromModName)
    {
        if (!this.HasExpandedPreconditionsUtility)
            this.Monitor.LogOnce($"The '{fromModName}' mod adds destinations with Expanded Preconditions Utility conditions, but you don't have Expanded Preconditions Utility installed. The destinations will default to always visible.", LogLevel.Warn);

        return conditions;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build the list of stops that can be accessed by the player.</summary>
    private List<StopModel> BuildStopsList()
    {
        ModConfig config = this.Config();

        // default stops
        List<StopModel> stops = [
            // boat
            StopModel.FromData(
                id: $"{this.ModId}_BoatTunnel",
                targetMapName: "BoatTunnel",
                targetX: 4,
                targetY: 9,
                facingDirectionAfterWarp: Game1.down,
                cost: 0,
                conditions: null,
                isBoat: true,
                displayNameTranslations: null,
                displayNameDefault: I18n.BoatStationDisplayName()
            ),
            StopModel.FromData(
                id: $"{this.ModId}_GingerIsland",
                targetMapName: "IslandSouth",
                targetX: 21,
                targetY: 43,
                facingDirectionAfterWarp: Game1.up,
                cost: (Game1.getLocationFromName("BoatTunnel") as BoatTunnel)?.TicketPrice ?? 1000,
                conditions: null,
                isBoat: true,
                displayNameTranslations: null,
                displayNameDefault: Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName")
            ),

            // train
            StopModel.FromData(
                id: $"{this.ModId}_Railroad",
                targetMapName: "Railroad",
                targetX: config.RailroadWarpX,
                targetY: config.RailroadWarpY,
                facingDirectionAfterWarp: Game1.down,
                cost: 0,
                conditions: null,
                isBoat: false,
                displayNameTranslations: null,
                displayNameDefault: I18n.TrainStationDisplayName()
            )
        ];

        // stops from legacy content packs & API
        foreach (StopModel stop in this.CustomStops)
        {
            // We need a copy of the model here, since (a) we don't want asset edits to be persisted between resets
            // and (b) we need the display name to be editable despite being auto-generated for legacy stop models.
            stops.Add(
                StopModel.FromData(stop)
            );
        }

        return stops;
    }
}
