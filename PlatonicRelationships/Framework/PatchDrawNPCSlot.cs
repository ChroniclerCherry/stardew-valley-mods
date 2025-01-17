namespace PlatonicRelationships.Framework;

//patching the method SocialPage.drawNPCSlot()
internal static class PatchDrawNpcSlotHeart
{
    /*********
    ** Public methods
    *********/
    public static void Prefix(ref bool isDating)
    {
        isDating = true; // don't lock hearts
    }
}
