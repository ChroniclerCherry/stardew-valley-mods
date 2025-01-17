using System.Collections.Generic;
using System.Linq;

namespace TrainStation.Framework;

public class Api : IApi
{
    /*********
    ** Public methods
    *********/
    public void OpenTrainMenu()
    {
        ModEntry.Instance.OpenTrainMenu();
    }

    public void OpenBoatMenu()
    {
        ModEntry.Instance.OpenBoatMenu();
    }

    public void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName)
    {
        var stop = ModEntry.Instance.TrainStops.SingleOrDefault(s => s.StopId.Equals(stopId));
        if (stop == null)
        {
            stop = new TrainStop();
            ModEntry.Instance.TrainStops.Add(stop);
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
        var stop = ModEntry.Instance.BoatStops.SingleOrDefault(s => s.StopId.Equals(stopId));
        if (stop == null)
        {
            stop = new BoatStop();
            ModEntry.Instance.BoatStops.Add(stop);
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
