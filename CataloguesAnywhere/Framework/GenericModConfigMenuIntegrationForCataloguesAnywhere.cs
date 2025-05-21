using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace CataloguesAnywhere.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCataloguesAnywhere : IGenericModConfigMenuIntegrationFor<ModConfig>
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
                name: I18n.Config_Enabled_Name,
                tooltip: I18n.Config_Enabled_Desc,
                get: config => config.Enabled,
                set: (config, value) => config.Enabled = value
            )
            .AddKeyBinding(
                name: I18n.Config_FurnitureKey_Name,
                tooltip: I18n.Config_FurnitureKey_Desc,
                get: config => config.FurnitureKey,
                set: (config, value) => config.FurnitureKey = value
            )
            .AddKeyBinding(
                name: I18n.Config_WallpaperKey_Name,
                tooltip: I18n.Config_WallpaperKey_Desc,
                get: config => config.WallpaperKey,
                set: (config, value) => config.WallpaperKey = value
            );
    }
}
