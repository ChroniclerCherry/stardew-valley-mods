using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace StardewAquarium.Menus
{
    public class DonateFishMenu : InventoryMenu
    {

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private bool _donated;
        private bool _pufferchickDonated;

        private static int PufferChickID { get => ModEntry.JsonAssets?.GetObjectId(ModEntry.PufferChickName) ?? -1; }

        public DonateFishMenu(IModHelper translate,IMonitor monitor) : base(Game1.viewport.Width / 2 - 768 / 2, Game1.viewport.Height / 2 + 36, true, null, Utils.IsUnDonatedFish, 36, 3)
        {
            showGrayedOutSlots = true;
            _helper = translate;
            _monitor = monitor;
            exitFunction = () => Utils.DonationMenuExit(_donated,_pufferchickDonated);
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

                var mp = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                mp.globalChatInfoMessage("StardewAquarium.FishDonated", new []{Game1.player.Name,item.Name});
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                base.drawBackground(b);

            string title = _helper.Translation.Get("DonationMenuTitle");
            SpriteText.drawStringWithScrollCenteredAt(b, title, Game1.viewport.Width / 2,
                Game1.viewport.Height / 2 - 128, title, 1f, -1, 0, 0.88f, false);

            Game1.drawDialogueBox(this.xPositionOnScreen - 64, this.yPositionOnScreen - 160, this.width + 128,
                this.height + 192, false, true);

            base.draw(b);
            base.drawMouse(b);
        }

    }
}
