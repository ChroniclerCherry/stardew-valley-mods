using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Menus;

namespace StardewAquarium.Menu
{
    public class DonateFishMenu : InventoryMenu
    {

        public static int FishCategory = -4;

        public DonateFishMenu() : base(Game1.viewport.Width / 2 - 768 / 2, Game1.viewport.Height / 2 + 36, true, null, Utils.IsUnDonatedFish, 36, 3)
        {
            showGrayedOutSlots = true;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var item = getItemAt(x, y);
            if (!Utils.IsUnDonatedFish(item))
                return;

            if (Utils.DonateFish(item))
            {
                Game1.playSound("newArtifact");
                item.Stack--;
                if (item.Stack == 0)
                    Game1.player.removeItemFromInventory(item);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                base.drawBackground(b);

            Game1.drawDialogueBox(this.xPositionOnScreen- 64, this.yPositionOnScreen-160, this.width+128, this.height+ 192, false, true);

            base.draw(b);
            base.drawMouse(b);
        }

    }
}
