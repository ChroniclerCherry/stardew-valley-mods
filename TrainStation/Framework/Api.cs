using System;
using System.Collections.Generic;
using System.Linq;

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

    public void OpenTrainMenu()
    {
        this.OpenTrainMenuImpl();
    }

    public void OpenBoatMenu()
    {
        this.OpenBoatMenuImpl();
    }

    public void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(this.ContentManager.TrainStops, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }

    public void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        this.Register(this.ContentManager.BoatStops, stopId, targetMapName, localizedDisplayName, targetX, targetY, cost, facingDirectionAfterWarp, conditions, translatedName);
    }


    /*********
    ** Private methods
    *********/
    private void Register(List<StopContentPackModel> stops, string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        var stop = this.ContentManager.BoatStops.SingleOrDefault(s => s.Id == stopId);
        if (stop == null)
        {
            stop = new StopContentPackModel();
            stops.Add(stop);
        }

        stop.Id = stopId;
        stop.DisplayName = translatedName;
        stop.LocalizedDisplayName = localizedDisplayName;
        stop.TargetMapName = targetMapName;
        stop.TargetX = targetX;
        stop.TargetY = targetY;
        stop.FacingDirectionAfterWarp = facingDirectionAfterWarp;
        stop.Cost = cost;
        stop.Conditions = conditions;
    }
}
