using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace WarpPylons.Menus
{
    class PylonButton : OptionsElement
    {
        internal IMonitor _monitor;
        internal PylonData _pylon;
        internal bool _hovered;
        internal readonly int _hoveredYOffset = 5;
        public PylonButton(IMonitor monitor, string label, PylonData pylon)
            : base(label)
        {
            _monitor = monitor;
            _pylon = pylon;
        }

        public void performHoverAction(int x, int y)
        {
            _hovered = this.bounds.Contains(x, y);
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            if (_hovered)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + this.bounds.X, slotY + this.bounds.Y + _hoveredYOffset, this.bounds.Width, this.bounds.Height, Color.White, 4f, false);
                Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.Center.X), (float)(slotY + this.bounds.Center.Y + 4 + _hoveredYOffset)) - Game1.dialogueFont.MeasureString(this.label) / 2f, Game1.textColor, 1f, 1f, -1, -1, 0.0f, 3);
            }
            else
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White, 4f, true);
                Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.Center.X), (float)(slotY + this.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.label) / 2f, Game1.textColor, 1f, 1f, -1, -1, 0.0f, 3);
            }
        }
    }
}
