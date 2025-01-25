using System;
using System.Collections.Generic;
using StardewValley;
using TrainStation.Framework.ContentModels;

namespace TrainStation.Framework;

public class Api : IApi
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages the available boat and train stops.</summary>
    private readonly StopManager StopManager;

    /// <summary>Open the UI to choose a boat (true) or train (false) destination.</summary>
    private readonly Action<bool> OpenMenu;

    /// <summary>The name of the mod calling the API.</summary>
    private readonly string FromModName;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="stopManager">Manages the available boat and train stops.</param>
    /// <param name="openMenu">Open the UI to choose a boat (true) or train (false) destination.</param>
    /// <param name="fromModName">The name of the mod calling the API.</param>
    internal Api(StopManager stopManager, Action<bool> openMenu, string fromModName)
    {
        this.StopManager = stopManager;
        this.OpenMenu = openMenu;
        this.FromModName = fromModName;
    }

    /// <inheritdoc />
    public void OpenTrainMenu()
    {
        this.OpenMenu(false);
    }

    /// <inheritdoc />
    public void OpenBoatMenu()
    {
        this.OpenMenu(true);
    }

    /// <inheritdoc />
    public void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(false, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }

    /// <inheritdoc />
    public void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(true, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add a stop to the boat or train network, overwriting it by ID if needed.</summary>
    /// <param name="isBoat">Whether this is a boat stop; else it's a train stop.</param>
    /// <param name="stopId">A unique identifier for this stop. This should be prefixed with your mod's unique ID, like <c>YourModId_StopId</c>.</param>
    /// <param name="targetMapName">The internal name of the location to which the player should warp when they select this stop.</param>
    /// <param name="localizedDisplayName">The display name translations for each language.</param>
    /// <param name="targetX">The X tile position to which the player should warp when they select this stop.</param>
    /// <param name="targetY">The Y tile position to which the player should warp when they select this stop.</param>
    /// <param name="cost">The gold price to go to that stop.</param>
    /// <param name="facingDirectionAfterWarp">The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</param>
    /// <param name="conditions">If set, the Expanded P a game state query which indicates whether this stop should appear in the menu at a given time. The contextual location is set to the player's current location.</param>
    /// <param name="translatedName">The default display name for the stop if not overridden by <paramref name="localizedDisplayName"/>, shown in the bus or train menu.</param>
    private void Register(bool isBoat, string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        List<StopModel> stops = this.StopManager.CustomStops;

        stops.RemoveAll(s => s.Id == stopId);
        stops.Add(
            StopModel.FromData(
                id: stopId,
                targetMapName: targetMapName,
                targetX: targetX,
                targetY: targetY,
                facingDirectionAfterWarp: facingDirectionAfterWarp,
                cost: cost,
                conditions: this.StopManager.ValidateExpandedPreconditionsInstalledIfNeeded(conditions, this.FromModName),
                isBoat: isBoat,
                displayNameTranslations: localizedDisplayName,
                displayNameDefault: translatedName
            )
        );
    }
}
