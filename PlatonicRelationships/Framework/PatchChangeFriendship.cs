using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace PlatonicRelationships.Framework;

//Patching the method Farmer.changeFriendship()
internal class PatchChangeFriendship
{
    /*********
    ** Public methods
    *********/
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
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
