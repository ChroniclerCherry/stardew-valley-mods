using Microsoft.Xna.Framework;

namespace StardewAquarium.Models
{
    public class ModData
    {
        public int LastDonatedFishCoordinateX { get; set; }
        public int LastDonatedFishCoordinateY { get; set; }
        public string ExteriorMapName { get; set; }

        public string[] ConversationTopicsOnDonate { get; set; }

        public float SeaMonsterChance { get; set; }

        public Rectangle SeaMonsterRange { get; set; }

    }
}
