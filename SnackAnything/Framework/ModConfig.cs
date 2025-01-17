using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SnackAnything.Framework;

internal class ModConfig
{
    public bool YummyArtefacts { get; set; } = false;

    public KeybindList HoldToActivate { get; set; } = KeybindList.ForSingle(SButton.LeftShift);
}
