namespace CustomCraftingStation.Framework
{
    public class CustomCraftingStationsApi : ICustomCraftingStationsApi
    {
        public void SetCCSCraftingMenuOverride(bool menuOverride)
        {
            ModEntry.MenuOverride = menuOverride;
        }
    }
}
