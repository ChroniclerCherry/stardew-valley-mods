# Shop Tile Framework

## Navigation
- [Intro](#intro)
- [Create a Content Pack](#create-a-content-pack)
- [Example](#example)
- [Adding store to the game](#adding-store-to-the-game)
- [Console Commands](#console-commands)

## Intro

**This page is a resource for modders to create a custom shop. If you're a player, check out [this link](https://www.nexusmods.com/stardewvalley/mods/5005) instead**

Shop Tile Framework is a tool for Modders of the game Stardew Valley, which allows you to define stores via a shops.json file. These stores are attached to tile properties which can be loaded anywhere into the game with another method. Content packs need a `shops.json` to define their shops

Stores can be opened with a custom tile property of "Shop" and a value of the ShopName defined in `shops.json`

## Create a content pack
To make a content pack for Shop Tile Framework, add `Cherry.ShopTileFramework` to the `ContentPackFor` section of your mod's [manifest file](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest).

Then from there, you need to make a `shops.json` file to define the properties of your shops. You can find an example [here](#adding-store-to-the-game), and each field is described below:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
Shops | N | An array of Shops | You can add as many shops as you want, as long as they have unique `ShopName` 

Each Shop contains:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ShopName | N | string | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
StoreCurrency | Y | string | The currency this store uses. Defaults to `"Money"` if not specified, but can also be `"festivalScore"` or `"clubCoins"`
PortraitPath | Y | string | The relative path to the image used as the portrait for this shop from the content pack's folder. If not provided, no portrait will be drawn
Quote | Y | string | A quote displayed on the shop menu screen. If not provided, no quote will appear
ShopPrice | Y | int | Sets the price of every item in the store to this if set.
MaxNumItemsSoldInStore | Y | int | The number of different items available. If there is more items within all the `ItemStocks` than this number, they will be randomly picked at the beginning of each day so that the total number of items match this.
ItemStocks | N | An array of `ItemStocks` | The items sold at this store. Each `ItemStocks` can contain one or more item of a single type


Each ItemStock contains:

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
ItemType | N | string |  Determines what kind of Object this ItemStock contains, necessary to find the right unique items.
StockPrice | Y | int | Sets the price for all items in this ItemStock. Overrides ShopPrice. If neither price fields are given, default item sell prices are used
StockItemCurrency | Y | string | You can specify an Object by name as the currency instead. This will charge both the specified item as well as the `StoreCurrency` unless the price is set to 0.
StockCurrencyStack | Y | int | The number of the `StockItemCurrency` it costs for each item. Defaults to 1
ItemIDs | Y/N | Array of ints | Adds a list of items by their IDS. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
ItemNames | Y/N | Array of strings | Adds a list of items by their internal names. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
JAPacks | Y/N | Array of strings | Adds all items of `ItemType` from the specified JA Packs, identified by their `UniqueID`. Crops and trees will sell their seeds/saplings. If you want to sell the produce themselves, they should be specified in the `ItemNames` section instead. One or more of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
Stock | Y | int | How many of each item is available to buy per day. If not set, the stock is unlimited
MaxNumItemsSoldInItemStock | Y | int | The number of different items available from this ItemStock. If there are more items in this ItemStock than `MaxNumItemsSoldInItemStock` a random set will be picked per day.
When | Y | Array of strings | A condition for the items in this ItemStock to appear. Currently takes all valid vanilla [event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). **Warning:** Avoid checks like `t` and `a` as they only check conditions at the start of the day, not when the user opens the shop menu

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

## Example
Example shops.json with all available options:
```json
{
  "Shops": [
    {
      "ShopName": "MyShop",
      "StoreCurrency": "festivalScore",
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
  ]
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

## Adding store to the game

The store defined in the above json can be opened by clicking on a tile with the following properties on the **Buildings** layer:

![Example tile properties](https://media.discordapp.net/attachments/305520470114172928/659874803498614795/unknown.png)

The empty `Action` Property is optional; it just changes the appearance of the game cursor when hovering over a shop to make it clear that it is interactable

These tile properties can be loaded into the game with any other method usually used to load in maps. Content Patcher, TMXL, or SMAPI mods can all add the property along with the shop itself. More info about modding maps can be found [here](https://stardewvalleywiki.com/Modding:Maps)

## Console Commands

A few console commands are added to SMAPI in order to help with debugging. Type `help` in the console to get a full list of available commands

Command | Description
------------ | -------------
 `open_shop <ShopName>` | Will open up the shop with the specified `ShopName`. Useful for testing without adding in a tile property / needing to go to the shop location
 `reset_shop <ShopName>` | Will reset the stock of the specified `ShopName`, which usually happens at the start of each day. Useful for checking that your conditions are applying / stock is randomizing as you'd like'
 `list_shops` | Lists all of the `ShopName`s registered with Shop Tile Framework
