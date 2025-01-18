using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TrainStation.Framework.ContentModels;
using TrainStation.Framework.LegacyContentModels;

namespace TrainStation.Framework;

public class Api : IApi
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages the Train Station content provided by content packs.</summary>
    private readonly ContentManager ContentManager;

    /// <summary>Open the UI to choose a boat destination.</summary>
    private readonly Action OpenBoatMenuImpl;

    /// <summary>Open the UI to choose a train destination.</summary>
    private readonly Action OpenTrainMenuImpl;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="contentManager">Manages the Train Station content provided by content packs.</param>
    /// <param name="openBoatMenu">Open the UI to choose a boat destination.</param>
    /// <param name="openTrainMenu">Open the UI to choose a train destination.</param>
    internal Api(ContentManager contentManager, Action openBoatMenu, Action openTrainMenu)
    {
        this.ContentManager = contentManager;
        this.OpenBoatMenuImpl = openBoatMenu;
        this.OpenTrainMenuImpl = openTrainMenu;
    }

    /// <inheritdoc />
    public void OpenTrainMenu()
    {
        this.OpenTrainMenuImpl();
    }

    /// <inheritdoc />
    public void OpenBoatMenu()
    {
        this.OpenBoatMenuImpl();
    }

    /// <inheritdoc />
    public void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(this.ContentManager.LegacyTrainStops, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }

    /// <inheritdoc />
    public void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(this.ContentManager.LegacyBoatStops, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add a stop to the boat or train network, overwriting it by ID if needed.</summary>
    /// <param name="stops">The boat or train stops list to update.</param>
    /// <param name="stopId">A unique identifier for this stop. This should be prefixed with your mod's unique ID, like <c>YourModId_StopId</c>.</param>
    /// <param name="targetMapName">The internal name of the location to which the player should warp when they select this stop.</param>
    /// <param name="localizedDisplayName">The display name translations for each language.</param>
    /// <param name="targetX">The X tile position to which the player should warp when they select this stop.</param>
    /// <param name="targetY">The Y tile position to which the player should warp when they select this stop.</param>
    /// <param name="cost">The gold price to go to that stop.</param>
    /// <param name="facingDirectionAfterWarp">The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</param>
    /// <param name="conditions">If set, the Expanded P a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</param>
    /// <param name="translatedName">The default display name for the stop if not overridden by <paramref name="localizedDisplayName"/>, shown in the bus or train menu.</param>
    private void Register(List<StopModel> stops, string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        stops.RemoveAll(s => s.Id == stopId);

        stops.Add(
            LegacyStopModel.FromApi(
                id: stopId,
                toLocation: targetMapName,
                toTile: new Point(targetX, targetY),
                toFacingDirection: facingDirectionAfterWarp,
                cost: cost,
                conditions: conditions,
                displayNameTranslations: localizedDisplayName,
                displayNameDefault: translatedName
            )
        );
    }
}
