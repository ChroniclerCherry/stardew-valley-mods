using StardewModdingAPI;

namespace CustomizeAnywhere.Framework
{
    class ModConfig
    {
        public bool canAccessMenusAnywhere { get; set; }
        public SButton ActivateButton { get; set; }
        public SButton customizeButton { get; set; }
        public SButton dyeButton { get; set; }
        public SButton tailoringButton { get; set; }
        public SButton dresserButton { get; set; }
        public bool canTailorWithoutEvent { get; set; }

        public ModConfig()
        {
            canAccessMenusAnywhere = true;
            ActivateButton = SButton.LeftShift;
            customizeButton = SButton.D1;
            dyeButton = SButton.D2;
            tailoringButton = SButton.D3;
            dresserButton = SButton.D4;
            canTailorWithoutEvent = false;
        }
    }
}
