using StardewModdingAPI.Utilities;

namespace StardewAquarium.Framework.Models;

internal class ModConfig
{
    public bool EnableDebugCommands { get; set; } = false;
    public KeybindList CheckDonationCollection { get; set; } = new();
}
