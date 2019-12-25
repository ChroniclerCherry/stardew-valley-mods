using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace ShopTileFramework
{
    class Shop
    {
        public string ShopName { get; set; }
        private Texture2D Portrait = null;
        private string Quote { get; set; }
        private int ShopPrice { get; set; }
        private ItemStock[] ItemStock { get; set; }
        private int MaxNumItemsSoldInStore { get; set; } = int.MaxValue;
        public Dictionary<ISalable, int[]> ItemPriceAndStock { get; set; } = new Dictionary<ISalable, int[]>();

        public Shop(ShopPack pack, IContentPack contentPack)
        {
            ShopName = pack.ShopName;
            ItemStock = pack.ItemStocks;
            Quote = pack.Quote;

            if (pack.PortraitPath != null)
            {
                Portrait = contentPack.LoadAsset<Texture2D>(pack.PortraitPath);
            }

        }

        public void UpdateItemPriceAndStock()
        {

        }

        public void DisplayStore()
        {

        }


    }
}
