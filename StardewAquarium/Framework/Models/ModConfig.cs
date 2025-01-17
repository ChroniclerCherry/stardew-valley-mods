using StardewModdingAPI;

namespace StardewAquarium.Framework.Models;

internal class ModConfig
{
    public bool EnableDebugCommands { get; set; } = false;
    public SButton? CheckDonationCollection { get; set; } = null;
}
