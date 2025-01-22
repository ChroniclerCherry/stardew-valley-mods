using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CataloguesAnywhere.Framework;

internal class ModConfig
{
    public bool Enabled { get; set; } = true;
    public KeybindList FurnitureKey { get; set; } = KeybindList.ForSingle(SButton.LeftControl, SButton.D1);
    public KeybindList WallpaperKey { get; set; } = KeybindList.ForSingle(SButton.LeftControl, SButton.D2);
}
