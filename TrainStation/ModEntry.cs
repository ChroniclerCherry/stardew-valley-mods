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
        this.ContentManager = new(this.ModManifest.UniqueID, () => this.Config, helper.GameContent, this.Monitor, helper.ModRegistry.IsLoaded("Cherry.ExpandedPreconditionsUtility"));

        helper.Events.Content.AssetRequested += this.ContentManager.OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Player.Warped += this.OnWarped;
    }

    /// <inheritdoc />
    public override object GetApi(IModInfo mod)
    {
        return new Api(this.ContentManager, this.OpenMenu, mod.Manifest.Name);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Open the menu to choose a boat or train destination.</summary>
    /// <param name="network">The network for which to get stops.</param>
    private void OpenMenu(StopNetwork network)
    {
        StopModel[] stops = this.ContentManager.GetAvailableStops(network).ToArray();
        if (stops.Length == 0)
        {
            Game1.drawObjectDialogue(I18n.NoDestinations());
            return;
        }

        Response[] responses = this.GetResponses(stops).ToArray();
        Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), responses, (_, selectedId) => this.OnDestinationPicked(selectedId, stops, network));
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
            this.OpenMenu(StopNetwork.Train);
        else if (tileProperty == "BoatTicket" && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed") && this.ContentManager.GetAvailableStops(StopNetwork.Boat).Any())
        {
            this.OpenMenu(StopNetwork.Boat);
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private List<Response> GetResponses(StopModel[] stops)
    {
        List<Response> responses = new List<Response>();

        foreach (StopModel stop in stops)
        {
            string label = stop.Cost > 0
                ? Game1.content.LoadString("Strings\\Locations:MineCart_DestinationWithPrice", stop.DisplayName, Utility.getNumberWithCommas(stop.Cost))
                : stop.DisplayName;

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
    /// <param name="network">The network containing the stop.</param>
    private void OnDestinationPicked(string stopId, StopModel[] stops, StopNetwork network)
    {
        // special cases
        switch (stopId)
        {
            case "Cancel":
                return;

            case "Cherry.TrainStation_GingerIsland" when network is StopNetwork.Boat && Game1.currentLocation is BoatTunnel tunnel:
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
            Game1.drawObjectDialogue(I18n.NotEnoughMoney(destinationName: stop.DisplayName));
            return;
        }

        // parse facing direction
        if (!Utility.TryParseDirection(stop.ToFacingDirection, out int toFacingDirection))
            toFacingDirection = Game1.down;

        // warp
        LocationRequest request = Game1.getLocationRequest(stop.ToLocation);
        if (network is StopNetwork.Train)
        {
            request.OnWarp += this.OnTrainWarped;
            this.DestinationMessage = I18n.ArrivalMessage(destinationName: stop.DisplayName);

            this.Cue = Game1.soundBank.GetCue("trainLoop");
            this.Cue.SetVariable("Volume", 100f);
            this.Cue.Play();
        }

        Game1.warpFarmer(request, stop.ToTile.X, stop.ToTile.Y, toFacingDirection);
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
