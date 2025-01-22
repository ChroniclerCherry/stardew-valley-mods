using System;
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
    private EventEditor EventEditor;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor);

        DataModel data = this.LoadDataFile();

        if (this.Config.AddDatingRequirementToRomanticEvents)
        {
            this.EventEditor = new EventEditor(data, helper.GameContent.ParseAssetName);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        this.EventEditor.OnAssetRequested(e);
    }

    /// <summary>Load the mod data from the <c>assets/data.json</c> file.</summary>
    private DataModel LoadDataFile()
    {
        const string filePath = "assets/data.json";
        try
        {
            DataModel data = this.Helper.Data.ReadJsonFile<DataModel>(filePath);
            if (data is not null)
            {
                data.RomanticEvents ??= new();
                data.RomanticTriggerActions ??= new();
                return data;
            }

            this.Monitor.Log($"The '{filePath}' file is missing or empty, so Platonic Relationships may not work correctly. You can try reinstalling the mod to reset the file.");
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"The '{filePath}' file couldn't be loaded, so Platonic Relationships may not work correctly. You can try reinstalling the mod to reset the file.\n\nTechnical details: {ex}");
        }

        return new DataModel();
    }
}
