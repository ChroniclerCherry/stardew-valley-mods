using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using xTile.ObjectModel;

namespace ShopTileFramework
{
    class ModEntry : Mod
    {
        public static IModHelper helper;
        public static IMonitor monitor;
        public static IJsonAssetsApi JsonAssets;
        internal static Dictionary<string, Shop> Shops;
        private GameLocation SourceLocation = null;
        public override void Entry(IModHelper h)
        {
            //make helper and monitor static so they can be accessed in other classes
            helper = h;
            monitor = Monitor;

            Shops = new Dictionary<string, Shop>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Player.Warped += Player_Warped;


            helper.ConsoleCommands.Add("open_shop", "Opens up a custom shop's menu. \n\nUsage: open_shop <ShopName>\n-ShopName: the name of the shop to open", this.DisplayShopMenus);
            helper.ConsoleCommands.Add("reset_shop", "Resets the stock of specified shop. Rechecks conditions and randomizations\n\nUsage: reset_shop <ShopName>\n-ShopName: the name of the shop to reset", this.ResetShopStock);
            helper.ConsoleCommands.Add("list_shops", "Lists all shops registered with Shop Tile Framework", this.ListAllShops);

            LoadContentPacks();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (SourceLocation != null && Game1.currentLocation != SourceLocation)
            {
                Game1.warpFarmer(SourceLocation.Name, Game1.player.getTileX(), Game1.player.getTileY(), (int)Game1.player.facingDirection);
            }
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Shop.GetObjectInfoSource();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            JsonAssets = helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            if (JsonAssets == null)
            {
                Monitor.Log("Json Assets API not detected. Custom JA items will not be added to shops.",
                    LogLevel.Info);
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            //refresh the stock of each store every day
            Monitor.Log($"Refreshing stock for all custom shops...", LogLevel.Debug);
            foreach (Shop Store in Shops.Values)
            {
                Store.UpdateItemPriceAndStock();
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            //context and button check
            if (!Context.CanPlayerMove)
                return;

            SourceLocation = null;

            if (e.Button != SButton.MouseRight)
                return;

            //check if clicked tile is near the player
            var clickedTile = Helper.Input.GetCursorPosition().Tile;
            if (!IsClickWithinReach(clickedTile))
                return;

            //check if there is a tile property on Buildings layer
            var tileProperty = GetTileProperty(Game1.currentLocation, "Buildings" ,clickedTile);
            if (tileProperty == null)
                return;

            //check if there is a Shop property on clicked tile
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);
            if (shopProperty == null)
            {
                return;
            } else if (shopProperty == "Vanilla!PierreShop")
            {
                helper.Input.Suppress(e.Button);
                var seedShop = new SeedShop();
                Game1.activeClickableMenu = new ShopMenu(seedShop.shopStock(), 0, "Pierre", null, null, null);

            }
            else if (shopProperty == "Vanilla!JojaShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getJojaStock(), 0, (string)null, null, null, null);
            }
            else if (shopProperty == "Vanilla!RobinShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0,
                    "Robin", null, null, null);
            }
            else if (shopProperty == "Vanilla!RobinBuildingsShop")
            {
                helper.Input.Suppress(e.Button);
                SourceLocation = Game1.currentLocation;
                Game1.activeClickableMenu = new CarpenterMenu(false);
            }
            else if (shopProperty == "Vanilla!ClintShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0,
                    "Clint", null, null, null);
            }
            else if (shopProperty == "Vanilla!ClintGeodes")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new GeodeMenu();
            }
            else if (shopProperty == "Vanilla!ClintToolUpgrades")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithUpgradeStock(Game1.player),
                    0, "ClintUpgrade", null, null, null);
            }
            else if (shopProperty == "Vanilla!MarlonShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(),
                    0, "Marlon", null, null, null);
            }
            else if (shopProperty == "Vanilla!MarnieShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(),
                    0, "Marnie", null, null, null);
            }
            else if (shopProperty == "Vanilla!MarnieAnimalShop")
            {
                helper.Input.Suppress(e.Button);
                SourceLocation = Game1.currentLocation;
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock());
            }
            else if (shopProperty == "Vanilla!TravellingMerchant")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getTravelingMerchantStock((int)((long)Game1.uniqueIDForThisGame + (long)Game1.stats.DaysPlayed)),
                    0, "Traveler", new Func<ISalable, Farmer, int, bool>(Utility.onTravelingMerchantShopPurchase), null, null);
            }
            else if (shopProperty == "Vanilla!HarveyShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock(),
                    0, null, null, null, null);
            }
            else if (shopProperty == "Vanilla!SandyShop")
            {
                helper.Input.Suppress(e.Button);
                var SandyStock = helper.Reflection.GetMethod(Game1.currentLocation, "sandyShopStock").Invoke<Dictionary<ISalable, int[]>>();
                Game1.activeClickableMenu = new ShopMenu(SandyStock, 0, "Sandy", new Func<ISalable,
                    Farmer, int, bool>(onSandyShopPurchase), null, null);

            }
            else if (shopProperty == "Vanilla!DesertTrader")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Desert.getDesertMerchantTradeStock(Game1.player),
                    0, "DesertTrade", new Func<ISalable, Farmer, int, bool>(boughtTraderItem),
                    null, null);

            }
            else if (shopProperty == "Vanilla!KrobusShop")
            {
                helper.Input.Suppress(e.Button);
                var sewer = new Sewer();
                Game1.activeClickableMenu = new ShopMenu(sewer.getShadowShopStock(),
                    0, "Krobus", new Func<ISalable, Farmer, int, bool>(sewer.onShopPurchase),
                    null, null);

            }
            else if (shopProperty == "Vanilla!DwarfShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0,
                    "Dwarf", null, null, null);

            }
            else if (shopProperty == "Vanilla!AdventureRecovery")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(),
                    0, "Marlon_Recovery", null, null, null);

            }
            else if (shopProperty == "Vanilla!GusShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", (item, farmer, amount) =>
                {
                    Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
                    return false;
                }, null, null);
            }
            else if (shopProperty == "Vanilla!WillyShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0,
                    "Willy", null, null, null);
            }
            else if (shopProperty == "Vanilla!WizardBuildings")
            {
                helper.Input.Suppress(e.Button);
                SourceLocation = Game1.currentLocation;
                Game1.activeClickableMenu = new CarpenterMenu(true);
            }
            else if (shopProperty == "Vanilla!QiShop")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(Utility.getQiShopStock(), 2, null, null, null, null);
            }
            else if (shopProperty == "Vanilla!IceCreamStand")
            {
                helper.Input.Suppress(e.Button);
                Game1.activeClickableMenu = new ShopMenu(new Dictionary<ISalable, int[]>()
                                {
                                    {
                                         new StardewValley.Object(233, 1, false, -1, 0),
                                        new int[2]{ 250, int.MaxValue }
                                    }
                                }, 0, null, null, null, null);
            }
            else
            {
                //Extract the tile property value
                string ShopName = shopProperty.ToString();
                if (Shops.ContainsKey(ShopName))
                {

                    helper.Input.Suppress(e.Button);
                    Shops[ShopName].DisplayShop();
                }
                else
                {
                    Monitor.Log($"A Shop tile was clicked, but a shop by the name \"{ShopName}\" " +
                        $"was not found.", LogLevel.Debug);
                }
            }
        }

        public bool boughtTraderItem(ISalable s, Farmer f, int i)
        {
            if (s.Name == "Magic Rock Candy")
                Desert.boughtMagicRockCandy = true;
            return false;
        }

        private bool onSandyShopPurchase(ISalable item, Farmer who, int amount)
        {
            Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Sandy, item, amount);
            return false;
        }
        private static IPropertyCollection GetTileProperty(GameLocation map, string layer, Vector2 tile)
        {
            if (map == null)
                return null;

            var checkTile = map.Map.GetLayer(layer).Tiles[(int)tile.X, (int)tile.Y];

            if (checkTile == null)
                return null;

            return checkTile.Properties;
        }

        public static void RegisterShops(ContentModel data, IContentPack contentPack)
        {
            foreach (ShopPack s in data.Shops)
            {
                if (Shops.ContainsKey(s.ShopName))
                {
                    monitor.Log($"A mod is trying to add a Shop \"{s.ShopName}\"," +
                        $" but a shop of this name has already been added. " +
                        $"It will not be added.", LogLevel.Warn);
                }
                else
                {
                    var shop = new Shop(s, contentPack);
                    Shops.Add(s.ShopName, shop);
                }
            }
        }

        private void LoadContentPacks()
        {
            monitor.Log("Adding Content Packs...", LogLevel.Info);
            foreach (IContentPack contentPack in helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("shops.json"))
                {
                    monitor.Log($"No shops.json found from the mod {contentPack.Manifest.UniqueID}. " +
                        $"Skipping pack.", LogLevel.Warn);
                }
                else
                {
                    ContentModel data = contentPack.ReadJsonFile<ContentModel>("shops.json");
                    Monitor.Log($"      {contentPack.Manifest.Name} by {contentPack.Manifest.Author} | " +
                        $"{contentPack.Manifest.Version} | {contentPack.Manifest.Description}", LogLevel.Info);
                    foreach (ShopPack s in data.Shops)
                    {
                        if (Shops.ContainsKey(s.ShopName))
                        {
                            monitor.Log($"      {contentPack.Manifest.UniqueID} is trying to add the shop " +
                                $"\"{s.ShopName}\", but a shop of this name has already been added. " +
                                $"It will not be added.", LogLevel.Warn);

                        } else
                        {
                            Shops.Add(s.ShopName, new Shop(s, contentPack));
                        }
                    }
                }

            }
        }

        private bool IsClickWithinReach(Vector2 tile)
        {
            var playerPosition = Game1.player.Position;
            var playerTile = new Vector2(playerPosition.X / 64, playerPosition.Y / 64);

            if (tile.X < (playerTile.X - 1.5) || tile.X > (playerTile.X + 1.5))
                return false;

            if (tile.Y < (playerTile.Y - 1.5) || tile.Y > (playerTile.Y + 1.5))
                return false;

            return true;
        }

        private void DisplayShopMenus(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"A shop name is required",LogLevel.Debug);
                return;
            }

            Shops.TryGetValue(args[0], out Shop value);
            if (value == null)
            {
                Monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            } else
            {
                if (!Context.IsPlayerFree)
                {
                    Monitor.Log($"The player isn't free to act; can't display a menu right now", LogLevel.Debug);
                    return;
                }

                value.DisplayShop();
            }
        }

        private void ResetShopStock(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"A shop name is required", LogLevel.Debug);
                return;
            }

            Shops.TryGetValue(args[0], out Shop value);
            if (value == null)
            {
                Monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            }
            else
            {
                if (!Context.IsWorldReady)
                {
                    Monitor.Log($"The world hasn't loaded; shop stock can't be updated at this time", LogLevel.Debug);
                    return;
                }
                value.UpdateItemPriceAndStock();
            }
        }

        private void ListAllShops(string command, string[] args)
        {
            if (Shops.Count == 0)
            {
                Monitor.Log($"No shops were found", LogLevel.Debug);
            } else
            {
                string temp = "";
                foreach (string k in Shops.Keys)
                {
                    temp += "\nShopName: " + k;
                }

                Monitor.Log(temp, LogLevel.Debug);
            }
        }
    }

    public interface IJsonAssetsApi
    {
        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);

        int GetObjectId(string name);
        int GetCropId(string name);
        int GetFruitTreeId(string name);
        int GetBigCraftableId(string name);
        int GetHatId(string name);
        int GetWeaponId(string name);
        int GetClothingId(string name);
    }
}
