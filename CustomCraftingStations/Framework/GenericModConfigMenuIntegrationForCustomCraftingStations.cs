using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace CustomCraftingStations.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCustomCraftingStations : IGenericModConfigMenuIntegrationFor<ModConfig>
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
                name: I18n.Config_CraftFromGlobalChests_Name,
                tooltip: () => I18n.Config_CraftFromGlobalChests_Desc(craftFromChestsRadiusName: I18n.Config_CraftFromChestRadius_Name()),
                get: config => config.GlobalCraftFromChest,
                set: (config, value) => config.GlobalCraftFromChest = value
            )
            .AddCheckbox(
                name: I18n.Config_CraftFromFridge_Name,
                tooltip: I18n.Config_CraftFromFridge_Desc,
                get: config => config.CraftFromFridgeWhenInHouse,
                set: (config, value) => config.CraftFromFridgeWhenInHouse = value
            )
            .AddNumberField(
                name: I18n.Config_CraftFromChestRadius_Name,
                tooltip: I18n.Config_CraftFromChestRadius_Desc,
                get: config => config.CraftingFromChestsRadius,
                set: (config, value) => config.CraftingFromChestsRadius = (int)value,
                min: 0,
                max: 100,
                interval: 1
            );
    }
}
