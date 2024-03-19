namespace PlatonicRelationships.Framework
{
    //patching the method SocialPage.drawNPCSlot()
    public static class PatchDrawNpcSlotHeart
    {
        internal static void Prefix(ref bool isDating)
        {
            isDating = true; // don't lock hearts
        }
    }
}
