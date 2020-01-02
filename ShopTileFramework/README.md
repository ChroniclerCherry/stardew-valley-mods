# Shop Tile Framework

## Navigation
- [Intro](#intro)
- [Requirements](#requirements)
- [Create a Content Pack](#create-a-content-pack)
    * [Regular shops](#regular-shops)
	* [Item Types](#itemtypes)
    * [Animal Shops](#animal-shops)
- [Example](#example)
- [Adding shops to the game](#adding-shops-to-the-game)
- [Placing Vanilla Shops](#placing-vanilla-shops)
- [Console Commands](#console-commands)
- [Contact the dev](#contact-the-dev)

## Intro

**This page is a resource for modders to create a custom shop. If you're a player, check out [this link](https://www.nexusmods.com/stardewvalley/mods/5005) instead**

Shop Tile Framework (STF) is a tool for Modders of the game Stardew Valley, which allows you to define shops via a shops.json file. These shops are attached to tile properties which can be loaded anywhere into the game with another method. Content packs need a `shops.json` to define their shops.

STF lets you fully customize what items are sold and how many, under what conditions etc, as well the currency used in the store: money, casino coins, festival score, and even items just like the vanilla desert trader.

STF also allows you to create customized animal shops, with support for custom animals added through BFAV 

Shops can be opened with a custom tile property of "Shop" or "AnimalShop" and a value of the ShopName defined in `shops.json`

## Requirements
STF is a standalone SMAPI mod with no prerequesites other than the newest version of SMAPI. However there is optional support for items added in with [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720). Custom JA items can be added to shops by item name or by content pack.

You do not need to make having the JA packs mandatory; if STF does not detect the item in the game, it will simple not add them.

The same with BFAV animals-- if the specific animals ( or the BFAV mod ) are not installed, they are simply not added

## Create a content pack
To make a content pack for Shop Tile Framework, add `Cherry.ShopTileFramework` to the `ContentPackFor` section of your mod's [manifest file](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest).

Then from there, you need to make a `shops.json` file to define the properties of your shops. You can find an example [here](#adding-store-to-the-game), and each field is described below:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
Shops | Y | An array of Shops | You can add as many shops as you want, as long as they have unique `ShopName`
AnimalShops | Y | An array of AnimalShops | You can add as many animal shops as you want, as long as they have unique `ShopName`

### Regular Shops
Each Shop contains:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ShopName | N | string | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
StoreCurrency | Y | string | The currency this store uses. Defaults to `"Money"` if not specified, but can also be `"festivalScore"` or `"clubCoins"`
CategoriesToSellHere | Y | Array of ints | The negative numbers for [categories](https://stardewvalleywiki.com/Modding:Object_data#Categories) of items the player can sell at this shop. If not provided, the player can not sell anything at this store
PortraitPath | Y | string | The relative path to the image used as the portrait for this shop from the content pack's folder. If not provided, no portrait will be drawn
Quote | Y | string | A quote displayed on the shop menu screen. If not provided, no quote will appear
ShopPrice | Y | int | Sets the price of every item in the store to this if set.
MaxNumItemsSoldInStore | Y | int | The number of different items available. If there is more items within all the `ItemStocks` than this number, they will be randomly picked at the beginning of each day so that the total number of items match this. This is how to randomize the stock of the entire store.
ItemStocks | N | An array of `ItemStocks` | The items sold at this store. Each `ItemStocks` can contain one or more item of a single type


An `ItemStock` is used to define a group of properties --things like price, conditions, the number sold-- that is applied to one or more items of a single ItemType. There are three ways to specify items ( ID, Name, or JA Pack) and all three can be used at once in a single item stock. You can have as many ItemStocks as you need
Each ItemStock contains:


Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ItemType | N | string |  Determines what kind of Object this ItemStock contains, necessary to find the right unique items.
StockPrice | Y | int | Sets the price for all items in this ItemStock. Overrides ShopPrice. If neither price fields are given, default item sell prices are used
StockItemCurrency | Y | string | You can specify an `Object` by name as trading currency. Note: this will charge both the specified item as well as the `StoreCurrency` unless the price is set to 0. These can include JA Objects.
StockCurrencyStack | Y | int | The number of the `StockItemCurrency` it costs for each item. Defaults to 1
ItemIDs | Y/N | Array of ints | Adds a list of items by their IDS. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
ItemNames | Y/N | Array of strings | Adds a list of items by their internal names. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
JAPacks | Y/N | Array of strings | Adds all items of `ItemType` from the specified JA Packs, identified by their `UniqueID`. Crops and Trees added through `JAPacks` specified with `Object` will sell the products, while `Seed` will sell the seeds/saplings.
Stock | Y | int | How many of each item is available to buy per day. If not set, the stock is unlimited
MaxNumItemsSoldInItemStock | Y | int | The number of different items available from this ItemStock. If there are more items in this ItemStock than `MaxNumItemsSoldInItemStock` a random set will be picked per day. This is used to randomize the items listed in this `ItemStock`
When | Y | Array of strings | A condition for the items in this ItemStock to appear. Currently takes all valid vanilla [event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). **Warning:** Avoid checks like `t` and `a` as STF will only check conditions at the start of the day, not when the user opens the shop menu. Only use these if you are planning to manually refresh the shop stock through a SMAPI mod

### ItemTypes
Possible `ItemType` determine which file from the game's `Contents` folder the item data is obtained from.

ItemType | Source | Notes
------------ | ------------- | -------------
"Object" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | Contains most objects in the game not covered by the other categories
"Ring" | [`data/ObjectInformation.json`](https://stardewvalleywiki.com/Modding:Object_data) | While sharing the same data file as most objects, it requires a unique constructor and thus is seperate
"BigCraftable" | [`data/BigCraftablesInformation.json`](https://stardewvalleywiki.com/Modding:Big_Craftables_data) | 
"Clothing" | `data/ClothingInformation.json` |
"Hat" | [`data/hats.json`](https://stardewvalleywiki.com/Modding:Hat_data) |
"Boot" | `data/Boots.json` |
"Furniture" | [`data/Furniture.json`](https://stardewvalleywiki.com/Modding:Furniture_data) |
"Weapon" | [`data/weapons.json`](https://stardewvalleywiki.com/Modding:Weapon_data) |
"Wallpaper" | Maps/walls_and_floors.png | Wallpapers have no name and thus have to be specified by `ItemIDs`
"Floors" | Maps/walls_and_floors.png | Floors have no name and thus have to be specified by `ItemIDs`
"Seed" | JA Packs Only | Use this ItemType if adding custom crops through `JAPacks` and you want the seeds/saplings instead of the produce

### Animal Shops
Note: The current animal purchasing menu is hardcoded to warp you to Marnie's. To work around that, STF will then proceed to warp you back to your original map. However you'll see Marnie's map appear for a brief moment.

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ShopName | N | string | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
AnimalStock | N | array of strings | A list of animals by name that are sold at this shop. For customize BFAV animals, this is what you would find under the animal's "category"

## Example
Example shops.json with all available options:
```json
{
  "Shops": [
    {
      "ShopName": "MyShop",
      "StoreCurrency": "festivalScore",
      "CategoriesToSellHere":[
                        -81,
                        -75,
                        ],
      "PortraitPath": "assets/Portrait.png",
      "Quote": "This is a store!",
      "ShopPrice": 80,
      "MaxNumItemsSoldInStore": 30,
      "ItemStocks": [
        {
          "ItemType": "Clothing",
		  "StockItemCurrency":"Parsnip",
          "StockPrice": 0,
		  "StockCurrencyStack": 5,
		  "JAPacks": [
			"missy.shirtsja",
          ],
          "Stock": 4,
          "MaxNumItemsSoldInItemStock": 5,
		  "When":[
		  "f Haley 1500",
		  "z winter",
		  ]
        },
        {
          "ItemType": "Clothing",
          "StockItemCurrency":"Prismatic Shard",
          "StockPrice": 0,
          "StockCurrencyStack": 1,
          "ItemIDs": [
		5,10,1015
          ],
          "ItemNames": [
		"Prismatic Shirt",
		"Prismatic Pants"
          ],
          "Stock": 4,
          "MaxNumItemsSoldInItemStock": 5,
        },
        {
          "ItemType": "Object",
          "ItemIDs": [
		60,74,90
          ],
          "ItemNames": [
		"Earth Crystal",
		"Pizza",
          ],
          "JAPacks": [
		"ppja.fruitsandveggies",
		"ppja.moretrees"
          ],
          "Stock": 10,
          "MaxNumItemsSoldInItemStock": 20,
        },
      ]
    },
  ],
  "AnimalShops":[
		{
	"ShopName":"MyAnimalShop",
	"AnimalStock":["Chicken","Fennec Fox","Phoenix","Raccoon", "Quail", "Warthog"]
	},
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

The regular shop defined in the above json can be opened by clicking on a tile with the following properties on the **Buildings** layer:

![Example tile properties](https://media.discordapp.net/attachments/305520470114172928/659874803498614795/unknown.png)

Animal shops are similar, just with an AnimalShop property:

![Example AnimalShop tile property](https://media.discordapp.net/attachments/431102536926101504/662324823443111957/unknown.png)

The empty `Action` Property is optional; it just changes the appearance of the game cursor when hovering over a shop to make it clear that it is interactable. These tile properties can be loaded into the game with any other method usually used to load in maps. Content Patcher, TMXL, or SMAPI mods can all add the property along with the shop itself. More info about modding maps can be found [here](https://stardewvalleywiki.com/Modding:Maps)

Shops can also be added into the game via a SMAPI mod. TSF provides an API that lets you register new shops, and programatically open them in-game, reset their stock, or directly change their stock.

## Placing Vanilla Shops

Vanilla shops can be called the same way as custom Shops: With a `Shop` tile propertie and the corresponding `ShopName`.
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
Vanilla!IceCreamStand | The icecream stand
Vanilla!WizardBuildings | The Wizard's buildings menu
Vanilla!MarnieAnimalShop | The animal purchase menu
Vanilla!ClintGeodes | Opening Geodes menu


## Console Commands

A few console commands are added to SMAPI in order to help with debugging. Type `help` in the console to get a full list of available commands

Command | Description
------------ | -------------
 `open_shop <ShopName>` | Will open up the shop with the specified `ShopName`. Useful for testing without adding in a tile property / needing to go to the shop location
 `open_animal_shop <ShopName>` | Will open up the animal shop with the specified `ShopName`. Useful for testing without adding in a tile property / needing to go to the shop location
 `reset_shop <ShopName>` | Will reset the stock of the specified `ShopName`, which usually happens at the start of each day. Useful for checking that your conditions are applying / stock is randomizing as you'd like'
 `list_shops` | Lists all of the `ShopName`s registered with Shop Tile Framework
 
 ## Contact The Dev
If you need to find me, the following methods are your best bets:
- Bug reports can be made by submitting an issue on this repositiory, or use the [bugs tab](https://www.nexusmods.com/stardewvalley/mods/5005?tab=bugs) on the Nexus page. Please provide a [log](https://smapi.io/log/) with all bug reports and as much information about the circumstances of the bug as possible.
- Suggestions should be submitted through an issue on this repository
- If you have questions that aren't answered here, you can DM me on discord at `Chronicler#9318`
