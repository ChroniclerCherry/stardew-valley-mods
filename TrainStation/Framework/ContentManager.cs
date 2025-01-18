using System;
using System.Collections.Generic;
using System.Linq;
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
    public List<BoatStop> BoatStops { get; } = new();

    /// <summary>The train stops registered by Train Station packs or through the API.</summary>
    public List<TrainStop> TrainStops { get; } = new();


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
        TrainStop railRoadStop = new TrainStop
        {
            TargetMapName = "Railroad",
            StopId = "Cherry.TrainStation",
            TargetX = config.RailroadWarpX,
            TargetY = config.RailroadWarpY,
            Cost = 0,
            TranslatedName = I18n.TrainStationDisplayName()
        };

        //create stop in willy's boat room
        BoatStop boatTunnelStop = new BoatStop()
        {
            TargetMapName = "BoatTunnel",
            StopId = "Cherry.TrainStation",
            TargetX = 4,
            TargetY = 9,
            Cost = 0,
            TranslatedName = I18n.BoatStationDisplayName()
        };

        ContentPack content = new ContentPack();
        content.TrainStops = new List<TrainStop>();
        content.BoatStops = new List<BoatStop>();

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
                    TrainStop stop = cp.TrainStops.ElementAt(i);
                    stop.StopId = $"{pack.Manifest.UniqueID}{i}"; //assigns a unique stopID to every stop
                    stop.TranslatedName = this.Localize(stop.LocalizedDisplayName);

                    this.TrainStops.Add(cp.TrainStops.ElementAt(i));
                }
            }

            if (cp.BoatStops != null)
            {
                for (int i = 0; i < cp.BoatStops.Count; i++)
                {
                    BoatStop stop = cp.BoatStops.ElementAt(i);
                    stop.StopId = $"{pack.Manifest.UniqueID}{i}"; //assigns a unique stopID to every stop
                    stop.TranslatedName = this.Localize(stop.LocalizedDisplayName);

                    this.BoatStops.Add(cp.BoatStops.ElementAt(i));
                }
            }
        }
    }

    /// <summary>Remove boat and train stops in locations which don't exist.</summary>
    public void RemoveInvalidLocations()
    {
        for (int i = this.TrainStops.Count - 1; i >= 0; i--)
        {
            TrainStop stop = this.TrainStops[i];
            if (Game1.getLocationFromName(stop.TargetMapName) == null)
            {
                this.Monitor.Log($"Could not find location {stop.TargetMapName}", LogLevel.Warn);
                this.TrainStops.RemoveAt(i);
            }
        }

        for (int i = this.BoatStops.Count - 1; i >= 0; i--)
        {
            BoatStop stop = this.BoatStops[i];
            if (Game1.getLocationFromName(stop.TargetMapName) == null)
            {
                this.Monitor.Log($"Could not find location {stop.TargetMapName}", LogLevel.Warn);
                this.BoatStops.RemoveAt(i);
            }
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
