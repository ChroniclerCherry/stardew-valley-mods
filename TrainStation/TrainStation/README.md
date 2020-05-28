# Train Station

Train Station is a mod that transforms the train station at the railroad into a working train station. A ticket booth is added that allows players to pay a fee to travel to other train stations set by modders.

Heavily inspired by the mod Bus Locations which does something similar but with the Bus.

This mod does more by not only adding destinations to the rail station, but also allow the placement of more rail stations via a map property. In this way, it's like a second minecart system.

## Adding a train station
An `Action` property with a value of `TrainStation` on the `Buildings` lair of the map will becomes an interactable point to bring up the menu showing available destinations. Tile properties can be added in through various methods such as Content patcher, TMXL Map Toolkit, or directly through code.

The menu will not show any destinations on the current map or those that do not meet the specified conditions

## Adding a destination

To add a destination to the train stops menu, make a content pack with a `TrainStops.json` targetting `Cherry.TrainStation`

```js
{
  "TrainStops": [
    {
      "TargetMapName": "Town", //the internal name of the map the player will be warped to
      "LocalizedDisplayName": { //The display name of the destination. Will default to english if no translations are found for the current language
        "en": "Town",
        "zh":"镇"
      },
      "TargetX": 10, //the X of where the player willl be warped to
      "TargetY": 10, //the Y of where the player willl be warped to
      "Cost": 500, //how much gold it costs to buy this ticket
      "FacingDirectionAfterWarp":-2, //Direction the player faces when they arrived. 0 is up, 1 is right, 2 is down, 3 is left. Defaults to 2
      "Conditions": "z winter/e 100"; //Conditions for the destination to be available using event preconditions
    },
  ]
}
```

You can have have multiple trainstops definied in each `TrainStops.json`. To see vailable event preconditions, check out the [wiki](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions)



## Compatibility
The ticket station and the warp to the railroad may end up somewhere invalid if a map mod changes the shape of the map. In such cases, those coordinates can be adjusted in the Config.json

```js
{
  "TicketStationX": 32,
  "TicketStationY": 40,
  "RailroadWarpX": 32,
  "RailroadWarpY": 42
}
```
