using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using TrainStation.Framework;
using xTile.Layers;
using xTile.Tiles;

namespace TrainStation;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    private ModConfig Config;
    private IConditionsChecker ConditionsApi;

    /// <summary>Manages the Train Station content provided by content packs.</summary>
    private ContentManager ContentManager;

    private readonly int TicketStationTopTile = 1032;
    private readonly int TicketStationBottomTile = 1057;
    private string DestinationMessage;
    private ICue Cue;
    private bool FinishedTrainWarp;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        this.Config = helper.ReadConfig<ModConfig>();
        this.ContentManager = new(() => this.Config, this.Monitor);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Player.Warped += this.OnWarped;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new Api(this.ContentManager, this.OpenBoatMenu, this.OpenTrainMenu);
    }

    public void OpenBoatMenu()
    {
        Response[] responses = this.GetBoatReponses().ToArray();

        Game1.currentLocation.createQuestionDialogue(I18n.ChooseDestination(), responses, this.BoatDestinationPicked);
    }

    public void OpenTrainMenu()
    {
        Response[] responses = this.GetReponses().ToArray();
        if (responses.Length <= 1) //only 1 response means there's only the cancel option
        {
            Game1.drawObjectDialogue(I18n.NoDestinations());
            return;
        }

        Game1.currentLocation.createQuestionDialogue(I18n.ChooseDestination(), responses, this.DestinationPicked);
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IPlayerEvents.Warped" />
    private void OnWarped(object sender, WarpedEventArgs e)
    {
        if (e.NewLocation.Name != "Railroad") return;

        string property = e.NewLocation.doesTileHaveProperty(this.Config.TicketStationX, this.Config.TicketStationY,
            "Action", "Buildings");
        if (property != "TrainStation")
            this.DrawInTicketStation();
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.ConditionsApi = this.Helper.ModRegistry.GetApi<IConditionsChecker>("Cherry.ExpandedPreconditionsUtility");
        if (this.ConditionsApi == null)
        {
            this.Monitor.Log("Expanded Preconditions Utility API not detected. Something went wrong, please check that your installation of Expanded Preconditions Utility is valid", LogLevel.Error);
            return;
        }

        this.ConditionsApi.Initialize(false, this.ModManifest.UniqueID);
    }


    /****
    ** Save loaded
    ****/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.ContentManager.UpdateSelectedLanguage();
        this.ContentManager.LoadContentPacks(this.Helper.ContentPacks.GetOwned());

        this.DrawInTicketStation();

        this.ContentManager.RemoveInvalidLocations();
    }

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
            buildingsLayer.Tiles[this.Config.TicketStationX, this.Config.TicketStationY] =
                new StaticTile(buildingsLayer, outdoorsTilesheet, BlendMode.Alpha, this.TicketStationBottomTile);
            frontLayer.Tiles[this.Config.TicketStationX, this.Config.TicketStationY - 1] =
                new StaticTile(frontLayer, outdoorsTilesheet, BlendMode.Alpha, this.TicketStationTopTile);
        }
        catch (Exception e)
        {
            this.Monitor.Log(e.ToString(), LogLevel.Error);
            this.Monitor.Log("Train station has recovered from a crash and will continue to function, however the ticket station may be invisible or looked glitched. This is caused by the map mod you are using changing tilesheet orders through renaming vanilla tilesheets or not naming custom tilesheets properly. Please report this to the map mod you are using to fix this issue.", LogLevel.Alert);
            //draw anything from the tilesheet
            buildingsLayer.Tiles[this.Config.TicketStationX, this.Config.TicketStationY] =
                new StaticTile(buildingsLayer, outdoorsTilesheet, BlendMode.Alpha, 1);
            frontLayer.Tiles[this.Config.TicketStationX, this.Config.TicketStationY - 1] =
                new StaticTile(frontLayer, outdoorsTilesheet, BlendMode.Alpha, 1);
        }

        //set the TrainStation property
        railway.setTileProperty(this.Config.TicketStationX, this.Config.TicketStationY, "Buildings", "Action", "TrainStation");

        railway.map.LoadTileSheets(Game1.mapDisplayDevice);
    }


    /****
    ** Input detection
    ****/
    /// <inheritdoc cref="IInputEvents.ButtonPressed" />
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
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

        if (tileProperty == "TrainStation")
        {
            this.OpenTrainMenu();
        }
        else if (this.ContentManager.BoatStops.Count > 0 && tileProperty == "BoatTicket" && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
        {
            this.OpenBoatMenu();
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private List<Response> GetBoatReponses()
    {
        List<Response> responses = new List<Response>();

        foreach (BoatStop stop in this.ContentManager.BoatStops)
        {
            if (stop.TargetMapName == Game1.currentLocation.Name) //remove stops to the current map
                continue;

            if (!this.ConditionsApi.CheckConditions(stop.Conditions)) //remove stops that don't meet conditions
                continue;

            string displayName = $"{stop.TranslatedName}";

            if (stop.Cost > 0)
            {
                displayName += $" - {stop.Cost}g";
            }

            responses.Add(new Response(stop.StopId, displayName));
        }

        if (Game1.currentLocation is BoatTunnel tunnel)
        {
            responses.Add(new Response("GingerIsland", I18n.GingerIsland() + $" - {tunnel.TicketPrice}g"));
        }
        responses.Add(new Response("Cancel", I18n.MenuCancelOption()));

        return responses;
    }

    private List<Response> GetReponses()
    {
        List<Response> responses = new List<Response>();

        foreach (TrainStop stop in this.ContentManager.TrainStops)
        {
            if (stop.TargetMapName == Game1.currentLocation.Name) //remove stops to the current map
                continue;

            if (!this.ConditionsApi.CheckConditions(stop.Conditions)) //remove stops that don't meet conditions
                continue;

            string displayName = $"{stop.TranslatedName}";

            if (stop.Cost > 0)
            {
                displayName += $" - {stop.Cost}g";
            }

            responses.Add(new Response(stop.StopId, displayName));
        }

        responses.Add(new Response("Cancel", I18n.MenuCancelOption()));

        return responses;
    }


    /****
    ** Warp after choosing destination
    ****/
    private void BoatDestinationPicked(Farmer who, string whichAnswer)
    {
        if (whichAnswer == "Cancel")
            return;

        if (whichAnswer == "GingerIsland")
        {
            if (Game1.currentLocation is BoatTunnel tunnel)
            {
                if (Game1.player.Money >= tunnel.TicketPrice)
                {
                    Game1.player.Money -= tunnel.TicketPrice;
                    tunnel.StartDeparture();
                }
                else if (Game1.player.Money < tunnel.TicketPrice)
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                }
            }
        }

        foreach (BoatStop stop in this.ContentManager.BoatStops)
        {
            if (stop.StopId == whichAnswer)
            {
                this.AttemptToWarpBoat(stop);
            }
        }
    }

    private void DestinationPicked(Farmer who, string whichAnswer)
    {
        if (whichAnswer == "Cancel")
            return;

        foreach (TrainStop stop in this.ContentManager.TrainStops)
        {
            if (stop.StopId == whichAnswer)
            {
                this.AttemptToWarp(stop);
            }
        }
    }

    private void AttemptToWarpBoat(BoatStop stop)
    {
        if (!this.TryToChargeMoney(stop.Cost))
        {
            Game1.drawObjectDialogue(I18n.NotEnoughMoney(destinationName: stop.TranslatedName));
            return;
        }
        LocationRequest request = Game1.getLocationRequest(stop.TargetMapName);
        Game1.warpFarmer(request, stop.TargetX, stop.TargetY, stop.FacingDirectionAfterWarp);
    }

    private void AttemptToWarp(TrainStop stop)
    {
        if (!this.TryToChargeMoney(stop.Cost))
        {
            Game1.drawObjectDialogue(I18n.NotEnoughMoney(destinationName: stop.TranslatedName));
            return;
        }
        LocationRequest request = Game1.getLocationRequest(stop.TargetMapName);
        request.OnWarp += this.Request_OnWarp;
        this.DestinationMessage = I18n.ArrivalMessage(destinationName: stop.TranslatedName);

        Game1.warpFarmer(request, stop.TargetX, stop.TargetY, stop.FacingDirectionAfterWarp);

        this.Cue = Game1.soundBank.GetCue("trainLoop");
        this.Cue.SetVariable("Volume", 100f);
        this.Cue.Play();
    }

    private void Request_OnWarp()
    {
        if (Game1.currentLocation?.currentEvent is null)
            Game1.pauseThenMessage(3000, this.DestinationMessage);

        this.FinishedTrainWarp = true;
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (!this.FinishedTrainWarp)
            return;

        if (e.NewMenu is DialogueBox)
        {
            this.AfterWarpPause();
        }

        this.FinishedTrainWarp = false;
    }

    private void AfterWarpPause()
    {
        //Game1.drawObjectDialogue(destinationMessage);
        Game1.playSound("trainWhistle");
        this.Cue.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.AsAuthored);
    }


    /****
    ** Utility
    ****/
    private bool TryToChargeMoney(int cost)
    {
        if (Game1.player.Money < cost)
        {
            return false;
        }

        Game1.player.Money -= cost;
        return true;
    }
}
