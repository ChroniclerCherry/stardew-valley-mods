using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterGreenhouse.Upgrades;

namespace BetterGreenhouse.src.data
{
    public class Data
    {
        public int JunimoPoints { get; set; } = 0;
        public List<Upgrade> Upgrades { get; set; } = new List<Upgrade>();
    }
}
