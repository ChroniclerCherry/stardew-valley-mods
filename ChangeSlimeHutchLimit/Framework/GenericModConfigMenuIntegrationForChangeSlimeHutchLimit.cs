using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace ChangeSlimeHutchLimit.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForChangeSlimeHutchLimit : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
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
            )

            .AddSectionTitle(
                text: () => "Slime Ball Settings"
            )
            
            .AddCheckbox(
                name: () => "Enable Slime Ball Override",
                tooltip: () => "Enable or disable the override for slime ball placement.",
                get: config => config.EnableSlimeBallOverride,
                set: (config, value) => config.EnableSlimeBallOverride = value
            )

            .AddNumberField(
                name: () => "Max Daily Slime Balls",
                tooltip: () => "Set the maximum number of slime balls that can spawn daily (set to 0 for unlimited).",
                get: config => config.MaxDailySlimeBalls,
                set: (config, value) => config.MaxDailySlimeBalls = (int)value,
                min: 0,
                max: 100,
                interval: 5
            );
    }
}
