using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace CustomizeAnywhere.Framework;

internal class DresserAndMirror
{
    /*********
    ** Fields
    *********/
    private IModHelper Helper;

    private readonly string ModId;
    private readonly string DresserShopId;

    private readonly string CatalogueId;
    private readonly string MirrorId;

    private readonly string CatalogueQualifiedId;
    private readonly string MirrorQualifiedId;


    /*********
    ** Public methods
    *********/
    public DresserAndMirror(IModHelper helper, string modId)
    {
        this.ModId = modId;
        this.DresserShopId = $"{modId}_Dresser";
        this.CatalogueId = $"{modId}_Catalogue";
        this.MirrorId = $"{modId}_Mirror";

        this.CatalogueQualifiedId = ItemRegistry.type_bigCraftable + this.CatalogueId;
        this.MirrorQualifiedId = ItemRegistry.type_bigCraftable + this.MirrorId;

        this.Helper = helper;
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
    }

    public void OpenDresser()
    {
        Utility.TryOpenShopMenu(this.DresserShopId, null as string);
        if (Game1.activeClickableMenu is ShopMenu menu)
        {
            menu.UseDresserTabs();
            menu.tabButtons.RemoveAll(p => p.myID is (ShopMenu.region_tabStartIndex + 4) or (ShopMenu.region_tabStartIndex + 5)); // remove boots + rings tabs
            menu.repositionTabs();
        }
    }


    /*********
    ** Private methods
    *********/
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        // add objects
        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, BigCraftableData>().Data;

                data[this.CatalogueId] = new BigCraftableData
                {
                    Name = this.CatalogueId,
                    DisplayName = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{this.CatalogueId}_Name"),
                    Description = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{this.CatalogueId}_Description"),
                    Texture = $"LooseSprites/{this.CatalogueId}"
                };
                data[this.MirrorId] = new BigCraftableData
                {
                    Name = this.CatalogueId,
                    DisplayName = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{this.MirrorId}_Name"),
                    Description = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{this.MirrorId}_Description"),
                    Texture = $"LooseSprites/{this.MirrorId}"
                };
            });
        }

        // add recipes
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                data[this.CatalogueId] = $"388 10/Field/{this.CatalogueId}/true/null/"; // 10 wood
                data[this.MirrorId] = $"388 10 338 2/Field/{this.MirrorId}/true/null/"; // 10 wood, 2 quartz
            });
        }

        // add dresser shop & add recipes
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, ShopData>().Data;

                // add catalogue + mirror recipes
                if (data.TryGetValue(Game1.shop_carpenter, out ShopData shop))
                {
                    shop.Items.Add(new ShopItemData
                    {
                        Id = this.CatalogueId,
                        ItemId = this.CatalogueId,
                        IsRecipe = true,
                        Price = 50_000
                    });
                    shop.Items.Add(new ShopItemData
                    {
                        Id = this.MirrorId,
                        ItemId = this.MirrorId,
                        IsRecipe = true,
                        Price = 50_000
                    });
                }

                // add dresser shop
                data[this.DresserShopId] = new ShopData
                {
                    Owners = new()
                    {
                        new ShopOwnerData
                        {
                            Id = "Default",
                            Name = "AnyOrNone",
                            Portrait = "",
                            Dialogues = new()
                        }
                    },

                    Items = new()
                    {
                        new ShopItemData
                        {
                            Id = "Shirts",
                            ItemId = "ALL_ITEMS (S)",
                            Price = 0
                        },
                        new ShopItemData
                        {
                            Id = "Pants",
                            ItemId = "ALL_ITEMS (P)",
                            Price = 0
                        },
                        new ShopItemData
                        {
                            Id = "Hats",
                            ItemId = "ALL_ITEMS (H)",
                            Price = 0
                        }
                    }
                };
            });
        }

        // add textures
        else if (e.NameWithoutLocale.IsEquivalentTo($"LooseSprites/{this.CatalogueId}"))
            e.LoadFromModFile<Texture2D>("assets/catalogue.png", AssetLoadPriority.Exclusive);
        else if (e.NameWithoutLocale.IsEquivalentTo($"LooseSprites/{this.MirrorId}"))
            e.LoadFromModFile<Texture2D>("assets/mirror.png", AssetLoadPriority.Exclusive);

        // add translation text
        else if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, string>().Data;

                data[$"{this.CatalogueId}_Name"] = this.Helper.Translation.Get("Catalogue_Name");
                data[$"{this.CatalogueId}_Description"] = this.Helper.Translation.Get("Catalogue_Description");

                data[$"{this.MirrorId}_Name"] = this.Helper.Translation.Get("Mirror_Name");
                data[$"{this.MirrorId}_Description"] = this.Helper.Translation.Get("Mirror_Description");
            });
        }
    }

    private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (Context.IsWorldReady && Game1.currentLocation != null && Game1.activeClickableMenu == null && e.Button.IsActionButton())
        {
            GameLocation loc = Game1.currentLocation;

            Vector2 tile = ModEntry.StaticHelper.Input.GetCursorPosition().GrabTile;
            if (loc.Objects.TryGetValue(tile, out Object obj))
            {
                if (obj.QualifiedItemId == this.CatalogueQualifiedId)
                {
                    this.OpenDresser();
                    this.Helper.Input.Suppress(e.Button);
                }
                else if (obj.QualifiedItemId == this.MirrorQualifiedId)
                {
                    Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
                    this.Helper.Input.Suppress(e.Button);
                }
            }
        }
    }
}
