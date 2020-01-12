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
        internal static IModHelper helper;
        internal static IMonitor monitor;

        internal static IJsonAssetsApi JsonAssets;
        internal static BFAVApi BFAV;

        internal static List<string> ExcludeFromMarnie = new List<string>();
        private bool ChangedMarnieStock = false;

        internal static Dictionary<string, Shop> Shops = new Dictionary<string, Shop>();
        internal static Dictionary<string, AnimalShop> AnimalShops = new Dictionary<string, AnimalShop>();

        internal static GameLocation SourceLocation = null;
        internal static Vector2 PlayerPos = Vector2.Zero;

        /// <summary>
        /// the Mod entry point called by SMAPI
        /// </summary>
        /// <param name="h">the helper provided by SMAPI</param>
        public override void Entry(IModHelper h)
        {
            //make helper and monitor static so they can be accessed in other classes
            helper = h;
            monitor = Monitor;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

            //add console commands
            helper.ConsoleCommands.Add("open_shop", "Opens up a custom shop's menu. \n\nUsage: open_shop <ShopName>\n-ShopName: the name of the shop to open", this.DisplayShopMenu);
            helper.ConsoleCommands.Add("open_animal_shop", "Opens up a custom animal shop's menu. \n\nUsage: open_shop <open_animal_shop>\n-ShopName: the name of the animal shop to open", this.DisplayAnimalShopMenus);
            helper.ConsoleCommands.Add("reset_shop", "Resets the stock of specified shop. Rechecks conditions and randomizations\n\nUsage: reset_shop <ShopName>\n-ShopName: the name of the shop to reset", this.ResetShopStock);
            helper.ConsoleCommands.Add("list_shops", "Lists all shops registered with Shop Tile Framework", this.ListAllShops);

            //get all the info from content packs
            LoadContentPacks();

            /*
            //harmony black magic
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Utility), "getPurchaseAnimalStock"),
                postfix: new HarmonyMethod(typeof(PatchGetPurchaseAnimalStock), nameof(PatchGetPurchaseAnimalStock.PostFix))
                );
                */
        }
        /// <summary>
        /// Checks for warps from the buildings/animals menu 
        /// and ensures the player is returned to their original location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            //Fixes the game warping the player to places we don't want them to warp
            //if buildings/animal purchase menus are brought up from a custom tile
            if (SourceLocation != null && (
                Game1.locationRequest?.Name == "AnimalShop" || 
                Game1.locationRequest?.Name == "WizardHouse" ||
                Game1.locationRequest?.Name == "ScienceHouse"))
            {
                Game1.locationRequest.Location = SourceLocation;
                Game1.locationRequest.IsStructure = SourceLocation.isStructure.Value;
            }
        }
        /// <summary>
        /// Stops Marnie's portrait from appearing in non-Marnie animal shops after animal purchasing
        /// And removes specified animals from Marnie's store
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            //this block fixes marnie's portrait popping up after purchasing an animal
            if (e.OldMenu is PurchaseAnimalsMenu && e.NewMenu is DialogueBox && SourceLocation != null)
            {
                var AnimalPurchaseMessage = ((DialogueBox)e.NewMenu).getCurrentString();

                //go away marnie we don't want you
                Game1.exitActiveMenu();

                //display the animal purchase message without Marnie's face
                Game1.activeClickableMenu = new DialogueBox(AnimalPurchaseMessage);
            }

            //this is the vanilla Marnie menu for us to exclude animals from	
            if (e.NewMenu is PurchaseAnimalsMenu && SourceLocation == null &&
                !ChangedMarnieStock && ExcludeFromMarnie.Count > 0)
            {
                //close the current menu to open our own	
                Game1.exitActiveMenu();
                var AllAnimalsStock = Utility.getPurchaseAnimalStock();
                var newAnimalStock = new List<StardewValley.Object>();
                foreach (var animal in AllAnimalsStock)
                {
                    if (!ExcludeFromMarnie.Contains(animal.Name))
                    {
                        newAnimalStock.Add(animal);
                    }
                }
                ChangedMarnieStock = true;
                Game1.activeClickableMenu = new PurchaseAnimalsMenu(newAnimalStock);
            }

            //idk why some menus have a habit of warping the player a tile to the left ocassionally
            //so im just gonna warp them back to their original location eh
            if (e.NewMenu == null && PlayerPos != Vector2.Zero)
            {
                Game1.player.position.Set(PlayerPos);
            }
        }
        /// <summary>
        /// Returns an instance of this mod's api
        /// </summary>
        /// <returns></returns>
        public override object GetApi()
        {
            return new Api();
        }
        /// <summary>
        /// refreshes the object information files on each save loaded in case of ids changing due to JA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Shop.GetObjectInfoSource();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            JsonAssets = helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            BFAV = helper.ModRegistry.GetApi<BFAVApi>("Paritee.BetterFarmAnimalVariety");

            if (JsonAssets == null)
            {
                Monitor.Log("Json Assets API not detected. Custom JA items will not be added to shops.",
                    LogLevel.Info);
            }

            if (BFAV == null)
            {
                Monitor.Log("BFAV API not detected. Custom farm animals will not be added to ANIMAL shops.",
                    LogLevel.Info);
            }
            else if (!BFAV.IsEnabled())
            {
                BFAV = null;
                Monitor.Log("BFAV is installed but not enabled. Custom farm animals will not be added to animal shops.",
                    LogLevel.Info);
            }
        }

        /// <summary>
        /// Refresh the stock of every store at the start of each day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Monitor.Log($"Refreshing stock for all custom shops...", LogLevel.Debug);
            foreach (Shop Store in Shops.Values)
            {
                Store.UpdateItemPriceAndStock();
            }
        }

        /// <summary>
        /// When input is received, check for shop tiles to open them as necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            //context and button check
            if (!Context.CanPlayerMove)
                return;

            //Resets the boolean I use to check if a menu used to move the player around came from my mod
            //and lets me return them to their original location
            SourceLocation = null;
            PlayerPos = Vector2.Zero;

            //checks if i've changed marnie's stock already after opening her menu
            ChangedMarnieStock = false;

            if (!e.Button.IsActionButton())
                return;

            Vector2 clickedTile = Vector2.Zero;

            clickedTile = Helper.Input.GetCursorPosition().GrabTile;

            //check if there is a tile property on Buildings layer
            var tileProperty = GetTileProperty(Game1.currentLocation, "Buildings", clickedTile);
            if (tileProperty == null)
                return;

            //check if there is a Shop property on clicked tile
            tileProperty.TryGetValue("Shop", out PropertyValue shopProperty);
            if (shopProperty != null)
            {
                //everything in this block is for the shop property "Shop"
                if (shopProperty == "Vanilla!PierreShop")
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
                    PlayerPos = Game1.player.position.Get();
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
                    PlayerPos = Game1.player.position.Get();
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
                    PlayerPos = Game1.player.position.Get();
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
            else
            {
                tileProperty.TryGetValue("AnimalShop", out shopProperty);
                if (shopProperty == null)
                {
                    return;
                }
                else
                {
                    string ShopName = shopProperty.ToString();
                    if (AnimalShops.ContainsKey(ShopName))
                    {

                        helper.Input.Suppress(e.Button);
                        AnimalShops[ShopName].DisplayShop();
                    }
                    else
                    {
                        Monitor.Log($"An Animal Shop tile was clicked, but a shop by the name \"{ShopName}\" " +
                            $"was not found.", LogLevel.Debug);
                    }
                }

            }
            //TODO: add another else check if no tile properties were found for bigcraftables

        }

        /// <summary>
        /// Copied over method to make the desert trader work without reflection bs
        /// </summary>
        /// <param name="s"></param>
        /// <param name="f"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool boughtTraderItem(ISalable s, Farmer f, int i)
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
            foreach (ShopPack shopPack in data.Shops)
            {
                if (Shops.ContainsKey(shopPack.ShopName))
                {
                    monitor.Log($"A mod is trying to add a Shop \"{shopPack.ShopName}\"," +
                        $" but a shop of this name has already been added. " +
                        $"It will not be added.", LogLevel.Warn);
                }
                else
                {
                    var shop = new Shop(shopPack, contentPack);
                    Shops.Add(shopPack.ShopName, shop);
                }
            }

            foreach (AnimalShopPack animalShopPack in data.AnimalShops)
            {
                if (AnimalShops.ContainsKey(animalShopPack.ShopName))
                {
                    monitor.Log($"A mod is trying to add an AnimalShop \"{animalShopPack.ShopName}\"," +
                        $" but a shop of this name has already been added. " +
                        $"It will not be added.", LogLevel.Warn);
                }
                else
                {
                    var animalShop = new AnimalShop(animalShopPack, animalShopPack.ShopName);
                    AnimalShops.Add(animalShopPack.ShopName, animalShop);
                }
            }
        }

        /// <summary>
        /// Reads content packs and loads the relevent information to create the shops
        /// </summary>
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
                    ContentModel data = null;
                    try
                    {
                        data = contentPack.ReadJsonFile<ContentModel>("shops.json");
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Invalid JSON provided by {contentPack.Manifest.UniqueID}. Skipping pack.", LogLevel.Error);
                        Monitor.Log(ex.Message + ex.StackTrace);
                        continue;
                    }
                    
                    Monitor.Log($"      {contentPack.Manifest.Name} by {contentPack.Manifest.Author} | " +
                        $"{contentPack.Manifest.Version} | {contentPack.Manifest.Description}", LogLevel.Info);

                    //adds shops
                    if (data.Shops != null)
                    {
                        foreach (ShopPack s in data.Shops)
                        {
                            if (Shops.ContainsKey(s.ShopName))
                            {
                                monitor.Log($"      {contentPack.Manifest.UniqueID} is trying to add the shop " +
                                    $"\"{s.ShopName}\", but a shop of this name has already been added. " +
                                    $"It will not be added.", LogLevel.Warn);
                            }
                            else
                            {
                                Shops.Add(s.ShopName, new Shop(s, contentPack));
                            }
                        }
                    }

                    //adds animal shops
                    if (data.AnimalShops != null)
                    {
                        foreach (AnimalShopPack animalShopPack in data.AnimalShops)
                        {
                            if (AnimalShops.ContainsKey(animalShopPack.ShopName))
                            {
                                monitor.Log($"      {contentPack.Manifest.UniqueID} is trying to add the animal shop " +
                                    $"\"{animalShopPack.ShopName}\", but a shop of this name has already been added. " +
                                    $"It will not be added.", LogLevel.Warn);
                            }
                            else
                            {
                                if (animalShopPack.ExcludeFromMarnies != null)
                                {
                                    ExcludeFromMarnie.AddRange(animalShopPack.ExcludeFromMarnies);
                                }

                                var animalShop = new AnimalShop(animalShopPack, animalShopPack.ShopName);
                                AnimalShops.Add(animalShopPack.ShopName, animalShop);
                            }
                        }
                    }
                }
            }
        }

        private void DisplayShopMenu(string command, string[] args)
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
                if (!Context.IsPlayerFree)
                {
                    Monitor.Log($"The player isn't free to act; can't display a menu right now", LogLevel.Debug);
                    return;
                }

                value.DisplayShop();
            }
        }

        private void DisplayAnimalShopMenus(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"A shop name is required", LogLevel.Debug);
                return;
            }

            AnimalShops.TryGetValue(args[0], out AnimalShop value);
            if (value == null)
            {
                Monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            }
            else
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
            }
            else
            {
                string temp = "";
                foreach (string k in Shops.Keys)
                {
                    temp += "\nShop: " + k;
                }

                foreach (string k in AnimalShops.Keys)
                {
                    temp += "\nAnimalShop: " + k;
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
