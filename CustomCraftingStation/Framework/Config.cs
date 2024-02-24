using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCraftingStation.Framework
{
    public class Config
    {
        public bool GlobalCraftFromChest { get; set; } = false;
        public bool CraftFromFridgeWhenInHouse { get; set; } = true;
        public int CraftingFromChestsRadius { get; set; } = 0;
    }
}
