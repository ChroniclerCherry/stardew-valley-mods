using System.Collections.Generic;

namespace ShopTileFramework.Data
{
    abstract class AnimalShopModel
    {
        public string ShopName { get; set; }
        public List<string> AnimalStock { get; set; }
        public string[] ExcludeFromMarnies { get; set; }
        public string[] When { get; set; } = null;
        public string ClosedMessage { get; set; } = null;
        public Dictionary<string, string> LocalizedClosedMessage { get; set; }
    }
}
