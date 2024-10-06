using System.Linq;

using Microsoft.Xna.Framework;

using StardewAquarium.Models;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace StardewAquarium
{
    internal sealed class LastDonatedFishSign
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private ModData _data => ModEntry.Data;

        // the last donated fish is stored as a member of farm's moddata.
        // this avoids us having to do our own multiplayer handling.
        private const string LastDonatedFishKey = "Cherry.StardewAquarium.LastDonatedFish";

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;

            this._helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this._helper.Events.Player.Warped += this.Player_Warped;

            this._helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
        }

        internal void UpdateLastDonatedFish(Item i)
        {
            Game1.getFarm().modData[LastDonatedFishKey] = i.QualifiedItemId;
            this._monitor.Log($"The last donated fish is {i.Name}");
        }

        /// <summary>
        /// Pick a random fish when the museum is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer || !Game1.MasterPlayer.mailReceived.Contains("AquariumCompleted"))
                return;

            Game1.getFarm().modData[LastDonatedFishKey] = ItemRegistry.ManuallyQualifyItemId(Utility.CreateDaySaveRandom().ChooseFrom(Utils.FishIDs), ItemRegistry.type_object);
        }

        /// <summary>
        /// Updates the fishing sign when players arrive at our location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation?.Name == this._data.ExteriorMapName && Game1.getFarm().modData.TryGetValue(LastDonatedFishKey, out string qid))
            {
                this.UpdateFishSign(qid, e.NewLocation);
            }
        }

        private void UpdateFishSign(string qid, GameLocation loc)
        {
            ParsedItemData parsedItem = ItemRegistry.GetData(qid);
            this._monitor.Log($"Updating sign with {parsedItem?.QualifiedItemId}");
            if (parsedItem is null)
                return;

            Vector2 position = new(this._data.LastDonatedFishCoordinateX * 64f, this._data.LastDonatedFishCoordinateY * 64f);
            Rectangle rect = parsedItem.GetSourceRect();

            TemporaryAnimatedSprite tas = new(parsedItem.GetTextureName(), rect, position, false, 0, Color.White)
            {
                animationLength = 1,
                sourceRectStartingPos = new(rect.X, rect.Y),
                interval = 50000f,
                totalNumberOfLoops = 9999,
                position = position,
                scale = Game1.pixelZoom,
                layerDepth = (this._data.LastDonatedFishCoordinateY / 10000f) + 0.01f, // a little offset so it doesn't show up on the floor.
                id = 777
            };

            loc.temporarySprites.Add(tas);
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

            string qid = Game1.objectData.FirstOrDefault(kvp => kvp.Value.Name == fish).Key;
            this._monitor.Log($"The last donated fish on this file is {qid}");
            if (qid is null)
            {
                return;
            }

            qid = ItemRegistry.ManuallyQualifyItemId(qid, ItemRegistry.type_object);

            Game1.getFarm().modData[LastDonatedFishKey] = qid;
            Game1.MasterPlayer.mailReceived.RemoveWhere(flag => flag.StartsWith("AquariumLastDonated:"));
        }
    }
}
