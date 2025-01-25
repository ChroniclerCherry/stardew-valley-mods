using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    /// <summary>The mod settings.</summary>
    private ModConfig Config;

    /// <summary>The Expanded Preconditions Utility API, if available.</summary>
    private IConditionsChecker ConditionsApi;

    /// <summary>Manages the available boat and train stops.</summary>
    private StopManager StopManager;

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
        this.StopManager = new(this.ModManifest.UniqueID, () => this.Config, helper.ModRegistry.IsLoaded("Cherry.ExpandedPreconditionsUtility"), () => this.ConditionsApi, this.Monitor);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Player.Warped += this.OnWarped;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod)
    {
        return new Api(this.StopManager, this.OpenMenu, mod.Manifest.Name);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Open the menu to choose a boat or train destination.</summary>
    /// <param name="isBoat">Whether to open the boat menu (true) or train menu (false).</param>
    private void OpenMenu(bool isBoat)
    {
        StopModel[] stops = this.StopManager.GetAvailableStops(isBoat).ToArray();
        if (stops.Length == 0)
        {
            Game1.drawObjectDialogue(I18n.NoDestinations());
            return;
        }

        Response[] responses = this.GetResponses(stops).ToArray();
        Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), responses, (_, selectedId) => this.OnDestinationPicked(selectedId, stops, isBoat));
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

        // load content packs
        this.StopManager.LoadContentPacks(this.Helper.ContentPacks.GetOwned());
    }


    /****
    ** Save loaded
    ****/
    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.StopManager.ResetData();
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
            this.OpenMenu(isBoat: false);
        else if (tileProperty == "BoatTicket" && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed") && this.StopManager.GetAvailableStops(isBoat: true).Any())
        {
            this.OpenMenu(isBoat: true);
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private List<Response> GetResponses(StopModel[] stops)
    {
        List<Response> responses = new List<Response>();

        foreach (StopModel stop in stops)
        {
            string label = stop.Cost > 0
                ? Game1.content.LoadString("Strings\\Locations:MineCart_DestinationWithPrice", stop.GetDisplayName(), Utility.getNumberWithCommas(stop.Cost))
                : stop.GetDisplayName();

            responses.Add(new Response(stop.Id, label));
        }

        responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));

        return responses;
    }


    /****
    ** Warp after choosing destination
    ****/
    /// <summary>Handle the player choosing a destination in the UI.</summary>
    /// <param name="stopId">The selected stop ID.</param>
    /// <param name="stops">The stops which the player chose from.</param>
    /// <param name="isBoat">Whether the player chose a boat destination (true) or train destination (false).</param>
    private void OnDestinationPicked(string stopId, StopModel[] stops, bool isBoat)
    {
        // special cases
        switch (stopId)
        {
            case "Cancel":
                return;

            case "Cherry.TrainStation_GingerIsland" when isBoat && Game1.currentLocation is BoatTunnel tunnel:
                if (this.TryToChargeMoney(tunnel.TicketPrice))
                    tunnel.StartDeparture();
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
                return;
        }

        // get stop
        StopModel stop = stops.FirstOrDefault(s => s.Id == stopId);
        if (stop is null)
            return;

        // charge ticket price
        if (!this.TryToChargeMoney(stop.Cost))
        {
            Game1.drawObjectDialogue(I18n.NotEnoughMoney(destinationName: stop.GetDisplayName()));
            return;
        }

        // warp
        LocationRequest request = Game1.getLocationRequest(stop.TargetMapName);
        if (!isBoat)
        {
            request.OnWarp += this.OnTrainWarped;
            this.DestinationMessage = I18n.ArrivalMessage(destinationName: stop.GetDisplayName());

            this.Cue = Game1.soundBank.GetCue("trainLoop");
            this.Cue.SetVariable("Volume", 100f);
            this.Cue.Play();
        }

        Game1.warpFarmer(request, stop.TargetX, stop.TargetY, stop.FacingDirectionAfterWarp);
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
        this.Cue.Stop(AudioStopOptions.AsAuthored);
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
