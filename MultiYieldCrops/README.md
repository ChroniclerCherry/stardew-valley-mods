# Multi Yield Crops

Allows crops to yield more than just its default item.

Each crop can have multiple Rules added to them, specifying the items to yield when that crop is harvested. Multiple rules can be put in place for the same crop

There are two ways to add multiple yields to crops. The first is via the `config.json`

The second is by making a content pack for `Cherry.MultiYieldCrops` and including a `HarvestRules.json`

Both the config and HarvestRules.json have the exact same format. Multiple rules for the same crop from different sources will all be applied.

## Compatibility

As all items are specified by name, works with Json Assets-added custom items and crops.

There are no known incompatibilities

## Template

Feel free to copy this to use as a base

```js
{
  "Harvests": [

  //Start of block for each crop. You can have as many of these as you want
    {
      "CropName": "Parsnip", //the crop to add more items to when harvesting
      "HarvestRules": [

      //Start of block for each extra item spawned by this crop. You can have as many of these as you want
        {
          "ItemName": "Sea Cucumber", //The name of the item to spawn

          //the below are all optional and can be left out
          "ExtraYieldItemType": "Object", //Defaults to Object. Scroll down for more available types
          "minHarvest": 1, //Defaults to 1. Minimum number of items to be spawned. Use negative numbers to decrease the chance of the item spawning at all
          "maxHarvest": 1, //Defaults to 1. Maximum number of items to be spawned. maxHarvestIncreasePerFarmingLevel is added to this
          "maxHarvestIncreasePerFarmingLevel": 0, //A decimal number. I suggest keeping this low. A value of 1 means an extra 10 crops per harvest at max level
          "disableWithMods": [ "mod1.example", "mod2.example"] //this rule will be ignored if any of the listed mods' unique IDs are installed
        }, //End of block for each extra item spawned by this crop. 
      ]
    }, //End of block for each crop

  ]
}

```

## ExtraYieldItemType
Shamelessly copied from STF

Possible `ExtraYieldItemType` determine which file from the game's `Contents` folder the item data is obtained from.

99.999% of the time you'll just be using Objects, but who am I to stop you from spawning galaxy swords out of your parsnips

ExtraYieldItemType | Source | Notes
------------ | ------------- | -------------
"Object" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | Contains most objects in the game not covered by the other categories. Note: Rings will be created without errors using the Object category. however this creates an Object version of the rings and it will not be wearable.
"Ring" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | While sharing the same data file as most objects, it requires a unique constructor and thus is separate
"BigCraftable" | [`data/BigCraftablesInformation.json`](https://stardewvalleywiki.com/Modding:Big_Craftables_data) | 
"Clothing" | `data/ClothingInformation.json` | This contains all shirts and pants
"Hat" | [`data/hats.json`](https://stardewvalleywiki.com/Modding:Hat_data) |
"Boot" | `data/Boots.json` |
"Furniture" | [`data/Furniture.json`](https://stardewvalleywiki.com/Modding:Furniture_data) |
"Weapon" | [`data/weapons.json`](https://stardewvalleywiki.com/Modding:Weapon_data) |

## Quantity and Quality

The code for the number of crops yielded is:
```cs
    int increaseMaxHarvest = 0;
    if (data.maxHarvestIncreasePerFarmingLevel > 0)
        increaseMaxHarvest = (int)(Game1.player.FarmingLevel * data.maxHarvestIncreasePerFarmingLevel);
    int quantity = random.Next(data.minHarvest, Math.Max(data.minHarvest, data.maxHarvest + increaseMaxHarvest));

    if (quantity < 0)
        quantity = 0;
```

Broken down:
    1: Extra number of maximum crops is multipled by the farming level + maxHarvestIncreasePerFarmingLevel
    2: A random number is generated between minHarvest and (maxHarvest+ increaseMaxHarvest)
    3: If a negative number is generated, 0 crops are spawned

Therefore, a negative minHarvest introduces the chance that no extra crops are spawned, and the lower it is compared to the maxHarvest, the less likely it is for that item to spawn.

The quality is calculated the same way as the game's crops. For each harvest, this is the code that runs

```cs
    Random random = new Random(xTile * 7 + yTile * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
    double highQualityChance = 0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizer * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
    double lowerQualityChance = Math.Min(0.75, highQualityChance * 2.0);
```

Then for each item spawned from that harvest, if the item rolls a random number higher than highQualityChance, it will be gold star. If it fails, it will roll against lowerQualityChance for a chance to be silver. If it fails both, then it will be regular quality. 