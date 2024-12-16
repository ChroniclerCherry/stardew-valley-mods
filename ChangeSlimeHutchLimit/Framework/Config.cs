namespace ChangeSlimeHutchLimit.Framework
{
    public class Config
    {
        public int MaxSlimesInHutch { get; set; } = 20; // Default maximum slimes
        public bool EnableSlimeBallOverride { get; set; } = true; // Default to enabled
        public int MaxDailySlimeBalls { get; set; } = 0; // Default to unlimited (0)
    }
}
