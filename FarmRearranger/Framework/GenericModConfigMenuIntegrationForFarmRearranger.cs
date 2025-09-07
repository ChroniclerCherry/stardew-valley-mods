using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace FarmRearranger.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForFarmRearranger : IGenericModConfigMenuIntegrationFor<ModConfig>
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
                name: I18n.Config_CanArrangeOutsideFarm_Name,
                tooltip: I18n.Config_CanArrangeOutsideFarm_Desc,
                get: config => config.CanArrangeOutsideFarm,
                set: (config, value) => config.CanArrangeOutsideFarm = value
            )
            .AddNumberField(
                name: I18n.Config_Price_Name,
                tooltip: I18n.Config_Price_Desc,
                get: config => config.Price,
                set: (config, value) => config.Price = (int)value,
                min: 0,
                max: 100_000,
                interval: 250
            )
            .AddNumberField(
                name: I18n.Config_FriendshipPointsRequired_Name,
                tooltip: I18n.Config_FriendshipPointsRequired_Desc,
                get: config => config.FriendshipPointsRequired,
                set: (config, value) => config.FriendshipPointsRequired = (int)value,
                min: 0,
                max: 2_500,
                interval: 250
            );
    }
}
