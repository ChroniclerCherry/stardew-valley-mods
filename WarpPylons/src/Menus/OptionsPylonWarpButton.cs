using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace WarpPylons.Menus
{
    class OptionsPylonWarpButton : OptionsElement
    {
        private IMonitor _monitor;
        private PylonData _pylon;
        private int xOffset = 600;
        public OptionsPylonWarpButton(IMonitor monitor, string label, PylonData pylon)
            : base(label)
        {
            _monitor = monitor;
            _pylon = pylon;
            this.bounds = new Rectangle(32+ xOffset, 15, (int)Game1.dialogueFont.MeasureString(label).X + 64, 50);
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (!this.bounds.Contains(x, y))
                return;

            _monitor.Log($"Warping to {_pylon.MapName} {_pylon.Coordinates}");
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White, 4f, true);
            Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.Center.X), (float)(slotY + this.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.label) / 2f, Game1.textColor, 1f, 1f, -1, -1, 0.0f, 3);
        }
    }
}
