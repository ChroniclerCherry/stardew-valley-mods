# Multi Yield Crops

Allows crops to yield more than just its default item.

Each crop can have multiple Rules added to them, specifying the items to yield when that crop is harvested. Multiple rules can be put in place for the same crop

There are two ways to add multiple yields to crops. The first is via the `config.json`

The second is by making a content pack for `Cherry.MultiYieldCrops` and including a `HarvestRules.json`

Both the config and HarvestRules.json have the exact same format. Multiple rules for the same crop from different sources will all be applied.

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
          "maxHarvestIncreasePerFarmingLevel": 0, //can be a decimal number
          "disableWithMods": [ "mod1.example", "mod2.example"] //this rule will be ignored if any of the listed mods' unique IDs are installed
        }, //End of block for each extra item spawned by this crop. 
      ]
    }, //End of block for each crop

  ]
}

```

## ExtraYieldItemType
Shamelessly copied from STF

Possible `ItemType` determine which file from the game's `Contents` folder the item data is obtained from.

99.999% of the time you'll just be using Objects, but I won't stop you from spawning galaxy swords out of your parsnips

ItemType | Source | Notes
------------ | ------------- | -------------
"Object" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | Contains most objects in the game not covered by the other categories. Note: Rings will be created without errors using the Object category. however this creates an Object version of the rings and it will not be wearable.
"Ring" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | While sharing the same data file as most objects, it requires a unique constructor and thus is separate
"BigCraftable" | [`data/BigCraftablesInformation.json`](https://stardewvalleywiki.com/Modding:Big_Craftables_data) | 
"Clothing" | `data/ClothingInformation.json` | This contains all shirts and pants
"Hat" | [`data/hats.json`](https://stardewvalleywiki.com/Modding:Hat_data) |
"Boot" | `data/Boots.json` |
"Furniture" | [`data/Furniture.json`](https://stardewvalleywiki.com/Modding:Furniture_data) |
"Weapon" | [`data/weapons.json`](https://stardewvalleywiki.com/Modding:Weapon_data) |