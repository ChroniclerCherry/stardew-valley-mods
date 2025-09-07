[← back to readme](README.md)

# Release notes
## 1.4.1
Released 07 September 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Fixed errors when the `config.json` has invalid values.
- Improved translations. Thanks to BlackRosePetals (updated Chinese)!
- Internal changes to simplify maintenance.

## 1.4.0
Released 25 January 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
- Improved translations. Thanks to BlackRosePetals (added Chinese)!

## 1.3.0
Released 21 January 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6.15.
- Added support for controller and [multi-key bindings](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Multi-key_bindings).  
  _This merges the former `ActivateButton` option into the specific key binds. If you customized the key binds, you'll need to do it again for the new format._
- Fixed unable to craft mirror.

## 1.2.0
Released 19 March 2024 for SMAPI 4.0.0 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6 and SMAPI 4.0.0.
- Json Assets is no longer required; the items are now added directly to the new item data in 1.6.
- Improved compatibility with mods that change the dresser UI or character customization UI.

**Breaking changes:**  
If you already had the Customization Mirror or Clothing Catalogue from previous versions, they may become Error Items
in Stardew Valley 1.6. That's a one-time issue due to the migration from Json Assets to the 1.6 item format.

You can get rid of the broken items in-game (e.g. by trashing them), then run these commands in the SMAPI console
window to get new ones:
```sh
player_add (BC)Cherry.CustomizeAnywhere_Mirror
player_add (BC)Cherry.CustomizeAnywhere_Catalogue
```

## 1.1.7
Released 09 January 2020 for SMAPI 3.0.0 or later.

- Added controller/keyboard interaction support.

## 1.1.6
Released 03 January 2020 for SMAPI 3.0.0 or later.

- Fixed clothing not rendering after exiting customization menu with escape.
- Fixed gender changes not reverting after hitting escape.
- Fixed skin colour being set to 1 after hitting escape.
- Refined checks for location of mirror and catalogue to be more in line with vanilla.

## 1.1.5
Released 20 December 2019 for SMAPI 3.0.0 or later.

- Added feature when you leave customization menu with escape key, no changes will be made to the character.
- Added Customization Mirror and Clothing Catalog items.

## 1.1.4
Released 19 December 2019 for SMAPI 3.0.0 or later.

- Gender buttons are now functional.
- Added dresser menu to spawn clothing items.

## 1.1.3
Released 02 December 2019 for SMAPI 3.0.0 or later.

- Changed source from `NewGame` to `Wizard`. Hopefully this causes less issues overall.

## 1.1.2
Released ? for SMAPI 3.0.0 or later.

- Fixed multiplayer money issues.

## 1.1.0
Released 30 November 2019 for SMAPI 3.0.0 or later.

- Combined customization and changing menu into one menu subclassed from `CharacterCustomization`. Added access to
  dyeing and tailoring menus as well.

## 1.0.0
Released 28 November 2019 for SMAPI 3.0.0 or later.

- Initial release.
