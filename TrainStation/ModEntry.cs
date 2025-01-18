using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using TrainStation.Framework;
using TrainStation.Framework.ContentModels;

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
        this.ContentManager = new(this.ModManifest.UniqueID, () => this.Config, helper.GameContent, this.Monitor);

        helper.Events.Content.AssetRequested += this.ContentManager.OnAssetRequested;
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

    /// <summary>Open the menu to choose a boat destination.</summary>
    public void OpenBoatMenu()
    {
        this.OpenMenu(this.ContentManager.GetAvailableBoatStops().ToArray(), isBoat: true);
    }

    /// <summary>Open the menu to choose a train destination.</summary>
    public void OpenTrainMenu()
    {
        this.OpenMenu(this.ContentManager.GetAvailableTrainStops().ToArray(), isBoat: false);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Open the menu to choose a boat or train destination.</summary>
    /// <param name="stops">The boat or train stops to choose from.</param>
    /// <param name="isBoat">Whether we're traveling by boat; else by train.</param>
    private void OpenMenu(StopModel[] stops, bool isBoat)
    {
        Response[] responses = this.GetResponses(stops).ToArray();
        if (responses.Length <= 1)
        {
            Game1.drawObjectDialogue(I18n.NoDestinations());
            return;
        }

        Game1.currentLocation.createQuestionDialogue(I18n.ChooseDestination(), responses, (_, selectedId) => this.DestinationPicked(selectedId, stops, isBoat));
    }

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
        // load Expanded Preconditions Utility
        this.ConditionsApi = this.Helper.ModRegistry.GetApi<IConditionsChecker>("Cherry.ExpandedPreconditionsUtility");
        this.ConditionsApi?.Initialize(false, this.ModManifest.UniqueID);
        if (this.ConditionsApi == null)
            this.Monitor.Log("Expanded Preconditions Utility API not detected. Something went wrong, please check that your installation of Expanded Preconditions Utility is valid", LogLevel.Error);

        // load content packs
        this.ContentManager.LoadContentPacks(this.Helper.ContentPacks.GetOwned());
    }


    /****
    ** Save loaded
    ****/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.ContentManager.ResetAsset();
        this.DrawInTicketStation();
    }

    private void DrawInTicketStation()
    {
        // draw ticket machine
        try
        {
            GameLocation railroad = Game1.RequireLocation("Railroad");

            railroad.setMapTile(this.Config.TicketStationX, this.Config.TicketStationY, this.TicketStationBottomTile, "Buildings", GameLocation.DefaultTileSheetId, action: "TrainStation");
            railroad.setMapTile(this.Config.TicketStationX, this.Config.TicketStationY - 1, this.TicketStationTopTile, "Front", GameLocation.DefaultTileSheetId);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Train Station couldn't add the ticket machine to the railroad. This is likely due to another mod changing the tilesheets in a non-recommended way.\n\nTechnical details: {ex}", LogLevel.Error);
        }
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

    private List<Response> GetResponses(StopModel[] stops)
    {
        List<Response> responses = new List<Response>();

        foreach (StopModel stop in stops)
        {
            string displayName = stop.DisplayName;
            if (stop.Cost > 0)
                displayName += $" - {stop.Cost}g";

            responses.Add(new Response(stop.Id, displayName));
        }

        responses.Add(new Response("Cancel", I18n.MenuCancelOption()));

        return responses;
    }


    /****
    ** Warp after choosing destination
    ****/
    private void DestinationPicked(string selectedId, StopModel[] stops, bool isBoat)
    {
        // special cases
        switch (selectedId)
        {
            case "Cancel":
                return;

            case "Cherry.TrainStation_GingerIsland" when Game1.currentLocation is BoatTunnel tunnel:
                if (this.TryToChargeMoney(tunnel.TicketPrice))
                    tunnel.StartDeparture();
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                return;
        }

        // get stop
        StopModel stop = stops.FirstOrDefault(s => s.Id == selectedId);
        if (stop is null)
            return;

        // charge ticket price
        if (!this.TryToChargeMoney(stop.Cost))
        {
            Game1.drawObjectDialogue(I18n.NotEnoughMoney(destinationName: stop.DisplayName));
            return;
        }

        // warp
        LocationRequest request = Game1.getLocationRequest(stop.ToLocation);
        if (!isBoat)
        {
            request.OnWarp += this.OnTrainWarped;
            this.DestinationMessage = I18n.ArrivalMessage(destinationName: stop.DisplayName);

            this.Cue = Game1.soundBank.GetCue("trainLoop");
            this.Cue.SetVariable("Volume", 100f);
            this.Cue.Play();
        }
        Game1.warpFarmer(request, stop.ToTile.X, stop.ToTile.Y, stop.ToFacingDirection);
    }

    private void OnTrainWarped()
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
