namespace HayBalesAsSilos.Framework;

class ModConfig
{
    public bool RequiresConstructedSilo { get; set; } = true;
    public int HayPerBale { get; set; } = 240;
    public int HaybalePrice { get; set; } = 5000;
}
