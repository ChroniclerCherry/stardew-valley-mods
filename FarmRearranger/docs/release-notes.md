# Release notes
## Upcoming release
- Added in-game config UI through [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
- Internal changes to simplify maintenance.

## 1.1.1
Released 21 January 2025 for SMAPI 4.1.10 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6.15.
- Improved translations. Thanks to CaranudLapin (updated French)!

## 1.1.0
Released 19 March 2024 for SMAPI 4.0.0 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6 and SMAPI 4.0.0.
- Json Assets is no longer required; the items are now added directly to the new item data in 1.6.

**Breaking changes:**  
If you already had the Furniture Catalogue from previous versions, it may become an Error Item in Stardew Valley 1.6.
That's a one-time issue due to the migration from Json Assets to the 1.6 item format.

You can get rid of the broken item in-game (e.g. by trashing it), then run this commands in the SMAPI console window to
get a new one:
```sh
player_add (BC)cel10e.Cherry.FarmRearranger_FarmRearranger
```

## 1.0.3
Released 02 May 2023 for SMAPI 3.0.0 or later.

- Improved translations. Thanks to asqwedcxz741 (added Chinese)!

## 1.0.2
Released 26 January 2020 for SMAPI 3.0.0 or later.

- Improved translations. Thanks to Minakie (added French, Italian, Spanish, and Portuguese)!

## 1.0.1
Released 07 January 2020 for SMAPI 3.0.0 or later.

- Fixed an oopsies with an extra Json Assets folder.

## 1.0.0
Released 07 January 2020 for SMAPI 3.0.0 or later.

- Initial release.
