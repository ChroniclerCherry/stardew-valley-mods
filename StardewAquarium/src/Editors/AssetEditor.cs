using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shirts;
using StardewValley.Network.NetEvents;
using SObject = StardewValley.Object;

namespace StardewAquarium.Editors;

internal static class AssetEditor
{
    internal const string LegendaryBaitId = "Cherry.StardewAquarium_LegendaryBait";
    internal const string LegendaryBaitQualifiedId = $"{ItemRegistry.type_object}{LegendaryBaitId}";

    internal const string PufferchickId = "Cherry.StardewAquarium_Pufferchick";
    internal const string PufferchickQualifiedId = $"{ItemRegistry.type_object}{PufferchickId}";

    private static readonly Dictionary<IAssetName, Action<IAssetData>> Handlers = [];
    private static readonly Dictionary<IAssetName, string> TextureLoaders = [];
    private static IMonitor Monitor;

    private const string AquariumOpenAfterLandslide = "StardewAquarium.Open";
    private const string AquariumOpenLater = "StardewAquarium.OpenLater";
    internal const string AquariumPlayerHasChicken = "StardewAquarium.PlayerHasChicken";

    internal static void Init(IGameContentHelper parser, IContentEvents events, IMonitor monitor)
    {
        Monitor = monitor;
        events.AssetRequested += Handle;

        Handlers[parser.ParseAssetName("Strings/UI")] = EditStringsUi;

        Handlers[parser.ParseAssetName("Data/Locations")] = EditDataLocations;

        Handlers[parser.ParseAssetName("Data/Shirts")] = EditShirtData;
        Handlers[parser.ParseAssetName("Strings/Shirts")] = EditShirtStrings;

        Handlers[parser.ParseAssetName("Data/Objects")] = EditObjectData;
        Handlers[parser.ParseAssetName("Strings/Objects")] = EditObjectStrings;

        Handlers[parser.ParseAssetName("Data/mail")] = EditDataMail;
        Handlers[parser.ParseAssetName("Data/TriggerActions")] = EditTriggerActions;

        Handlers[parser.ParseAssetName("Data/Achievements")] = AchievementEditor.Edit;

        Handlers[parser.ParseAssetName("Data/AquariumFish")] = EditAquariumFish;
        Handlers[parser.ParseAssetName("Data/Fish")] = EditFish;

        TextureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Shirts")] = "assets/shirts.png";
        TextureLoaders[parser.ParseAssetName("Mods/StardewAquarium/Items")] = "assets/items.png";
        TextureLoaders[parser.ParseAssetName("Mods/StardewAquarium/AquariumFish")] = "assets/aquarium.png";
    }

