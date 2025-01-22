using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomizeAnywhere.Framework;

internal class ModConfig
{
    public bool CanAccessMenusAnywhere { get; set; } = true;
    public KeybindList CustomizeKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D1);
    public KeybindList DyeKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D2);
    public KeybindList TailoringKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D3);
    public KeybindList DresserKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D4);
    public bool CanTailorWithoutEvent { get; set; } = false;
}
