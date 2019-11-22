using Harmony;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

namespace PlatonicRelationships
{
    //Patching the method Farmer.changeFriendship()
    public class PatchChangeFriendship
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ModEntry.monitor.Log("Patching Farmer.changeFriendship",StardewModdingAPI.LogLevel.Debug);
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldc_I4)
                {
                    if (instructionList[i].operand.ToString() == "2000")
                    {
                        //change the cap from 8 hearts to 10 when increasing friendship
                        instructionList[i].operand = 2500;
                    }
                    else if (instructionList[i].operand.ToString() == "2498")
                    {
                        //changes the hard cap for non-dating from 2498 to 10 hearts
                        instructionList[i].operand = 2500;
                        break;
                    }
                }
            }
            return instructionList.AsEnumerable();
        }

    }
}