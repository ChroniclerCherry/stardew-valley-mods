using Microsoft.Xna.Framework;

namespace StardewAquarium.Models
{
    public class ModData
    {
        public int LastDonatedFishCoordinateX { get; set; }
        public int LastDonatedFishCoordinateY { get; set; }
        public string ExteriorMapName { get; set; }

        public string[] ConversationTopicsOnDonate { get; set; }

        public float DolphinChance { get; set; }

        public Rectangle DolphinRange { get; set; }

        public int DolphinAnimationFrames { get; set; }
    }
}
