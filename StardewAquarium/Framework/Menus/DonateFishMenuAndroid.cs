using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace StardewAquarium.Framework.Menus;

internal class DonateFishMenuAndroid : ShopMenu
{
    /*********
    ** Fields
    *********/
    private readonly Dictionary<ISalable, ItemStockInformation> Donations = new Dictionary<ISalable, ItemStockInformation>();


    /*********
    ** Accessors
    *********/
    public static bool Donated;
    public static bool PufferchickDonated;


    /*********
    ** Public methods
    *********/
    public DonateFishMenuAndroid()
        : base("-1", new Dictionary<ISalable, ItemStockInformation>())
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
            display.displayName = ContentPackHelper.LoadString("Donate") + display.DisplayName;
            this.Donations.Add(display, new(0, 1, fish, 1));
        }

        this.setItemPriceAndStock(this.Donations);
    }

    public void OnExit()
    {
        Utils.DonationMenuExit(Donated, PufferchickDonated);
    }
}
