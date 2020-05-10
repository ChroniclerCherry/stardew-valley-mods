# Shop Tile Framework

## Navigation
- [Intro](#intro)
- [Requirements](#requirements)
- [Create a Content Pack](#create-a-content-pack)
    * [JA Integration](#ja-integration)
    * [Regular shops](#regular-shops)
      * [Item Types](#itemtypes)
    * [Animal Shops](#animal-shops)
    * [Condition Checking](#condition-checking)
      * [Available Conditions](#available-conditions)
    * [Translations](#translations)
    * [Portrait](#portrait)
- [Example](#example)
- [Adding shops to the game](#adding-shops-to-the-game)
- [Placing Vanilla Shops](#placing-vanilla-shops)
- [Console Commands](#console-commands)
- [Contact the dev](#contact-the-dev)

## Intro

**This page is a resource for modders to create a custom shop. If you're a player, check out [this link](https://www.nexusmods.com/stardewvalley/mods/5005) instead**

Shop Tile Framework (STF) is a tool for Modders of the game Stardew Valley, which allows you to define shops via a shops.json file. These shops are attached to tile properties which can be loaded anywhere into the game with another method. Content packs need a `shops.json` to define their shops.

STF lets you fully customize what items are sold and how many, under what conditions etc, as well the currency used in the store: money, casino coins, festival score, and even items just like the vanilla desert trader.

STF also allows you to create customized animal shops, with support for custom animals added through Better Farm Animal Varioety (BFAV) 

Shops can be opened with a custom tile property of "Shop" or "AnimalShop" and a value of the ShopName defined in `shops.json`

## Requirements
STF is a standalone SMAPI mod with no prerequisites other than the newest version of SMAPI. However there is optional support for items added in with [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720). Custom JA items can be added to shops by item name or by content pack.

You do not need to make having the JA packs mandatory; if STF does not detect the item in the game, it will simple not add them.

The same with BFAV animals-- if the specific animals ( or the BFAV mod ) are not installed, they are simply not added

## Create a content pack
To make a content pack for Shop Tile Framework, add `Cherry.ShopTileFramework` to the `ContentPackFor` section of your mod's [manifest file](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest).

Then from there, you need to make a `shops.json` file to define the properties of your shops. You can find an example [here](#adding-store-to-the-game), and each field is described below:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
Shops | Optional | An array of Shops | You can add as many shops as you want, as long as they have unique `ShopName`among Shops.
AnimalShops | Optional | An array of AnimalShops | You can add as many animal shops as you want, as long as they have unique `ShopName` among AnimalShops.

### JA Integration
There are a few ways of selling custom JA items in the store. JA items or packs can be included in the shop.json as described below.

STF shops can also be targetted in the Json Assets's json file, by setting the `PurchaseFrom` or `SeedPurchaseFrom` field to `"STF.<ShopName>"`. Anything added to the shop from JA will always be added to the shop under the conditions specified by `PurchaseRequirements` in the JA json. It will not be subject to the conditions set in the STF shops.json, including shop-global settings such as price, and maximum items. The exception is currency: money/festival score/casino points, which will always apply to the entire store.

Note: Due to how it is implemented, only stores with a portrait image can be targetted by json assets

### Regular Shops
Each Shop contains:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ShopName | Mandatory | string | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
StoreCurrency | Optional | string | The currency this store uses. Defaults to `"Money"` if not specified, but can also be `"festivalScore"` or `"clubCoins"`
CategoriesToSellHere | Optional | Array of ints | The negative numbers for [categories](https://stardewvalleywiki.com/Modding:Object_data#Categories) of items the player can sell at this shop. If not provided, the player can not sell anything at this store
PortraitPath | Optional | string | The relative path to the image used as the portrait for this shop from the content pack's folder. If not provided, no portrait will be drawn
Quote | Optional | string | A quote displayed on the shop menu screen. If not provided, no quote will appear
ShopPrice | Optional | int | Sets the price of every item in the store to this if set.
MaxNumItemsSoldInStore | Optional | int | The number of different items available. If there is more items within all the `ItemStocks` than this number, they will be randomly picked at the beginning of each day so that the total number of items match this. This is how to randomize the stock of the entire store.
ItemStocks | Mandatory | An array of `ItemStocks` | The items sold at this store. Each `ItemStocks` can contain one or more item of a single type
When | Optional | Array of strings | The conditions for this store to open, checked each time a player interacts with it. More info can be found under [Condition Checking](#condition-checking)
ClosedMessage | Optional | string | The message that displays if a user interacts with the store when conditions are not met. If not set, no message will be displayed.
LocalizedQuote | Optional | Dictionary<string,string> | Translations for the store quote. Refer to [Translations](#translations) for details.
LocalizedClosedMessage | Optional | Dictionary<string,string> | Translations for the closed message. Refer to [Translations](#translations) for details.


An `ItemStock` is used to define a group of properties --things like price, conditions, the number sold-- that is applied to one or more items of a single ItemType. There are three ways to specify items ( ID, Name, or JA Pack) and all three can be used at once in a single item stock. You can have as many ItemStocks as you need

Each ItemStock contains:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ItemType | Mandatory | string |  Determines what kind of Object this ItemStock contains, necessary to find the right unique items.
IsRecipe | Optional | boolean | Only works for Objects and BigCraftables. If set to true, will sell the recipes instead of the object. Defaults to false. **Warning:** If you specify items that do not have a recipe, the recipe will be sold in the store but the player won't learn the recipe because...well, it doesn't exist.
StockPrice | Optional | int | Sets the price for all items in this ItemStock. Overrides ShopPrice. If neither price fields are given, default item sell prices are used
StockItemCurrency | Optional | string | You can specify an `Object` by name as trading currency. Note: this will charge both the specified item as well as the `StoreCurrency` unless the price is set to 0. These can include JA Objects.
StockCurrencyStack | Optional | int | The number of the `StockItemCurrency` it costs for each item. Defaults to 1
Quality | Optional | int | The quality of the sold items. 0  for normal, 1 for silver, 2 for gold, and 4 for iridium. 3 is not a valid quality.
ItemIDs | Optional | Array of ints | Adds a list of items by their IDS. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
ItemNames | Optional | Array of strings | Adds a list of items by their internal names. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
JAPacks | Optional | Array of strings | Adds all items of `ItemType` from the specified JA Packs, identified by their `UniqueID`. Crops and Trees added through `JAPacks` specified with `Object` will sell the products, while `Seed` will sell the seeds/saplings.
Stock | Optional | int | How many of each item is available to buy per day. If not set, the stock is unlimited
MaxNumItemsSoldInItemStock | Optional | int | The number of different items available from this ItemStock. If there are more items in this ItemStock than `MaxNumItemsSoldInItemStock` a random set will be picked per day. This is used to randomize the items listed in this `ItemStock`
When | Optional | Array of strings | A condition for the items in this ItemStock to appear. More info can be found under [Condition Checking](#condition-checking) **Warning:** Avoid checks like `t` and `a` as conditions for ItemStocks are only checked at the start of each day, not when the user opens the shop menu. Only use these if you are planning to manually refresh the shop stock through a SMAPI mod.

### ItemTypes
Possible `ItemType` determine which file from the game's `Contents` folder the item data is obtained from.

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
"Wallpaper" | Maps/walls_and_floors.png | Wallpapers have no name and thus have to be specified by `ItemIDs`
"Floors" | Maps/walls_and_floors.png | Floors have no name and thus have to be specified by `ItemIDs`
"Seed" | JA Packs Only | Use this ItemType if adding custom crops through `JAPacks` and you want the seeds/saplings instead of the produce

### Animal Shops

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ShopName | Mandatory | string | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
AnimalStock | Mandatory | array of strings | A list of animals by name that are sold at this shop. For custom BFAV animals, this is what you would find under the animal's "category". Currently only supports BFAV animals added to Marnie's store
ExcludeFromMarnies | Optional | array of strings | A list of animals to remove from Marnie's shop. This is a way to have the animal exclusively sold by your custom shop
When | Optional | Array of strings | The conditions for this store to open, checked each time a player interacts with it. More info can be found under [Condition Checking](#condition-checking)
ClosedMessage | Optional | string | The message that displays if a user interacts with the store when conditions are not met. If not set, no message will be displayed.
LocalizedClosedMessage | Optional | Dictionary<string,string> | Translations for the closed message. Refer to [Translations](#translations) for details.

### Condition Checking
All `When` fields used for various condition checking uses vanilla [event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions) as well as several custom ones. `When` conditions can be used to determine conditions for a shop opening ( such as hours, or when an NPC is nearby ) as well as for setting conditions for ItemStocks to be added to stores or not when stocks are refreshed.

`When` takes an array of strings. Each String can be a full list of conditions that must ALL be met seperated by `/` values just like vanilla event conditions.

You can check the opposite of any condition by putting a `!` in front of it. For example `!f Linus 2500` would return true only if the player was NOT at 2500FP/10 hearts with Linus

**Note:** For ItemStock condition checks, they are only checked at the beginning of each day! Avoid checks that don't make sense at the beginning of the day, such as store hours or checking for if an NPC is on the map

When multiple fields are provided, the condition will work if _any_ of the strings return a true. Here's an example of a shop that has different opening hours based on season:
```js
{
  "Shops": [
    {
      "ShopName": "MyShop",
      "ItemStocks": [
        {
          "ItemType": "Object",
          "ItemNames": [
            "Parsnip"
          ]
        }
      ],
      "When": [
        "!z spring/t 600 1000", //in spring, only opens between 6am and 10am
        "!z summer/t 1000 1400", //in summer only opens from 10am to 2pm
        "z spring/z summer/t 1800 20000", //in fall and winter, only open from 6pm to 10pm
        "f Linus 2500" //is always open if player has 2500 friendship points / 10 hearts with linus
      ],
      "ClosedMessage": "This shop is closed."
    }
  ]
}
```
#### Available Conditions

All [event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions) are available, as well as:

Syntax | Description | Example
------------- | ------------- | -------------
`NPCAt <s:NPCName> [<i:x> <i:y>]` | This will check if the named NPC is at the given tile coordinates on the current map. Multiple x/y coordinates can be given, and will return true if the NPC is at any of them. | `NPCAt Pierre 5 10 5 11 5 12` will check if Pierre is at (5,10) (5,11) or (5,12)
`HasMod [<s:UniqueID>]` | This will check if the given Unique ID of certain mods is installed. Multiple can be supplied and will return true only if the player has all of them installed. | `HasMod Cherry.CustomizeAnywhere Cherry.PlatonicRelationships` returns true if both Customize Anywhere and Platonic Relationships are installed
`SkillLevel [<s:SkillName> <i:SkillLevel>]` | This will check if the player has at least the given skill level for named skills. Multiple skill-level pairs can be provided, and returns true if all of them are matched. Valid skills are: `combat`, `farming`, `fishing`, `foraging`, `luck` (unsued in vanilla), and `mining` | `SkillLevel farming 5 fishing 3` Would return true if the player has at least level 5 farm and level 3 fishing
`CommunityCenterComplete` | Returns true if the Community center is completed on this save file| 
`JojaMartComplete` | Returns true if the joja mart route was completed on this save file |

I am always taking requests for more conditions as they are needed! Open an issue any time

##### Some useful vanilla preconditions of note ( taken directly from the Wiki ):

Syntax | Description
------------- | -------------
`r <number>` | A random probability check, where `number` is the probability between 0 and 1 (e.g. 0.2 for 20% chance).
`t <min time> <max time>` | Current time is between between the specified times. Can range from 600 to 2600.
`d <day of week>` | Today is not one of the specified days (may specify multiple days). Valid values: Mon, Tue, Wed, Thu, Fri, Sat, Sun.
`y <year>` | If `year` is 1, must be in the first year. Otherwise, year must be at least this value.
`z <season>` | Current season is not `season`. ( Tip: To specify that it _is_ `season`, use `!z <season>` instead )
`e <event ID>` | Current player has seen the specified event (may contain multiple event IDs).
`p <name>` | Specified NPC is in the current player's location. ( useful for having your shop open only when the NPC is near the shop, without specifying every tile )
`f <name> <number>` | Current player has at least `number` friendship points with the `name` NPC. Can specify multiple name and number pairs, in which case the player must meet all of them.

### Translations
Each store has localization fields that can be used to translate the message displayed when closed, or the shop quote. To add a translation, use the language code as the key and then the translation as the value. for example:
```js
    "LocalizedQuote": { "zh": "你好，世界" },
    "LocalizedClosedMessage": { "zh": "再见" }
```

The available language codes supported by the game are `zh` (Chinese), `fr` (French), `de` (German), `hu`(Hungarian), `it` (Italian), `ja` (Japanese), `ko`(Korean), `pt` Portuguese, `ru` (Russian), `es` (Spanish), and `tr` (Turkish).

Any languages not provided will default to english

### Portrait
Similar to content patcher, the shop portrait can be made seasonal by adding the season to the end of the file name. For example, you have a portrait for your shop in `assets/Bob.png`. To make the portrait different during the Summer, you'd add the summer portrait to the same folder but name it `Bob_summer.png`

```
YourMod
    manifest.json
    shops.json
    assets
        Bob.png
        Bob_summer.png
```

The result would be for spring, fall, and winter, `Bob.png` will be the portrait used, but during summer, `Bob_summer.png` will be used instead

## Example
Example shops.json:
```js
{
  "Shops": [
    {
      "ShopName": "MyShop",
      "StoreCurrency": "festivalScore", //uses festival score as the currency for the whole shop
      "CategoriesToSellHere": [ //player can sell Forage and Vegetables to this store
        -81, 
        -75
      ],
      "PortraitPath": "assets/Portrait.png",
      "Quote": "This is a store!", 
      "ShopPrice": 80,
      "MaxNumItemsSoldInStore": 30, //if all items total over 30, a random 30 will be picked each day
      "ItemStocks": [
        {
          "ItemType": "Clothing",
          "StockItemCurrency": "Parsnip", //This Itemstock charges Parsnips
          "StockCurrencyStack": 5, //and it takes 5 parsnips each time
          "StockPrice": 0, //This ItemStock doesn't charge any currency (festival score for this shop)
          "JAPacks": [
            "missy.shirtsja"
          ],
          "Stock": 4,
          "MaxNumItemsSoldInItemStock": 5, //if there's more than 5 items total in this Item stock, a random 5 will be picked each day
          "When": [ 
            "f Haley 1500/z winter", //only sell this ItemStock if the player has 1500 friendship points/6 hearts and it's not winter
            "f Emily 1500/z spring/z summer/z fall", //or if you have 1500 friendship points with Emily during the winter
          ]
        },
        {
          "ItemType": "Clothing",
          "StockPrice": 500,
          "ItemIDs": [
            5,
            10,
            1015
          ],
          "ItemNames": [
            "Prismatic Shirt",
            "Prismatic Pants"
          ],
          "Stock": 4,
          "MaxNumItemsSoldInItemStock": 5
        }
      ],
      "When": [ //only open the store from 8AM to 6PM
        "t 800 1800"
      ],
      "ClosedMessage": "This store is open daily from 8AM to 6PM" //the message displayed if the store is closed
    }
  ],
"AnimalShops":[
{
	"ShopName":"MyAnimalShop",
	"AnimalStock":["Chicken","Fennec Fox","Phoenix","Raccoon", "Quail", "Warthog"],
	"ExcludeFromMarnies":["Chicken","Fennec Fox","Phoenix","Raccoon", "Quail", "Warthog"] //don't sell these animals at Marnie's anymore
	},
  ],
}
```

And the manifest:
```json
{
  "Name": "A store mod",
  "Author": "your name",
  "Version": "1.0.0",
  "Description": "One or two sentences about the mod.",
  "UniqueID": "YourName.YourProjectName",
  "MinimumApiVersion": "3.0.0",
  "UpdateKeys": [],
  "ContentPackFor": {
    "UniqueID": "Cherry.ShopTileFramework"
  }
}
```

## Adding shops to the game

The regular shop defined in the above json can be opened by clicking on a tile with the following properties on the **Buildings** layer ( With the property being `Shop` and the property value being your ShopName:

![Example tile properties](https://i.gyazo.com/20d6645d35c72a61977c5d2e900ae182.png)

Animal shops are similar, just with an AnimalShop property:

![Example AnimalShop tile property](https://i.gyazo.com/9b3b0a69506dbf3a1fb6824cf5c1c382.png)

The empty `Action` Property is optional; it just changes the appearance of the game cursor when hovering over a shop to make it clear that it is interactable. These tile properties can be loaded into the game with any other method usually used to load in maps. Content Patcher, TMXL, or SMAPI mods can all add the property along with the shop itself. More info about modding maps can be found [here](https://stardewvalleywiki.com/Modding:Maps)

## Placing Vanilla Shops

Vanilla shops can be called the same way as custom Shops: With a `Shop` tile property and the corresponding `ShopName`.
Note that these shop tiles do not check for conditions and will always bring up the menu when clicked, and override any custom shops that use the same `Shopname` ( so please don't prefix your modded shops with Vanilla! )

ShopName | Description
------------ | -------------
Vanilla!PierreShop | Pierre's store
Vanilla!JojaShop | The Joja store
Vanilla!RobinShop | Robin's supplies store
Vanilla!RobinBuildingsShop | The carpenter building menu
Vanilla!ClintShop | Clint's supplies store
Vanilla!ClintToolUpgrades | Clint's tool upgrades store
Vanilla!MarlonShop | The Adventurer's Guild store
Vanilla!AdventureRecovery | The Adventurer's Guild's store for recovering lost items
Vanilla!MarnieShop | Marnie's supplies store
Vanilla!HarveyShop | The hospital store
Vanilla!SandyShop | Sandy's store
Vanilla!DesertTrader | The desert trader's store
Vanilla!KrobusShop | The sewer's store
Vanilla!DwarfShop | The dwarf's store
Vanilla!GusShop | The Saloon's store
Vanilla!WillyShop | The fish store
Vanilla!QiShop | The casino store
Vanilla!IceCreamStand | The Ice-cream stand
Vanilla!WizardBuildings | The Wizard's buildings menu
Vanilla!MarnieAnimalShop | The animal purchase menu
Vanilla!ClintGeodes | Opening Geodes menu

Example:

![This will open the animal purchasing menu](https://i.gyazo.com/b4fbcdd09772f0f556f41f56de299f57.png)

## Console Commands

A few console commands are added to SMAPI in order to help with debugging. Type `help` in the console to get a full list of available commands. None of these work for the vanilla shops

Command | Description
------------ | -------------
 `open_shop <ShopName>` | Will open up the shop with the specified `ShopName`. Useful for testing without adding in a tile property / needing to go to the shop location
 `open_animal_shop <ShopName>` | Will open up the animal shop with the specified `ShopName`. Useful for testing without adding in a tile property / needing to go to the shop location
 `reset_shop <ShopName>` | Will reset the stock of the specified `ShopName`, which usually happens at the start of each day. Useful for checking that your conditions are applying / stock is randomizing as you'd like'
 `list_shops` | Lists all of the `ShopName`s registered with Shop Tile Framework
 
 ## Contact The Dev
If you need to find me, the following methods are your best bets:
- Bug reports can be made by submitting an issue on this repositiory, or use the [bugs tab](https://www.nexusmods.com/stardewvalley/mods/5005?tab=bugs) on the Nexus mod page. Please provide a [log](https://smapi.io/log/) with all bug reports and as much information about the circumstances of the bug as possible.
- Suggestions should be submitted through an issue on this repository
- If you have questions that aren't answered here or requires clarification, you can DM me on discord at `Chronicler#9318`
