namespace ChangeSlimeHutchLimit.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /// <summary>The maximum number of slimes that can spawn per slime hutch.</summary>
    public int MaxSlimesInHutch { get; set; } = 20;

    /// <summary>Enable to remove 4 slime ball limit.</summary>
    public bool EnableSlimeBallOverride { get; set; } = true;

    /// <summary>The maximum number of slime balls that can spawn per day - Set to 0 for unlimited.</summary>
    public int MaxDailySlimeBalls { get; set; } = 0;
}
