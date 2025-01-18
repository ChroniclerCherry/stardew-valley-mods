**Expanded Preconditions Utility** lets you use [event precondition syntax](https://stardewvalleywiki.com/Modding:Event_data)
in game state queries or custom code, and extends them with new preconditions.

## Contents
* [Introduction](#introduction)
  * [Usage](#usage)
  * [Condition syntax](#condition-syntax)
* [Available conditions](#available-conditions)
  * [Basic preconditions](#basic-preconditions)
  * [CommunityCenterComplete & JojaMartComplete](#communitycentercomplete-jojamartcomplete)
  * [FarmHouseUpgradeLevel](#farmhouseupgradelevel)
  * [HasCookingRecipe & HasCraftingRecipe](#hascookingrecipe-hascraftingrecipe)
  * [HasItem](#hasitem)
  * [HasMod](#hasmod)
  * [NPCAt](#npcat)
  * [SkillLevel](#skilllevel)
  * [_(Advanced)_ SeededRandom](#advanced-seededrandom)
* [For framework mod programmers](#for-framework-mod-programmers)
* [See also](#see-also)

## Introduction
### Usage
You can use extended preconditions in content pack fields that specifically support them (e.g. Shop Tile Framework).
Depending on the framework mod consuming the content pack, conditions may be given as a single string:
```js
// all listed conditions must be true
"Conditions": "season spring/time 600 1000" // (season is spring) AND (time is between 6am to 10am)
```
Or as an array of possible conditions:
```js
// at least one of the strings must be true
"Conditions": [
    "season spring/time 600 1000", // (season is spring) AND (time is between 6am to 10am)
    "friendship Linus 2500"        // OR (player has 2500+ friendship points with Linus)
]
```

### Condition syntax
Expanded Preconditions Utility uses the same syntax as [event preconditions](https://stardewvalleywiki.com/Modding:Event_data).
Each condition is separated by `/` in a string, and you can prefix a precondition with `!` to check the opposite of the
precondition.

For example:
```js
// true if (Monday or Friday) AND (player has a PinkCake in their inventory) AND (it's not raining)
"Condition": "DayOfWeek Mon Fri/hasItem Pink Cake/!weather rainy"
```

You can freely mix vanilla preconditions and extended ones, in any order.

## Available conditions
### Basic preconditions
You can use all [event preconditions](https://stardewvalleywiki.com/Modding:Event_data) provided by the game, using
either the short alias (like `i`) or full name (like `HasItem`).

### CommunityCenterComplete & JojaMartComplete
Syntax: `CommunityCenterComplete` _or_ `JojaMartComplete`

Check if the community center or JojaMart has been completed in this save.

For example:
```js
"Condition": "CommunityCenterComplete"
```

### FarmHouseUpgradeLevel
Syntax: `FarmHouseUpgradeLevel [<i:house upgrade levels>]+`

Check if the player has the specified upgrade level. If you list multiple levels, the condition is true if the player
has _any_ of them.

The possible values are 0 (initial farmhouse), 1 (adds kitchen), 2 (add children's bedroom), and 3 (adds cellar).

For example:
```js
// player has house upgrade 2 OR 3
"Condition": "FarmHouseUpgradeLevel 2 3"
```

### HasCookingRecipe & HasCraftingRecipe
Syntax: `HasCookingRecipe [<s:recipe name>]+` _or_ `HasCraftingRecipe [<s:recipe name>]+`

Check if the player has learned the given cooking or crafting recipe. Spaces in recipe names should be replaced with
`-`. If you list multiple recipes, the condition is true if _all_ of them learned.

For example:
```js
// player can make Cookies AND Pink Cake
"Condition": "HasCookingRecipe Cookies Pink-Cake"
```

### HasItem
Syntax: `HasItem <s:item name>`

Check if the player has an item with the given name in their inventory.

For example:
```js
// player has a Pink Cake in their inventory
"Condition": "HasItem Pink Cake"

### HasMod
Syntax: `HasMod [<s:UniqueID>]+`

Check if a mod is installed, given the unique ID from its `manifest.json` file. If you list multiple IDs, the condition
is true if _all_ of them are installed.

For example:
```js
// Customize Anywhere AND Platonic Relationships are installed
"Condition": "HasMod Cherry.CustomizeAnywhere Cherry.PlatonicRelationships"
```

### NPCAt
Syntax: `NPCAt <s:NPCName> [<i:x> <i:y>]+`

Check if the named NPC is at a tile position in the current location. If you list multiple coordinates, the condition
is true if the NPC is at any of them.

For example:
```js
// Pierre is at (5,10), (5,11), or (5,12)
"Condition": "NPCAt Pierre 5 10 5 11 5 12"
```

### SkillLevel
Syntax: `SkillLevel [<s:SkillName> <i:SkillLevel>]+`

Check if the player has at least the given skill level. If you list multiple skill/level pairs, the condition is true
if _all_ of them are marched. The valid skill names are `combat`, `farming`, `fishing`, `foraging`, `luck`, and `mining`.

For example:
```js
// player is farming level 5+ AND fishing level 3+
"Condition": "SkillLevel farming 5 fishing 3"
```

### _(Advanced)_ SeededRandom
Syntax: `SeededRandom <i:offset> <i:timeInterval/s:timeInterval> <f:random lower bounds> <f: random upper bounds>`

Perform a random check which can be synchronized across different checks/mods and remain constant over a given period
of time.

Essentially it rolls a random number for the given time period (e.g. once per week), then checks that number a range to
be true or false. This number stays consistent for the entire time period until it's rerolled at the start of the next
one.
 
The condition has for arguments:

parameter    | type    | description
------------ | ------- | --------
offset       | integer | Can be any number. Two conditions using the same `offset` and `timeInterval` will receive the same random roll.
timeInterval | integer _or_ string | How long a seeded random roll lasts. You can specify...<ul><li>a number of days;</li><li>one of these periods: `Day` (1), `Week` (7), `Month` or `Season` (28), and `Year` (112);</li><li>or `Game` to use the same seed for the entire game regardless of time, allowing for save-game unique conditions.</li></ul>
random lower/upper bounds | decimal number | The range the random number needs to be in to return true. The lower bound is inclusive and the upper bound is exclusive.
 
For example, let's say you want two things with a mutually-exclusive 50% chance of happening each week. You can give
them the same offset and time interval, and shift their random upper/lower bounds:

```js
// true if the RNG chooses between 0.5 and 1
"Condition": "SeededRandom 22 Week 0.5 1.0",

// true if the RNG chooses between 0 and 0.5
"Condition": "SeededRandom 22 Week 0 0.5"
```

## For framework mod programmers
When writing a SMAPI (C#) mod, you can add support for extended preconditions like this:

1. Copy the [IConditionsChecker](#/IConditionsChecker.cs) interface into your project.
1. In SMAPI's `GameLaunched` event, register and initialize the API:
   ```cs
   IConditionsChecker ConditionsApi = Helper.ModRegistry.GetApi<IConditionsChecker>("Cherry.ExpandedPreconditionsUtility");
   bool verboseMode = true; //reconmended you tie this to a config, or turn it to false upon release to prevent console spam
   ConditionsApi.Initialize(verboseMode, this.ModManifest.UniqueID);
   ```
3. Call the API to check preconditions.

   You can process an array of string conditions. This is the full intended use designed for Shop Tile Framework,
   allowing a combination of AND and OR combination of conditions. Each string resolves to true if every condition is
   met, while the entire array of strings resolves to true if at least one string does.
   ```cs
   string[] conditions = [ ... ];
   bool isMatch = conditionsChecker.CheckConditions(conditions);
   ```

   Or you can process a single string for event preconditions. This will resolve to true if all conditions given are
   met, but does not allow for OR clauses.
   ```cs
   string conditions = "...";
   bool isMatch = conditionsChecker.CheckConditions(conditions);
   ```

## See also
* [Release notes](release-notes.md)
