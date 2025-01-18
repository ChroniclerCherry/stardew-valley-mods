using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using TrainStation.Framework.ContentModels;
using TrainStation.Framework.ContentPackModels;

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


    /*********
    ** Accessors
    *********/
    /// <summary>The boat stops registered by Train Station packs or through the API.</summary>
    public List<StopContentPackModel> BoatStops { get; } = new();

    /// <summary>The train stops registered by Train Station packs or through the API.</summary>
    public List<StopContentPackModel> TrainStops { get; } = new();


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modId">The unique mod ID for Train Station.</param>
    /// <param name="config">Get the current mod config.</param>
    /// <param name="contentHelper">The SMAPI API for loading and managing content assets.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ContentManager(string modId, Func<ModConfig> config, IGameContentHelper contentHelper, IMonitor monitor)
    {
        this.ModId = modId;
        this.DataAssetName = $"Mods/{modId}/Destinations";
        this.Config = config;
        this.ContentHelper = contentHelper;
        this.Monitor = monitor;
    }

    /// <summary>Get the boat stops which can be selected from the current location.</summary>
    public IEnumerable<StopModel> GetAvailableBoatStops()
    {
        return this.GetAvailableStops(content => content.BoatStops);
    }

    /// <summary>Get the train stops which can be selected from the current location.</summary>
    public IEnumerable<StopModel> GetAvailableTrainStops()
    {
        return this.GetAvailableStops(content => content.TrainStops);
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
        this.TrainStops.Clear();
        this.BoatStops.Clear();

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
                    StopContentPackModel stop = cp.TrainStops[i];
                    stop.Id = $"{pack.Manifest.UniqueID}_{i}";
                    stop.DisplayName = this.Localize(stop.LocalizedDisplayName);

                    this.TrainStops.Add(stop);
                }
            }

            if (cp.BoatStops != null)
            {
                for (int i = 0; i < cp.BoatStops.Count; i++)
                {
                    StopContentPackModel stop = cp.BoatStops[i];
                    stop.Id = $"{pack.Manifest.UniqueID}_{i}";
                    stop.DisplayName = this.Localize(stop.LocalizedDisplayName);

                    this.BoatStops.Add(stop);
                }
            }
        }
    }

    /// <summary>Get the localized text for a content pack dictionary.</summary>
    /// <param name="translations">The translation dictionary to read.</param>
    /// <param name="defaultName">The default text to return if no translation is found, or <c>null</c> for a generic 'no translation' message.</param>
    /// <returns>Returns the matching translation, else the English text, else the <paramref name="defaultName"/>, else the text 'No translation'.</returns>
    public string Localize(Dictionary<string, string> translations, string defaultName = null)
    {
        return
            translations?.GetValueOrDefault(LocalizedContentManager.CurrentLanguageCode.ToString())
            ?? translations?.GetValueOrDefault("en")
            ?? defaultName
            ?? "No translation";
    }

    /// <summary>Build a game state query equivalent to the provided Expanded Preconditions Utility conditions.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    public string BuildGameQueryForExpandedPreconditions(string[] conditions)
    {
        switch (conditions?.Length)
        {
            case null:
            case < 1:
                return null;

            case 1:
                return $"{this.ModId} {conditions[0]}";

            default:
                {
                    string[] queries = new string[conditions.Length];
                    for (int i = 0; i < conditions.Length; i++)
                        queries[i] = $"{this.ModId} {conditions[i]}";

                    return "ANY \"" + string.Join("\" \"", queries) + "\"";
                }
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Build the data asset model with the default stops and those provided through Train Station content packs and its API.</summary>
    private ContentModel BuildDefaultContentModel()
    {
        var config = this.Config();
        var model = new ContentModel();

        // default stops
        model.BoatStops.AddRange([
            new StopModel
            {
                Id = $"{this.ModId}_BoatTunnel",
                DisplayName = I18n.BoatStationDisplayName(),
                ToLocation = "BoatTunnel",
                ToTile = new Point(4, 9)
            },
            new StopModel
            {
                Id = $"{this.ModId}_GingerIsland",
                DisplayName = I18n.GingerIsland(),
                ToLocation = "IslandSouth",
                ToTile = new Point(21, 43),
                ToFacingDirection = Game1.up,
                Cost = (Game1.getLocationFromName("BoatTunnel") as BoatTunnel)?.TicketPrice ?? 1000,
                Conditions = "PLAYER_HAS_MAIL Host willyBoatTicketMachine, PLAYER_HAS_MAIL Host willyBoatFixed"
            }
        ]);
        model.TrainStops.Add(new StopModel
        {
            Id = $"{this.ModId}_Railroad",
            DisplayName = I18n.TrainStationDisplayName(),
            ToLocation = "Railroad",
            ToTile = new Point(config.RailroadWarpX, config.RailroadWarpY)
        });

        // stops from content packs & API
        foreach (StopContentPackModel stop in this.BoatStops)
        {
            model.BoatStops.Add(new StopModel
            {
                Id = stop.Id,
                DisplayName = this.Localize(stop.LocalizedDisplayName, stop.DisplayName),
                ToLocation = stop.TargetMapName,
                ToTile = new Point(stop.TargetX, stop.TargetY),
                ToFacingDirection = stop.FacingDirectionAfterWarp,
                Cost = stop.Cost,
                Conditions = this.BuildGameQueryForExpandedPreconditions(stop.Conditions)
            });
        }

        foreach (StopContentPackModel stop in this.TrainStops)
        {
            model.TrainStops.Add(new StopModel
            {
                Id = stop.Id,
                DisplayName = this.Localize(stop.LocalizedDisplayName, stop.DisplayName),
                ToLocation = stop.TargetMapName,
                ToTile = new Point(stop.TargetX, stop.TargetY),
                ToFacingDirection = stop.FacingDirectionAfterWarp,
                Cost = stop.Cost,
                Conditions = this.BuildGameQueryForExpandedPreconditions(stop.Conditions)
            });
        }

        return model;

    }

    /// <summary>Get the stops which can be selected from the current location.</summary>
    /// <param name="getStops">Get the list of stops from the content model.</param>
    private IEnumerable<StopModel> GetAvailableStops(Func<ContentModel, List<StopModel>> getStops)
    {
        ContentModel content = this.ContentHelper.Load<ContentModel>(this.DataAssetName);

        foreach (StopModel stop in getStops(content))
        {
            if (stop.ToLocation == Game1.currentLocation.Name || Game1.getLocationFromName(stop.ToLocation) is null || !GameStateQuery.CheckConditions(stop.Conditions))
                continue;

            yield return stop;
        }
    }
}
