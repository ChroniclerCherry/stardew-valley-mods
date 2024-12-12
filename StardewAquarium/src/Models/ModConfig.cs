using StardewModdingAPI;

namespace StardewAquarium.Models;

public sealed class ModConfig
{
    public bool EnableDebugCommands { get; set; } = false;
    public SButton? CheckDonationCollection { get; set; } = null;
}
