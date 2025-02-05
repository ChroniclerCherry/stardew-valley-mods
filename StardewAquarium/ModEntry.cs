using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewAquarium.Framework;
using StardewAquarium.Framework.Editors;
using StardewAquarium.Framework.Framework;
using StardewAquarium.Framework.Menus;
using StardewAquarium.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Menus;

namespace StardewAquarium;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private ModConfig Config = null!;

    /// <summary>The chance that a dolphin Easter egg appears in the player's current location.</summary>
    private readonly PerScreen<float> DolphinChance = new();

    /// <summary>The tile area where the dolphin Easter egg can appear in the player's current location, if applicable.</summary>
    private readonly PerScreen<Rectangle> DolphinRange = new();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        GamePatcher.Apply(this.ModManifest.UniqueID, this.Monitor, helper.Reflection, helper.Translation);

        Utils.Initialize(helper, this.Monitor, this.ModManifest);
        TileActions.Init(this.Monitor);

        AssetEditor.Init(helper.Events.Content, this.Monitor);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.Player.Warped += this.OnWarped;

        CrabPotHandler.Init(helper.Events.GameLoop, this.Monitor);

        if (Constants.TargetPlatform == GamePlatform.Android)
        {
            helper.Events.Display.MenuChanged += this.AndroidPlsHaveMercyOnMe;
        }

        this.Config = helper.ReadConfig<ModConfig>();

#if !DEBUG
        if (Config.EnableDebugCommands)
#endif
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                helper.ConsoleCommands.Add("donatefish", "", this.AndroidDonateFish);
            else
                helper.ConsoleCommands.Add("donatefish", "", this.OpenDonationMenuCommand);

            helper.ConsoleCommands.Add("aquariumprogress", "", this.OpenAquariumCollectionMenu);
            helper.ConsoleCommands.Add("removedonatedfish", "", this.RemoveDonatedFish);
            helper.ConsoleCommands.Add("spawn_missing_fishes", "Fills the player's inventory with fishes they have not donated yet.", this.SpawnMissingFish);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        // stats are not reliable in multiplayer
        // thus, we set a flag when the stat WOULD be set instead.
        if (Game1.player.hasOrWillReceiveMail(AssetEditor.AquariumPlayerHasChicken))
            return;

        Farmer player = Game1.player;
        Utility.ForEachLocation(loc =>
        {
            if (loc.Animals?.Count() is 0 or null)
                return true;

            foreach (FarmAnimal animal in loc.Animals.Values)
            {
                if (animal.ownerID?.Value == player.UniqueMultiplayerID && animal.isAdult())
                {
                    if (animal.GetAnimalData()?.StatToIncrementOnProduce
                        ?.Any(static stat => StatKeys.ChickenEggsLayed.Equals(stat.StatName, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        player.mailReceived.Add(AssetEditor.AquariumPlayerHasChicken);
                        return false;
                    }
                }
            }

            return true;
        },
        includeInteriors: true);
    }

    /// <summary>Fill the inventory with un-donated fish.</summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private void SpawnMissingFish(string command, string[] args)
    {
        if (!Context.IsWorldReady)
            return;

        foreach ((string key, ObjectData data) in Game1.objectData)
        {
            if (data.Category != -4)
                continue;

            if (Utils.IsUnDonatedFish(data.Name))
            {
                if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create(ItemRegistry.ManuallyQualifyItemId(key, ItemRegistry.type_object))))
                {
                    break;
                }
            }
        }
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.CanPlayerMove)
            return;

        if (this.Config.CheckDonationCollection.JustPressed())
            Game1.activeClickableMenu = new AquariumCollectionMenu(ContentPackHelper.LoadString("CollectionsMenu"));
    }

    /// <inheritdoc cref="IPlayerEvents.Warped" />
    private void OnWarped(object sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer)
            return;

        // load dolphin Easter egg info
        this.DolphinRange.Value = e.NewLocation.TryGetMapPropertyAs($"{ContentPackHelper.ContentPackId}_DolphinRange", out Rectangle rawRange)
            ? rawRange
            : Rectangle.Empty;
        if (this.DolphinRange.Value != Rectangle.Empty)
        {
            this.DolphinChance.Value = e.NewLocation.TryGetMapPropertyAs($"{ContentPackHelper.ContentPackId}_DolphinChance", out double rawChance)
                ? (float)rawChance
                : 0.00001f;
        }
        else
            this.DolphinChance.Value = 0;
    }

    /// <inheritdoc cref="IGameLoopEvents.UpdateTicked" />
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (Game1.isTimePaused) return;

        // very rarely show dolphin
        //This is derived from East Scarpe's sea monster code.
        if (Game1.eventUp || !Game1.random.NextBool(this.DolphinChance.Value))
            return;

        // Randomly find a starting position within the range.
        Vector2 position = 64f * new Vector2(
            Game1.random.Next(this.DolphinRange.Value.Left, this.DolphinRange.Value.Right + 1),
            Game1.random.Next(this.DolphinRange.Value.Top, this.DolphinRange.Value.Bottom + 1)
        );

        GameLocation loc = Game1.currentLocation;

        // Confirm there is water tiles in the 3x2 area the dolphin spawns in
        Vector2[] tiles = [
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(2, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(2, 1)
        ];
        foreach (Vector2 tile in tiles)
        {
            if (loc.doesTileHaveProperty((int)((position.X / 64) + tile.X), (int)((position.Y / 64) + tile.Y), "Water", "Back") == null)
            {
                return;
            }
        }

        loc.temporarySprites.Add(new DolphinAnimatedSprite(position, this.Helper.GameContent.Load<Texture2D>($"Mods/{ContentPackHelper.ContentPackId}/Dolphin")));
    }

    private void AndroidPlsHaveMercyOnMe(object sender, MenuChangedEventArgs e)
    {
        //don't ask me what the heck is going on here but its the only way to get it to work
        if (e.OldMenu is not DonateFishMenuAndroid androidMenu)
            return;
        //80% sure this is a DonateFishMenuAndroid but it won't work if i check for that but the harmony patch seems to work on it so idk
        if (e.NewMenu is not ShopMenu menu)
            return;

        menu.exitFunction += androidMenu.OnExit;
    }

    private void AndroidDonateFish(string arg1, string[] arg2)
    {
        Game1.activeClickableMenu = new DonateFishMenuAndroid();
    }

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded" />
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (!Context.IsMainPlayer)
        {
            IMultiplayerPeer mainPlayer = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID);
            IMultiplayerPeerMod mainPlayerMod = mainPlayer?.GetMod(this.ModManifest.UniqueID);
            if (mainPlayerMod is null)
            {
                this.Monitor.Log("Host seems to be missing Stardew Aquarium. Certain features may not work as advertised.", LogLevel.Error);
            }
        }

        if (Context.IsMainPlayer)
        {
            // check my location references.
            void CheckMap(string? map)
            {
                if (map is null || Game1.getLocationFromName(map) is null)
                {
                    this.Monitor.Log($"{map} cannot be found, there seems to be an issue with the data file.");
                }
            }

            CheckMap(ContentPackHelper.ExteriorLocationName);
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                CheckMap(ContentPackHelper.InteriorLocationName);
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
        Game1.activeClickableMenu = new AquariumCollectionMenu(ContentPackHelper.LoadString("CollectionsMenu"));
    }

    private void OpenDonationMenuCommand(string arg1, string[] arg2)
    {
        Game1.activeClickableMenu = new DonateFishMenu();
    }

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        AquariumGameStateQuery.Init();
    }
}
