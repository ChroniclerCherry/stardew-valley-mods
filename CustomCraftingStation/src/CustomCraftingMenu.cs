using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomCraftingStation
{
    public class CustomCraftingMenu : IClickableMenu
    {
        private string descriptionText = "";
        private string hoverText = "";
        protected List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();
        public List<ClickableComponent> currentPageClickableComponents = new List<ClickableComponent>();
        private string hoverTitle = "";
        public const int howManyRecipesFitOnPage = 40;
        public const int numInRow = 10;
        public const int numInCol = 4;
        public const int region_upArrow = 88;
        public const int region_downArrow = 89;
        public const int region_craftingSelectionArea = 8000;
        public const int region_craftingModifier = 200;
        private Item hoverItem;
        private Item lastCookingHover;
        public InventoryMenu inventory;
        private Item heldItem;
        private int currentCraftingPage;
        private CraftingRecipe hoverRecipe;
        public ClickableTextureComponent upButton;
        public ClickableTextureComponent downButton;
        public ClickableTextureComponent trashCan;
        public ClickableComponent dropItemInvisibleButton;
        public float trashCanLidRotation;
        protected List<Chest> _materialContainers;
        private int hoverAmount;

        public CustomCraftingMenu(
            int x,
            int y,
            int width,
            int height,
            List<Chest> material_containers, List<string> craftingRecipes, List<string> cookingRecipes)
            : base(x, y, width, height, false)
        {

            inventory = new InventoryMenu(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen + spaceToClearTopBorder + borderWidth + 320 - 16, false, (IList<Item>)null, (InventoryMenu.highlightThisItem)null, -1, 3, 0, 0, true);
            inventory.showGrayedOutSlots = true;
            currentPageClickableComponents = new List<ClickableComponent>();
            foreach (ClickableComponent clickableComponent in inventory.GetBorder(InventoryMenu.BorderSide.Top))
                clickableComponent.upNeighborID = -99998;
            _materialContainers = material_containers;
            List<Chest> materialContainers = _materialContainers;
            initializeUpperRightCloseButton();
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - 32 - borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false);
            textureComponent1.myID = 106;
            trashCan = textureComponent1;
            dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPositionOnScreen - borderWidth - spaceToClearSideBorder - 64, trashCan.bounds.Y, 64, 64), "")
            {
                myID = 107,
                rightNeighborID = 0
            };
            
                layoutRecipe(craftingRecipes, cookingRecipes);
            if (pagesOfCraftingRecipes.Count > 1)
            {
                ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY(), 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false);
                textureComponent2.myID = 88;
                textureComponent2.downNeighborID = 89;
                textureComponent2.rightNeighborID = 106;
                textureComponent2.leftNeighborID = -99998;
                upButton = textureComponent2;
                ClickableTextureComponent textureComponent3 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY() + 192 + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false);
                textureComponent3.myID = 89;
                textureComponent3.upNeighborID = 88;
                textureComponent3.rightNeighborID = 106;
                textureComponent3.leftNeighborID = -99998;
                downButton = textureComponent3;
            }
            _UpdateCurrentPageButtons();
            if (!Game1.options.SnappyMenus)
                return;
            snapToDefaultClickableComponent();
        }

        protected virtual IList<Item> getContainerContents()
        {
            if (_materialContainers == null)
                return (IList<Item>)null;
            List<Item> objList = new List<Item>();
            for (int index = 0; index < _materialContainers.Count; ++index)
                objList.AddRange((IEnumerable<Item>)_materialContainers[index].items);
            return (IList<Item>)objList;
        }

        private int craftingPageY()
        {
            return yPositionOnScreen + spaceToClearTopBorder + borderWidth - 16;
        }

        private ClickableTextureComponent[,] createNewPageLayout()
        {
            return new ClickableTextureComponent[10, 4];
        }

        private Dictionary<ClickableTextureComponent, CraftingRecipe> createNewPage()
        {
            Dictionary<ClickableTextureComponent, CraftingRecipe> dictionary = new Dictionary<ClickableTextureComponent, CraftingRecipe>();
            pagesOfCraftingRecipes.Add(dictionary);
            return dictionary;
        }

        private bool spaceOccupied(
            ClickableTextureComponent[,] pageLayout,
            int x,
            int y,
            CraftingRecipe recipe)
        {
            if (pageLayout[x, y] != null)
                return true;
            if (!recipe.bigCraftable)
                return false;
            return y + 1 >= 4 || pageLayout[x, y + 1] != null;
        }

        private int? getNeighbor(
            ClickableTextureComponent[,] pageLayout,
            int x,
            int y,
            int dx,
            int dy)
        {
            if (x < 0 || y < 0 || (x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1)))
                return new int?();
            ClickableTextureComponent textureComponent1 = pageLayout[x, y];
            ClickableTextureComponent textureComponent2;
            for (textureComponent2 = textureComponent1; textureComponent2 == textureComponent1; textureComponent2 = pageLayout[x, y])
            {
                x += dx;
                y += dy;
                if (x < 0 || y < 0 || (x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1)))
                    return new int?();
            }
            return textureComponent2?.myID;
        }
        private void layoutRecipe(List<string> craftingRecipes, List<string> cookingingRecipes)
        {

            int craftingPageX = xPositionOnScreen + spaceToClearSideBorder + borderWidth - 16;
            int spaceBetweenCraftingIcons = 8;
            var newPage = createNewPage();
            int x = 0;
            int y = 0;
            int numRecipes = 0;
            ClickableTextureComponent[,] newPageLayout = createNewPageLayout();
            foreach (string playerRecipe in craftingRecipes)
            {
                ++numRecipes;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, false);
                while (spaceOccupied(newPageLayout, x, y, recipe))
                {
                    ++x;
                    if (x >= 10)
                    {
                        x = 0;
                        ++y;
                        if (y >= 4)
                        {
                            newPage = createNewPage();
                            newPageLayout = createNewPageLayout();
                            x = 0;
                            y = 0;
                        }
                    }
                }

                int id = 200 + numRecipes;
                var textureComponent = new ClickableTextureComponent("",
                    new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY() + y * 72, 64,
                        recipe.bigCraftable ? 128 : 64), null,
                    Game1.player.craftingRecipes.ContainsKey(recipe.name) ? "" : "ghosted",
                    recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet,
                    recipe.bigCraftable
                        ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32,
                            recipe.getIndexOfMenuView())
                        : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(),
                            16, 16), 4f)
                {
                    myID = id,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                ClickableTextureComponent component = textureComponent;
                newPage.Add(component, recipe);
                newPageLayout[x, y] = component;
                if (recipe.bigCraftable)
                    newPageLayout[x, y + 1] = component;
            }

            foreach (string playerRecipe in cookingingRecipes)
            {
                ++numRecipes;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, true);
                while (spaceOccupied(newPageLayout, x, y, recipe))
                {
                    ++x;
                    if (x >= 10)
                    {
                        x = 0;
                        ++y;
                        if (y >= 4)
                        {
                            newPage = createNewPage();
                            newPageLayout = createNewPageLayout();
                            x = 0;
                            y = 0;
                        }
                    }
                }

                int num5 = 200 + numRecipes;
                var textureComponent = new ClickableTextureComponent("",
                    new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY() + y * 72, 64,
                        recipe.bigCraftable ? 128 : 64), null,
                    Game1.player.cookingRecipes.ContainsKey(recipe.name) ? "" : "ghosted",
                    recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet,
                    recipe.bigCraftable
                        ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32,
                            recipe.getIndexOfMenuView())
                        : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(),
                            16, 16), 4f)
                {
                    myID = num5,
                    rightNeighborID = -99998,
                    leftNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    fullyImmutable = true,
                    region = 8000
                };
                ClickableTextureComponent key = textureComponent;
                newPage.Add(key, recipe);
                newPageLayout[x, y] = key;
                if (recipe.bigCraftable)
                    newPageLayout[x, y + 1] = key;
            }

        }
        protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
        {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion != 8000 || direction != 2)
                return;
            currentlySnappedComponent = getComponentWithID(oldID % 10);
            currentlySnappedComponent.upNeighborID = oldID;
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = currentCraftingPage < pagesOfCraftingRecipes.Count ? (ClickableComponent)pagesOfCraftingRecipes[currentCraftingPage].First<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key : (ClickableComponent)null;
            snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion != 9000 || oldRegion == 0)
                return;
            for (int index = 0; index < 10; ++index)
            {
                if (inventory.inventory.Count > index)
                    inventory.inventory[index].upNeighborID = currentlySnappedComponent.upNeighborID;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (!key.Equals((object)Keys.Delete) || heldItem == null || !heldItem.canBeTrashed())
                return;
            Utility.trashItem(heldItem);
            heldItem = (Item)null;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentCraftingPage > 0)
            {
                --currentCraftingPage;
                _UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (!Game1.options.SnappyMenus)
                    return;
                setCurrentlySnappedComponentTo(88);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                if (direction >= 0 || currentCraftingPage >= pagesOfCraftingRecipes.Count - 1)
                    return;
                ++currentCraftingPage;
                _UpdateCurrentPageButtons();
                Game1.playSound("shwip");
                if (!Game1.options.SnappyMenus)
                    return;
                setCurrentlySnappedComponentTo(89);
                snapCursorToCurrentSnappedComponent();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            heldItem = inventory.leftClick(x, y, heldItem, true);
            if (upButton != null && upButton.containsPoint(x, y) && currentCraftingPage > 0)
            {
                Game1.playSound("coin");
                currentCraftingPage = Math.Max(0, currentCraftingPage - 1);
                _UpdateCurrentPageButtons();
                upButton.scale = upButton.baseScale;
            }
            if (downButton != null && downButton.containsPoint(x, y) && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
            {
                Game1.playSound("coin");
                currentCraftingPage = Math.Min(pagesOfCraftingRecipes.Count - 1, currentCraftingPage + 1);
                _UpdateCurrentPageButtons();
                downButton.scale = downButton.baseScale;
            }
            foreach (ClickableTextureComponent key in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                int num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (int index = 0; index < num; ++index)
                {
                    if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && pagesOfCraftingRecipes[currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(getContainerContents()))
                        clickCraftingRecipe(key, index == 0);
                }
                if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift) && (heldItem.maximumStackSize() == 1 && Game1.player.couldInventoryAcceptThisItem(heldItem)))
                {
                    Game1.player.addItemToInventoryBool(heldItem, false);
                    heldItem = (Item)null;
                }
            }
            if (trashCan != null && trashCan.containsPoint(x, y) && (heldItem != null && heldItem.canBeTrashed()))
            {
                Utility.trashItem(heldItem);
                heldItem = (Item)null;
            }
            else
            {
                if (heldItem == null || isWithinBounds(x, y) || !heldItem.canBeTrashed())
                    return;
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, (GameLocation)null, -1);
                heldItem = (Item)null;
            }
        }

        protected void _UpdateCurrentPageButtons()
        {
            currentPageClickableComponents.Clear();
            foreach (ClickableComponent key in pagesOfCraftingRecipes[currentCraftingPage].Keys)
                currentPageClickableComponents.Add(key);
            populateClickableComponentList();
        }

        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            Item obj = pagesOfCraftingRecipes[currentCraftingPage][c].createItem();
            if (heldItem == null)
            {
                pagesOfCraftingRecipes[currentCraftingPage][c].consumeIngredients(_materialContainers);
                heldItem = obj;
                if (playSound)
                    Game1.playSound("coin");
            }
            else
            {
                if (!heldItem.Name.Equals(obj.Name) || heldItem.Stack + pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft - 1 >= heldItem.maximumStackSize())
                    return;
                heldItem.Stack += pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft;
                pagesOfCraftingRecipes[currentCraftingPage][c].consumeIngredients(_materialContainers);
                if (playSound)
                    Game1.playSound("coin");
            }
            Game1.player.checkForQuestComplete((NPC)null, -1, -1, obj, (string)null, 2, -1);

            bool cooking = heldItem!= null && CraftingRecipe.cookingRecipes.ContainsKey(heldItem.Name);

            if (!cooking && Game1.player.craftingRecipes.ContainsKey(pagesOfCraftingRecipes[currentCraftingPage][c].name))
                Game1.player.craftingRecipes[pagesOfCraftingRecipes[currentCraftingPage][c].name] += pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft;
            if (cooking)
                Game1.player.cookedRecipe(heldItem.ParentSheetIndex);
            if (!cooking)
                Game1.stats.checkForCraftingAchievements();
            else
                Game1.stats.checkForCookingAchievements();
            if (!Game1.options.gamepadControls || heldItem == null || !Game1.player.couldInventoryAcceptThisItem(heldItem))
                return;
            Game1.player.addItemToInventoryBool(heldItem, false);
            heldItem = (Item)null;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            heldItem = inventory.rightClick(x, y, heldItem, true, false);
            foreach (ClickableTextureComponent key in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && pagesOfCraftingRecipes[currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(getContainerContents()))
                    clickCraftingRecipe(key, true);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            hoverTitle = "";
            descriptionText = "";
            hoverText = "";
            hoverRecipe = (CraftingRecipe)null;
            hoverItem = inventory.hover(x, y, hoverItem);
            hoverAmount = -1;
            if (hoverItem != null)
            {
                hoverTitle = inventory.hoverTitle;
                hoverText = inventory.hoverText;
            }
            foreach (ClickableTextureComponent key in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                if (key.containsPoint(x, y))
                {
                    if (key.hoverText.Equals("ghosted"))
                    {
                        hoverText = "???";
                    }
                    else
                    {
                        hoverRecipe = pagesOfCraftingRecipes[currentCraftingPage][key];
                        if (lastCookingHover == null || !lastCookingHover.Name.Equals(hoverRecipe.name))
                            lastCookingHover = hoverRecipe.createItem();
                        key.scale = Math.Min(key.scale + 0.02f, key.baseScale + 0.1f);
                    }
                }
                else
                    key.scale = Math.Max(key.scale - 0.02f, key.baseScale);
            }
            if (upButton != null)
            {
                if (upButton.containsPoint(x, y))
                    upButton.scale = Math.Min(upButton.scale + 0.02f, upButton.baseScale + 0.1f);
                else
                    upButton.scale = Math.Max(upButton.scale - 0.02f, upButton.baseScale);
            }
            if (downButton != null)
            {
                if (downButton.containsPoint(x, y))
                    downButton.scale = Math.Min(downButton.scale + 0.02f, downButton.baseScale + 0.1f);
                else
                    downButton.scale = Math.Max(downButton.scale - 0.02f, downButton.baseScale);
            }
            if (trashCan == null)
                return;
            if (trashCan.containsPoint(x, y))
            {
                if ((double)trashCanLidRotation <= 0.0)
                    Game1.playSound("trashcanlid");
                trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, 1.570796f);
                if (heldItem == null || Utility.getTrashReclamationPrice(heldItem, Game1.player) <= 0)
                    return;
                hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
            }
            else
                trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 48f, 0.0f);
        }

        public override bool readyToClose()
        {
            return heldItem == null;
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, (string)null, false, true, -1, -1, -1);
            drawHorizontalPartition(b, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256, false, -1, -1, -1);
            inventory.draw(b);
            if (trashCan != null)
            {
                trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2((float)(trashCan.bounds.X + 60), (float)(trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            foreach (ClickableTextureComponent key in pagesOfCraftingRecipes[currentCraftingPage].Keys)
            {
                if (key.hoverText.Equals("ghosted"))
                    key.draw(b, Color.Black * 0.35f, 0.89f, 0);
                else if (!pagesOfCraftingRecipes[currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(getContainerContents()))
                {
                    key.draw(b, Color.LightGray * 0.4f, 0.89f, 0);
                    if (pagesOfCraftingRecipes[currentCraftingPage][key].numberProducedPerCraft > 1)
                        NumberSprite.draw(pagesOfCraftingRecipes[currentCraftingPage][key].numberProducedPerCraft, b, new Vector2((float)(key.bounds.X + 64 - 2), (float)(key.bounds.Y + 64 - 2)), Color.LightGray * 0.75f, (float)(0.5 * ((double)key.scale / 4.0)), 0.97f, 1f, 0, 0);
                }
                else
                {
                    key.draw(b);
                    if (pagesOfCraftingRecipes[currentCraftingPage][key].numberProducedPerCraft > 1)
                        NumberSprite.draw(pagesOfCraftingRecipes[currentCraftingPage][key].numberProducedPerCraft, b, new Vector2((float)(key.bounds.X + 64 - 2), (float)(key.bounds.Y + 64 - 2)), Color.White, (float)(0.5 * ((double)key.scale / 4.0)), 0.97f, 1f, 0, 0);
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (hoverItem != null)
                drawToolTip(b, hoverText, hoverTitle, hoverItem, heldItem != null, -1, 0, -1, -1, (CraftingRecipe)null, -1);
            else if (!string.IsNullOrEmpty(hoverText))
            {
                if (hoverAmount > 0)
                    drawToolTip(b, hoverText, hoverTitle, (Item)null, true, -1, 0, -1, -1, (CraftingRecipe)null, hoverAmount);
                else
                    drawHoverText(b, hoverText, Game1.smallFont, heldItem != null ? 64 : 0, heldItem != null ? 64 : 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null, (IList<Item>)null);
            }
            if (heldItem != null)
                heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
            base.draw(b);
            if (downButton != null && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
                downButton.draw(b);
            if (upButton != null && currentCraftingPage > 0)
                upButton.draw(b);

            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
            if (this.hoverRecipe == null)
                return;
            SpriteBatch b1 = b;
            SpriteFont smallFont = Game1.smallFont;
            int xOffset = heldItem != null ? 48 : 0;
            int yOffset = heldItem != null ? 48 : 0;
            string displayName = this.hoverRecipe.DisplayName;
            string[] buffIconsToDisplay;
            buffIconsToDisplay = (string[])null;

            if (this.lastCookingHover != null && Game1.objectInformation.ContainsKey(
                (this.lastCookingHover as StardewValley.Object)?.parentSheetIndex))
            {
                if (Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex]?.Split('/').Length > 7)
                {
                    buffIconsToDisplay = Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex].Split('/')[7].Split(' ');
                }
            }


            Item lastCookingHover = this.lastCookingHover;
            CraftingRecipe hoverRecipe = this.hoverRecipe;
            IList<Item> containerContents = getContainerContents();
            drawHoverText(b1, " ", smallFont, xOffset, yOffset, -1, displayName, -1, buffIconsToDisplay, lastCookingHover, 0, -1, -1, -1, -1, 1f, hoverRecipe, containerContents);
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
        {
            return false;
        }

        public override bool IsAutomaticSnapValid(
            int direction,
            ClickableComponent a,
            ClickableComponent b)
        {
            return (a != downButton && a != upButton || (direction != 3 || b.region == 8000)) && (a.region != 8000 || direction != 3 && direction != 1 || b.region != 9000) && (a.region != 8000 || direction != 2 || b != upButton && b != downButton) && base.IsAutomaticSnapValid(direction, a, b);
        }
    }
}
