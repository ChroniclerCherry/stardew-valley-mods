namespace HayBalesAsSilos.Framework;

/// <summary>The mod settings model.</summary>
internal class ModConfig
{
    /// <summary>Whether the player needs at least one silo in a location for hay bales to work there.</summary>
    public bool RequiresConstructedSilo { get; set; } = true;

    /// <summary>How much hay can be stored in each bale.</summary>
    public int HayPerBale { get; set; } = 240;

    /// <summary>The gold price to purchase a hay bale.</summary>
    public int HayBalePrice { get; set; } = 5000;
}
