namespace ProfitMargins.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /// <summary>Whether to change profit margins when hosting a multiplayer server. If false, it only applies in single-player.</summary>
    public bool EnableInMultiplayer { get; set; } = false;

    /// <summary>The percentage profit margins as a decimal value, where 1 (i.e. 100%) is the default value.</summary>
    public float ProfitMargin { get; set; } = 1f;
}
