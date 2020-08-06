using Microsoft.Xna.Framework;

namespace BetterGreenhouse.data
{
    public class Config
    {
        public bool EnableDebugging { get; set; } = false;
        public int SizeUpgradeCost { get; set; } = 50000;
        public int AutoWaterUpgradeCost { get; set; } = 50000;
        public Vector2 JojaMartUpgradeCoordinates { get; set; } = new Vector2(22,25);
        public Vector2 CommunityCenterUpgradeCoordinates { get; set; } = new Vector2(14, 4);
    }
}
