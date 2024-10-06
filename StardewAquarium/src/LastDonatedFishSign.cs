using System.Linq;

using Microsoft.Xna.Framework;

using StardewAquarium.Models;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Extensions;

namespace StardewAquarium
{
    public class LastDonatedFishSign
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ModData _data { get => ModEntry.Data; }

        private const string LastDonatedFishKey = "Cherry.StardewAquarium.LastDonatedFish";

        public LastDonatedFishSign(IModHelper helper, IMonitor monitor)
        {
            this._helper = helper;
            this._monitor = monitor;

            this._helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this._helper.Events.Player.Warped += this.Player_Warped;

            this._helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
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

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation?.Name == this._data.ExteriorMapName && Game1.getFarm().modData.TryGetValue(LastDonatedFishKey, out string qid))
            {
                this.UpdateFishSign(qid, e.NewLocation);
            }
        }

        private void UpdateFishSign(string qid, GameLocation loc)
        {
            var parsedItem = ItemRegistry.GetData(qid);
            if (parsedItem is null)
                return;

            var position = new Vector2(this._data.LastDonatedFishCoordinateX * 64f, this._data.LastDonatedFishCoordinateY * 64f);
            var rect = parsedItem.GetSourceRect();

            var tas = new TemporaryAnimatedSprite
            {
                texture = parsedItem.GetTexture(),
                sourceRect = rect,
                animationLength = 1,
                sourceRectStartingPos = new(rect.X, rect.Y),
                interval = 50000f,
                totalNumberOfLoops = 9999,
                position = position,
                scale = Game1.pixelZoom,
                layerDepth = (((position.Y) * Game1.tileSize) / 10000f) + 0.01f, // a little offset so it doesn't show up on the floor.
                id = 777,
            };

            loc.temporarySprites.Add(tas);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
                this.MigrateLastDonatedFish();
        }

        public void UpdateLastDonatedFish(Item i)
        {
            Game1.getFarm().modData[LastDonatedFishKey] = i.QualifiedItemId;
            this._monitor.Log($"The last donated fish is {i.Name}");
        }

        public void MigrateLastDonatedFish()
        {

            if (Game1.getFarm().modData.ContainsKey(LastDonatedFishKey))
            {
                return;
            }

            string fish = Game1.MasterPlayer.mailReceived
                .Where(flag => flag
                    .StartsWith("AquariumLastDonated:"))
                .Select(flag => flag.Split(':')[1])
                .FirstOrDefault();

            if (fish is null)
            {
                return;
            }

            string qid = Game1.objectData.FirstOrDefault(kvp => kvp.Value.Name == fish).Key;
            this._monitor.Log($"The last donated fish on this file is {qid}");

            Game1.getFarm().modData[LastDonatedFishKey] = qid;
            Game1.MasterPlayer.mailReceived.RemoveWhere(flag => flag.StartsWith("AquariumLastDonated:"));
        }
    }
}
