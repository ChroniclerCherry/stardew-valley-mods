using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    public partial class ModEntry
    {
        private Vector2 LastDonatedFishCoordinates;

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Utils.SetLastDonatedFish();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            //ExteriorMuseum
            if (Game1.currentLocation.Name != "ExteriorMuseum")
                return;

            if (Utils.LastDonatedFish == null)
                return;

            var loc = Game1.GlobalToLocal(LastDonatedFishCoordinates);


            Utils.LastDonatedFish.drawInMenu(e.SpriteBatch, loc, 1);
        }
    }
}