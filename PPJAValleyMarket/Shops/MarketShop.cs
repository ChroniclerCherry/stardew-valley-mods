using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace PPJAValleyMarket.Shops
{
    public abstract class MarketShop
    {
        public abstract string Shopkeeper { get; }

        public virtual string ShopKeeperDisplayName => _helper.Translation.Get($"{Shopkeeper}.Name");
        public virtual string ShopQuote => _helper.Translation.Get($"{Shopkeeper}.Quote");

        public virtual string ShopClosed => _helper.Translation.Get($"{Shopkeeper}.Closed");
        public abstract string[] JAPacks { get; }
        public abstract Dictionary<ISalable, int[]> ItemStockAndPrice { get; set; }
        public abstract bool CanOpen();

        internal IModHelper _helper;
        internal IMonitor _monitor;

        public MarketShop(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public virtual ShopMenu CreateShopMenu()
        {
            var menu = new ShopMenu(ItemStockAndPrice);
            var npc = Game1.getCharacterFromName(Shopkeeper);
            if (npc == null)
            {
                Texture2D portrait = _helper.Content.Load<Texture2D>($"Portraits\\{Shopkeeper}");
                npc = new NPC()
                {
                    Portrait = portrait
                };
            }

            menu.potraitPersonDialogue = ShopQuote;
            menu.portraitPerson = npc;
            return menu;
        }
        public abstract void Update();
    }
}
