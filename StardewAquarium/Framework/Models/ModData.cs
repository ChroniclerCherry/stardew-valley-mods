using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewAquarium.Framework.Models;

public sealed class ModData
{
    public int LastDonatedFishCoordinateX { get; set; }
    public int LastDonatedFishCoordinateY { get; set; }

    public HashSet<string> ConversationTopicsOnDonate { get; set; }

    public float DolphinChance { get; set; }

    public Rectangle DolphinRange { get; set; }

    public int DolphinAnimationFrames { get; set; }
}
