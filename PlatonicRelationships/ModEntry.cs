using PlatonicRelationships.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PlatonicRelationships;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config;

    private readonly AddDatingPrereq Editor = new AddDatingPrereq();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor);

        if (this.Config.AddDatingRequirementToRomanticEvents)
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
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
}
