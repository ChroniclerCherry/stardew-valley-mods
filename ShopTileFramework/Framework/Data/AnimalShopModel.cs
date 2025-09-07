#nullable disable

using System.Collections.Generic;

namespace ShopTileFramework.Framework.Data;

internal abstract class AnimalShopModel
{
    public string ShopName { get; set; }
    public List<string> AnimalStock { get; set; }
    public string[] When { get; set; } = null;
    public string ClosedMessage { get; set; } = null;
    public Dictionary<string, string> LocalizedClosedMessage { get; set; }
}
