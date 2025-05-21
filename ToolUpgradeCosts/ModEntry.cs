using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using ToolUpgradeCosts.Framework;

namespace ToolUpgradeCosts;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private readonly Dictionary<UpgradeMaterials, string> DefaultMaterials = new()
    {
        {UpgradeMaterials.Copper, "334"},
        {UpgradeMaterials.Steel, "335"},
        {UpgradeMaterials.Gold, "336"},
        {UpgradeMaterials.Iridium, "337"}
    };

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

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        foreach (KeyValuePair<UpgradeMaterials, Upgrade> upgrade in this.Config.UpgradeCosts)
        {
            string? name = upgrade.Value.MaterialName;

            string id = Game1.objectData.FirstOrDefault(kvp => kvp.Value.Name == name).Key;
            if (id is null)
            {
                this.Monitor.Log($"Object named \"{name}\" not found for the tool upgrade level of {upgrade.Key}. Vanilla upgrade item will be used", LogLevel.Error);
                id = this.DefaultMaterials[upgrade.Key];
            }
            upgrade.Value.MaterialId = id;
        }
    }
}
