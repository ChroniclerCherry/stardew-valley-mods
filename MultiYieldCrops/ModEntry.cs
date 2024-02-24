using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using MultiYieldCrops.Framework;

namespace MultiYieldCrops
{
    class ModEntry : Mod
    {
        public static ModEntry instance;
        private Dictionary<string, IDictionary<int, string>> ObjectInfoSource { get; set; }

        private Dictionary<string, List<Rule>> allHarvestRules;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            //harmony stuff
            HarvestPatches.Initialize(Monitor);
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
                prefix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.CropHarvest_prefix)),
                postfix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.CropHarvest_postfix))
                );

            /* patch for handling tea leaves
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.TerrainFeatures.Bush), nameof(StardewValley.TerrainFeatures.Bush.performUseAction)),
                postfix: new HarmonyMethod(typeof(HarvestPatches), nameof(HarvestPatches.BushPerformUseAction_postfix))
                );
                */

            helper.Events.GameLoop.SaveLoaded += UpdateObjectInfoSource;

            InitializeHarvestRules();
        }

        public void SpawnHarvest(Vector2 tileLocation, string cropName, int fertilizer, JunimoHarvester junimo = null)
        {

            if (!allHarvestRules.ContainsKey(cropName))
                return;

            Vector2 location = new Vector2((tileLocation.X * 64 + 32), (tileLocation.Y * 64 + 32));

            foreach (Rule data in allHarvestRules[cropName])
            {
                foreach (Item item in SpawnItems(data,fertilizer))
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

        private IEnumerable<Item> SpawnItems(Rule data, int fertilizer)
        {
            int quality = fertilizer;
            int itemID = GetIndexByName(data.ItemName, data.ExtraYieldItemType);
            int xTile = Game1.player.getTileX();
            int yTile = Game1.player.getTileY(); ;

            //stole this code from the game to calculate crop quality
            Random random = new Random(xTile * 7 + yTile * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
            double highQualityChance = 0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizer * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
            double lowerQualityChance = Math.Min(0.75, highQualityChance * 2.0);

            //stole this code from the game to calculate # of crops
            int increaseMaxHarvest = 0;
            if (data.maxHarvestIncreasePerFarmingLevel > 0)
                increaseMaxHarvest = (int)(Game1.player.FarmingLevel * data.maxHarvestIncreasePerFarmingLevel);
            int quantity = random.Next(data.minHarvest, Math.Max(data.minHarvest, data.maxHarvest + increaseMaxHarvest + 1));

            if (quantity < 0)
                quantity = 0;

            if (itemID < 0)
            {
                Monitor.Log($"No idea what {data.ExtraYieldItemType} {data.ItemName} is", LogLevel.Warn);
                yield return null;
            }

            for (int i = 0; i < quantity; i++)
            {
                if (random.NextDouble() < highQualityChance)
                    quality = 2;
                else if (random.NextDouble() < lowerQualityChance)
                    quality = 1;
                yield return CreateItem(itemID, data.ExtraYieldItemType, quality);
            }

        }

        private Item CreateItem(int itemID,string ItemType,int quality)
        {
            switch (ItemType)
            {
                case "Object":
                    return new StardewValley.Object(itemID, 1, false, quality: quality);
                case "BigCraftable":
                    return new StardewValley.Object(Vector2.Zero, itemID);
                case "Clothing":
                    return new Clothing(itemID);
                case "Ring":
                    return new Ring(itemID);
                case "Hat":
                    return new Hat(itemID);
                case "Boot":
                    return new Boots(itemID);
                case "Furniture":
                    return new Furniture(itemID, Vector2.Zero);
                case "Weapon":
                    return new MeleeWeapon(itemID);
                default: return null;
            }
        }

        public int GetIndexByName(string name,string itemType)
        {
            //there's multiple stone items and 390 is the one that works
            if (itemType == "Object" && name == "Stone")
                return 390;

            foreach (KeyValuePair<int, string> kvp in ObjectInfoSource[itemType])
            {
                if (kvp.Value.Split('/')[0] == name)
                {
                    return kvp.Key;
                }
            }
            return -1;
        }
        private void UpdateObjectInfoSource(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            //load up all the object information into a static dictionary
            ObjectInfoSource = new Dictionary<string, IDictionary<int, string>>
            {
                { "Object", Game1.objectInformation },
                { "BigCraftable", Game1.bigCraftablesInformation },
                { "Clothing", Game1.clothingInformation },
                { "Ring", Game1.objectInformation },
                {
                    "Hat",
                    Helper.Content.Load<Dictionary<int, string>>
                        (@"Data/hats", ContentSource.GameContent)
                },
                {
                    "Boot",
                    Helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Boots", ContentSource.GameContent)
                },
                {
                    "Furniture",
                    Helper.Content.Load<Dictionary<int, string>>
                            (@"Data/Furniture", ContentSource.GameContent)
                },
                {
                    "Weapon",
                    Helper.Content.Load<Dictionary<int, string>>
                            (@"Data/weapons", ContentSource.GameContent)
                }
            };

        }



        private void InitializeHarvestRules()
        {
            allHarvestRules = new Dictionary<string, List<Rule>>();
            try
            {
                ContentModel data = Helper.ReadConfig<ContentModel>();
                if (data.Harvests != null)
                {
                    LoadContentPack(data);
                }

            } catch(Exception ex)
            {
                Monitor.Log(ex.Message + ex.StackTrace,LogLevel.Error);
            }

            foreach (var pack in Helper.ContentPacks.GetOwned())
            {
                if (!pack.HasFile("HarvestRules.json"))
                {
                    Monitor.Log($"{pack.Manifest.UniqueID} does not have a HarvestRules.json", LogLevel.Error);
                    continue;
                }
                
                LoadContentPack(pack.ReadJsonFile<ContentModel>("HarvestRules.json"));
                
            }
        }
        private void LoadContentPack(ContentModel data)
        {
            if (data == null)
                return;

            foreach (var harvests in data.Harvests)
            {
                LoadCropHarvestRulesFor(harvests.CropName,harvests.HarvestRules);
            }
        }

        private void LoadCropHarvestRulesFor(string cropName, List<Rule> harvestRules)
        {
            foreach(Rule rule in harvestRules)
            {
                if (rule.disableWithMods != null)
                {
                    bool skipRule = false;
                    foreach (string mod in rule.disableWithMods)
                    {
                        if (Helper.ModRegistry.IsLoaded(mod))
                        {
                            Monitor.Log($"A rule was skipped for {cropName} because {mod} was found", LogLevel.Trace);
                            skipRule = true;
                            break;
                        }
                    }

                    if (skipRule)
                        continue;
                }


                if (allHarvestRules.ContainsKey(cropName)){
                    allHarvestRules[cropName].Add(rule);
                } else
                {
                    allHarvestRules[cropName] = new List<Rule>();
                    allHarvestRules[cropName].Add(rule);
                }
                
            }
        }
    }
}
