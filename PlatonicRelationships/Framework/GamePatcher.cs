using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace PlatonicRelationships.Framework;

/// <summary>Applies Harmony patches to the game code.</summary>
internal static class GamePatcher
{
    /*********
    ** Public methods
    *********/
    /// <summary>Apply the patches.</summary>
    /// <param name="modId">The unique mod ID.</param>
    /// <param name="monitor">Encapsulates monitoring and logging</param>
    public static void Apply(string modId, IMonitor monitor)
    {
        Harmony harmony = new(modId);

        try
        {
            monitor.Log("Transpile patching SocialPage.drawNPCSlotHeart");
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlotHeart"),
                prefix: new HarmonyMethod(methodType: typeof(GamePatcher), nameof(GamePatcher.Before_SocialPage_DrawNpcSlotHeart))
            );
        }
        catch (Exception e)
        {
            monitor.Log($"Failed in Patching SocialPage.drawNPCSlotHeart: \n{e}", LogLevel.Error);
            return;
        }

        try
        {
            monitor.Log("Postfix patching Utility.GetMaximumHeartsForCharacter");
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), name: "GetMaximumHeartsForCharacter"),
                postfix: new HarmonyMethod(typeof(GamePatcher), nameof(GamePatcher.After_Utility_GetMaximumHeartsForCharacter))
            );
        }
        catch (Exception e)
        {
            monitor.Log($"Failed in Patching Utility.GetMaximumHeartsForCharacter: \n{e}", LogLevel.Error);
        }
    }


    /*********
    ** Private methods
    *********/
    private static void Before_SocialPage_DrawNpcSlotHeart(ref bool isDating)
    {
        isDating = true; // don't lock hearts
    }

    private static void After_Utility_GetMaximumHeartsForCharacter(ref int __result)
    {
        if (__result == 8)
            __result = 10;
    }

    //private static IEnumerable<CodeInstruction> Transpile_Farmer_ChangeFriendship(IEnumerable<CodeInstruction> instructions)
    //{
    //    List<CodeInstruction> instructionList = instructions.ToList();
    //    for (int i = 0; i < instructionList.Count; i++)
    //    {
    //        if (instructionList[i].opcode == OpCodes.Ldc_I4)
    //        {
    //            if (instructionList[i].operand.ToString() == "2000")
    //            {
    //                //change the cap from 8 hearts to 10 when increasing friendship
    //                instructionList[i].operand = 2500;
    //            }
    //            else if (instructionList[i].operand.ToString() == "2498")
    //            {
    //                //changes the hard cap for non-dating from 2498 to 10 hearts
    //                instructionList[i].operand = 2500;
    //                break;
    //            }
    //        }
    //    }
    //    return instructionList.AsEnumerable();
    //}
}
