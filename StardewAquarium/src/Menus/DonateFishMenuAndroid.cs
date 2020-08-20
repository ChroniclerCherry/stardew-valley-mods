using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace StardewAquarium.Menus
{
    class DonateFishMenuAndroid : ShopMenu
    {
        public static bool Donated;
        public static bool PufferchickDonated;


        private Dictionary<ISalable, int[]> donations = new Dictionary<ISalable, int[]>();

        public DonateFishMenuAndroid(IModHelper helper, IMonitor monitor) : base(new Dictionary<ISalable, int[]>())
        {
            //look android forced me to do this terrible thing don't judge me just pretend they're not static
            Donated = PufferchickDonated = false;

            /*
             *why do i have a whole custom class for something that gets immediately replaced by a vanilla one by smapi? bc screw u
             * ( initial concept was to keep harmony targeting and menu detection clean without using an NPC name but like, idk anymore lmao )
             * I'll come back and readdress this someday probably, when I have more sanity to spend. I'm all out atm
            */

            List<int> fishes = Utils.GetUndonatedFishInInventory().Distinct().ToList();
            if (fishes.Count == 0)
                return;

            foreach (int fish in fishes)
            {
                Object display = new Object(fish, 1);
                display.DisplayName = "Donate " + display.DisplayName;
                donations.Add(display,new int[] {0,1,fish,1});
            }

            setItemPriceAndStock(donations);
        }

        public void OnExit()
        {
            Utils.DonationMenuExit(Donated,PufferchickDonated);
        }
    }
}
