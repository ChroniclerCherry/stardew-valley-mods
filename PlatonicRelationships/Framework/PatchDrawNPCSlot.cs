namespace PlatonicRelationships.Framework;

//patching the method SocialPage.drawNPCSlot()
internal static class PatchDrawNpcSlotHeart
{
    public static void Prefix(ref bool isDating)
    {
        isDating = true; // don't lock hearts
    }
}
