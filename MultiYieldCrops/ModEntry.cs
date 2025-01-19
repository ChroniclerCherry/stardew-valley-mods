using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MultiYieldCrops.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace MultiYieldCrops;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    public static ModEntry Instance;

    private Dictionary<string, List<Rule>> AllHarvestRules;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Instance = this;

        //harmony stuff
        HarvestPatches.Initialize(this.Monitor);
        Harmony harmony = new(this.ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
            prefix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.CropHarvest_prefix)),
            postfix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.CropHarvest_postfix))
            );

        //// patch for handling tea leaves
        //harmony.Patch(
        //    original: AccessTools.Method(typeof(StardewValley.TerrainFeatures.Bush), nameof(StardewValley.TerrainFeatures.Bush.performUseAction)),
        //    postfix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.BushPerformUseAction_postfix))
        //);

        this.InitializeHarvestRules();
    }

    public void SpawnHarvest(Vector2 tileLocation, string cropName, int fertilizer, JunimoHarvester junimo = null)
    {
        if (!this.AllHarvestRules.ContainsKey(cropName))
            return;

        Vector2 location = new Vector2((tileLocation.X * 64 + 32), (tileLocation.Y * 64 + 32));

        foreach (Rule data in this.AllHarvestRules[cropName])
        {
            foreach (Item item in this.SpawnItems(data, fertilizer))
            {
                if (item == null)
                    continue;
                if (junimo == null)
                {
                    Game1.createItemDebris(item, location, -1);
                }
                else
                {
                    junimo.tryToAddItemToHut(item);
                }
            }
        }
    }


    /*********
    ** Private methods
    *********/
    private IEnumerable<Item> SpawnItems(Rule data, int fertilizerQualityLevel)
    {
        int quality = fertilizerQualityLevel;
        string itemId = this.GetIdByName(data.ItemName, data.ExtraYieldItemType);
        Point tile = Game1.player.TilePoint;

        //stole this code from the game to calculate crop quality
        Random random = new Random(tile.X * 7 + tile.Y * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
        double highQualityChance = 0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizerQualityLevel * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
        double lowerQualityChance = Math.Min(0.75, highQualityChance * 2.0);

        //stole this code from the game to calculate # of crops
        int increaseMaxHarvest = 0;
        if (data.MaxHarvestIncreasePerFarmingLevel > 0)
            increaseMaxHarvest = (int)(Game1.player.FarmingLevel * data.MaxHarvestIncreasePerFarmingLevel);
        int quantity = random.Next(data.MinHarvest, Math.Max(data.MinHarvest, data.MaxHarvest + increaseMaxHarvest + 1));

        if (quantity < 0)
            quantity = 0;

        if (itemId is null)
        {
            this.Monitor.Log($"No idea what {data.ExtraYieldItemType} {data.ItemName} is", LogLevel.Warn);
            yield return null;
        }

        for (int i = 0; i < quantity; i++)
        {
            if (random.NextDouble() < highQualityChance)
                quality = 2;
            else if (random.NextDouble() < lowerQualityChance)
                quality = 1;
            yield return ItemRegistry.Create(itemId, 1, quality);
        }
    }

    /// <summary>Get the qualified item ID to spawn given its name.</summary>
    /// <param name="name">The item name.</param>
    /// <param name="itemType">The item type, matching a key recognized by <see cref="GetItemDataDefinitionFromType"/>.</param>
    /// <returns>Returns the item's qualified item ID, or <c>null</c> if not found.</returns>
    private string GetIdByName(string name, string itemType)
    {
        // there's multiple stone items and 390 is the one that works
        if (itemType == "Object" && name == "Stone")
            return ItemRegistry.type_object + "390";

        foreach (IItemDataDefinition itemDataDefinition in this.GetItemDataDefinitionFromType(itemType))
        {
            foreach (ParsedItemData data in itemDataDefinition.GetAllData())
            {
                if (data.InternalName == name)
                    return data.QualifiedItemId;
            }
        }

        return null;
    }

    /// <summary>Get the item data definition which provides items of a given type.</summary>
    /// <param name="itemType">The Multi Yield Crops type ID.</param>
    private IEnumerable<IItemDataDefinition> GetItemDataDefinitionFromType(string itemType)
    {
        switch (itemType)
        {
            case "BigCraftable":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_bigCraftable);
                break;

            case "Boot":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_boots);
                break;

            case "Clothing":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_pants);
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_shirt);
                break;

            case "Furniture":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_furniture);
                break;

            case "Hat":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_hat);
                break;

            case "Object":
            case "Ring":
                yield return ItemRegistry.GetObjectTypeDefinition();
                break;

            case "Weapon":
                yield return ItemRegistry.GetTypeDefinition(ItemRegistry.type_weapon);
                break;
        }
    }

    private void InitializeHarvestRules()
    {
        this.AllHarvestRules = new Dictionary<string, List<Rule>>();
        try
        {
            ContentModel data = this.Helper.ReadConfig<ContentModel>();
            if (data.Harvests != null)
            {
                this.LoadContentPack(data);
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log(ex.Message + ex.StackTrace, LogLevel.Error);
        }

        foreach (IContentPack pack in this.Helper.ContentPacks.GetOwned())
        {
            if (!pack.HasFile("HarvestRules.json"))
            {
                this.Monitor.Log($"{pack.Manifest.UniqueID} does not have a HarvestRules.json", LogLevel.Error);
                continue;
            }

            this.LoadContentPack(pack.ReadJsonFile<ContentModel>("HarvestRules.json"));

        }
    }

    private void LoadContentPack(ContentModel data)
    {
        if (data == null)
            return;

        foreach (CropHarvestRules harvests in data.Harvests)
        {
            this.LoadCropHarvestRulesFor(harvests.CropName, harvests.HarvestRules);
        }
    }

    private void LoadCropHarvestRulesFor(string cropName, List<Rule> harvestRules)
    {
        foreach (Rule rule in harvestRules)
        {
            if (rule.DisableWithMods != null)
            {
                bool skipRule = false;
                foreach (string mod in rule.DisableWithMods)
                {
                    if (this.Helper.ModRegistry.IsLoaded(mod))
                    {
                        this.Monitor.Log($"A rule was skipped for {cropName} because {mod} was found");
                        skipRule = true;
                        break;
                    }
                }

                if (skipRule)
                    continue;
            }


            if (this.AllHarvestRules.ContainsKey(cropName))
            {
                this.AllHarvestRules[cropName].Add(rule);
            }
            else
            {
                this.AllHarvestRules[cropName] = new List<Rule>();
                this.AllHarvestRules[cropName].Add(rule);
            }
        }
    }
}
