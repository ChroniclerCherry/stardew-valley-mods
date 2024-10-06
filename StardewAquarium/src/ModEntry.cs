using System;
using System.IO;

using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewAquarium.Editors;
using StardewAquarium.Menus;
using StardewAquarium.Models;
using StardewAquarium.Patches;
using StardewAquarium.src;
using StardewAquarium.src.Editors;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

using SObject = StardewValley.Object;

namespace StardewAquarium;

internal sealed class ModEntry : Mod
{
    internal static ModConfig Config { get; private set; } = null!;
    internal static ModData Data { get; private set; } = null!;

    public const string PufferChickName = "Pufferchick";
    public const string LegendaryBaitName = "Legendary Bait";
    private readonly bool _isAndroid = Constants.TargetPlatform == GamePlatform.Android;
    public static Harmony Harmony { get; } = new Harmony("Cherry.StardewAquarium");

    private AchievementEditor AchievementEditor;
    private FishEditor FishEditor;
    private ObjectEditor ObjectEditor;
    private MiscEditor MiscEditor;

    public static IJsonAssetsApi JsonAssets { get; set; }

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);

        Utils.Initialize(this.Helper, this.Monitor, this.ModManifest);
        TileActions.Init(helper, this.Monitor);
        AquariumMessage.Initialize(this.Helper);

        AssetEditor.Init(this.Helper.GameContent, this.Helper.Events.Content, this.Monitor);

        this.AchievementEditor = new(this.Helper);
        this.FishEditor = new(this.Helper);
        this.ObjectEditor = new();
        this.MiscEditor = new(this.Helper);

        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
        this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStart;

        if (this._isAndroid)
        {
            AndroidShopMenuPatch.Initialize(this.Helper, this.Monitor);
            this.Helper.Events.Display.MenuChanged += this.AndroidPlsHaveMercyOnMe;
        }

        new ReturnTrain(this.Helper, this.Monitor);

        Config = this.Helper.ReadConfig<ModConfig>();

        string dataPath = Path.Combine("data", "data.json");
        Data = helper.Data.ReadJsonFile<ModData>(dataPath);

        if (Config.EnableDebugCommands
#if DEBUG
            || true
#endif
            )
        {
            if (this._isAndroid || true)
                this.Helper.ConsoleCommands.Add("donatefish", "", this.AndroidDonateFish);
            else
                this.Helper.ConsoleCommands.Add("donatefish", "", this.OpenDonationMenuCommand);

            this.Helper.ConsoleCommands.Add("aquariumprogress", "", this.OpenAquariumCollectionMenu);
            this.Helper.ConsoleCommands.Add("removedonatedfish", "", this.RemoveDonatedFish);
        }
    }

    /// <summary>
    /// Updates crab pots
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDayStart(object sender, DayStartedEventArgs e)
    {
        var loc = Game1.getLocationFromName(Data.ExteriorMapName);
        if (loc is null)
            return;

        // the original method actually flat out re-implemented the code for crab pots. We're...not going to do that anymore.
        // it was meant to mimick crabpot behavior on the beach, which is now in the data.

        // HOWEVER that wasn't actually what it did. Instead, it just...caught something. Even if not baited.
        // We will mimic this by baiting the crabpots ourselves.

        foreach (var obj in loc.objects.Values)
        {
            if (obj is not CrabPot pot || (pot.heldObject.Value is not null && pot.heldObject.Value.Category != SObject.junkCategory))
            {
                continue;
            }

            try
            {
                pot.bait.Value ??= new SObject("685", 1); // normal bait.
                pot.DayUpdate();
            }
            catch (Exception ex)
            {
                this.Monitor.Log(ex.ToString());
            }
        }
    }

    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (this.AchievementEditor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.AchievementEditor.Edit);
        else if (this.FishEditor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.FishEditor.Edit);
        else if (this.ObjectEditor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.ObjectEditor.Edit);
        else if (this.MiscEditor.CanEdit(e.NameWithoutLocale))
            e.Edit(this.MiscEditor.Edit);
    }

    private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (Context.CanPlayerMove && Config.CheckDonationCollection == e.Button)
        {
            Game1.activeClickableMenu = new AquariumCollectionMenu(this.Helper.Translation.Get("CollectionsMenu"));
        }
    }

    private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (Game1.isTimePaused) return;
        //This code was borrowed from East Scarpe

        // Very rarely show the Sea Monster.
        if (Game1.eventUp || !(Game1.random.NextDouble() < Data.DolphinChance))
            return;
        if (Game1.currentLocation?.Name != Data.ExteriorMapName)
            return;

        // Randomly find a starting position within the range.
        Vector2 position = 64f * new Vector2
        (Game1.random.Next(Data.DolphinRange.Left,
                Data.DolphinRange.Right + 1),
            Game1.random.Next(Data.DolphinRange.Top,
                Data.DolphinRange.Bottom + 1));

        var loc = Game1.currentLocation;

        // Confirm there is water tiles in the 3x2 area the dolphin spawns in
        Vector2[] tiles = [ new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 0),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, 1) ];
        foreach (var tile in tiles)
        {
            if (loc.doesTileHaveProperty((int)((position.X / 64) + tile.X), (int)((position.Y / 64) + tile.Y), "Water", "Back") == null)
            {
                return;
            }
        }

        loc.temporarySprites.Add(new DolphinAnimatedSprite(position, this.Helper.ModContent.Load<Texture2D>("data/dolphin.png")));
    }

    private void AndroidPlsHaveMercyOnMe(object sender, MenuChangedEventArgs e)
    {
        //don't ask me what the heck is going on here but its the only way to get it to work
        if (e.OldMenu is not DonateFishMenuAndroid androidMenu) return;
        //80% sure this is a DonateFishMenuAndroid but it won't work if i check for that but the harmony patch seems to work on it so idk
        if (e.NewMenu is not ShopMenu menu) return;

        menu.exitFunction = androidMenu.OnExit;
    }

    private void AndroidDonateFish(string arg1, string[] arg2)
    {
        Game1.activeClickableMenu = new DonateFishMenuAndroid(this.Helper, this.Monitor);
    }

    private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (!Context.IsMainPlayer)
        {
            IMultiplayerPeer master = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID);
            IMultiplayerPeerMod us = master.GetMod(this.ModManifest.UniqueID);
            if (us is null)
            {
                this.Monitor.Log($"Host seems to be missing Stardew Aquarium. Certain features may not work as advertised.", LogLevel.Error);
            }
        }

        if (Utils.CheckAchievement())
            Utils.UnlockAchievement();

    }
    
    private void RemoveDonatedFish(string arg1, string[] arg2)
    {
        Game1.MasterPlayer.mailReceived.RemoveWhere(item => item.StartsWith("AquariumDonated:") || item.StartsWith("AquariumFishDonated:"));
    }

    private void OpenAquariumCollectionMenu(string arg1, string[] arg2)
    {
        Game1.activeClickableMenu = new AquariumCollectionMenu(I18n.CollectionsMenu());
    }

    private void OpenDonationMenuCommand(string arg1, string[] arg2)
    {
        Game1.activeClickableMenu = new DonateFishMenu(this.Helper, this.Monitor);
    }

    private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
    {
        JsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
        JsonAssets.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "data"));

        Event.RegisterCommand("GiveAquariumTrophy1", GiveAquariumTrophy1);
        Event.RegisterCommand("GiveAquariumTrophy2", GiveAquariumTrophy2);
    }

    public static void GiveAquariumTrophy1(Event e, string[] args, EventContext context)
    {
        string id = JsonAssets.GetBigCraftableId("Stardew Aquarium Trophy");
        SObject trophy = ItemRegistry.Create<SObject>(id);
        e.farmer.holdUpItemThenMessage(trophy, true);
        ++e.CurrentCommand;
    }
    public static void GiveAquariumTrophy2(Event e, string[] args, EventContext context)
    {
        string id = JsonAssets.GetBigCraftableId("Stardew Aquarium Trophy");
        SObject trophy = new(Vector2.Zero, id);
        e.farmer.addItemByMenuIfNecessary(trophy);
        if (Game1.activeClickableMenu == null)
            ++e.CurrentCommand;
        ++e.CurrentCommand;
    }
}
