using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace StardewAquarium.Framework;

internal sealed class LastDonatedFishSign
{
    /*********
    ** Fields
    *********/
    private readonly IMonitor Monitor;

    // the last donated fish is stored as a member of farm's mod data.
    // this avoids us having to do our own multiplayer handling.
    private const string LastDonatedFishKey = "Cherry.StardewAquarium.LastDonatedFish";


    /*********
    ** Public methods
    *********/
    public LastDonatedFishSign(IModEvents events, IMonitor monitor)
    {
        this.Monitor = monitor;

        events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
        events.Player.Warped += this.Player_Warped;

        events.GameLoop.DayStarted += this.GameLoop_DayStarted;
    }

    public void UpdateLastDonatedFish(Item i)
    {
        Game1.getFarm().modData[LastDonatedFishKey] = i.QualifiedItemId;
        this.Monitor.Log($"The last donated fish is {i.Name}");
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Pick a random fish when the museum is complete.</summary>
    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
    {
        if (!Context.IsMainPlayer || !Game1.MasterPlayer.mailReceived.Contains("AquariumCompleted"))
            return;

        Game1.getFarm().modData[LastDonatedFishKey] = ItemRegistry.ManuallyQualifyItemId(Utility.CreateDaySaveRandom().ChooseFrom(Utils.FishIDs), ItemRegistry.type_object);
    }

    /// <summary>Updates the fishing sign when players arrive at our location.</summary>
    /// <inheritdoc cref="IPlayerEvents.Warped" />
    private void Player_Warped(object sender, WarpedEventArgs e)
    {
        if (Game1.getFarm().modData.TryGetValue(LastDonatedFishKey, out string qid))
            this.UpdateFishSign(qid, e.NewLocation);
    }

    private void UpdateFishSign(string fishId, GameLocation loc)
    {
        if (!loc.TryGetMapPropertyAs($"{ContentPackHelper.ContentPackId}_LastDonatedFishSign", out Point signTile))
            return;

        ParsedItemData parsedItem = ItemRegistry.GetData(fishId);
        this.Monitor.Log($"Updating sign with {parsedItem?.QualifiedItemId}");
        if (parsedItem is null)
            return;

        Vector2 position = new(signTile.X * Game1.tileSize, signTile.Y * Game1.tileSize);
        Rectangle rect = parsedItem.GetSourceRect();

        TemporaryAnimatedSprite sprite = new(parsedItem.GetTextureName(), rect, position, false, 0, Color.White)
        {
            animationLength = 1,
            sourceRectStartingPos = new(rect.X, rect.Y),
            interval = 50000f,
            totalNumberOfLoops = 9999,
            position = position,
            scale = Game1.pixelZoom,
            layerDepth = (signTile.Y / 10000f) + 0.01f, // a little offset, so it doesn't show up on the floor.
            id = 777
        };

        loc.temporarySprites.Add(sprite);
    }

    private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (Context.IsMainPlayer)
            this.MigrateLastDonatedFish();
    }

    private void MigrateLastDonatedFish()
    {
        if (Game1.getFarm().modData.ContainsKey(LastDonatedFishKey))
        {
            return;
        }

        string fish = Game1.MasterPlayer.mailReceived
            .Where(flag => flag.StartsWith("AquariumLastDonated:"))
            .Select(flag => flag.Split(':')[1])
            .FirstOrDefault();

        if (fish is null)
        {
            return;
        }

        string fishId = Game1.objectData.FirstOrDefault(kvp => kvp.Value.Name == fish).Key;
        this.Monitor.Log($"The last donated fish on this file is {fishId}");
        if (fishId is null)
        {
            return;
        }

        fishId = ItemRegistry.ManuallyQualifyItemId(fishId, ItemRegistry.type_object);

        Game1.getFarm().modData[LastDonatedFishKey] = fishId;
        Game1.MasterPlayer.mailReceived.RemoveWhere(flag => flag.StartsWith("AquariumLastDonated:"));
    }
}
