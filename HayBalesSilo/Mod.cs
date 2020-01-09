using StardewModdingAPI;
using StardewValley;
using Harmony;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StardewValley.Menus;

namespace HayBalesSilo
{
    public class ModEntry : Mod, IAssetEditor
    {
        internal static IMonitor monitor;
        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.numSilos)),
                postfix: new HarmonyMethod(typeof(PatchNumSilos), nameof(PatchNumSilos.Postfix))
                );

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.MenuChanged += Display_MenuChanged;

        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            //we don't care if it's not a shop menu
            if (!(e.NewMenu is ShopMenu))
                return;

            var shop = (ShopMenu)e.NewMenu;

            //we don't care if it's not Marnie's store
            if (shop.portraitPerson == null || !(shop.portraitPerson.Name == "Marnie"))
                return;

            //create the farm renderer object and add it to robin's stock
            var itemStock = shop.itemPriceAndStock;

            foreach (var item in itemStock)
            {
                if (item.Key.Name == "Ornamental Hay Bale")
                {
                    item.Value[0] = 5000; //change the price
                    break;
                }
            }

            shop.setItemPriceAndStock(itemStock);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //ignore input if the player isnt free to move aka world not loaded,
            //they're in an event, a menu is up, etc
            if (!Context.CanPlayerMove)
                return;

            if (Game1.currentLocation.Name != "Farm")
                return;

            //action button works for right click on mouse and action button for controllers
            if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
                return;

            //checks if the clicked tile is actually adjacent to the player
            var clickedTile = Helper.Input.GetCursorPosition().Tile;
            if (!IsClickWithinReach(clickedTile))
                return;

            //check if the clicked tile contains a Farm Renderer
            Vector2 tile = e.Cursor.Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj != null && obj.bigCraftable.Value)
            {
                if (obj.Name == "Ornamental Hay Bale")
                {
                    if (Utility.numSilos() == 0)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
                        return;
                    }

                    if (e.Button.IsActionButton())
                    {

                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay",
                            Game1.getFarm().piecesOfHay.Value,
                            (Utility.numSilos() * 240)));
                    }
                    else if (e.Button.IsUseToolButton())
                    {
                        //if holding hay, try to add it
                        if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Hay")
                        {
                            int stack = Game1.player.ActiveObject.Stack;
                            int tryToAddHay = Game1.getFarm().tryToAddHay(Game1.player.ActiveObject.Stack);
                            Game1.player.ActiveObject.Stack = tryToAddHay;

                            if (Game1.player.ActiveObject.Stack < stack)
                            {
                                Game1.playSound("Ship");
                                DelayedAction.playSoundAfterDelay("grassyStep", 100, (GameLocation)null, -1);
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", (object)(stack - Game1.player.ActiveObject.Stack)));
                            }
                            if (Game1.player.ActiveObject.Stack <= 0)
                                Game1.player.removeItemFromInventory((Item)Game1.player.ActiveObject);
                        }
                    }
                }

            }
        }

        private bool IsClickWithinReach(Vector2 tile)
        {
            var playerPosition = Game1.player.Position;
            var playerTile = new Vector2(playerPosition.X / 64, playerPosition.Y / 64);

            if (tile.X < (playerTile.X - 1.5) || tile.X > (playerTile.X + 1.5) ||
                tile.Y < (playerTile.Y - 1.5) || tile.Y > (playerTile.Y + 1.5))
                return false;

            return true;
        }

        internal static int NumHayBales()
        {
            int numHayBales = 0;

            foreach (var temp in Game1.getFarm().Objects)
            {
                foreach (var obj in temp.Values)
                {
                    if (obj.Name == "Ornamental Hay Bale")
                    {
                        numHayBales++;
                    }
                }
            }

            return numHayBales;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/BigCraftablesInformation");
        }

        public void Edit<T>(IAssetData asset)
        {

            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                string[] fields = data[45].Split('/');

                fields[4] = Helper.Translation.Get("Description"); //description
                fields[8] = Helper.Translation.Get("DisplayName"); //display name

                data[45] = string.Join("/", fields);
            }
        }
    }
    public class PatchNumSilos
    {
        internal static void Postfix(ref int __result)
        {
            if (__result > 0)
            {
                __result = __result + ModEntry.NumHayBales();
            }
        }
    }
}
