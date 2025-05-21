using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace ProfitMargins.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForProfitMargins : IGenericModConfigMenuIntegrationFor<ModConfig>
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
                name: I18n.Config_EnableInMultiplayer_Name,
                tooltip: I18n.Config_EnableInMultiplayer_Desc,
                get: config => config.EnableInMultiplayer,
                set: (config, value) => config.EnableInMultiplayer = value
            )
            .AddNumberField(
                name: I18n.Config_ProfitMargin_Name,
                tooltip: I18n.Config_ProfitMargin_Desc,
                get: config => config.ProfitMargin,
                set: (config, value) => config.ProfitMargin = value,
                min: 0.1f,
                max: 3f,
                interval: 0.1f
            );
    }
}
