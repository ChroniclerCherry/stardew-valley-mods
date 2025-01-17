using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace HayBalesAsSilos.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForHayBaysAsSilos : IGenericModConfigMenuIntegrationFor<ModConfig>
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
                name: I18n.Config_RequireConstructedSilo_Name,
                tooltip: I18n.Config_RequireConstructedSilo_Desc,
                get: config => config.RequiresConstructedSilo,
                set: (config, value) => config.RequiresConstructedSilo = value
            )
            .AddNumberField(
                name: I18n.Config_HayPerBale_Name,
                tooltip: I18n.Config_HayPerBale_Desc,
                get: config => config.HayPerBale,
                set: (config, value) => config.HayPerBale = value,
                min: 0,
                max: null
            )
            .AddNumberField(
                name: I18n.Config_PurchasePrice_Name,
                tooltip: I18n.Config_PurchasePrice_Desc,
                get: config => config.HaybalePrice,
                set: (config, value) => config.HaybalePrice = value,
                min: 0,
                max: null
            );
    }
}
