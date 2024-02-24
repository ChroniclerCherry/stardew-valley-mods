using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace CustomizeAnywhere.Framework
{
    class DresserAndMirror
    {
        private IModHelper Helper;

        private readonly string ModId;
        private readonly string DresserShopId;

        private readonly string CatalogueId;
        private readonly string MirrorId;

        private readonly string CatalogueQualifiedId;
        private readonly string MirrorQualifiedId;

        public DresserAndMirror(IModHelper helper, string modId)
        {
            ModId = modId;
            DresserShopId = $"{modId}_Dresser";
            CatalogueId = $"{modId}_Catalogue";
            MirrorId = $"{modId}_Mirror";

            CatalogueQualifiedId = ItemRegistry.type_bigCraftable + CatalogueId;
            MirrorQualifiedId = ItemRegistry.type_bigCraftable + MirrorId;

            this.Helper = helper;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
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

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            // add objects
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;

                    data[CatalogueId] = new BigCraftableData
                    {
                        Name = CatalogueId,
                        DisplayName = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{CatalogueId}_Name"),
                        Description = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{CatalogueId}_Description"),
                        Texture = $"LooseSprites/{CatalogueId}"
                    };
                    data[MirrorId] = new BigCraftableData
                    {
                        Name = CatalogueId,
                        DisplayName = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{MirrorId}_Name"),
                        Description = TokenStringBuilder.LocalizedText($"Strings/BigCraftables:{MirrorId}_Description"),
                        Texture = $"LooseSprites/{MirrorId}"
                    };
                });
            }

            // add recipes
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    data[CatalogueId] = $"388 10/Field/{CatalogueId}/true/null/"; // 10 wood
                    data[MirrorId] = $"388 10 338 2/Field/{MirrorId}/true/null/"; // 10 wood, 2 quartz
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
            else if (e.NameWithoutLocale.IsEquivalentTo($"LooseSprites/{CatalogueId}"))
                e.LoadFromModFile<Texture2D>("assets/catalogue.png", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo($"LooseSprites/{MirrorId}"))
                e.LoadFromModFile<Texture2D>("assets/mirror.png", AssetLoadPriority.Exclusive);

            // add translation text
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    data[$"{CatalogueId}_Name"] = this.Helper.Translation.Get("Catalogue_Name");
                    data[$"{CatalogueId}_Description"] = this.Helper.Translation.Get("Catalogue_Description");

                    data[$"{MirrorId}_Name"] = this.Helper.Translation.Get("Mirror_Name");
                    data[$"{MirrorId}_Description"] = this.Helper.Translation.Get("Mirror_Description");
                });
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady && Game1.currentLocation != null && Game1.activeClickableMenu == null && e.Button.IsActionButton())
            {
                GameLocation loc = Game1.currentLocation;

                Vector2 tile = ModEntry.helper.Input.GetCursorPosition().GrabTile;
                if (loc.Objects.TryGetValue(tile, out Object obj))
                {
                    if (obj.QualifiedItemId == CatalogueQualifiedId)
                    {
                        this.OpenDresser();
                        Helper.Input.Suppress(e.Button);
                    }
                    else if (obj.QualifiedItemId == MirrorQualifiedId)
                    {
                        Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
        }
    }
}
