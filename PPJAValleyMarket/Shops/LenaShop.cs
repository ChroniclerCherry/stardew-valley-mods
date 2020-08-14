using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace PPJAValleyMarket.Shops
{
    class LenaShop : MarketShop
    {
        public LenaShop(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
        }

        public override string Shopkeeper { get; } = "Lena";

        public override string[] JAPacks { get; } =
        {
            "ppja.moretrees",
            "amburr.spoopyvalley",
            "Hadi.JASoda",
            "BonsterTrees",
            "Cinnanimroll.CoconutTree"
        };
        public override Dictionary<ISalable, int[]> ItemStockAndPrice { get; set; }
        public override bool CanOpen()
        {
            //always open?
            return true;
        }

        public override void Update()
        {
            //only sell "lumisteria.smallfruittrees" after greenhouse unlocked


        }
    }
}
