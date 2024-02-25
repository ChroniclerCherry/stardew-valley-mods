using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SnackEverything.Framework
{
    class ModConfig
    {
        public bool YummyArtefacts { get; set; } = false;

        public KeybindList HoldToActivate { get; set; } = KeybindList.ForSingle(SButton.LeftShift);
    }
}
