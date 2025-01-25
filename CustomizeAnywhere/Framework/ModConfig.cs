using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomizeAnywhere.Framework;

/// <summary>The mod settings.</summary>
internal class ModConfig
{
    /// <summary>Whether the player can use the key bindings to open the menus. If false, the menus can only be accessed through the in-game Clothing Catalogue and Customization Mirror. Default true.</summary>
    public bool CanAccessMenusAnywhere { get; set; } = true;

    /// <summary>Whether the tailoring UI is available before the player has unlocked tailoring in the base game.</summary>
    public bool CanTailorWithoutEvent { get; set; } = false;

    /// <summary>The key bind which opens the character customization UI.</summary>
    public KeybindList CustomizeKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D1);

    /// <summary>The key bind which opens the dye pot UI.</summary>
    public KeybindList DyeKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D2);

    /// <summary>The key bind which opens the tailoring UI.</summary>
    public KeybindList TailoringKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D3);

    /// <summary>The key bind which opens the clothing catalogue UI.</summary>
    public KeybindList DresserKey { get; set; } = KeybindList.ForSingle(SButton.LeftShift, SButton.D4);
}
