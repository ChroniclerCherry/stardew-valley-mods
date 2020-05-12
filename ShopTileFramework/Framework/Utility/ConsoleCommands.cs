using ShopTileFramework.Framework.Shop;
using StardewModdingAPI;

namespace ShopTileFramework.Framework.Utility
{
    class ConsoleCommands
    {
        internal void Register()
        {
            ModEntry.helper.ConsoleCommands.Add("open_shop", "Opens up a custom shop's menu. \n\nUsage: open_shop <ShopName>\n-ShopName: the name of the shop to open", DisplayShopMenu);
            ModEntry.helper.ConsoleCommands.Add("open_animal_shop", "Opens up a custom animal shop's menu. \n\nUsage: open_shop <open_animal_shop>\n-ShopName: the name of the animal shop to open", DisplayAnimalShopMenus);
            ModEntry.helper.ConsoleCommands.Add("reset_shop", "Resets the stock of specified shop. Rechecks conditions and randomizations\n\nUsage: reset_shop <ShopName>\n-ShopName: the name of the shop to reset", ResetShopStock);
            ModEntry.helper.ConsoleCommands.Add("list_shops", "Lists all shops registered with Shop Tile Framework", ListAllShops);
        }
        private void DisplayShopMenu(string command, string[] args)
        {
            if (args.Length == 0)
            {
                ModEntry.monitor.Log($"A shop name is required", LogLevel.Debug);
                return;
            }

            ShopManager.ItemShops.TryGetValue(args[0], out ItemShop value);
            if (value == null)
            {
                ModEntry.monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            }
            else
            {
                if (!Context.IsPlayerFree)
                {
                    ModEntry.monitor.Log($"The player isn't free to act; can't display a menu right now", LogLevel.Debug);
                    return;
                }

                value.DisplayShop();
            }
        }

        private void DisplayAnimalShopMenus(string command, string[] args)
        {
            if (args.Length == 0)
            {
                ModEntry.monitor.Log($"A shop name is required", LogLevel.Debug);
                return;
            }

            ShopManager.AnimalShops.TryGetValue(args[0], out AnimalShop value);
            if (value == null)
            {
                ModEntry.monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            }
            else
            {
                if (!Context.IsPlayerFree)
                {
                    ModEntry.monitor.Log($"The player isn't free to act; can't display a menu right now", LogLevel.Debug);
                    return;
                }

                value.DisplayShop();
            }
        }

        private void ResetShopStock(string command, string[] args)
        {
            if (args.Length == 0)
            {
                ModEntry.monitor.Log($"A shop name is required", LogLevel.Debug);
                return;
            }

            ShopManager.ItemShops.TryGetValue(args[0], out ItemShop shop);
            if (shop == null)
            {
                ModEntry.monitor.Log($"No shop with a name of {args[0]} was found.", LogLevel.Debug);
            }
            else
            {
                if (!Context.IsWorldReady)
                {
                    ModEntry.monitor.Log($"The world hasn't loaded; shop stock can't be updated at this time", LogLevel.Debug);
                    return;
                }
                shop.UpdateItemPriceAndStock();
            }
        }
        private void ListAllShops(string command, string[] args)
        {
            if (ShopManager.ItemShops.Count == 0)
            {
                ModEntry.monitor.Log($"No shops were found", LogLevel.Debug);
            }
            else
            {
                string temp = "";
                foreach (string k in ShopManager.ItemShops.Keys)
                {
                    temp += "\nShop: " + k;
                }

                foreach (string k in ShopManager.AnimalShops.Keys)
                {
                    temp += "\nAnimalShop: " + k;
                }

                ModEntry.monitor.Log(temp, LogLevel.Debug);
            }
        }


    }
}
