using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace PPJAValleyMarket.Shops
{
    class ZamirShop : MarketShop
    {
        public override string Shopkeeper { get; } = "Zamir";
        public override string[] JAPacks { get; } = new []{""};
        public override Dictionary<ISalable, int[]> ItemStockAndPrice { get; set; }

        public ZamirShop(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            
        }
        public override bool CanOpen()
        {
            //only open monday to fri
            var days = new[] {"Mon","Tues","Wed", "Thurs","Fri" };
            return days.Contains(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

    }
}
