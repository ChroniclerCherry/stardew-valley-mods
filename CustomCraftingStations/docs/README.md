# Custom Crafting Stations

Custom Crafting Stations (CCS) is a framework mod that allows modders to create specialized crafting stations. These stations can be attached to a BigCraftable object, or to tiledata.

Recipes from these stations can be excluded from non-custom cooking/crafting menus, allowing access to recipes to be limited to only the custom crafting station.

Each custom stations can support either crafting recipes or cooking recipes (but not both at once).

## Compatibility

The android version of Stardew Valley uses a completely different crafting menu class, which means this mod needs to be almost completely rewritten to support it. Therefore, there are no current plans to make an android version.

Other mods that make custom crafting menus will be affected, because this mod removes certain recipes from vanilla menus and there's no way to tell if a menu was opened by vanilla or another mod. In the case of such conflicts, please make a bug report and I will see what I can do.

While not reconmended to use bigcraftables that already have an action attached ( things like the workbench, or machines edited with Producer Framework Mod ) nothing will stop you from doing so. Right click/action button will open up thecustom menu, while left click/use tool button will trigger the normal left click action

**Compatibility has been added for:**
* Remote Fridge Storage

## Content Pack

Content packs are created by targetting `Cherry.CustomCraftingStations` in the [manifest.json](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest)

The stations themselves are specified in a `content.json` in the following format:

```js
{
	"CraftingStations": [
	{
		"BigCraftable": "BigCraftableName",
		"TileData": "StationName",
		"ExclusiveRecipes": true,
		"CraftingRecipes": ["recipe1","recipe2"],
		"CookingRecipes": ["recipe1","recipe2"]
	},
	]
}
```

You can have as many CraftingStations as you want

Field | Optional | Format | Description
------------ | ------------- | ------------- | -------------
BigCraftable | Optional | string | The name of a bigcraftable where the player can open the menu*
TileData | Optional | string | The name of the Tiledata where the player can open the menu*
ExclusiveRecipes | Optional | boolean | Whether recipes from this station will be removed from vanilla menus or not. Defaults to true if not specified
CraftingRecipes | Optional | list of strings | A list of the name of crafting recipes available at this station**
CookingRecipes | Optional | list of strings | A list of the name of cooking  recipes available at this station **

\* You can specify both if you wish to attach the station to both a bigcraftable and tiledata.

\** You should specify one or the other, not both.

BigCraftables can be vanilla, but most likely you'll want to add custom ones.

For Tiledata, they should be an `Action` tile on the `Buildings` layer with a format of `CraftingStation <StationName>` with StationName being what was put into the `TileData` field of the content.json

![Example of what the TileData looks like as described above in Tiled](https://imgur.com/9ZiQxyM.png)

## API
There are plans to add an API, to allow other mods to programatically open crafting menus through this mod. With optional dependency set up properly, this should help with incompatibility issues in the future.

## See also
* [Release notes](release-notes.md)
