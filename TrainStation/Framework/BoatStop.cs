using System.Collections.Generic;

namespace TrainStation.Framework
{
    public class BoatStop
    {
        public string TargetMapName { get; set; }
        public Dictionary<string, string> LocalizedDisplayName { get; set; }

        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public int Cost { get; set; } = 0;
        public int FacingDirectionAfterWarp { get; set; } = 2;
        public string[] Conditions { get; set; }

        internal string StopID; //assigned by the mod's uniqueID and the number of stops from that pack
        internal string TranslatedName;
    }
}
