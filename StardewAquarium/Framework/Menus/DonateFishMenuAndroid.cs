using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace StardewAquarium.Framework.Menus;

class DonateFishMenuAndroid : ShopMenu
{
    public static bool Donated;
    public static bool PufferchickDonated;

    private Dictionary<ISalable, ItemStockInformation> donations = new Dictionary<ISalable, ItemStockInformation>();

    public DonateFishMenuAndroid(IModHelper helper, IMonitor monitor) : base("-1", new Dictionary<ISalable, ItemStockInformation>())
    {
        //look android forced me to do this terrible thing don't judge me just pretend they're not static
        Donated = PufferchickDonated = false;

        /*
         *why do i have a whole custom class for something that gets immediately replaced by a vanilla one by smapi? bc screw u
         * ( initial concept was to keep harmony targeting and menu detection clean without using an NPC name but like, idk anymore lmao )
         * I'll come back and readdress this someday probably, when I have more sanity to spend. I'm all out atm
        */

        List<string> fishes = Utils.GetUndonatedFishInInventory().Distinct().ToList();
        if (fishes.Count == 0)
            return;

        foreach (string fish in fishes)
        {
            Object display = new(fish, 1);
            display.displayName = I18n.Donate() + display.DisplayName;
            this.donations.Add(display, new(0, 1, fish, 1));
        }

        this.setItemPriceAndStock(this.donations);
    }

    public void OnExit()
    {
        Utils.DonationMenuExit(Donated, PufferchickDonated);
    }
}
