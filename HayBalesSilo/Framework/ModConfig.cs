namespace HayBalesSilo.Framework
{
    class ModConfig
    {
        public bool RequiresConstructedSilo { get; set; } = true;
        public int HayBaleEquivalentToHowManySilos { get; set; } = 1;
        public int HaybalePrice { get; set; } = 5000;
    }
}
