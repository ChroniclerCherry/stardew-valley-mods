using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using TrainStation.Framework.ContentModels;
using TrainStation.Framework.LegacyContentModels;

namespace TrainStation.Framework;

/// <summary>Manages the Train Station content provided by content packs.</summary>
internal class ContentManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The unique mod ID for Train Station.</summary>
    private readonly string ModId;

    /// <summary>The asset name for the data asset containing boat and train stops.</summary>
    private readonly string DataAssetName;

    /// <summary>Get the current mod config.</summary>
    private readonly Func<ModConfig> Config;

    /// <summary>The SMAPI API for loading and managing content assets.</summary>
    private readonly IGameContentHelper ContentHelper;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Whether the Expanded Preconditions Utility mod is installed.</summary>
    private readonly bool HasExpandedPreconditionsUtility;


    /*********
    ** Accessors
    *********/
    /// <summary>The stops registered by Train Station packs or through the API.</summary>
    public List<StopModel> LegacyStops { get; } = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modId">The unique mod ID for Train Station.</param>
    /// <param name="config">Get the current mod config.</param>
    /// <param name="contentHelper">The SMAPI API for loading and managing content assets.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    /// <param name="hasExpandedPreconditionsUtility">Whether the Expanded Preconditions Utility mod is installed.</param>
    public ContentManager(string modId, Func<ModConfig> config, IGameContentHelper contentHelper, IMonitor monitor, bool hasExpandedPreconditionsUtility)
    {
        this.ModId = modId;
        this.DataAssetName = $"Mods/{modId}/Destinations";
        this.Config = config;
        this.ContentHelper = contentHelper;
        this.Monitor = monitor;
        this.HasExpandedPreconditionsUtility = hasExpandedPreconditionsUtility;
    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="network">The network for which to get stops.</param>
    public IEnumerable<StopModel> GetAvailableStops(StopNetwork network)
    {
        foreach (StopModel stop in this.ContentHelper.Load<List<StopModel>>(this.DataAssetName))
        {
            if (stop?.Network != network || stop.ToLocation == Game1.currentLocation.Name || Game1.getLocationFromName(stop.ToLocation) is null || !GameStateQuery.CheckConditions(stop.Conditions))
                continue;

            yield return stop;
        }
    }

    /// <summary>Reload the data asset, so it'll be reloaded next time it's accessed.</summary>
    public void ResetAsset()
    {
        this.ContentHelper.InvalidateCache(this.DataAssetName);
    }

    /// <inheritdoc cref="IPlayerEvents.Warped" />
    public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.DataAssetName))
            e.LoadFrom(this.BuildDefaultContentModel, AssetLoadPriority.Exclusive);
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
            if (cp.TrainStops != null)
            {
                for (int i = 0; i < cp.TrainStops.Count; i++)
                {
                    this.LegacyStops.Add(
                        LegacyStopModel.FromContentPack($"{pack.Manifest.UniqueID}_{i}", cp.TrainStops[i], StopNetwork.Train, ConvertExpandedPreconditions)
                    );
                }
            }

            if (cp.BoatStops != null)
            {
                for (int i = 0; i < cp.BoatStops.Count; i++)
                {
                    this.LegacyStops.Add(
                        LegacyStopModel.FromContentPack($"{pack.Manifest.UniqueID}_{i}", cp.BoatStops[i], StopNetwork.Boat, ConvertExpandedPreconditions)
                    );
                }
            }

            string ConvertExpandedPreconditions(string[] conditions) => this.BuildGameQueryForExpandedPreconditionsIfInstalled(conditions, pack.Manifest.Name);
        }
    }

    /// <summary>Build a game state query equivalent to the provided Expanded Preconditions Utility conditions. If that mod isn't installed, log a warning instead.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    /// <param name="fromModName">The name of the mod for which the conditions are being parsed.</param>
    public string BuildGameQueryForExpandedPreconditionsIfInstalled(string[] conditions, string fromModName)
    {
        if (!this.HasExpandedPreconditionsUtility)
        {
            this.Monitor.LogOnce($"The '{fromModName}' mod adds destinations with Expanded Preconditions Utility conditions, but you don't have Expanded Preconditions Utility installed. The destinations will default to always visible.", LogLevel.Warn);
            return null;
        }

        return LegacyStopModel.BuildGameQueryForExpandedPreconditions(conditions);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build the data asset model with the default stops and those provided through Train Station content packs and its API.</summary>
    private List<StopModel> BuildDefaultContentModel()
    {
        var config = this.Config();
        var stops = new List<StopModel>();

        // default stops
        stops.AddRange([
            // boat
            new StopModel
            {
                Id = $"{this.ModId}_BoatTunnel",
                DisplayName = I18n.BoatStationDisplayName(),
                ToLocation = "BoatTunnel",
                ToTile = new Point(4, 9),
                Network = StopNetwork.Boat
            },
            new StopModel
            {
                Id = $"{this.ModId}_GingerIsland",
                DisplayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName"),
                ToLocation = "IslandSouth",
                ToTile = new Point(21, 43),
                ToFacingDirection = "up",
                Cost = (Game1.getLocationFromName("BoatTunnel") as BoatTunnel)?.TicketPrice ?? 1000,
                Network = StopNetwork.Boat
            },

            // train
            new StopModel
            {
                Id = $"{this.ModId}_Railroad",
                DisplayName = I18n.TrainStationDisplayName(),
                ToLocation = "Railroad",
                ToTile = new Point(config.RailroadWarpX, config.RailroadWarpY),
                Network = StopNetwork.Train
            }
        ]);

        // stops from legacy content packs & API
        foreach (StopModel stop in this.LegacyStops)
        {
            stops.Add(
                // We need a copy of the model here, since (a) we don't want asset edits to be persisted between resets
                // and (b) we need the display name to be editable despite being auto-generated for legacy stop models.
                new StopModel
                {
                    Id = stop.Id,
                    DisplayName = stop.DisplayName,
                    ToLocation = stop.ToLocation,
                    ToTile = stop.ToTile,
                    ToFacingDirection = stop.ToFacingDirection,
                    Cost = stop.Cost,
                    Conditions = stop.Conditions,
                    Network = stop.Network
                }
            );
        }

        return stops;
    }
}
