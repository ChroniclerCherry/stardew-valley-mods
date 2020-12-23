using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using xTile.Layers;
using xTile.Tiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using StardewValley.Locations;
using StardewValley.Menus;

namespace TrainStation
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        public static ModEntry Instance;

        private List<TrainStop> TrainStops;
        private List<BoatStop> BoatStops;
        private IConditionsChecker ConditionsApi;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            Instance = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Player.Warped += Player_Warped;
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e.NewLocation.Name != "Railroad") return;

            string property = e.NewLocation.doesTileHaveProperty(Config.TicketStationX, Config.TicketStationY,
                "Action", "Buildings");
            if (property != "TrainStation")
                DrawInTicketStation();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ConditionsApi = Helper.ModRegistry.GetApi<IConditionsChecker>("Cherry.ExpandedPreconditionsUtility");
            if (ConditionsApi == null)
            {
                Monitor.Log("Expanded Preconditions Utility API not detected. Something went wrong, please check that your installation of Expanded Preconditions Utility is valid", LogLevel.Error);
                return;
            }
                
            
            ConditionsApi.Initialize(false, this.ModManifest.UniqueID);
            
        }

        public override object GetApi()
        {
            return new Api();
        }


        /*****************
        ** Save Loaded **
        ******************/
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            UpdateSelectedLanguage(); //get language code
            LoadContentPacks();

            DrawInTicketStation();

            RemoveInvalidLocations();
        }

        private readonly int TicketStationTopTile = 1032;
        private readonly int TicketStationBottomTile = 1057;

        private void DrawInTicketStation()
        {
            
            GameLocation railway = Game1.getLocationFromName("Railroad");

            //get references to all the stuff I need to edit the railroad map
            Layer buildingsLayer = railway.map.GetLayer("Buildings");
            Layer frontLayer = railway.map.GetLayer("Front");

            TileSheet outdoorsTilesheet = railway.map.TileSheets[1];

            try
            {
                //draw the ticket station
                buildingsLayer.Tiles[Config.TicketStationX, Config.TicketStationY] =
                    new StaticTile(buildingsLayer, outdoorsTilesheet, BlendMode.Alpha, TicketStationBottomTile);
                frontLayer.Tiles[Config.TicketStationX, Config.TicketStationY - 1] =
                    new StaticTile(frontLayer, outdoorsTilesheet, BlendMode.Alpha, TicketStationTopTile);
            }
            catch (Exception e)
            {
                Monitor.Log(e.ToString(),LogLevel.Error);
                Monitor.Log("Train station has recovered from a crash and will continue to function, however the ticket station may be invisible or looked glitched. This is caused by the map mod you are using changing tilesheet orders through renaming vanilla tilesheets or not naming custom tilesheets properly. Please report this to the map mod you are using to fix this issue.",LogLevel.Alert);
                //draw anything from the tilesheet
                buildingsLayer.Tiles[Config.TicketStationX, Config.TicketStationY] =
                    new StaticTile(buildingsLayer, outdoorsTilesheet, BlendMode.Alpha, 1);
                frontLayer.Tiles[Config.TicketStationX, Config.TicketStationY - 1] =
                    new StaticTile(frontLayer, outdoorsTilesheet, BlendMode.Alpha, 1);
            }


            //set the TrainStation property
            railway.setTileProperty(Config.TicketStationX, Config.TicketStationY, "Buildings", "Action", "TrainStation");

            railway.map.LoadTileSheets(Game1.mapDisplayDevice);
        }


        private void LoadContentPacks()
        {
            //create the stop at the vanilla Railroad map
            TrainStop RailRoadStop = new TrainStop
            {
                TargetMapName = "Railroad",
                StopID = "Cherry.TrainStation",
                TargetX = Config.RailroadWarpX,
                TargetY = Config.RailroadWarpY,
                Cost = 0,
                TranslatedName = Helper.Translation.Get("TrainStationDisplayName")
            };

            //create stop in willy's boat room
            BoatStop BoatTunnelStop = new BoatStop()
            {
                TargetMapName = "BoastTunnel",
                StopID = "Cherry.TrainStation",
                TargetX = 4,
                TargetY = 9,
                Cost = 0,
                TranslatedName = Helper.Translation.Get("BoatStationDisplayName")
            };

            ContentPack content = new ContentPack();
            content.TrainStops = new List<TrainStop>();
            content.BoatStops = new List<BoatStop>();

            TrainStops = new List<TrainStop>() { RailRoadStop };
            BoatStops = new List<BoatStop>(){ BoatTunnelStop };

            foreach (IContentPack pack in Helper.ContentPacks.GetOwned())
            {
                if (!pack.HasFile("TrainStops.json"))
                {
                    Monitor.Log($"{pack.Manifest.UniqueID} is missing a \"TrainStops.json\"", LogLevel.Error);
                    continue;
                }

                ContentPack cp = pack.LoadAsset<ContentPack>("TrainStops.json");
                if (cp.TrainStops != null)
                {
                    for (int i = 0; i < cp.TrainStops.Count; i++)
                    {
                        TrainStop stop = cp.TrainStops.ElementAt(i);
                        stop.StopID = $"{pack.Manifest.UniqueID}{i}"; //assigns a unique stopID to every stop
                        stop.TranslatedName = Localize(stop.LocalizedDisplayName);

                        TrainStops.Add(cp.TrainStops.ElementAt(i));
                    }
                }

                if (cp.BoatStops != null)
                {
                    for (int i = 0; i < cp.BoatStops.Count; i++)
                    {
                        BoatStop stop = cp.BoatStops.ElementAt(i);
                        stop.StopID = $"{pack.Manifest.UniqueID}{i}"; //assigns a unique stopID to every stop
                        stop.TranslatedName = Localize(stop.LocalizedDisplayName);

                        BoatStops.Add(cp.BoatStops.ElementAt(i));
                    }
                }

            }

        }
        private void RemoveInvalidLocations()
        {
            for (int i = TrainStops.Count - 1; i >= 0; i--)
            {
                TrainStop stop = TrainStops[i];
                if (Game1.getLocationFromName(stop.TargetMapName) == null)
                {
                    Monitor.Log($"Could not find location {stop.TargetMapName}", LogLevel.Warn);
                    TrainStops.RemoveAt(i);
                }

            }

            for (int i = BoatStops.Count - 1; i >= 0; i--)
            {
                BoatStop stop = BoatStops[i];
                if (Game1.getLocationFromName(stop.TargetMapName) == null)
                {
                    Monitor.Log($"Could not find location {stop.TargetMapName}", LogLevel.Warn);
                    BoatStops.RemoveAt(i);
                }

            }
        }

        /********************
        ** Input detection **
        *********************/

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                if (e.Button != SButton.MouseLeft)
                    return;
                if (e.Cursor.GrabTile != e.Cursor.Tile)
                    return;
            }
            else if (!e.Button.IsActionButton())
                return;

            Vector2 grabTile = e.Cursor.GrabTile;

            string tileProperty = Game1.currentLocation.doesTileHaveProperty((int)grabTile.X, (int)grabTile.Y, "Action", "Buildings");

            VanillaPreconditionsMethod = Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition");
            if (tileProperty == "TrainStation")
            {
                OpenTrainMenu();
            } else if (tileProperty == "BoatTicket" && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
            {
                OpenBoatMenu();
                Helper.Input.Suppress(e.Button);
            }

        }

        public void OpenBoatMenu()
        {
            Response[] responses = GetBoatReponses().ToArray();

            Game1.currentLocation.createQuestionDialogue(Helper.Translation.Get("ChooseDestination"), responses, BoatDestinationPicked);
        }

        private List<Response> GetBoatReponses()
        {
            List<Response> responses = new List<Response>();

            foreach (BoatStop stop in BoatStops)
            {
                if (stop.TargetMapName == Game1.currentLocation.Name) //remove stops to the current map
                    continue;

                if (!ConditionsApi.CheckConditions(stop.Conditions)) //remove stops that don't meet conditions
                    continue;

                string displayName = $"{stop.TranslatedName}";

                if (stop.Cost > 0)
                {
                    displayName += $" - {stop.Cost}g";
                }

                responses.Add(new Response(stop.StopID, displayName));
            }

            if (Game1.currentLocation is BoatTunnel tunnel)
            {
                responses.Add(new Response("GingerIsland",this.Helper.Translation.Get("GingerIsland") + $" - {tunnel.GetTicketPrice()}g"));
            }
            responses.Add(new Response("Cancel", Helper.Translation.Get("MenuCancelOption")));

            return responses;
        }

        public void OpenTrainMenu()
        {
            Response[] responses = GetReponses().ToArray();
            if (responses.Length <= 1) //only 1 response means there's only the cancel option
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("NoDestinations"));
                return;
            }

            Game1.currentLocation.createQuestionDialogue(Helper.Translation.Get("ChooseDestination"), responses, DestinationPicked);
        }

        private List<Response> GetReponses()
        {
            List<Response> responses = new List<Response>();

            foreach (TrainStop stop in TrainStops)
            {
                if (stop.TargetMapName == Game1.currentLocation.Name) //remove stops to the current map
                    continue;

                if (!ConditionsApi.CheckConditions(stop.Conditions)) //remove stops that don't meet conditions
                    continue;

                string displayName = $"{stop.TranslatedName}";

                if (stop.Cost > 0)
                {
                    displayName += $" - {stop.Cost}g";
                }

                responses.Add(new Response(stop.StopID, displayName));
            }

            responses.Add(new Response("Cancel", Helper.Translation.Get("MenuCancelOption")));

            return responses;
        }

        /************************************
        ** Warp after choosing destination **
        *************************************/

        private void BoatDestinationPicked(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "Cancel")
                return;

            if (whichAnswer == "GingerIsland")
            {

                if (Game1.currentLocation is BoatTunnel tunnel)
                {
                    if (Game1.player.Money >= tunnel.GetTicketPrice())
                    {
                        Game1.player.Money -= tunnel.GetTicketPrice();
                        tunnel.StartDeparture();
                    }
                    else if (Game1.player.Money < tunnel.GetTicketPrice())
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                    }
                }
            }

            foreach (BoatStop stop in BoatStops)
            {
                if (stop.StopID == whichAnswer)
                {
                    AttemptToWarpBoat(stop);
                }
            }
        }


        private void DestinationPicked(Farmer who, string whichAnswer)
        {
            if (whichAnswer == "Cancel")
                return;

            foreach (TrainStop stop in TrainStops)
            {
                if (stop.StopID == whichAnswer)
                {
                    AttemptToWarp(stop);
                }
            }
        }
        string destinationMessage;
        ICue cue;

        private void AttemptToWarpBoat(BoatStop stop)
        {

            if (!TryToChargeMoney(stop.Cost))
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("NotEnoughMoney", new { DestinationName = stop.TranslatedName }));
                return;
            }
            LocationRequest request = Game1.getLocationRequest(stop.TargetMapName);
            Game1.warpFarmer(request, stop.TargetX, stop.TargetY, stop.FacingDirectionAfterWarp);
        }
        private void AttemptToWarp(TrainStop stop)
        {

            if (!TryToChargeMoney(stop.Cost))
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("NotEnoughMoney", new { DestinationName = stop.TranslatedName }));
                return;
            }
            LocationRequest request = Game1.getLocationRequest(stop.TargetMapName);
            request.OnWarp += Request_OnWarp;
            destinationMessage = Helper.Translation.Get("ArrivalMessage", new { DestinationName = stop.TranslatedName });

            Game1.warpFarmer(request, stop.TargetX, stop.TargetY, stop.FacingDirectionAfterWarp);

            cue = Game1.soundBank.GetCue("trainLoop");
            cue.SetVariable("Volume", 100f);
            cue.Play();
        }

        private bool finishedTrainWarp = false;

        private void Request_OnWarp()
        {
            Game1.pauseThenMessage(3000, destinationMessage, false);
            finishedTrainWarp = true;
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (!finishedTrainWarp)
                return;

            if (e.NewMenu is DialogueBox)
            {
                AfterWarpPause();
            }
            finishedTrainWarp = false;
        }

        private void AfterWarpPause()
        {
            //Game1.drawObjectDialogue(destinationMessage);
            Game1.playSound("trainWhistle");
            cue.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.AsAuthored);
        }

        /******************
        **    Utility    **
        *******************/

        public static IReflectedMethod VanillaPreconditionsMethod;

        private bool CheckConditions(string conditions)
        {
            if (string.IsNullOrEmpty(conditions))
                return true;

            int result = VanillaPreconditionsMethod.Invoke<int>("-5005/" + conditions);
            return result != -1;
        }

        private bool TryToChargeMoney(int cost)
        {
            if (Game1.player.Money < cost)
            {
                return false;
            }

            Game1.player.Money -= cost;
            return true;

        }

        /***********************
        ** Localization stuff **
        ************************/

        private static LocalizedContentManager.LanguageCode selectedLanguage;
        private void UpdateSelectedLanguage()
        {
            selectedLanguage = LocalizedContentManager.CurrentLanguageCode;
        }

        private string Localize(Dictionary<string, string> translations)
        {
            if (!translations.ContainsKey(selectedLanguage.ToString()))
            {
                return translations.ContainsKey("en") ? translations["en"] : "No translation";
            }

            return translations[selectedLanguage.ToString()];
        }
    }

    /*******************
    ** Content models **
    ********************/

    public class ModConfig
    {
        public int TicketStationX = 32;
        public int TicketStationY = 40;
        public int RailroadWarpX = 32;
        public int RailroadWarpY = 42;
    }

    public class ContentPack
    {
        public List<TrainStop> TrainStops { get; set; }
        public List<BoatStop> BoatStops { get; set; }
    }

    public class TrainStop
    {
        public string TargetMapName { get; set; }
        public Dictionary<string, string> LocalizedDisplayName { get; set; }

        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public int Cost { get; set; } = 0;
        public int FacingDirectionAfterWarp { get; set; } = 2;
        public string[] Conditions { get; set; }

        internal string StopID; //assigned by the mod's uniqueID and the number of stops from that pack
        internal string TranslatedName;
    }

    public class BoatStop
    {
        public string TargetMapName { get; set; }
            public Dictionary<string, string> LocalizedDisplayName { get; set; }

            public int TargetX { get; set; }
            public int TargetY { get; set; }
            public int Cost { get; set; } = 0;
            public int FacingDirectionAfterWarp { get; set; } = 2;
            public string[] Conditions { get; set; }

            internal string StopID; //assigned by the mod's uniqueID and the number of stops from that pack
            internal string TranslatedName;
    }

    public interface IApi
    {
        void OpenTrainMenu();
        void OpenBoatMenu();
    }

    public class Api : IApi
    {
        public void OpenTrainMenu()
        {
            ModEntry.Instance.OpenTrainMenu();
        }

        public void OpenBoatMenu()
        {
            ModEntry.Instance.OpenBoatMenu();
        }
    }

    public interface IConditionsChecker
    {
        /// <summary>
        /// Must be called before any condition checking is done. Verbose mode will turn on logging for every step of the condition checking process
        /// </summary>
        /// <param name="verbose">Turning verbose mode true will log every step of the condition checking process. Useful for debugging but spams the debug log. It is recommended to have this false during release, or provided in a config set to a default of false.</param>
        /// <param name="uniqueId">The unique ID of your mod. Will be prepended to all logs so it is clear which mod called the condition checking</param>
        void Initialize(bool verbose, string uniqueId);

        /// <summary>
        /// Checks an array of condition strings. Each string will be evaluated as true if every single condition provided is true. All the strings together will evaluate as true if any string is true
        /// </summary>
        /// <param name="conditions">An array of condition strings.</param>
        /// <returns></returns>
        bool CheckConditions(string[] conditions);

        /// <summary>
        /// Checks a single condition string. The string will be evaluated as true if every single condition provided is true.
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        bool CheckConditions(string conditions);
    }
}
