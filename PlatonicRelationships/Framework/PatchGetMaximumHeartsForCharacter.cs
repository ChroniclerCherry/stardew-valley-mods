namespace PlatonicRelationships.Framework;

class PatchGetMaximumHeartsForCharacter
{
    internal static void Postfix(ref int __result)
    {
        if (__result == 8)
            __result = 10;
    }
}
