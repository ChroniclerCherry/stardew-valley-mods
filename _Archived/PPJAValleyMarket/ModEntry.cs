using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PPJAValleyMarket.Shops;
using StardewModdingAPI;
using StardewValley;

namespace PPJAValleyMarket
{
    public class ModEntry : Mod
    {
        private Dictionary<string, MarketShop> shops = new Dictionary<string, MarketShop>();

        //these function the same as the STF fields of the same name
        //private readonly string[] _itemToRemove = new [] { "Item1"};
        //private readonly string[] _recipePacksToRemove = new[] { "Pack1" };
        private readonly string[] _packsToRemove = new[] {      "ppja.moretrees",
            "Hadi.JASoda",
            "ppja.fruitsandveggies",
            "mizu.flowers",
            "PPJA.cannabiskit",
            "ParadigmNomad.FantasyCrops",
            "paradigmnomad.freshmeat",
            "ppja.artisanvalleymachinegoods",
            "Hadi.FrozenTreatsJA",
            "zcsnightmare.naw",
            "KAYA.ColorfulGarden",
            "KAYA.GrowFence",
            "KAYA.KoreaBlossom",
            "KAYA.Mulberry",
            "amburr.spoopyvalley",
            "amburr.stardewvineyard",
            "vNIANHUAv.MoreTeaJA",
            "Popobug.SPCFW",
            "fwippy.champagnewishes",
            "jfujii.TeaTime",
            "Cinnanimroll.CoconutTree",
            "MelindaC.FizzyDrinks",
            "lumisteria.smallfruittrees",
            "key.cropspack",
            "deffiliation.JAKimchiMaker",
            "xiaobaishu.JA.fenjieji",
            "furyx639.ComposterJA",
            "Ritsune.BakersLifeJA",
            "Ritsune.ChocolatierJA",
            "Ivycrowned.JARealisticLooms",
            "KAYA.JACandyMachine",
            "KAYA.JAMoreMead",
            "richardtrle.expandedcrops",
            "zhellybelly.ancientfruitvariants",
            "BonsterTrees",
            "minervamaga.JA.EemieCrops",
            "callysquared.marshmallows" };
        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            //ShopTileFramework.Utility.ItemsUtil.ItemsToRemove.AddRange(_itemToRemove);
            //ShopTileFramework.Utility.ItemsUtil.RecipePacksToRemove.AddRange(_recipePacksToRemove);
            ShopTileFramework.Utility.ItemsUtil.PacksToRemove.AddRange(_packsToRemove);

            shops.Add("Zamir",new ZamirShop(Helper,Monitor));
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                if (e.Button != SButton.MouseLeft)
                    return;
                if (e.Cursor.GrabTile != e.Cursor.Tile)
                    return;
            }
            else if (!e.Button.IsActionButton())
                return;

            Vector2 clickedTile = Helper.Input.GetCursorPosition().GrabTile;

            //check if there is a tile property on Buildings layer
            string property = Game1.currentLocation.doesTileHaveProperty((int) clickedTile.X, (int) clickedTile.Y, "PPJAVMarket", "Buildings");
            if (property == null) return; //not our property not our problem
            if (!shops.ContainsKey(property)) return; //uuuuh this shouldn't happen

            var shop = shops[property];
            if (shop.CanOpen())
            {
                Game1.activeClickableMenu = shop.CreateShopMenu();
            }
            else
            {
                Game1.drawObjectDialogue(shop.ShopClosed);
            }
        }
    }
}
