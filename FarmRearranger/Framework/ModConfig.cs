namespace FarmRearranger.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /// <summary>The cost to buy the farm rearranger.</summary>
    public int Price { get; set; } = 25000;

    /// <summary>The minimum friendship points with Robin required before she begins selling the farm rearranger.</summary>
    public int FriendshipPointsRequired { get; set; } = 2000;
}
