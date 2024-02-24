using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using HarmonyLib;
using PlatonicRelationships.Framework;
using StardewModdingAPI.Events;

namespace PlatonicRelationships
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private readonly AddDatingPrereq Editor = new AddDatingPrereq();

        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            if (Config.AddDatingRequirementToRomanticEvents)
                helper.Events.Content.AssetRequested += OnAssetRequested;

            //apply harmony patches
            ApplyPatches();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (Editor.CanEdit(e.NameWithoutLocale))
                e.Edit(Editor.Edit);
        }

        public void ApplyPatches()
        {
            var harmony = new Harmony("cherry.platonicrelationships");

            try
            {
                this.Monitor.Log("Transpile patching SocialPage.drawNPCSlot", StardewModdingAPI.LogLevel.Debug);
                harmony.Patch(
                    original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlot"),
                    transpiler: new HarmonyMethod(methodType: typeof(PatchDrawNPCSlot), nameof(PatchDrawNPCSlot.Transpiler))
                );
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed in Patching SocialPage.drawNPCSlot: \n{e}", LogLevel.Error);
                return;
            }

            try
            {
                this.Monitor.Log("Postfix patching Utility.GetMaximumHeartsForCharacter", StardewModdingAPI.LogLevel.Debug);
                harmony.Patch(
                    original: AccessTools.Method(typeof(Utility), name: "GetMaximumHeartsForCharacter"),
                    postfix: new HarmonyMethod(typeof(patchGetMaximumHeartsForCharacter), nameof(patchGetMaximumHeartsForCharacter.Postfix))
                );
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed in Patching Utility.GetMaximumHeartsForCharacter: \n{e}", LogLevel.Error);
                return;
            }
        }
    }
}
