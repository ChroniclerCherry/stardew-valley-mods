using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CustomShopActionFramework
{
    class ContentModel
    {
        public ShopPack[] Shops { get; set; }
    }

    class ShopPack
    {
        public string ShopName { get; set; }
        public string Portrait { get; set; }
        public string Quote { get; set; }
        public int ShopPrice { get; set; }
        public Stock[] Stock { get; set; }

        public ShopPack()
        {
            var npc = new NPC
            {
                Portrait = ModEntry.helper.Content.Load<Texture2D>(Portrait)
            };
            //Game1.removeThisCharacterFromAllLocations(npc);
        }
    }

    class Stock
    {
        public string ItemType { get; set; }
        public int StockPrice { get; set; }
        public string[] Items { get; set; }
        public string When { get; set; }
    }
}
