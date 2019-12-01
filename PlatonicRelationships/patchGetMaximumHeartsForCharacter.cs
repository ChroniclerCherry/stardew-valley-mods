using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatonicRelationships
{
    class patchGetMaximumHeartsForCharacter
    {
        internal static void Postfix(ref int __result)
        {
            if (__result == 8)
                __result = 10;
        }
    }
}
