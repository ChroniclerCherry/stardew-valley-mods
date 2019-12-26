#Shop Tile Framework

## Navigation
- [Intro](#intro)
- [Install](#install)
- [Create a Content Pack](#create-a-content-pack)
- [Adding store to the game] (#adding-store-to-the-game)

## Intro

**This page is a resource for modders. If you're a player, check out this link instead**

Shop Tile Framework is a tool for Modders to define stores and attach them to tile properties which can be loaded anywhere into the game with another method. Content packs need a shops.json to define their shops

Stores can be opened with a custom tile property of "Shop" and a value of the ShopName defined in shops.json

## Install
//TODO

## Create a content pack
//TODO


Field | Optional | Description
------------ | ------------- | -------------
Shops | N | You can add as many shops as you want, as long as they have unique `ShopName` 

Each Shop contains:

Field | Optional | Description
------------ | ------------- | -------------
ShopName | N | The name of the shop is the value of the tile property used to open this shop in-game. It must be unique among all downloaded mods.
PortraitPath | Y | The relative path to the image used as the portrait for this shop from the content pack's folder. If not provided, no portrait will be drawn
Quote | Y | A quote displayed on the shop menu screen. If not provided, no quote will appear
ShopPrice | Y | Sets the price of every item in the store to this if set.
MaxNumItemsSoldInStore | Y | The number of different items available. If there is more items within all the `ItemStocks` than this number, they will be randomly picked at the beginning of each day so that the total number of items match this.
ItemStocks | N | The items sold at this store. Each `ItemStocks` can contain one or more item of a single type


Each ItemStock contains:

Field | Optional | Description
------------ | ------------- | -------------
ItemType | N |  A String that determines what kind of Object this ItemStock contains, necessary to find the right unique items.
StockPrice | Y | Sets the price for all items in this ItemStock. Overrides ShopPrice. If neither price fields are given, default item sell prices are used
ItemIDs | Y/N | A list of items by their IDS. One of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
ItemNames | Y/N | A list of items by their internal names. One of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item.
JAPacks | Y/N | A list of items by the JA pack they came from. One of `ItemIDs`,`ItemNames` or `JAPacks` is needed in order to add an item. **This is not yet implemented**
Stock | Y | How many of each item is available to buy per day. If not set, the stock is unlimited
MaxNumItemsSoldInItemStock | Y | The number of different items available from this ItemStock. If there are more items in this ItemStock than `MaxNumItemsSoldInItemStock` a random set will be picked per day.
When | Y | A condition for the items in this ItemStock to appear. Currently takes [valid event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions)

Possible `ItemType` determine which game file the item data is obtained from.

ItemType | Source
------------ | -------------
"Object" | `data/ObjectInformation.json`
"BigCraftable" | `data/BigCraftablesInformation.json`
"Clothing" | `data/ClothingInformation.json`
"Hat" | `data/hats.json`
"Boots" | `data/Boots.json`
"Furniture" | `data/Furniture.json`
"Weapon" | `data/weapons.json`
"Fish" | `data/Fish.json`

## Example
Example shops.json with all available options:
```json
{
  "Shops": [
    {
      "ShopName": "MyShop",
      "PortraitPath": "assets/Portrait.png",
      "Quote": "This is a store!",
      "ShopPrice": 1000,
      "MaxNumItemsSoldInStore": 20,
      "ItemStocks": [
		{
          "ItemType": "Object",
          "StockPrice": 100,
          "ItemIDs": [
            60,74,90
          ],
          "ItemNames": [
			"Earth Crystal",
			"Pizza",
          ],
		  "JAPacks": [
			"ppja.ancientcrops",
			"ppja.fruitsandveggies"
          ],
          "Stock": 10,
          "MaxNumItemsSoldInItemStock": 50,
          "When": [
          	"u 20"
          ]
        },
      ]
    },
  ]
}
```

## Adding store to the game

The store defined in the above json can be opened by clicking on a tile with the following properties on the **Buildings** layer:

![Example tile properties](https://media.discordapp.net/attachments/305520470114172928/659874803498614795/unknown.png)

The empty `Action` Property is optional; it just changes the appearance of the game cursor when hovering over a shop to make it clear that it is interactable

These tile properties can be loaded into the game with any other method usually used to load in maps. Content Patcher, TMXL, or SMAPI mods can all add the property along with the shop itself. More info about modding maps can be found [here](https://stardewvalleywiki.com/Modding:Maps)
