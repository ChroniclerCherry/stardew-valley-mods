using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using xTile.ObjectModel;
using xTile.Tiles;

namespace ShopTileFramework.Framework.Utility
{
    /// <summary>
    /// This class contains static utility methods for dealing with tile properties
    /// </summary>
    class TileUtility
    {
        /// <summary>
        /// Returns the tile property found at the given parameters
        /// </summary>
        /// <param name="map">an instance of the the map location</param>
        /// <param name="layer">the name of the layer</param>
        /// <param name="tile">the coordinates of the tile</param>
        /// <returns>The tile property if there is one, null if there isn't</returns>
        public  static IPropertyCollection GetTileProperty(GameLocation map, string layer, Vector2 tile)
        {
            if (map == null)
                return null;

            Tile checkTile = map.Map.GetLayer(layer).Tiles[(int)tile.X, (int)tile.Y];

            return checkTile?.Properties;
        }

        /// <summary>
        /// Given the name of a shop property, return an instance of the vanilla menu that matches the property
        /// </summary>
        /// <param name="shopProperty">the name of the property, as outlined in the README.md</param>
        /// <param name="warpingShop">is true for animal shops and carpenter shops, which need special handling due to
        /// the fact it physically warps the players to hard-coded locations</param>
        /// <returns>Returns whether a menu was opened.</returns>
        public static bool TryOpenVanillaShop(string shopProperty, out bool warpingShop)
        {
            warpingShop = false;
            switch (shopProperty)
            {
                case "Vanilla!PierreShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_generalStore, "Pierre");

                case "Vanilla!JojaShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_jojaMart, null as string);

                case "Vanilla!RobinShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_carpenter, "Robin");

                case "Vanilla!RobinBuildingsShop":
                    warpingShop = true;
                    Game1.activeClickableMenu = new CarpenterMenu(Game1.builder_robin);
                    return true;

                case "Vanilla!ClintShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_blacksmith, "Clint");

                case "Vanilla!ClintGeodes":
                    Game1.activeClickableMenu = new GeodeMenu();
                    return true;

                case "Vanilla!ClintToolUpgrades":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, "Clint");

                case "Vanilla!MarlonShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, "Marlon");

                case "Vanilla!MarnieShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_animalSupplies, "Marnie");

                case "Vanilla!MarnieAnimalShop":
                    warpingShop = true;
                    Game1.activeClickableMenu = new PurchaseAnimalsMenu(StardewValley.Utility.getPurchaseAnimalStock(Game1.getFarm()));
                    return true;

                case "Vanilla!TravellingMerchant":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_travelingCart, null as string);

                case "Vanilla!HarveyShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_hospital, null as string);

                case "Vanilla!SandyShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_sandy, "Sandy");

                case "Vanilla!DesertTrader":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_desertTrader, null as string);

                case "Vanilla!KrobusShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_krobus, "Krobus");

                case "Vanilla!DwarfShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_dwarf, "Dwarf");

                case "Vanilla!AdventureRecovery":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_adventurersGuildItemRecovery, "Marlon");

                case "Vanilla!GusShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_saloon, "Gus");

                case "Vanilla!WillyShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_fish, "Willy");

                case "Vanilla!WizardBuildings":
                    warpingShop = true;
                    Game1.activeClickableMenu = new CarpenterMenu(Game1.builder_wizard);
                    return true;

                case "Vanilla!QiShop":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_qiGemShop, null as string);

                case "Vanilla!IceCreamStand":
                    return StardewValley.Utility.TryOpenShopMenu(Game1.shop_iceCreamStand, null as string);

                default:
                    return false;
            }
        }
    }
}
