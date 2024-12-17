using System.Collections.Generic;

namespace StardewAquarium.Framework.Models;

public sealed class ModData
{
    public int LastDonatedFishCoordinateX { get; set; }
    public int LastDonatedFishCoordinateY { get; set; }

    public HashSet<string> ConversationTopicsOnDonate { get; set; }
}
