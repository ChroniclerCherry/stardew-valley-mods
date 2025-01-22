namespace HayBalesAsSilos.Framework;

internal class ModConfig
{
    public bool RequiresConstructedSilo { get; set; } = true;
    public int HayPerBale { get; set; } = 240;
    public int HayBalePrice { get; set; } = 5000;
}
