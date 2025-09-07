using SnackAnything.Framework;
using StardewModdingAPI;

namespace SnackAnything;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();

        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor, () => this.Config);
    }
}
