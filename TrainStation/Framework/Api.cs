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
        var stop = this.ContentManager.TrainStops.SingleOrDefault(s => s.StopId.Equals(stopId));
        if (stop == null)
        {
            stop = new TrainStop();
            this.ContentManager.TrainStops.Add(stop);
        }

        stop.StopId = stopId;
        stop.TargetMapName = targetMapName;
        stop.LocalizedDisplayName = localizedDisplayName;
        stop.TargetX = targetX;
        stop.TargetY = targetY;
        stop.Cost = cost;
        stop.FacingDirectionAfterWarp = facingDirectionAfterWarp;
        stop.Conditions = conditions;
        stop.TranslatedName = translatedName;
    }

    public void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        var stop = this.ContentManager.BoatStops.SingleOrDefault(s => s.StopId.Equals(stopId));
        if (stop == null)
        {
            stop = new BoatStop();
            this.ContentManager.BoatStops.Add(stop);
        }

        stop.StopId = stopId;
        stop.TargetMapName = targetMapName;
        stop.LocalizedDisplayName = localizedDisplayName;
        stop.TargetX = targetX;
        stop.TargetY = targetY;
        stop.Cost = cost;
        stop.FacingDirectionAfterWarp = facingDirectionAfterWarp;
        stop.Conditions = conditions;
        stop.TranslatedName = translatedName;
    }
}
