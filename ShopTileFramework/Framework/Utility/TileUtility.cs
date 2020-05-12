using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using xTile.ObjectModel;

namespace ShopTileFramework.Framework.Utility
{
    class TileUtility
    {
        public  static IPropertyCollection GetTileProperty(GameLocation map, string layer, Vector2 tile)
        {
            if (map == null)
                return null;

            xTile.Tiles.Tile checkTile = map.Map.GetLayer(layer).Tiles[(int)tile.X, (int)tile.Y];

            return checkTile?.Properties;
        }

        public static IClickableMenu CheckVanillaShop(string shopProperty, out bool warpingShop)
        {
            warpingShop = false;
            switch (shopProperty)
            {
                case "Vanilla!PierreShop":
                    {
                        var seedShop = new SeedShop();
                        return new ShopMenu(seedShop.shopStock(), 0, "Pierre", null, null, null);

                    }

                case "Vanilla!JojaShop":
                    return new ShopMenu(StardewValley.Utility.getJojaStock(), 0, null, null, null, null);
                case "Vanilla!RobinShop":
                    return new ShopMenu(StardewValley.Utility.getCarpenterStock(), 0,"Robin", null, null, null);
                case "Vanilla!RobinBuildingsShop":
                    warpingShop = true;
                    return new CarpenterMenu(false);
                case "Vanilla!ClintShop":
                    return new ShopMenu(StardewValley.Utility.getBlacksmithStock(), 0,"Clint", null, null, null);
                case "Vanilla!ClintGeodes":
                    return new GeodeMenu();
                case "Vanilla!ClintToolUpgrades":
                    return new ShopMenu(StardewValley.Utility.getBlacksmithUpgradeStock(Game1.player),0, "ClintUpgrade", null, null, null);
                case "Vanilla!MarlonShop":
                    return new ShopMenu(StardewValley.Utility.getAdventureShopStock(),0, "Marlon", null, null, null);
                case "Vanilla!MarnieShop":
                    return new ShopMenu(StardewValley.Utility.getAnimalShopStock(),0, "Marnie", null, null, null);
                case "Vanilla!MarnieAnimalShop":
                    warpingShop = true;
                    return new PurchaseAnimalsMenu(StardewValley.Utility.getPurchaseAnimalStock());
                case "Vanilla!TravellingMerchant":
                    return new ShopMenu(StardewValley.Utility.getTravelingMerchantStock((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed)),
                        0, "Traveler", new Func<ISalable, Farmer, int, bool>(StardewValley.Utility.onTravelingMerchantShopPurchase), null, null);
                case "Vanilla!HarveyShop":
                    return new ShopMenu(StardewValley.Utility.getHospitalStock(),0, null, null, null, null);
                case "Vanilla!SandyShop":
                    {
                        var SandyStock = ModEntry.helper.Reflection.GetMethod(Game1.currentLocation, "sandyShopStock").Invoke<Dictionary<ISalable, int[]>>();
                        return new ShopMenu(SandyStock, 0, "Sandy", new Func<ISalable,
                            Farmer, int, bool>(onSandyShopPurchase), null, null);

                    }

                case "Vanilla!DesertTrader":
                    return new ShopMenu(Desert.getDesertMerchantTradeStock(Game1.player),0,
                        "DesertTrade", new Func<ISalable, Farmer, int, bool>(boughtTraderItem),null, null);
                case "Vanilla!KrobusShop":
                    {
                        var sewer = new Sewer();
                        return new ShopMenu(sewer.getShadowShopStock(),
                            0, "Krobus", new Func<ISalable, Farmer, int, bool>(sewer.onShopPurchase),
                            null, null);

                    }

                case "Vanilla!DwarfShop":
                    return new ShopMenu(StardewValley.Utility.getDwarfShopStock(), 0,"Dwarf", null, null, null);
                case "Vanilla!AdventureRecovery":
                    return new ShopMenu(StardewValley.Utility.getAdventureRecoveryStock(),0, "Marlon_Recovery", null, null, null);
                case "Vanilla!GusShop":
                    {
                        return new ShopMenu(StardewValley.Utility.getSaloonStock(), 0, "Gus", (item, farmer, amount) =>
                        {
                            Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
                            return false;
                        }, null, null);
                    }

                case "Vanilla!WillyShop":
                    return new ShopMenu(StardewValley.Utility.getFishShopStock(Game1.player), 0,"Willy", null, null, null);
                case "Vanilla!WizardBuildings":
                    warpingShop = true;
                    return new CarpenterMenu(true);
                case "Vanilla!QiShop":
                    Game1.activeClickableMenu = new ShopMenu(StardewValley.Utility.getQiShopStock(), 2, null, null, null, null);
                    break;
                case "Vanilla!IceCreamStand":
                    return new ShopMenu(new Dictionary<ISalable, int[]>()
                                {
                                    {
                                         new StardewValley.Object(233, 1, false, -1, 0),
                                        new int[2]{ 250, int.MaxValue }
                                    }
                                }, 0, null, null, null, null);
            }

            return null;
        }

        /// <summary>
        /// Copied over method to make the desert trader work without reflection bs
        /// </summary>
        /// <param name="s"></param>
        /// <param name="f"></param>
        /// <param name="i"></param>
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
        /// <param name="item"></param>
        /// <param name="who"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private static bool onSandyShopPurchase(ISalable item, Farmer who, int amount)
        {
            Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Sandy, item, amount);
            return false;
        }
    }
}
