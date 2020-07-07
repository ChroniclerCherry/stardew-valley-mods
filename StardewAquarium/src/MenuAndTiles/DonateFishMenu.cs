using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Collections.Generic;

namespace StardewAquarium.MenuAndTiles
{
    public class DonateFishMenu : InventoryMenu
    {

        private readonly ITranslationHelper _translation;
        private bool _donated;
        private bool _achievementUnlock;
        private bool _pufferchickDonated;

        private static int PufferChickID { get => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1; }

        public DonateFishMenu(ITranslationHelper translate) : base(Game1.viewport.Width / 2 - 768 / 2, Game1.viewport.Height / 2 + 36, true, null, Utils.IsUnDonatedFish, 36, 3)
        {
            //  UnlockAchievement();
            //  TryAwardTrophy();

            showGrayedOutSlots = true;
            _translation = translate;
            exitFunction = () => Utils.DonationMenuExit(_achievementUnlock,_donated,_pufferchickDonated);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var item = getItemAt(x, y);
            if (!Utils.IsUnDonatedFish(item))
                return;

            if (Utils.DonateFish(item))
            {
                _donated = true;
                Game1.playSound("newArtifact");
                item.Stack--;
                if (item.Stack == 0)
                    Game1.player.removeItemFromInventory(item);

                if (item.ParentSheetIndex == PufferChickID)
                {
                    Game1.playSound("openChest");
                    _pufferchickDonated = true;
                }

                _achievementUnlock = Utils.CheckAchievement();

            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                base.drawBackground(b);

            string title = _translation.Get("DonationMenuTitle");
            SpriteText.drawStringWithScrollCenteredAt(b, title, Game1.viewport.Width / 2, Game1.viewport.Height / 2 - 128, title, 1f, -1, 0, 0.88f, false);

            Game1.drawDialogueBox(this.xPositionOnScreen- 64, this.yPositionOnScreen-160, this.width+128, this.height+ 192, false, true);

            base.draw(b);
            base.drawMouse(b);
        }

    }
}
