using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace WarpPylons.src.Menus
{
    class OptionsPylonRenameButton : OptionsElement
    {
        private IMonitor _monitor;
        private PylonData _pylon;
        private int xOffset = 380;
        public OptionsPylonRenameButton(IMonitor monitor, string label, PylonData pylon)
            : base(label)
        {
            _monitor = monitor;
            _pylon = pylon;
            this.bounds = new Rectangle(32+ xOffset, 0, (int)Game1.dialogueFont.MeasureString(label).X + 64, 50);
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (!this.bounds.Contains(x, y))
                return;
            
            _monitor.Log($"Renaming {_pylon.Name}");
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width, this.bounds.Height, Color.White, 4f, true);
            Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2((float)(slotX + this.bounds.Center.X), (float)(slotY + this.bounds.Center.Y + 4)) - Game1.dialogueFont.MeasureString(this.label) / 2f, Game1.textColor, 1f, 1f, -1, -1, 0.0f, 3);
        }
    }
}
