using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace ChangeSlimeHutchLimit.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForChangeSlimeHutchLimit : IGenericModConfigMenuIntegrationFor<Config>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<Config> menu, IMonitor monitor)
    {
        menu
            .Register()
            .AddNumberField(
                name: I18n.Config_MaxSlimes_Name,
                tooltip: I18n.Config_MaxSlimes_Desc,
                get: config => config.MaxSlimesInHutch,
                set: (config, value) => config.MaxSlimesInHutch = (int)value,
                min: 20,
                max: null,
                interval: 5
            );
    }
}
