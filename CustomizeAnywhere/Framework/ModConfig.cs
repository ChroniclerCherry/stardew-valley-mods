using StardewModdingAPI;

namespace CustomizeAnywhere.Framework;

class ModConfig
{
    public bool CanAccessMenusAnywhere { get; set; } = true;
    public SButton ActivateButton { get; set; } = SButton.LeftShift;
    public SButton CustomizeButton { get; set; } = SButton.D1;
    public SButton DyeButton { get; set; } = SButton.D2;
    public SButton TailoringButton { get; set; } = SButton.D3;
    public SButton DresserButton { get; set; } = SButton.D4;
    public bool CanTailorWithoutEvent { get; set; } = false;
}
