using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace UpgradeEmptyCabins.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForUpgradeEmptyCabins : IGenericModConfigMenuIntegrationFor<Config>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<Config> menu, IMonitor monitor)
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
