[← back to readme](README.md)

# Release notes
## 1.4.1
Released 07 September 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Fixed errors when some content pack data is invalid.
- Internal changes to simplify maintenance.

## 1.4.0
Released 25 January 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
- Added validation error if a custom station has both cooking and crafting recipes, which isn't supported by the vanilla menu used since 1.2.0.
- Fixed cooking recipes in recent versions.

## 1.3.0
Released 21 January 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6.15.
- Older content packs which target Json Assets recipes before Json Assets 1.11.0 are now automatically migrated to Json Assets' new recipe keys.

## 1.2.0
Released 19 March 2024 for SMAPI 4.0.0 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6 and SMAPI 4.0.0.
- Improved compatibility with mods that change crafting menus.
- Renamed mod folder from `CustomCraftingStation` to `CustomCraftingStations` (with an `s`) to match the mod name. You should delete the old folder when updating to avoid issues.

## 1.1.2
Released 23 December 2020 for SMAPI 3.8.0 or later.

- Will pull ingredients from fridge while in farmhouse.
- Updated crafting menus to work with some 1.5 features.

## 1.1.1
Released 26 June 2020 for SMAPI 3.5.0 or later.

- Added ability to craft from nearby chests or all chests globally. Can be changed via config.

## 1.1.0
Released 17 June 2020 for SMAPI 3.5.0 or later.

- Changed the menu to a custom one.  
  _This means there is little to no chance of other mods that edit/replace the crafting menu from breaking CCS or vice
  versa. However, this means that extra features added to the crafting menu from other mods won't be applied to custom
  crafting stations. I will work specifically on compatibility with other mods as the demand arises-- but I think a base
  of "Works, with special compatibility bridges added in over time" is better than it constantly breaking._
- Fixed some bugs.

## 1.0.1
Released 14 June 2020 for SMAPI 3.5.0 or later.

- Fixed issue with all crafting recipes being available in vanilla menu.
- Fixed compatibility with remote fridge storage.

## 1.0.0
Released 13 June 2020 for SMAPI 3.5.0 or later.

- Initial release.
