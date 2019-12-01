using StardewModdingAPI;
using Harmony;
using System.Reflection;
using StardewValley;
using StardewValley.Menus;

namespace PlatonicRelationships
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        { 
            //keep a static version of Monitor so I can make logs from the patch classes
            var harmony = HarmonyInstance.Create("cherry.platonicrelationships");

            this.Monitor.Log("Transpile patching Farmer.changeFriendship", StardewModdingAPI.LogLevel.Debug);
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlot"),
                transpiler: new HarmonyMethod(type: typeof(PatchDrawNPCSlot), nameof(PatchDrawNPCSlot.Transpiler))
            );

            this.Monitor.Log("Postfix patching Utility.GetMaximumHeartsForCharacter", StardewModdingAPI.LogLevel.Debug);
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), name: "GetMaximumHeartsForCharacter"),
                postfix: new HarmonyMethod(typeof(patchGetMaximumHeartsForCharacter), nameof(patchGetMaximumHeartsForCharacter.Postfix))
            );
        }
    }
}