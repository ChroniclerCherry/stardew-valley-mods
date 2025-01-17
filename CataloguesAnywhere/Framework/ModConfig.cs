using StardewModdingAPI;

namespace CataloguesAnywhere.Framework;

internal class ModConfig
{
    public bool Enabled { get; set; } = true;
    public SButton ActivateButton { get; set; } = SButton.LeftControl;
    public SButton furnitureButton { get; set; } = SButton.D1;
    public SButton WallpaperButton { get; set; } = SButton.D2;
}
