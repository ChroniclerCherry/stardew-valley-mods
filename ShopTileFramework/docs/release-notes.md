[← back to readme](README.md)

# Release notes
## Upcoming release
- Improved error-handling in game patches.
- Removed integration with Better Farm Animal Variety.
- Removed unused mod API methods.
- Internal refactoring.

## 1.0.10
Released 19 March 2024 for SMAPI 4.0.0 or later. Updated by Pathoschild.

- Updated for Stardew Valley 1.6 and SMAPI 4.0.0.

## 1.0.9
Released 21 December 2020 for SMAPI 3.0.0 or later.

- Updated for Stardew Valley 1.5.

## 1.0.8
Released 22 August 2020 for SMAPI 3.0.0 or later.

- Added an exclude function for Json Assets packs.

## 1.0.7
Released 26 July 2020 for SMAPI 3.0.0 or later.

- Now requires Expanded Preconditions Utility, which hamdles the conditions system.
- Fixed error with not removing saplings and seeds from vanilla shops.
- Added option to filter JA pack seeds by season.

## 1.0.6
Released 18 July 2020 for SMAPI 3.0.0 or later.

- Added condition for item in inventory.
- Fixed an issue with crafting recipes not disappearing after being bought.
- Added a new console command to test conditions.

## 1.0.5
Released 06 July 2020 for SMAPI 3.0.0 or later.

- Added conditions for house upgrade and player recipes.

## 1.0.4
Released 24 June 2020 for SMAPI 3.0.0 or later.

- Fixed a missing null check.

## 1.0.3
Released 14 June 2020 for SMAPI 3.0.0 or later.

- Added a way to seperately remove Json Asset pack items and Json Asset pack recipes.
- Added a new condition that allows for synchronized random checks across different stock, shops, and over many days.

## 1.0.2
Released 29 May 2020 for SMAPI 3.0.0 or later.

- Added ability to put custom stock above the vanilla stock instead of only after.
- Added `DefaultSellPriceMultipler` field. Before, any item not explicitly given a price uses the game's sell price, but this will let you multiply all default prices to quickly price a lot of items at once, such as a whole Json Assets pack.
- Added `PriceMultiplierWhen` which when the given conditions are met, will change the price of the entire store. useful for sales days or things like friendship-based discounts.

## 1.0.1
Released 24 May 2020 for SMAPI 3.0.0 or later.

- Fixed input detection on mobile devices.

## 1.0.0
Released 23 May 2020 for SMAPI 3.0.0 or later.

- Completely rewrote from the ground up.
- Added a way to remove Json Assets packs from vanilla stores.
- Added support for modifying vanilla stores.
- Added a new template to the github to help make it easier for modders to get started.

## 0.11.6
Released 10 May 2020 for SMAPI 3.0.0 or later.

- Added localization support.
- Made shops created by Shop Tile Framework directly targetable by Json Assets.
- Added support for seasonal portraits.
- Added support for selling quality items.

## 0.11.5
Released 01 February 2020 for SMAPI 3.0.0 or later.

- Added Rings as a separate item type, so that they can function as rings instead of objects.

## 0.11.4
Released 01 February 2020 for SMAPI 3.0.0 or later.

- Fixed an issue where you could sell recipes for items that had no valid recipes.
- Fixed a null reference exception being thrown when shops try to add a recipe for custom items that aren't installed.

## 0.11.3
Released 20 January 2020 for SMAPI 3.0.0 or later.

- Fixed issue where specifying items by name only matched the beginning of item names, causing cases such as "Lime" returning "Limestone".
- Fixed issue where setting "isRecipe" to true wouldn't work for JA objects and bigcraftables.

## 0.11.2
Released 12 January 2020 for SMAPI 3.0.0 or later.

- Added several new conditions, such as checking if an NPC is at the right tiles for the store.
- Refactored condition checking code to more easily add new conditions and allows the use of the character `!` to reverse conditions.
- Fixed Carpenter/Animal menus sometimes moving the player one tile to the left.

## 0.11.1
Released 09 January 2020 for SMAPI 3.0.0 or later.

- Added controller/keyboard interaction support.

## 0.11.0
Released 06 January 2020 for SMAPI 3.0.0 or later.

- Added ability to remove specified animals from Marnie's store.
- Added controller/keyboard interaction support.
- Changed how `When` condition checks are done to allow for both AND as well as OR conditional groups.
- Moved the logic handling Marnie's store to a prefix patch.

## 0.10.2
Released 04 January 2020 for SMAPI 3.0.0 or later.

- Added conditional checks to opening the shop. This allows you to define store hours among other things.

## 0.10.1
Released ? for SMAPI 3.0.0 or later.

- Added ability to sell recipes.
- Fixed problem with BigCraftables showing up as random Objects instead.

## 0.10.0
Released 03 January 2020 for SMAPI 3.0.0 or later.

- Added support for animal shops
- Fixed warping issues with vanilla menus that is hardcoded to warp player to certain maps.
- Minor bug fixes.

## 0.9.0
Released 01 January 2020 for SMAPI 3.0.0 or later.

- Initial release.
