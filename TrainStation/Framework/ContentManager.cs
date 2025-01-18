using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace TrainStation.Framework;

/// <summary>Manages the Train Station content provided by content packs.</summary>
internal class ContentManager
{
    /*********
    ** Fields
    *********/
    /// <summary>Get the current mod config.</summary>
    private readonly Func<ModConfig> Config;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The currently selected language code.</summary>
    private LocalizedContentManager.LanguageCode SelectedLanguage;


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
    /// <param name="config">Get the current mod config.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ContentManager(Func<ModConfig> config, IMonitor monitor)
    {
        this.Config = config;
        this.Monitor = monitor;
    }

    /// <summary>Load the boat and train stops from loaded content packs.</summary>
    /// <param name="contentPacks">The loaded content packs to read.</param>
    public void LoadContentPacks(IEnumerable<IContentPack> contentPacks)
    {
        var config = this.Config();

        //create the stop at the vanilla Railroad map
        StopContentPackModel railRoadStop = new()
        {
            Id = "Cherry.TrainStation",
            DisplayName = I18n.TrainStationDisplayName(),
            TargetMapName = "Railroad",
            TargetX = config.RailroadWarpX,
            TargetY = config.RailroadWarpY,
            Cost = 0
        };

        //create stop in willy's boat room
        StopContentPackModel boatTunnelStop = new()
        {
            Id = "Cherry.TrainStation",
            DisplayName = I18n.BoatStationDisplayName(),
            TargetMapName = "BoatTunnel",
            TargetX = 4,
            TargetY = 9,
            Cost = 0
        };

        this.TrainStops.Clear();
        this.BoatStops.Clear();

        this.TrainStops.Add(railRoadStop);
        this.BoatStops.Add(boatTunnelStop);

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

    /// <summary>Remove boat and train stops in locations which don't exist.</summary>
    public void RemoveInvalidLocations()
    {
        foreach (var stops in new[] { this.BoatStops, this.TrainStops })
        {
            stops.RemoveAll(stop =>
            {
                if (Game1.getLocationFromName(stop.TargetMapName) == null)
                {
                    this.Monitor.Log($"Could not find location {stop.TargetMapName}", LogLevel.Warn);
                    return true;
                }

                return false;
            });
        }
    }

    /// <summary>Update the selected language.</summary>
    public void UpdateSelectedLanguage()
    {
        this.SelectedLanguage = LocalizedContentManager.CurrentLanguageCode;
    }

    /// <summary>Get the localized text for a content pack dictionary.</summary>
    /// <param name="translations">The translation dictionary to read.</param>
    /// <returns>Returns the matching translation, else the English text, else the text 'No translation'.</returns>
    public string Localize(Dictionary<string, string> translations)
    {
        if (!translations.ContainsKey(this.SelectedLanguage.ToString()))
        {
            return translations.ContainsKey("en") ? translations["en"] : "No translation";
        }

        return translations[this.SelectedLanguage.ToString()];
    }
}
