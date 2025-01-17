using StardewModdingAPI;

//
// Original code by Pathoschild: https://github.com/Pathoschild/StardewMods, covered by the MIT license.
//

namespace ChroniclerCherry.Common.Integrations.GenericModConfigMenu;

/// <summary>Implements the integration with Generic Mod Config Menu for a specific mod.</summary>
/// <typeparam name="TConfig">The config model type.</typeparam>
internal interface IGenericModConfigMenuIntegrationFor<TConfig>
    where TConfig : class, new()
{
    /// <summary>Register the config UI for this mod. This should only be called if Generic Mod Config Menu is available.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    void Register(GenericModConfigMenuIntegration<TConfig> menu, IMonitor monitor);
}
