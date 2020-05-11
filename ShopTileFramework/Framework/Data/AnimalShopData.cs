using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTileFramework.Framework.Data
{
    abstract class AnimalShopData
    {
        public string ShopName { get; set; }
        public List<string> AnimalStock { get; set; }
        public string[] ExcludeFromMarnies { get; set; } = null;
        public string[] When { get; set; } = null;
        public string ClosedMessage { get; set; } = null;
        public Dictionary<string, string> LocalizedClosedMessage { get; set; }
    }
}