    private static void Handle(object sender, AssetRequestedEventArgs e)
    {
        // move fish descriptions OUT.
        if (e.NameWithoutLocale.IsEquivalentTo("Mods/StardewAquarium/FishDescriptions"))
        {
            e.LoadFrom(static () => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }

        if (Handlers.TryGetValue(e.NameWithoutLocale, out Action<IAssetData> action))
        {
            e.Edit(action, AssetEditPriority.Late);
        }
        if (TextureLoaders.TryGetValue(e.NameWithoutLocale, out string path))
        {
            e.LoadFromModFile<Texture2D>(path, AssetLoadPriority.Exclusive);
        }
    }

    private static void EditObjectStrings(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data["StardewAquarium_Legendary_Bait_Name"] = I18n.LegendaryBaitName();
        data["StardewAquarium_Legendary_Bait_Description"] = I18n.LegendaryBaitDescription();
        data["StardewAquarium_Pufferchick_Name"] = I18n.PufferchickName();
        data["StardewAquarium_Pufferchick_Description"] = I18n.PufferchickDescription();
    }

    private static void EditObjectData(IAssetData asset)
    {
        IDictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;
        const string texture = "Mods/StardewAquarium/Items";

        data[LegendaryBaitId] = new()
        {
            Name = "Legendary Bait",
            Type = "Basic",
            SpriteIndex = 4,
            Category = SObject.baitCategory,
            Texture = texture,
            Price = 10,
            DisplayName = "[LocalizedText Strings\\Objects:StardewAquarium_Legendary_Bait_Name]",
            Description = "[LocalizedText Strings\\Objects:StardewAquarium_Legendary_Bait_Description]",
            ContextTags = ["fish_legendary"],
            CanBeGivenAsGift = false,
            CanBeTrashed = false,
            ExcludeFromShippingCollection = true,
            ExcludeFromRandomSale = true,
        };
        data[PufferchickId] = new()
        {
            Name = "Pufferchick",
            Type = "Fish",
            SpriteIndex = 2,
            Category = SObject.FishCategory,
            Edibility = -200,
            Texture = texture,
            Price = 2000,
            DisplayName = "[LocalizedText Strings\\Objects:StardewAquarium_Pufferchick_Name]",
            Description = "[LocalizedText Strings\\Objects:StardewAquarium_Pufferchick_Description]",
            ContextTags = ["fish_legendary"],
        };
    }

    private static void EditFish(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data.Add(PufferchickId, $"{ItemRegistry.GetDataOrErrorItem(PufferchickQualifiedId).InternalName}/95/mixed/28/28/0 2600/spring summer fall winter/both/688 .05/5/0/0/0/false");
    }

    private static void EditAquariumFish(IAssetData asset)
    {
        IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
        data[PufferchickId] = "0/float/////Mods\\StardewAquarium\\AquariumFish";
    }

    /// <summary>Copies over 1.5 data from beach data, edits fish data for legendary fish.</summary>
    /// <param name="asset"></param>
    private static void EditDataLocations(IAssetData asset)
    {
        IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

        foreach ((string key, LocationData values) in data)
        {
            if (values.Fish?.Count is 0 or null)
            {
                continue;
            }

            List<SpawnFishData> newEntries = [];
            foreach (SpawnFishData fish in values.Fish)
            {
                if (fish.IsBossFish && fish.CatchLimit == 1)
                {
                    newEntries.Add(fish.MakeLegendaryBaitEntry());
                }
            }

            if (newEntries.Count > 0)
            {
                Monitor.Log($"Added {newEntries.Count} entries for legendary bait in {key}.");
                values.Fish.AddRange(newEntries);
            }
        }


        if (!data.TryGetValue(ModEntry.Data.ExteriorMapName, out LocationData museumData))
        {
            Monitor.Log($"{ModEntry.Data.ExteriorMapName} data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }

        // add pufferchick.
        string originalCondition = $"PLAYER_HAS_CAUGHT_FISH Current (O)128, PLAYER_HAS_MAIL Current {AquariumPlayerHasChicken} Any";
        SpawnFishData basePuffer = new()
        {
            ItemId = PufferchickQualifiedId,
            Id = PufferchickQualifiedId,
            CanUseTrainingRod = false,
            IsBossFish = true,
            CatchLimit = 1,
            Condition = $"{originalCondition}, {AquariumGameStateQuery.RandomChanceForPuffer}"
        };
        museumData.Fish.Add(basePuffer);

        SpawnFishData pufferCopy = basePuffer.MakeLegendaryBaitEntry();
        pufferCopy.Condition = $"{originalCondition}, {AquariumGameStateQuery.HasBaitQuery} Current {LegendaryBaitQualifiedId}";
        museumData.Fish.Add(pufferCopy);

        if (!data.TryGetValue("Beach", out LocationData beachData))
        {
            Monitor.Log("Beach data seems missing, cannot copy.", LogLevel.Warn);
            return;
        }

        museumData.ArtifactSpots ??= [];
        museumData.ArtifactSpots.AddRange(beachData.ArtifactSpots ?? Enumerable.Empty<ArtifactSpotDropData>());

        if (beachData.FishAreas is not null)
        {
            museumData.FishAreas ??= [];
            foreach ((string key, FishAreaData fishArea) in beachData.FishAreas)
            {
                beachData.FishAreas.TryAdd(key, fishArea);
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
                Action = $"AddMail Current {AquariumOpenAfterLandslide} {nameof(MailType.Now)}",
                Condition = $"DAYS_PLAYED 30 30, !PLAYER_HAS_MAIL Current {AquariumOpenLater} Any",
                Id = $"{AquariumOpenAfterLandslide}_Trigger"
            }
        );
        data.Add(
            new()
            {
                Trigger = "DayStarted",
                Action = $"AddMail Current {AquariumOpenLater} {nameof(MailType.Now)}",
                Condition = $"DAYS_PLAYED 31, !PLAYER_HAS_MAIL Current {AquariumOpenAfterLandslide} Any",
                Id = $"{AquariumOpenLater}_Trigger"
            }
        );
    }
}

file static class AssetEditExtensions
{
    private static void AddCondition(this GenericSpawnItemDataWithCondition spawnable, string newCondition)
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

    internal static SpawnFishData MakeLegendaryBaitEntry(this SpawnFishData spawnable)
    {
        SpawnFishData copy = spawnable.DeepClone();
        copy.Id += "_LegendaryBait";
        copy.AddCondition($"{AquariumGameStateQuery.HasBaitQuery} Current {AssetEditor.LegendaryBaitQualifiedId}");
        copy.CatchLimit = -1;
        copy.Chance = 1;
        copy.IgnoreFishDataRequirements = true;

        return copy;
    }
}
