namespace CustomCraftingStations.Framework;

public class CustomCraftingStationsApi : ICustomCraftingStationsApi
{
    /*********
    ** Public methods
    *********/
    public void SetCCSCraftingMenuOverride(bool menuOverride)
    {
        ModEntry.MenuOverride = menuOverride;
    }
}
