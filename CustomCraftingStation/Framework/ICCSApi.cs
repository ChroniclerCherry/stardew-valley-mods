namespace CustomCraftingStation.Framework
{
    public interface ICCSApi
    {
        void SetCCSCraftingMenuOverride(bool menuOverride);
    }

    public class CCSApi : ICCSApi
    {
        public void SetCCSCraftingMenuOverride(bool menuOverride)
        {
            ModEntry.MenuOverride = menuOverride;
        }
    }
}
