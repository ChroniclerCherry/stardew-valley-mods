using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;

namespace StardewAquarium.Framework.Editors;

internal static class AssetEditor
{
    private static IMonitor Monitor;

    internal const string AquariumPlayerHasChicken = "StardewAquarium.PlayerHasChicken";

    internal static void Init(IContentEvents events, IMonitor monitor)
    {
        Monitor = monitor;
        events.AssetRequested += OnAssetRequested;
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            e.Edit(EditDataLocations);
    }

    /// <summary>Set up Legendary Bait fish entries, and copy the beach data into the aquarium exterior location.</summary>
    /// <param name="asset"></param>
    private static void EditDataLocations(IAssetData asset)
    {
        IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

        // add Legendary Bait fish entries
        foreach ((string key, LocationData values) in data)
        {
            if (values.Fish?.Count is not > 0)
                continue;

            List<SpawnFishData> newEntries = [];
            foreach (SpawnFishData fish in values.Fish)
            {
                if (fish.IsBossFish && fish.CatchLimit == 1)
                    newEntries.Add(MakeLegendaryBaitEntry(fish));
            }

            if (newEntries.Count > 0)
            {
                Monitor.Log($"Added {newEntries.Count} entries for legendary bait in {key}.");
                values.Fish.AddRange(newEntries);
            }
        }

        // copy beach data into aquarium exterior
        if (!data.TryGetValue(ContentPackHelper.ExteriorLocationName, out LocationData museumData))
            Monitor.Log($"Could not find location data for '{ContentPackHelper.ExteriorLocationName}'. Is Stardew Aquarium correctly installed?", LogLevel.Warn);
        else if (!data.TryGetValue("Beach", out LocationData beachData))
            Monitor.Log("Beach data seems missing, cannot copy.", LogLevel.Warn);
        else
        {
            museumData.ArtifactSpots ??= [];
            museumData.ArtifactSpots.AddRange(beachData.ArtifactSpots ?? Enumerable.Empty<ArtifactSpotDropData>());

            if (beachData.FishAreas is not null)
            {
                museumData.FishAreas ??= [];
                foreach ((string key, FishAreaData fishArea) in beachData.FishAreas)
                    beachData.FishAreas.TryAdd(key, fishArea);
            }

            museumData.Fish ??= [];
            museumData.Fish.AddRange(beachData.Fish ?? Enumerable.Empty<SpawnFishData>());

            museumData.Forage ??= [];
            museumData.Forage.AddRange(beachData.Forage ?? Enumerable.Empty<SpawnForageData>());
        }
    }

    private static SpawnFishData MakeLegendaryBaitEntry(SpawnFishData spawnable)
    {
        SpawnFishData newEntry = spawnable.DeepClone();
        newEntry.Id += "_LegendaryBait";
        newEntry.CatchLimit = -1;
        newEntry.Chance = 1;
        newEntry.IgnoreFishDataRequirements = true;

        const string condition = $"{AquariumGameStateQuery.HasBaitQuery} Current {ContentPackHelper.LegendaryBaitQualifiedId}";
        newEntry.Condition = string.IsNullOrWhiteSpace(newEntry.Condition)
            ? condition
            : $"{newEntry.Condition}, {condition}";

        return newEntry;
    }
}
