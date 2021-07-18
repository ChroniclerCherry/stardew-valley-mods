using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace WarpPylons.Menus
{
    class OptionsPylonRenameButton : PylonButton
    {
        private readonly int xOffset = 380;
        public OptionsPylonRenameButton(IMonitor monitor, string label, PylonData pylon)
            : base(monitor,label, pylon)
        {
            this.bounds = new Rectangle(32+ xOffset, 15, (int)Game1.dialogueFont.MeasureString(label).X + 64, 50);
        }

        public override void receiveLeftClick(int x, int y)
        {
            if (!this.bounds.Contains(x, y))
                return;
            
            var namingMenu = new NamingMenu(this.ChangeName, $"Pick a new name for {_pylon.Name}",_pylon.Name);
            Game1.activeClickableMenu = namingMenu;
        }

        private void ChangeName(string s)
        {
            _pylon.Name = s;
            Game1.exitActiveMenu();
        }
    }
}
