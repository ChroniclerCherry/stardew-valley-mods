using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Diagnostics;

namespace PlatonicRelationships
{
    //patching the method SocialPage.drawNPCSlot()
    public static class PatchDrawNPCSlot
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            int datingInstance = 1; //look for calls to the isDating() method
            for (int i = 0; i < instructionList.Count && datingInstance < 3; i++)
            {
                //found the call to isDating()
                if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i].operand.ToString() == "Boolean IsDating()")
                {
                    if (datingInstance == 1)
                    {
                        //remove changing the xsource for datables
                        changeXSource(instructionList, i);
                    }
                    else if (datingInstance == 2)
                    {
                        //stops black hearts from being filled in for datables
                        fillHearts(instructionList, i);
                    }
                    datingInstance++;

                }
            }
            return instructionList.AsEnumerable();
        }

        private static void changeXSource(List<CodeInstruction> instructionList, int startIndex)
        {
            for (int i = startIndex; i < instructionList.Count; i++)
            {
                //The code changes xsource to 211 when drawing hearts for non-dating datables
                
                if (instructionList[i].opcode == OpCodes.Ldc_I4 && instructionList[i].operand.ToString() == "211")
                {
                    //simply remove the code that changes the xSource
                    instructionList[i].opcode = OpCodes.Nop;
                    instructionList[i + 1].opcode = OpCodes.Nop;
                    break;
                }

            }
        }

        private static void fillHearts(List<CodeInstruction> instructionList, int startIndex)
        {
            //before rendering the hearts just tell it that nobody is datable
            instructionList[startIndex - 3] = new CodeInstruction(OpCodes.Ldc_I4_0);
        }
    }
}