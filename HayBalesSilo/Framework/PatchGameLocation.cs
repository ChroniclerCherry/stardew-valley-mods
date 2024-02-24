namespace HayBalesSilo.Framework
{
    public class PatchNumSilos
    {
        internal static void Postfix(ref int __result)
        {

            if (__result > 0 || !ModEntry.Config.RequiresConstructedSilo)
            {
                __result = __result + ModEntry.NumHayBales() * ModEntry.Config.HayBaleEquivalentToHowManySilos;
            }
        }
    }
}
