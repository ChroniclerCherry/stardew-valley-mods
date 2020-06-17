using StardewModdingAPI;

namespace WarpPylons
{
    public partial class WarpPylonsEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.ConsoleCommands.Add("Pylons","Opens the Warp Pylons menu",this.OpenWarPylonsMenuCommand);
        }
    }
}
