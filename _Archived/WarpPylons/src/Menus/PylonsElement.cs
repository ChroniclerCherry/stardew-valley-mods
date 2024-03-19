using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace WarpPylons.Menus
{
    class PylonsElement
    {
        private OptionsElement _label;
        private OptionsPylonRenameButton _rename;
        private OptionsPylonWarpButton _warp;

        private PylonData _pylon;
        private IMonitor _monitor;

        public Rectangle bounds { get; set; }
        public PylonsElement(IMonitor monitor, PylonData pylon)
        {

            bounds = new Rectangle(0,15,800,50);

            _monitor = monitor;
            _pylon = pylon;

            _label = new OptionsElement($"{pylon.Name}", 32, 16, 36, 50);
            _rename = new OptionsPylonRenameButton(_monitor, "Rename", pylon);
            _warp = new OptionsPylonWarpButton(_monitor, "Warp", pylon);

        }

        public void receiveLeftClick(int x, int y)
        {
            _rename.receiveLeftClick(x,y);
            _warp.receiveLeftClick(x,y);
        }

        internal void leftClickHeld(int x, int y)
        {
            _rename.leftClickHeld(x,y);
            _warp.leftClickHeld(x,y);
        }

        public void leftClickReleased(int x, int y)
        {
            _rename.leftClickReleased(x,y);
            _warp.leftClickReleased(x,y);
        }

        public void draw(SpriteBatch spriteBatch, int x, int y)
        {
            _label.draw(spriteBatch,x,y);
            _rename.draw(spriteBatch,x,y);
            _warp.draw(spriteBatch,x,y);
        }

        public void performHoverAction(int x, int y)
        {
            _rename.performHoverAction(x, y);
            _warp.performHoverAction(x, y);
        }
    }
}
