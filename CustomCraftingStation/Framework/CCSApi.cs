namespace CustomCraftingStation.Framework
{
    public class CCSApi : ICCSApi
    {
        public void SetCCSCraftingMenuOverride(bool menuOverride)
        {
            ModEntry.MenuOverride = menuOverride;
        }
    }
}
