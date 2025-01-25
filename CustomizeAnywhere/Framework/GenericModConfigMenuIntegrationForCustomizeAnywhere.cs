using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace CustomizeAnywhere.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCustomizeAnywhere : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        menu
            .Register()

            .AddSectionTitle(I18n.Config_Section_Main)
            .AddCheckbox(
                name: I18n.Config_EnableKeyBinds_Name,
                tooltip: I18n.Config_EnableKeyBinds_Desc,
                get: config => config.CanAccessMenusAnywhere,
                set: (config, value) => config.CanAccessMenusAnywhere = value
            )
            .AddCheckbox(
                name: I18n.Config_AlwaysAllowTailoring_Name,
                tooltip: I18n.Config_AlwaysAllowTailoring_Desc,
                get: config => config.CanTailorWithoutEvent,
                set: (config, value) => config.CanTailorWithoutEvent = value
            )

            .AddSectionTitle(I18n.Config_Section_Controls)
            .AddKeyBinding(
                name: I18n.Config_ClothingCatalogueKey_Name,
                tooltip: I18n.Config_ClothingCatalogueKey_Desc,
                get: config => config.DresserKey,
                set: (config, value) => config.DresserKey = value
            )
            .AddKeyBinding(
                name: I18n.Config_CustomizeKey_Name,
                tooltip: I18n.Config_CustomizeKey_Desc,
                get: config => config.CustomizeKey,
                set: (config, value) => config.CustomizeKey = value
            )
            .AddKeyBinding(
                name: I18n.Config_DyeKey_Name,
                tooltip: I18n.Config_DyeKey_Desc,
                get: config => config.DyeKey,
                set: (config, value) => config.DyeKey = value
            )
            .AddKeyBinding(
                name: I18n.Config_TailorKey_Name,
                tooltip: I18n.Config_TailorKey_Desc,
                get: config => config.TailoringKey,
                set: (config, value) => config.TailoringKey = value
            );
    }
}
