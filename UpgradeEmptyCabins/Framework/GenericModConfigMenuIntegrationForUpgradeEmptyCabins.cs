using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace UpgradeEmptyCabins.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForUpgradeEmptyCabins : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        menu
            .Register()
            .AddCheckbox(
                name: I18n.Config_InstantBuild_Name,
                tooltip: I18n.Config_InstantBuild_Desc,
                get: config => config.InstantBuild,
                set: (config, value) => config.InstantBuild = value
            );
    }
}
