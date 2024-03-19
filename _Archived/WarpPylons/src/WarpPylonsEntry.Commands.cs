using System.Linq;
using StardewValley;

namespace WarpPylons
{
    public partial class WarpPylonsEntry
    {
        private void OpenWarPylonsMenuCommand(string arg1, string[] arg2)
        {
            Game1.activeClickableMenu = new WarpPylonsMenu(Monitor,Utils.GetTestPylons(20).ToList());
        }
    }
}