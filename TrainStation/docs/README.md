**Train Station** lets players travel to other mods' destinations by boat or train by interacting with ticket machines
at the railroad, Willy's shop, or optionally other mods' locations.

Mod authors can add custom destinations with optional ticket prices, and can add an automatic rail station to their own
maps using a map property.

## Contents
* [For players](#for-players)
  * [Buy a ticket](#buy-a-ticket)
  * [Compatibility](#compatibility)
* [For mod authors](#for-mod-authors)
  * [Add a destination](#add-a-destination)
  * [Add a train station](#add-a-train-station)
* [See also](#see-also)

## For players
### Buy a ticket
To take the train, walk to the railroad and buy a ticket from the machine next to the station:  
![](train-station.png)

To take the boat, enter [Willy's back room](https://stardewvalleywiki.com/Fish_Shop#Willy.27s_Boat)
and interact with the ticket machine there once it's been repaired:  
![](boat-dock.png)

You can also interact with ticket machines in various mod locations.

### Compatibility
The ticket station and the warp to the railroad may end up somewhere invalid if a map mod changes the shape of the map.
If so, you can adjust those coordinates in the `config.json` file:
```js
{
  "TicketStationX": 32,
  "TicketStationY": 40,
  "RailroadWarpX": 32,
  "RailroadWarpY": 42
}
```

## For mod authors
### Add a destination
To add a destination to the train stops menu:

1. [Create a content pack](https://stardewvalleywiki.com/Modding:Content_packs#Create_a_content_pack). For the
   `ContentPackFor` field, use `Cherry.TrainStation`.
2. Create a `TrainStops.json` file in your content pack folder with this content:

   ```js
   {
       "TrainStops": [
           {
               "TargetMapName": "Town",
               "LocalizedDisplayName": {
                   "en": "Town",
                   "zh":"镇"
               },
               "TargetX": 10,
               "TargetY": 10,
               "Cost": 500,
               "FacingDirectionAfterWarp": 2,
               "Conditions": [ "z winter/e 100", "HasMod ACoolMod.UniqueID" ]
           }
       ],
       "BoatStops": [
           // exact same fields as a train stop
       ]
   }
   ```
3. Edit the data accordingly. You can list any number of boat or train stops in the same file.

The available fields for a boat or train stop are:

field name             | usage
---------------------- | -----
`TargetMapName`        | The internal name of the location to which the player should be warped to. You can see internal location names in-game using [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679).
`TargetX`<br />`TargetY` | The tile position to which the player should be warped to. You can see tile coordinates in-game using [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679).
`LocalizedDisplayName` | The translated display name of the destination for each [language code](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation#i18n_folder). If there's no translation for the player's current language, it'll use the `en` entry.
`FacingDirectionAfterWarp` | _(Optional)_ The direction the player should face after warping. The possible values are `0` (up), `1` (right), `2` (down), or `3` (left). Default down.
`Cost`                 | _(Optional)_ The gold price to purchase a ticket. Default free.
`Conditions`           | _(Optional)_ If set, the conditions which must be met for the destination to appear in the menu. You can use the [Expanded Preconditions Utility format](https://github.com/ChroniclerCherry/stardew-valley-mods/blob/Develop/ExpandedPreconditionsUtility/README.md).

### Add a ticket machine
To add a train ticket machine to a custom map:

1. Add the desired sprites to the map (e.g. the ticket machine).
2. Add an `Action: BoatTicket` or `Action: TrainStation` [map property](https://stardewvalleywiki.com/Modding:Maps) on
   the `Buildings` layer where the player can activate it.

When the player clicks the tile with the `Action` property, they'll see the UI to choose a boat or ticket destination.
The menu will automatically hide destinations in their current location.

## See also
* [Release notes](release-notes.md)
