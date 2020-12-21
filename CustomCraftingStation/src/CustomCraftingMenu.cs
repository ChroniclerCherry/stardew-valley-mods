using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Menus
{
    public class CustomCraftingMenu : IClickableMenu
    {
        public const int howManyRecipesFitOnPage = 40;

        public const int numInRow = 10;

        public const int numInCol = 4;

        public const int region_upArrow = 88;

        public const int region_downArrow = 89;

        public const int region_craftingSelectionArea = 8000;

        public const int region_craftingModifier = 200;

        private string descriptionText = "";

        private string hoverText = "";

        private Item hoverItem;

        private Item lastCookingHover;

        public InventoryMenu inventory;

        private Item heldItem;

        [SkipForClickableAggregation]
        public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

        private int currentCraftingPage;

        private CraftingRecipe hoverRecipe;

        public ClickableTextureComponent upButton;

        public ClickableTextureComponent downButton;

        public ClickableTextureComponent trashCan;

        public ClickableComponent dropItemInvisibleButton;

        public float trashCanLidRotation;

        public List<Chest> _materialContainers;

        private int hoverAmount;

        public List<ClickableComponent> currentPageClickableComponents = new List<ClickableComponent>();

        private string hoverTitle = "";

        public CustomCraftingMenu(int x, int y, int width, int height,
            List<Chest> material_containers,
            List<string> craftingRecipes, List<string> cookingRecipes)
            : base(x, y, width, height)
        {
            this.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16, playerInventory: false);
            this.inventory.showGrayedOutSlots = true;
            this.currentPageClickableComponents = new List<ClickableComponent>();
            foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
            {
                item.upNeighborID = -99998;
            }
            this._materialContainers = material_containers;
            _ = this._materialContainers;
            base.initializeUpperRightCloseButton();
            this.trashCan = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width + 4, base.yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
            {
                myID = 106
            };
            this.dropItemInvisibleButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.trashCan.bounds.Y, 64, 64), "")
            {
                myID = 107,
                rightNeighborID = 0
            };
            List<string> playerRecipes = new List<string>();
            Game1.playSound("bigSelect");
            this.layoutRecipes(craftingRecipes,cookingRecipes);
            if (this.pagesOfCraftingRecipes.Count > 1)
            {
                this.upButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 768 + 32, this.craftingPageY(), 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 0.8f)
                {
                    myID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                this.downButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 768 + 32, this.craftingPageY() + 192 + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 0.8f)
                {
                    myID = 89,
                    upNeighborID = 88,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
            }
            this._UpdateCurrentPageButtons();
            if (Game1.options.SnappyMenus)
            {
                this.snapToDefaultClickableComponent();
            }
        }

        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
        }

        protected virtual IList<Item> getContainerContents()
        {
            if (this._materialContainers == null)
            {
                return null;
            }
            List<Item> items = new List<Item>();
            for (int i = 0; i < this._materialContainers.Count; i++)
            {
                items.AddRange(this._materialContainers[i].items);
            }
            return items;
        }

        private int craftingPageY()
        {
            return base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
        }

        private ClickableTextureComponent[,] createNewPageLayout()
        {
            return new ClickableTextureComponent[10, 4];
        }

        private Dictionary<ClickableTextureComponent, CraftingRecipe> createNewPage()
        {
            Dictionary<ClickableTextureComponent, CraftingRecipe> page = new Dictionary<ClickableTextureComponent, CraftingRecipe>();
            this.pagesOfCraftingRecipes.Add(page);
            return page;
        }

        private bool spaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y, CraftingRecipe recipe)
        {
            if (pageLayout[x, y] != null)
            {
                return true;
            }
            if (!recipe.bigCraftable)
            {
                return false;
            }
            if (y + 1 < 4)
            {
                return pageLayout[x, y + 1] != null;
            }
            return true;
        }

        private int? getNeighbor(ClickableTextureComponent[,] pageLayout, int x, int y, int dx, int dy)
        {
            if (x < 0 || y < 0 || x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1))
            {
                return null;
            }
            ClickableTextureComponent currentElement = pageLayout[x, y];
            ClickableTextureComponent neighborElement;
            for (neighborElement = currentElement; neighborElement == currentElement; neighborElement = pageLayout[x, y])
            {
                x += dx;
                y += dy;
                if (x < 0 || y < 0 || x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1))
                {
                    return null;
                }
            }
            return neighborElement?.myID;
        }

        private void layoutRecipes(List<string> craftingRecipes, List<string> cookingingRecipes)
        {
            int craftingPageX = base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
            int spaceBetweenCraftingIcons = 8;
            Dictionary<ClickableTextureComponent, CraftingRecipe> currentPage = this.createNewPage();
            int x = 0;
            int y = 0;
            int i = 0;
            ClickableTextureComponent[,] pageLayout = this.createNewPageLayout();
            List<ClickableTextureComponent[,]> pageLayouts = new List<ClickableTextureComponent[,]>();
            pageLayouts.Add(pageLayout);
            foreach (string playerRecipe in craftingRecipes)
            {
                i++;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, false);
                while (this.spaceOccupied(pageLayout, x, y, recipe))
                {
                    x++;
                    if (x >= 10)
                    {
                        x = 0;
                        y++;
                        if (y >= 4)
                        {
                            currentPage = this.createNewPage();
                            pageLayout = this.createNewPageLayout();
                            pageLayouts.Add(pageLayout);
                            x = 0;
                            y = 0;
                        }
                    }
                }
                int id = 200 + i;
                ClickableTextureComponent component = new ClickableTextureComponent("", new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), this.craftingPageY() + y * 72, 64, recipe.bigCraftable ? 128 : 64), null, (!Game1.player.cookingRecipes.ContainsKey(recipe.name) && !Game1.player.craftingRecipes.ContainsKey(recipe.name)) ? "ghosted" : "", recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, recipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, recipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(), 16, 16), 4f)
                {
                    myID = id,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                currentPage.Add(component, recipe);
                pageLayout[x, y] = component;
                if (recipe.bigCraftable)
                {
                    pageLayout[x, y + 1] = component;
                }
            }

            foreach (string playerRecipe in cookingingRecipes)
            {
                i++;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, true);
                while (this.spaceOccupied(pageLayout, x, y, recipe))
                {
                    x++;
                    if (x >= 10)
                    {
                        x = 0;
                        y++;
                        if (y >= 4)
                        {
                            currentPage = this.createNewPage();
                            pageLayout = this.createNewPageLayout();
                            pageLayouts.Add(pageLayout);
                            x = 0;
                            y = 0;
                        }
                    }
                }
                int id = 200 + i;
                ClickableTextureComponent component = new ClickableTextureComponent("", new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), this.craftingPageY() + y * 72, 64, recipe.bigCraftable ? 128 : 64), null, (!Game1.player.cookingRecipes.ContainsKey(recipe.name) && !Game1.player.craftingRecipes.ContainsKey(recipe.name)) ? "ghosted" : "", recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, recipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, recipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(), 16, 16), 4f)
                {
                    myID = id,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                currentPage.Add(component, recipe);
                pageLayout[x, y] = component;
                if (recipe.bigCraftable)
                {
                    pageLayout[x, y + 1] = component;
                }
            }
        }


        protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
        {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion == 8000 && direction == 2)
            {
                base.currentlySnappedComponent = base.getComponentWithID(oldID % 10);
                base.currentlySnappedComponent.upNeighborID = oldID;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = ((this.currentCraftingPage < this.pagesOfCraftingRecipes.Count) ? this.pagesOfCraftingRecipes[this.currentCraftingPage].First().Key : null);
            this.snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion != 9000 || oldRegion == 0)
            {
                return;
            }
            for (int i = 0; i < 10; i++)
            {
                if (this.inventory.inventory.Count > i)
                {
                    this.inventory.inventory[i].upNeighborID = base.currentlySnappedComponent.upNeighborID;
                }
            }
        }

        protected virtual bool checkHeldItem(Func<Item, bool> f = null)
        {
            return f?.Invoke(Game1.player.CursorSlotItem) ?? (Game1.player.CursorSlotItem != null);
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (key.Equals(Keys.Delete) && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                Utility.trashItem(this.heldItem);
                this.heldItem = null;
            }
            if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.heldItem != null)
            {
                Game1.setMousePosition(this.trashCan.bounds.Center);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentCraftingPage > 0)
            {
                this.currentCraftingPage--;
                this._UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus)
                {
                    this.setCurrentlySnappedComponentTo(88);
                    this.snapCursorToCurrentSnappedComponent();
                }
            }
            else if (direction < 0 && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                this.currentCraftingPage++;
                this._UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (Game1.options.SnappyMenus)
                {
                    this.setCurrentlySnappedComponentTo(89);
                    this.snapCursorToCurrentSnappedComponent();
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y);
            this.heldItem = this.inventory.leftClick(x, y, this.heldItem);
            if (this.upButton != null && this.upButton.containsPoint(x, y) && this.currentCraftingPage > 0)
            {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Max(0, this.currentCraftingPage - 1);
                this._UpdateCurrentPageButtons();
                this.upButton.scale = this.upButton.baseScale;
            }
            if (this.downButton != null && this.downButton.containsPoint(x, y) && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this.currentCraftingPage + 1);
                this._UpdateCurrentPageButtons();
                this.downButton.scale = this.downButton.baseScale;
            }
            foreach (ClickableTextureComponent c in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                int times = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : 5;
                for (int i = 0; i < times; i++)
                {
                    if (c.containsPoint(x, y) && !c.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(this.getContainerContents()))
                    {
                        this.clickCraftingRecipe(c, i == 0);
                    }
                }
                if (this.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.heldItem.maximumStackSize() == 1 && Game1.player.couldInventoryAcceptThisItem(this.heldItem))
                {
                    Game1.player.addItemToInventoryBool(this.heldItem);
                    this.heldItem = null;
                }
            }
            if (this.trashCan != null && this.trashCan.containsPoint(x, y) && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                Utility.trashItem(this.heldItem);
                this.heldItem = null;
            }
            else if (this.heldItem != null && !this.isWithinBounds(x, y) && this.heldItem.canBeTrashed())
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                this.heldItem = null;
            }
        }

        protected void _UpdateCurrentPageButtons()
        {
            this.currentPageClickableComponents.Clear();
            foreach (ClickableTextureComponent component in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                this.currentPageClickableComponents.Add(component);
            }
            base.populateClickableComponentList();
        }

        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            Item crafted = this.pagesOfCraftingRecipes[this.currentCraftingPage][c].createItem();
            List<KeyValuePair<int, int>> seasoning = null;
            if (crafted is Object && (crafted as Object).Quality == 0)
            {
                seasoning = new List<KeyValuePair<int, int>>();
                seasoning.Add(new KeyValuePair<int, int>(917, 1));
                if (CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(seasoning, this.getContainerContents()))
                {
                    (crafted as Object).Quality = 2;
                }
                else
                {
                    seasoning = null;
                }
            }
            if (this.heldItem == null)
            {
                this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients(this._materialContainers);
                this.heldItem = crafted;
                if (playSound)
                {
                    Game1.playSound("coin");
                }
            }
            else
            {
                if (!this.heldItem.Name.Equals(crafted.Name) || !this.heldItem.getOne().canStackWith(crafted.getOne()) || this.heldItem.Stack + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft - 1 >= this.heldItem.maximumStackSize())
                {
                    return;
                }
                this.heldItem.Stack += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
                this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients(this._materialContainers);
                if (playSound)
                {
                    Game1.playSound("coin");
                }
            }
            if (seasoning != null)
            {
                if (playSound)
                {
                    Game1.playSound("breathin");
                }
                CraftingRecipe.ConsumeAdditionalIngredients(seasoning, this._materialContainers);
                if (!CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(seasoning, this.getContainerContents()))
                {
                    Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Seasoning_UsedLast"));
                }
            }
            Game1.player.checkForQuestComplete(null, -1, -1, crafted, null, 2);
            if (Game1.player.craftingRecipes.ContainsKey(this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name))
            {
                Game1.player.craftingRecipes[this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name] += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
            }
            if (CraftingRecipe.cookingRecipes.ContainsKey(this.heldItem.Name))
            {
                
                Game1.player.cookedRecipe(this.heldItem.parentSheetIndex);
                Game1.stats.checkForCookingAchievements();
            }
            else
            {
                Game1.stats.checkForCraftingAchievements();
            }
            if (Game1.options.gamepadControls && this.heldItem != null && Game1.player.couldInventoryAcceptThisItem(this.heldItem))
            {
                Game1.player.addItemToInventoryBool(this.heldItem);
                this.heldItem = null;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.heldItem = this.inventory.rightClick(x, y, this.heldItem);
            foreach (ClickableTextureComponent c in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (c.containsPoint(x, y) && !c.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(this.getContainerContents()))
                {
                    this.clickCraftingRecipe(c);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverTitle = "";
            this.descriptionText = "";
            this.hoverText = "";
            this.hoverRecipe = null;
            this.hoverItem = this.inventory.hover(x, y, this.hoverItem);
            this.hoverAmount = -1;
            if (this.hoverItem != null)
            {
                this.hoverTitle = this.inventory.hoverTitle;
                this.hoverText = this.inventory.hoverText;
            }
            foreach (ClickableTextureComponent c in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (c.containsPoint(x, y))
                {
                    if (c.hoverText.Equals("ghosted"))
                    {
                        this.hoverText = "???";
                        continue;
                    }
                    this.hoverRecipe = this.pagesOfCraftingRecipes[this.currentCraftingPage][c];
                    if (this.lastCookingHover == null || !this.lastCookingHover.Name.Equals(this.hoverRecipe.name))
                    {
                        this.lastCookingHover = this.hoverRecipe.createItem();
                    }
                    c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
                }
                else
                {
                    c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
                }
            }
            if (this.upButton != null)
            {
                if (this.upButton.containsPoint(x, y))
                {
                    this.upButton.scale = Math.Min(this.upButton.scale + 0.02f, this.upButton.baseScale + 0.1f);
                }
                else
                {
                    this.upButton.scale = Math.Max(this.upButton.scale - 0.02f, this.upButton.baseScale);
                }
            }
            if (this.downButton != null)
            {
                if (this.downButton.containsPoint(x, y))
                {
                    this.downButton.scale = Math.Min(this.downButton.scale + 0.02f, this.downButton.baseScale + 0.1f);
                }
                else
                {
                    this.downButton.scale = Math.Max(this.downButton.scale - 0.02f, this.downButton.baseScale);
                }
            }
            if (this.trashCan == null)
            {
                return;
            }
            if (this.trashCan.containsPoint(x, y))
            {
                if (this.trashCanLidRotation <= 0f)
                {
                    Game1.playSound("trashcanlid");
                }
                this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
                if (this.heldItem != null && Utility.getTrashReclamationPrice(this.heldItem, Game1.player) > 0)
                {
                    this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                    this.hoverAmount = Utility.getTrashReclamationPrice(this.heldItem, Game1.player);
                }
            }
            else
            {
                this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - (float)Math.PI / 48f, 0f);
            }
        }

        public override bool readyToClose()
        {
            return this.heldItem == null;
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
            base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256);
            this.inventory.draw(b);
            if (this.trashCan != null)
            {
                this.trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2(this.trashCan.bounds.X + 60, this.trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            foreach (ClickableTextureComponent c in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (c.hoverText.Equals("ghosted"))
                {
                    c.draw(b, Color.Black * 0.35f, 0.89f);
                }
                else if (!this.pagesOfCraftingRecipes[this.currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(this.getContainerContents()))
                {
                    c.draw(b, Color.DimGray * 0.4f, 0.89f);
                    if (this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft > 1)
                    {
                        NumberSprite.draw(this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft, b, new Vector2(c.bounds.X + 64 - 2, c.bounds.Y + 64 - 2), Color.LightGray * 0.75f, 0.5f * (c.scale / 4f), 0.97f, 1f, 0);
                    }
                }
                else
                {
                    c.draw(b);
                    if (this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft > 1)
                    {
                        NumberSprite.draw(this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft, b, new Vector2(c.bounds.X + 64 - 2, c.bounds.Y + 64 - 2), Color.White, 0.5f * (c.scale / 4f), 0.97f, 1f, 0);
                    }
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (this.hoverItem != null)
            {
                IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoverItem, this.heldItem != null);
            }
            else if (!string.IsNullOrEmpty(this.hoverText))
            {
                if (this.hoverAmount > 0)
                {
                    IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, null, heldItem: true, -1, 0, -1, -1, null, this.hoverAmount);
                }
                else
                {
                    IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, (this.heldItem != null) ? 64 : 0, (this.heldItem != null) ? 64 : 0);
                }
            }
            if (this.heldItem != null)
            {
                this.heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
            }
            base.draw(b);
            if (this.downButton != null && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                this.downButton.draw(b);
            }
            if (this.upButton != null && this.currentCraftingPage > 0)
            {
                this.upButton.draw(b);
            }
            Game1.mouseCursorTransparency = 1f;
                base.drawMouse(b);
                if (this.hoverRecipe != null)
            {
                IClickableMenu.drawHoverText(b, " ", Game1.smallFont, (this.heldItem != null) ? 48 : 0, (this.heldItem != null) ? 48 : 0, -1, this.hoverRecipe.DisplayName + ((this.hoverRecipe.numberProducedPerCraft > 1) ? (" x" + this.hoverRecipe.numberProducedPerCraft) : ""), -1, (this.lastCookingHover != null && Game1.objectInformation[(this.lastCookingHover as Object).parentSheetIndex].Split('/').Length > 7) ? Game1.objectInformation[(this.lastCookingHover as Object).parentSheetIndex].Split('/')[7].Split(' ') : null, this.lastCookingHover, 0, -1, -1, -1, -1, 1f, this.hoverRecipe, this.getContainerContents());
            }
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
        {
            return false;
        }

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if ((a == this.downButton || a == this.upButton) && direction == 3 && b.region != 8000)
            {
                return false;
            }
            if (a.region == 8000 && (direction == 3 || direction == 1) && b.region == 9000)
            {
                return false;
            }
            if (a.region == 8000 && direction == 2 && (b == this.upButton || b == this.downButton))
            {
                return false;
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (this.heldItem != null)
            {
                Item item = this.heldItem;
                this.heldItem = null;
                Utility.CollectOrDrop(item);
            }
        }
    }
}