using System;
using HarmonyLib;
using PlatonicRelationships.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace PlatonicRelationships;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private ModConfig Config;
    private readonly AddDatingPrereq Editor = new AddDatingPrereq();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        if (this.Config.AddDatingRequirementToRomanticEvents)
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

        //apply harmony patches
        this.ApplyPatches();
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (this.Editor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.Editor.Edit);
    }

    private void ApplyPatches()
    {
        Harmony harmony = new("cherry.platonicrelationships");

        try
        {
            this.Monitor.Log("Transpile patching SocialPage.drawNPCSlotHeart");
            harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), name: "drawNPCSlotHeart"),
                prefix: new HarmonyMethod(methodType: typeof(PatchDrawNpcSlotHeart), nameof(PatchDrawNpcSlotHeart.Prefix))
            );
        }
        catch (Exception e)
        {
            this.Monitor.Log($"Failed in Patching SocialPage.drawNPCSlotHeart: \n{e}", LogLevel.Error);
            return;
        }

        try
        {
            this.Monitor.Log("Postfix patching Utility.GetMaximumHeartsForCharacter");
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), name: "GetMaximumHeartsForCharacter"),
                postfix: new HarmonyMethod(typeof(PatchGetMaximumHeartsForCharacter), nameof(PatchGetMaximumHeartsForCharacter.Postfix))
            );
        }
        catch (Exception e)
        {
            this.Monitor.Log($"Failed in Patching Utility.GetMaximumHeartsForCharacter: \n{e}", LogLevel.Error);
        }
    }
}
