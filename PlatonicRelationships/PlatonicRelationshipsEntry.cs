using StardewModdingAPI;
using Harmony;
using System.Reflection;
using StardewValley;
using StardewValley.Menus;

namespace PlatonicRelationships
{
    public class ModEntry : Mod
    {
        static internal IMonitor monitor;
        public override void Entry(IModHelper helper)
        { 
            //keep a static version of Monitor so I can make logs from the patch classes
            monitor = Monitor;
            var harmony = HarmonyInstance.Create("cherry.platonicrelationships");
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlot"),
                transpiler: new HarmonyMethod(type: typeof(PatchDrawNPCSlot), nameof(PatchDrawNPCSlot.Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), name: "changeFriendship"),
                transpiler: new HarmonyMethod(typeof(PatchChangeFriendship), nameof(PatchChangeFriendship.Transpiler))
            );
        }
    }
}