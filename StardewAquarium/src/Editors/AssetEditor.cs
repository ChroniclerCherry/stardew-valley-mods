using System;
using System.Collections.Generic;
using System.Linq;

using Force.DeepCloner;

using Microsoft.Xna.Framework.Graphics;

using StardewAquarium.Editors;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Shirts;

namespace StardewAquarium.src.Editors;
internal static class AssetEditor
{
    private static string? LegendaryBaitId => ModEntry.JsonAssets?.GetObjectId(ModEntry.LegendaryBaitName);

    private readonly static Dictionary<IAssetName, Action<IAssetData>> _handlers = [];
    private readonly static Dictionary<IAssetName, string> _textureLoaders = [];
    private static IMonitor Monitor;

    private const string AquariumOpenAfterLandslide = "StardewAquarium.Open";
    private const string AquariumOpenLater = "StardewAquarium.OpenLater";

    internal static void Init(IGameContentHelper parser, IContentEvents events, IMonitor monitor)
    {
        Monitor = monitor;
        events.AssetRequested += Handle;

        _handlers[parser.ParseAssetName("Strings/UI")] = EditStringsUi;

        _handlers[parser.ParseAssetName("Data/Locations")] = EditDataLocations;
        _handlers[parser.ParseAssetName("Data/Shirts")] = EditShirtData;
        _handlers[parser.ParseAssetName("Strings/Shirts")] = EditShirtStrings;

        _handlers[parser.ParseAssetName("Data/mail")] = EditDataMail;
        _handlers[parser.ParseAssetName("Data/TriggerActions")] = EditTriggerActions;

        _handlers[parser.ParseAssetName("Data/Achievements")] = AchievementEditor.Edit;

        _textureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Shirts")] = "assets/shirts.png";
        _textureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Items")] = "assets/items.png";

    }

    private static void Handle(object sender, AssetRequestedEventArgs e)
    {
        // move fish descriptions OUT.
        if (e.NameWithoutLocale.IsEquivalentTo("Mods/StardewAquarium/FishDescriptions"))
        {
            e.LoadFrom(static () => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }

        if (_handlers.TryGetValue(e.NameWithoutLocale, out Action<IAssetData> action))
        {
            e.Edit(action, AssetEditPriority.Late);
        }
        if (_textureLoaders.TryGetValue(e.NameWithoutLocale, out string path))
        {
            e.LoadFromModFile<Texture2D>(path, AssetLoadPriority.Exclusive);
        }
    }

    #region editors

    /// <summary>
    /// copies over 1.5 data from beach data.
    /// </summary>
    /// <param name="asset"></param>
    private static void EditDataLocations(IAssetData asset)
    {
        // TODO: don't do this once JA integration is fully removed.
        string? legendaryBaitQID = LegendaryBaitId is not null ? ItemRegistry.ManuallyQualifyItemId(LegendaryBaitId, ItemRegistry.type_object) : null;

        IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

        // add legendary bait entries.
        if (legendaryBaitQID is not null)
        {
            foreach (var (key, values) in data)
            {
                if (values.Fish?.Count is 0 or null)
                {
                    continue;
                }

                List<SpawnFishData> newEntries = [];
                foreach (var fish in values.Fish)
                {
                    if (fish.IsBossFish && fish.CatchLimit == 1)
                    {
                        newEntries.Add(fish.MakeLegendaryBaitEntry(legendaryBaitQID));
                    }
                }

                if (newEntries.Count > 0)
                {
                    Monitor.Log($"Added {newEntries.Count} entries for legendary bait in {key}");
                    values.Fish.AddRange(newEntries);
                }
            }
        }

        if (!data.TryGetValue("Beach", out LocationData beachData))
        {
            Monitor.Log("Beach data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }


        if (!data.TryGetValue(ModEntry.Data.ExteriorMapName, out LocationData museumData))
        {
            Monitor.Log("MuseumExterior data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }

        museumData.ArtifactSpots ??= [];
        museumData.ArtifactSpots.AddRange(beachData.ArtifactSpots ?? Enumerable.Empty<ArtifactSpotDropData>());

        if (beachData.FishAreas is not null)
        {
            museumData.FishAreas ??= [];
            foreach ((string key, FishAreaData fisharea) in beachData.FishAreas)
            {
                beachData.FishAreas.TryAdd(key, fisharea);
            }
        }

        museumData.Fish ??= [];
        museumData.Fish.AddRange(beachData.Fish ?? Enumerable.Empty<SpawnFishData>());

        museumData.Forage ??= [];
        museumData.Forage.AddRange(beachData.Forage ?? Enumerable.Empty<SpawnForageData>());

    }

    private static void EditDataMail(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data[AquariumOpenAfterLandslide] = I18n.AquariumOpenLandslide();
        data[AquariumOpenLater] = I18n.AquariumOPenLater();
    }

    private static void EditShirtStrings(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data["StardewAquarium_Pufferchick_Shirt_Name"] = I18n.PufferchickShirtName();
        data["StardewAquarium_Pufferchick_Shirt_Description"] = I18n.PufferchickShirtDescription();
    }

    private static void EditShirtData(IAssetData asset)
    {
        IDictionary<string, ShirtData> data = asset.AsDictionary<string, ShirtData>().Data;
        data["Cherry.StardewAquarium_PufferchickShirt"] = new()
        {
            Name = "Pufferchick Shirt",
            DisplayName = "[LocalizedText Strings\\Shirts:StardewAquarium_Pufferchick_Shirt_Name]",
            Description = "[LocalizedText Strings\\Shirts:StardewAquarium_Pufferchick_Shirt_Description]",
            Price = 200,
            Texture = "Mods/StardewAquarium/Shirts",
            SpriteIndex = 0,
        };
    }

    private static void EditStringsUi(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data.Add("Chat_StardewAquarium.FishDonated", I18n.FishDonatedMP());
        data.Add("Chat_StardewAquarium.AchievementUnlocked", I18n.AchievementUnlockedMP());
    }

    private static void EditTriggerActions(IAssetData asset)
    {
        List<TriggerActionData> data = asset.GetData<List<TriggerActionData>>();

        // add aquarium mail.
        data.Add(
            new()
            {
                Trigger = "DayStarted",
                Action = $"AddMail Current {AquariumOpenAfterLandslide} Now, !PLAYER_HAS_MAIL Current {AquariumOpenLater} Any",
                Condition = "DAYS_PLAYED 30 30",
                Id = $"{AquariumOpenAfterLandslide}_Trigger"
            });

        data.Add(
            new()
            {
                Trigger = "DayStarted",
                Action = $"AddMail Current {AquariumOpenLater} Now, !PLAYER_HAS_MAIL Current {AquariumOpenAfterLandslide} Any",
                Condition = "DAYS_PLAYED 31",
                Id = $"{AquariumOpenLater}_Trigger"
            });
    }

    #endregion
}

file static class AssetEditExtensions
{
    internal static void AddCondition(this GenericSpawnItemDataWithCondition spawnable, string newCondition)
    {
        if (string.IsNullOrWhiteSpace(spawnable.Condition))
        {
            spawnable.Condition = newCondition;
        }
        else
        {
            spawnable.Condition += $", {newCondition}";
        }
    }

    internal static SpawnFishData MakeLegendaryBaitEntry(this SpawnFishData spawnable, string legendaryBaitQID)
    {
        var copy = spawnable.DeepClone();
        copy.AddCondition($"{AquariumGameStateQuery.HasBaitQuery} Current {legendaryBaitQID}");
        copy.CatchLimit = -1;

        return copy;
    }
}
