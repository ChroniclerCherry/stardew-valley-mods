# Expanded Preconditions Utility
Event preconditions is a commonly used conditions system across SMAPI mods due to the fact it already exists in game and thus allows a large number of conditions to be used with very little additional work. However, it can be limited in what it offers. This mod seeks to provide a conditions system in the same format as event preconditions, but with additional custom conditions. [More will be added over time as people make requests](https://github.com/ChroniclerCherry/stardew-valley-mods/issues/8)

## Syntax
EPU takes conditions in the same syntax as event preconditions do. Each condition is seperated by `/` in a string. EPU also allows you to check the opposite of any individual condition by adding a `!` at the very start

For example: `d Mon Fri/HasItem Pink Cake/!JojaMartComplete/!w rainy` checks that it is not Monday or Friday, that the user has a Pink Cake in their inventory, that the Jojamart is not complete, and that it is not rainy. Vanilla preconditions and custom conditions can be freely mixed in any order

Depending on the mod that uses EPU, conditions may be given as a single string like the above, or as an array of strings. When it is an array of strings, more flexibility is allowed as you can check OR conditions. For example:
```cs
[
        "!z spring/t 600 1000", //true if it's `During Spring` AND `The time is between 6AM to 10AM`
        //OR
        "f Linus 1000/w rainy/z spring" //true if `Player has at least 1000 friendship points with Linus' AND 'It is rainy` AND `It's not Spring`,
        //OR
        "f Linus 2500" //true if `Player has at least 2500 friendship points with Linus`
]
```

## Available Conditions

All [event preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions) are available, as well as:

Syntax | Description | Example
------------- | ------------- | -------------
`NPCAt <s:NPCName> [<i:x> <i:y>]` | This will check if the named NPC is at the given tile coordinates on the current map. Multiple x/y coordinates can be given, and will return true if the NPC is at any of them. | `NPCAt Pierre 5 10 5 11 5 12` will check if Pierre is at (5,10) (5,11) or (5,12)
`HasMod [<s:UniqueID>]` | This will check if the given Unique ID of certain mods is installed. Multiple can be supplied and will return true only if the player has all of them installed. | `HasMod Cherry.CustomizeAnywhere Cherry.PlatonicRelationships` returns true if both Customize Anywhere and Platonic Relationships are installed
`SkillLevel [<s:SkillName> <i:SkillLevel>]` | This will check if the player has at least the given skill level for named skills. Multiple skill-level pairs can be provided, and returns true if all of them are matched. Valid skills are: `combat`, `farming`, `fishing`, `foraging`, `luck` (unused in vanilla), and `mining` | `SkillLevel farming 5 fishing 3` Would return true if the player has at least level 5 farming and level 3 fishing
`CommunityCenterComplete` | Returns true if the Community center is completed on this save file| 
`JojaMartComplete` | Returns true if the joja mart route was completed on this save file |
`SeededRandom <i:offset> <i:timeInterval/s:timeInterval> <f:random lower bounds> <f: random upper bounds>`| Used to make synchronized random checks, which can be used across different checks/mods and remain constant over given periods of time | `SeededRandom 123 Season 0.5 1` [Find more detailed explanation here](#seeded-random)
`HasCookingRecipe [<s:recipe name>]` | Returns true if the player has learned all the listed recipes. **Note** spaces should be replaced with `-` | `HasCookingRecipe Fried-Egg Salad` will return true if the player knows how to cook both Fried Egg and salad
`HasCraftingRecipe [<s:recipe name>]` | Returns true if the player has learned all the listed recipes. **Note** spaces should be replaced with `-` | `HasCraftingRecipe Oil-Maker` will return true if the player knows how to craft Oil Makers
`FarmHouseUpgradeLevel [<i:house upgrade levels>]` | Returns true if the player's current house levels matches any of the given numbers. Starter house is 0 and cellar is 3 | `FarmHouseUpgradeLevel 2 3` will return true if the player is on the final house upgrade and has the cellar
`HasItem <s:item name>` | Returns true if the given item is in the player's inventory | `HasItem Pink Cake` would return true if any item named `Pink Cake` was found in the player inventory

# Using EPU in your project
**This section is for programmers looking to add EPU to their SMAPI mod, not users writing conditions for a content pack.**

Copy the [IConditionsChecker](#/IConditionsChecker.cs) interface into your project, and then in SMAPI's GameLaunched event, register and initialize the API.

```cs
            IConditionsChecker ConditionsApi = Helper.ModRegistry.GetApi<IConditionsChecker>("Cherry.ExpandedPreconditionsUtility");
            bool verboseMode = true; //reconmended you tie this to a config, or turn it to false upon release to prevent console spam
            ConditionsApi.Initialize(verboseMode, this.ModManifest.UniqueID);
```
After that, you're set to go. The API provides two ways to check conditions.

One takes an array of strings: `bool CheckConditions(string[] conditions);` This one is the full intended use designed for Shop Tile Framework, allowing a combination of AND and OR combination of conditions. Each string resolves to true if every condition is met, while the entire array of strings resolves to true if at least one string does

The second one is easier to add into projects that take single strings for event preconditions: `bool CheckConditions(string conditions);` This will resolve to true if all conditions given is met but does not allow for OR clauses

## Complex conditions
Most conditions are fairly straightforward, but some can be difficult to grasp at first. This section will attempt to explain and give examples for those

---------------------
### Seeded Random
 
 SeededRandom is an available condition that can be useful for synchronizing randomness across different condition checks.

 Essentially it rolls a random number for the given time period ( for example, once a week ) and then that random number can be checked against a range to be true or false. This number stays for the entire time period until it is rerolled at the start of the next period
 
 The structure of the condition looks like this:
 
 `SeededRandom <i:offset> <i:timeInterval/s:timeInterval> <f:random lower bounds> <f: random upper bounds>`
 
 Broken down:
 Parameter | Type | Description
 ---------|--------|--------
 offset | integer | Can be any number-- two conditions both using this number and the same timeInterval will receive the same random roll
 timeInterval | integer or a string | How long a seeded random roll lasts in days. This also supports entering certain strings as a shortcut.
 random lower/upper bounds | decimal number | This is the range the random number needs to be in to return a true. The lower bound is inclusive and the upper bound is exclusive
 
##### timeInterval strings

There's strings to provide a more readable shortcut for most use cases. If you want a different interval, for example every 3 days, then you'll be entering the timeInterval as an integer instead

* `Day` = 1, `Week` = 7, `Month` = `Season` = 28, `Year` = 112

* `Game` = 0, which will allow the same seed for the entire game regardless of time, allowing for save-game unique conditions

#### Examples

Let's say you wanted something of having a 50/50 chance happening each week, and for that to persist for the whole week. You would take the two scenarios you want, setting one of the conditions to `SeededRandom 22 Week 0.5 1.0` and the second one to `SeededRandom 22 Week 0 0.5` This means a random number is generated between 0 and 1 each week. If it is between 0 and 0.5 the first one resolves to true and the second one resolves to false and vice versa. This number will remain for the entire week, until it is rerolled again after 7 days

---------------------

## See also
* [Release notes](release-notes.md)
