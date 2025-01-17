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
            .AddSectionTitle(
                text: () => "Slime Hutch Settings"
            )
            .AddNumberField(
                name: () => "Max # of Slimes in Hutch",
                tooltip: () => "Set the maximum number of slimes allowed in the slime hutch.",
                get: config => config.MaxSlimesInHutch,
                set: (config, value) => config.MaxSlimesInHutch = (int)value,
                min: 20,
                max: null,
                interval: 5
            );
    }
}
