using System.Collections.Generic;
using System.Linq;
using ChroniclerCherry.Common.Integrations.GenericModConfigMenu;
using HarmonyLib;
using HayBalesAsSilos.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Shops;

namespace HayBalesAsSilos;

public class ModEntry : Mod
{
    internal static IMonitor monitor;
    internal static ModConfig Config;

    internal static readonly string HayBaleId = "45";
    internal static readonly string HayBaleQualifiedId = ItemRegistry.type_bigCraftable + "45";

    public override void Entry(IModHelper helper)
    {
        // init
        I18n.Init(helper.Translation);
        monitor = this.Monitor;
        Config = this.Helper.ReadConfig<ModConfig>();

        // add patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.GetHayCapacity)),
            postfix: new HarmonyMethod(typeof(PatchGameLocation), nameof(PatchGameLocation.After_GetHayCapacity))
        );

        // hook events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForHayBaysAsSilos(),
            get: () => ModEntry.Config,
            set: config => ModEntry.Config = config
        );
    }

    private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        //ignore input if the player isnt free to move aka world not loaded,
        //they're in an event, a menu is up, etc
        if (!Context.CanPlayerMove)
            return;

        GameLocation location = Game1.currentLocation;
        if (!GetAllAffectedMaps().Contains(location))
            return;

        //action button works for right click on mouse and action button for controllers
        if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
            return;

        //check if the clicked tile contains a Farm Renderer
        Vector2 tile = this.Helper.Input.GetCursorPosition().GrabTile;
        if (location.Objects.TryGetValue(tile, out Object obj) && obj.QualifiedItemId == HayBaleQualifiedId)
        {
            if (location.getBuildingByType("Silo") is null)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
                return;
            }

            if (e.Button.IsActionButton())
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay", location.piecesOfHay.Value, location.GetHayCapacity()));
            else if (e.Button.IsUseToolButton())
            {
                //if holding hay, try to add it
                if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Hay")
                {
                    int stack = Game1.player.ActiveObject.Stack;
                    int tryToAddHay = location.tryToAddHay(Game1.player.ActiveObject.Stack);
                    Game1.player.ActiveObject.Stack = tryToAddHay;

                    if (Game1.player.ActiveObject.Stack < stack)
                    {
                        Game1.playSound("Ship");
                        DelayedAction.playSoundAfterDelay("grassyStep", 100);
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", stack - Game1.player.ActiveObject.Stack));
                    }
                    if (Game1.player.ActiveObject.Stack <= 0)
                        Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                }
            }
        }
    }

    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        // edit hay bale text
        if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                data["OrnamentalHayBale_Name"] = this.Helper.Translation.Get("DisplayName");
                data["OrnamentalHayBale_Description"] = this.Helper.Translation.Get("Description").ToString().Replace("{{capacity}}", Config.HayPerBale.ToString());
            });
        }

        // add to shop
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;

                if (data.TryGetValue(Game1.shop_animalSupplies, out ShopData shop))
                {
                    shop.Items.Add(new ShopItemData
                    {
                        Id = HayBaleId,
                        ItemId = HayBaleId,
                        Price = Config.HaybalePrice,
                        Condition = "PLAYER_HAS_MAIL Current FarmRearrangerMail Received"
                    });
                }
            });
        }
    }

    internal static IEnumerable<GameLocation> GetAllAffectedMaps()
    {
        yield return Game1.getFarm();
        foreach (Building building in Game1.getFarm().buildings.Where(building => building.indoors.Value != null))
        {
            yield return building.indoors.Value;
        }
    }
}
