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

    /// <summary>Adjusts event data to disable vanilla 10-heart events for NPCs the player isn't dating.</summary>
    private readonly EventEditor EventEditor = new EventEditor();


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
        if (this.EventEditor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.EventEditor.Edit);
    }
}
