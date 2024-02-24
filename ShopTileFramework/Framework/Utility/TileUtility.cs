using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Util;
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
        /// <returns>An instance of the vanilla stores if the property matches, null otherwise</returns>
        public static IClickableMenu CheckVanillaShop(string shopProperty, out bool warpingShop)
        {
            warpingShop = false;
            switch (shopProperty)
            {
                case "Vanilla!PierreShop":
                    {
                        var seedShop = new SeedShop();
                        return new ShopMenu(seedShop.shopStock(), 0, "Pierre");
                    }

                case "Vanilla!JojaShop":
                    return new ShopMenu(StardewValley.Utility.getJojaStock());
                case "Vanilla!RobinShop":
                    return new ShopMenu(StardewValley.Utility.getCarpenterStock(), 0,"Robin");
                case "Vanilla!RobinBuildingsShop":
                    warpingShop = true;
                    return new CarpenterMenu();
                case "Vanilla!ClintShop":
                    return new ShopMenu(StardewValley.Utility.getBlacksmithStock(), 0,"Clint");
                case "Vanilla!ClintGeodes":
                    return new GeodeMenu();
                case "Vanilla!ClintToolUpgrades":
                    return new ShopMenu(StardewValley.Utility.getBlacksmithUpgradeStock(Game1.player),0, "ClintUpgrade");
                case "Vanilla!MarlonShop":
                    return new ShopMenu(StardewValley.Utility.getAdventureShopStock(),0, "Marlon");
                case "Vanilla!MarnieShop":
                    return new ShopMenu(StardewValley.Utility.getAnimalShopStock(),0, "Marnie");
                case "Vanilla!MarnieAnimalShop":
                    warpingShop = true;
                    return new PurchaseAnimalsMenu(StardewValley.Utility.getPurchaseAnimalStock());
                case "Vanilla!TravellingMerchant":
                    return new ShopMenu(StardewValley.Utility.getTravelingMerchantStock((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)),
                        0, "Traveler", StardewValley.Utility.onTravelingMerchantShopPurchase);
                case "Vanilla!HarveyShop":
                    return new ShopMenu(StardewValley.Utility.getHospitalStock());
                case "Vanilla!SandyShop":
                    {
                        var SandyStock = ModEntry.helper.Reflection.GetMethod(Game1.currentLocation, "sandyShopStock").Invoke<Dictionary<ISalable, int[]>>();
                        return new ShopMenu(SandyStock, 0, "Sandy", onSandyShopPurchase);

                    }

                case "Vanilla!DesertTrader":
                    return new ShopMenu(Desert.getDesertMerchantTradeStock(Game1.player),0,
                        "DesertTrade", boughtTraderItem);
                case "Vanilla!KrobusShop":
                    {
                        var sewer = new Sewer();
                        return new ShopMenu(sewer.getShadowShopStock(),
                            0, "Krobus", sewer.onShopPurchase);

                    }

                case "Vanilla!DwarfShop":
                    return new ShopMenu(StardewValley.Utility.getDwarfShopStock(), 0,"Dwarf");
                case "Vanilla!AdventureRecovery":
                    return new ShopMenu(StardewValley.Utility.getAdventureRecoveryStock(),0, "Marlon_Recovery");
                case "Vanilla!GusShop":
                    {
                        return new ShopMenu(StardewValley.Utility.getSaloonStock(), 0, "Gus", (item, farmer, amount) =>
                        {
                            Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
                            return false;
                        });
                    }

                case "Vanilla!WillyShop":
                    return new ShopMenu(StardewValley.Utility.getFishShopStock(Game1.player), 0,"Willy");
                case "Vanilla!WizardBuildings":
                    warpingShop = true;
                    return new CarpenterMenu(true);
                case "Vanilla!QiShop":
                    Game1.activeClickableMenu = new ShopMenu(StardewValley.Utility.getQiShopStock(), 2);
                    break;
                case "Vanilla!IceCreamStand":
                    return new ShopMenu(new Dictionary<ISalable, int[]>
                    {
                                    {
                                         new Object(233, 1),
                                        new[]{ 250, int.MaxValue }
                                    }
                                });
            }

            return null;
        }

        /// <summary>
        /// Copied over method to make the desert trader work without reflection bs
        /// </summary>
        /// <returns></returns>
        private static bool boughtTraderItem(ISalable s, Farmer f, int i)
        {
            if (s.Name == "Magic Rock Candy")
                Desert.boughtMagicRockCandy = true;
            return false;
        }

        /// <summary>
        /// Copied over method to make Sandy's shop work without reflection bs
        /// </summary>
        /// <returns></returns>
        private static bool onSandyShopPurchase(ISalable item, Farmer who, int amount)
        {
            Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Sandy, item, amount);
            return false;
        }
    }
}
