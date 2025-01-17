namespace PlatonicRelationships.Framework;

internal class PatchGetMaximumHeartsForCharacter
{
    /*********
    ** Public methods
    *********/
    public static void Postfix(ref int __result)
    {
        if (__result == 8)
            __result = 10;
    }
}
