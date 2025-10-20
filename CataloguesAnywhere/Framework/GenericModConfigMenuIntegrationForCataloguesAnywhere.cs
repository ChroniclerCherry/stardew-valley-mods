using System;
using System.Collections.Generic;
using System.Linq;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CataloguesAnywhere.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForCataloguesAnywhere : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>Get the current mod config.</summary>
    private readonly Func<ModConfig> GetConfig;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="getConfig">Get the current mod config.</param>
    public GenericModConfigMenuIntegrationForCataloguesAnywhere(Func<ModConfig> getConfig)
    {
        this.GetConfig = getConfig;
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        // general settings
        menu
            .Register()
            .AddSectionTitle(I18n.Config_Sections_GeneralOptions)
            .AddCheckbox(
                name: I18n.Config_Enabled_Name,
                tooltip: I18n.Config_Enabled_Desc,
                get: config => config.Enabled,
                set: (config, value) => config.Enabled = value
            );

        // default keys
        menu.AddSectionTitle(I18n.Config_Sections_DefaultKeybinds);
        ShopField[] defaultFields = [
            new(ShopIds.FurnitureCatalogueId, I18n.Config_FurnitureKey_Name, I18n.Config_FurnitureKey_Desc),
            new(ShopIds.JojaFurnitureCatalogueId, I18n.Config_JojaFurnitureKey_Name, I18n.Config_JojaFurnitureKey_Desc),
            new(ShopIds.JunimoFurnitureCatalogueId, I18n.Config_JunimoFurnitureKey_Name, I18n.Config_JunimoFurnitureKey_Desc),
            new(ShopIds.RetroFurnitureCatalogueId, I18n.Config_RetroFurnitureKey_Name, I18n.Config_RetroFurnitureKey_Desc),
            new(ShopIds.TrashFurnitureCatalogueId, I18n.Config_TrashFurnitureKey_Name, I18n.Config_TrashFurnitureKey_Desc),
            new(ShopIds.WizardFurnitureCatalogueId, I18n.Config_WizardFurnitureKey_Name, I18n.Config_WizardFurnitureKey_Desc),

            new(ShopIds.WallpaperCatalogueId, I18n.Config_WallpaperKey_Name, I18n.Config_WallpaperKey_Desc)
        ];
        this.AddShopKeyFields(menu, defaultFields);

        // custom keys
        menu.AddSectionTitle(I18n.Config_Sections_CustomKeybinds);
        {
            HashSet<string> defaultShopIds = new HashSet<string>(defaultFields.Select(p => p.ShopId), StringComparer.OrdinalIgnoreCase);

            ShopField[] customFields = this.GetConfig().Catalogues.Keys
                .Where(id => !defaultShopIds.Contains(id))
                .Select(id => new ShopField(
                    ShopId: id,
                    Name: () => I18n.Config_CustomKey_Name(shopId: id),
                    Tooltip: () => I18n.Config_CustomKey_Desc(shopId: id)
                ))
                .ToArray();

            if (customFields.Length > 0)
                this.AddShopKeyFields(menu, customFields);
            else
                menu.AddParagraph(I18n.Config_NoCustomKeys);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add keybind config fields for shop IDs.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="fields">The shop ID fields to add.</param>
    private void AddShopKeyFields(GenericModConfigMenuIntegration<ModConfig> menu, params ShopField[] fields)
    {
        foreach (ShopField field in fields.OrderBy(p => p.Name()))
        {
            menu.AddKeyBinding(
                name: field.Name,
                tooltip: field.Tooltip,
                get: config => config.Catalogues.GetValueOrDefault(field.ShopId) ?? KeybindList.ForSingle(),
                set: (config, value) => config.Catalogues[field.ShopId] = value
            );
        }
    }

    /// <summary>The info about an open-shop keybind config field.</summary>
    /// <param name="ShopId">The shop ID opened by the keybind.</param>
    /// <param name="Name">The label text to show in the form.</param>
    /// <param name="Tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    private record ShopField(string ShopId, Func<string> Name, Func<string> Tooltip);
}
