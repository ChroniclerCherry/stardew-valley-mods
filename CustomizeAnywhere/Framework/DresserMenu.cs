using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace CustomizeAnywhere.Framework
{
    public class DresserMenu : IClickableMenu
    {
        private string descriptionText = "";
        private string hoverText = "";
        private string boldTitleText = "";
        public string purchaseSound = "purchaseClick";
        public string purchaseRepeatSound = "purchaseRepeat";
        public List<ISalable> forSale = new List<ISalable>();
        public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();
        public List<int> categoriesToSellHere = new List<int>();
        public Dictionary<ISalable, ItemStockInformation> itemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();
        private float sellPercentage = 1f;
        private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        public int hoverPrice = -1;
        public List<ClickableTextureComponent> tabButtons = new List<ClickableTextureComponent>();
        public const int region_shopButtonModifier = 3546;
        public const int region_upArrow = 97865;
        public const int region_downArrow = 97866;
        public const int region_tabStartIndex = 99999;
        public const int howManyRecipesFitOnPage = 28;
        public const int infiniteStock = 2147483647;
        public const int salePriceIndex = 0;
        public const int stockIndex = 1;
        public const int extraTradeItemIndex = 2;
        public const int extraTradeItemCountIndex = 3;
        public const int itemsPerPage = 4;
        public const int numberRequiredForExtraItemTrade = 5;
        public InventoryMenu inventory;
        public ISalable heldItem;
        public ISalable hoveredItem;
        private TemporaryAnimatedSprite poof;
        private Rectangle scrollBarRunner;
        public int currency;
        public int currentItemIndex;
        public ClickableTextureComponent upArrow;
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        public NPC portraitPerson;
        public string potraitPersonDialogue;
        public object source;
        private bool scrolling;
        public Func<ISalable, Farmer, int, bool> onPurchase;
        public Func<ISalable, bool> onSell;
        public Func<int, bool> canPurchaseCheck;
        protected int currentTab;
        protected bool _isStorageShop;

        public DresserMenu()
            : base(Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true)
        {

            this.itemPriceAndStock = getClothingItems();

            this.updatePosition();
            this.currency = 0;
            this.onPurchase = null;
            this.onSell = null;
            Game1.player.forceCanMove();
            Game1.playSound("dwop");
            this.inventory = new InventoryMenu(this.xPositionOnScreen + this.width, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, false, (IList<Item>)null, new InventoryMenu.highlightThisItem(this.highlightItemToSell), -1, 3, 0, 0, true)
            {
                showGrayedOutSlots = true
            };
            this.inventory.movePosition(-this.inventory.width - 32, 0);
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
            textureComponent1.myID = 97865;
            textureComponent1.downNeighborID = 106;
            textureComponent1.leftNeighborID = 3546;
            this.upArrow = textureComponent1;
            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
            textureComponent2.myID = 106;
            textureComponent2.upNeighborID = 97865;
            textureComponent2.leftNeighborID = 3546;
            this.downArrow = textureComponent2;
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
            for (int index = 0; index < 4; ++index)
                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + index * ((this.height - 256) / 4), this.width - 32, (this.height - 256) / 4 + 4), string.Concat((object)index))
                {
                    myID = index + 3546,
                    rightNeighborID = 97865,
                    fullyImmutable = true
                });
            this.updateSaleButtonNeighbors();
            this.setUpStore();
            if (this.tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in this.forSaleButtons)
                    forSaleButton.leftNeighborID = -99998;
            }
            this.applyTab();
            foreach (ClickableComponent clickableComponent in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
                clickableComponent.upNeighborID = -99998;
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        private Dictionary<ISalable, ItemStockInformation> getClothingItems()
        {
            Dictionary<ISalable, ItemStockInformation> itemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();

            //add Clothing
            foreach (KeyValuePair<int, string> keyValuePair in (IEnumerable<KeyValuePair<int, string>>)Game1.clothingInformation)
            {
                try
                {
                    itemPriceAndStock.Add(new Clothing(keyValuePair.Key), new ItemStockInformation(0, int.MaxValue));
                }
                catch (Exception ex)
                {
                    ModEntry.monitor.Log("Something went wrong loading clothing items:\n" + ex, LogLevel.Warn);
                }
            }

            //add hats
            foreach (KeyValuePair<int, string> keyValuePair in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
            {
                try
                {
                    itemPriceAndStock.Add(new Hat(keyValuePair.Key), new ItemStockInformation(0, int.MaxValue));
                }
                catch (Exception ex)
                {
                    ModEntry.monitor.Log("Something went wrong loading hats:\n" + ex, LogLevel.Warn);
                }
            }

            return itemPriceAndStock;
        }
        public void updateSaleButtonNeighbors()
        {
            ClickableComponent clickableComponent = this.forSaleButtons[0];
            for (int index = 0; index < this.forSaleButtons.Count; ++index)
            {
                ClickableComponent forSaleButton = this.forSaleButtons[index];
                forSaleButton.upNeighborImmutable = true;
                forSaleButton.downNeighborImmutable = true;
                forSaleButton.upNeighborID = index > 0 ? index + 3546 - 1 : -7777;
                forSaleButton.downNeighborID = index >= 3 || index >= this.forSale.Count - 1 ? -7777 : index + 3546 + 1;
                if (index >= this.forSale.Count)
                {
                    if (forSaleButton == this.currentlySnappedComponent)
                    {
                        this.currentlySnappedComponent = clickableComponent;
                        if (Game1.options.SnappyMenus)
                            this.snapCursorToCurrentSnappedComponent();
                    }
                }
                else
                    clickableComponent = forSaleButton;
            }
        }

        public virtual void setUpStore()
        {
            this.tabButtons = new List<ClickableTextureComponent>();
                    this._isStorageShop = true;
                    ClickableTextureComponent textureComponent4 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f, false);
                    textureComponent4.myID = 99999 + this.tabButtons.Count;
                    textureComponent4.upNeighborID = -99998;
                    textureComponent4.downNeighborID = -99998;
                    textureComponent4.rightNeighborID = 3546;
                    this.tabButtons.Add(textureComponent4);
                    ClickableTextureComponent textureComponent5 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f, false);
                    textureComponent5.myID = 99999 + this.tabButtons.Count;
                    textureComponent5.upNeighborID = -99998;
                    textureComponent5.downNeighborID = -99998;
                    textureComponent5.rightNeighborID = 3546;
                    this.tabButtons.Add(textureComponent5);
                    ClickableTextureComponent textureComponent6 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f, false);
                    textureComponent6.myID = 99999 + this.tabButtons.Count;
                    textureComponent6.upNeighborID = -99998;
                    textureComponent6.downNeighborID = -99998;
                    textureComponent6.rightNeighborID = 3546;
                    this.tabButtons.Add(textureComponent6);
                    ClickableTextureComponent textureComponent7 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f, false);
                    textureComponent7.myID = 99999 + this.tabButtons.Count;
                    textureComponent7.upNeighborID = -99998;
                    textureComponent7.downNeighborID = -99998;
                    textureComponent7.rightNeighborID = 3546;
                    this.tabButtons.Add(textureComponent7);

            this.repositionTabs();
            if (!this._isStorageShop)
                return;
            this.purchaseSound = (string)null;
            this.purchaseRepeatSound = (string)null;
        }

        public void repositionTabs()
        {
            for (int index = 0; index < this.tabButtons.Count; ++index)
            {
                if (index == this.currentTab)
                    this.tabButtons[index].bounds.X = this.xPositionOnScreen - 56;
                else
                    this.tabButtons[index].bounds.X = this.xPositionOnScreen - 64;
                this.tabButtons[index].bounds.Y = this.yPositionOnScreen + index * 16 * 4 + 16;
            }
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            switch (direction)
            {
                case 0:
                    if (this.currentItemIndex <= 0)
                        break;
                    this.upArrowPressed();
                    this.currentlySnappedComponent = this.getComponentWithID(3546);
                    this.snapCursorToCurrentSnappedComponent();
                    break;
                case 2:
                    if (this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
                    {
                        this.downArrowPressed();
                        break;
                    }
                    int num = -1;
                    for (int index = 0; index < 12; ++index)
                    {
                        this.inventory.inventory[index].upNeighborID = oldID;
                        if (num == -1 && this.heldItem != null && (this.inventory.actualInventory != null && this.inventory.actualInventory.Count > index) && this.inventory.actualInventory[index] == null)
                            num = index;
                    }
                    this.currentlySnappedComponent = this.getComponentWithID(num != -1 ? num : 0);
                    this.snapCursorToCurrentSnappedComponent();
                    break;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(3546);
            this.snapCursorToCurrentSnappedComponent();
        }
        public bool highlightItemToSell(Item i)
        {
            return this.categoriesToSellHere.Contains(i.Category);
        }

        public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
        {
            switch (currencyType)
            {
                case 0:
                    return who.Money;
                case 1:
                    return who.festivalScore;
                case 2:
                    return who.clubCoins;
                default:
                    return 0;
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (!this.scrolling)
                return;
            int y1 = this.scrollBar.bounds.Y;
            this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + 20));
            this.currentItemIndex = Math.Min(this.forSale.Count - 4, Math.Max(0, (int)((double)this.forSale.Count * (double)((float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height))));
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
            int y2 = this.scrollBar.bounds.Y;
            if (y1 == y2)
                return;
            Game1.playSound("shiny4");
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.scrolling = false;
        }

        private void setScrollBarToCurrentIndex()
        {
            if (this.forSale.Count <= 0)
                return;
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.forSale.Count - 4 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
            if (this.currentItemIndex != this.forSale.Count - 4)
                return;
            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, this.forSale.Count - 4))
                    return;
                this.downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        private void downArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            ++this.currentItemIndex;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            --this.currentItemIndex;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            if (Game1.activeClickableMenu == null)
                return;
            Vector2 clickableComponent = this.inventory.snapToClickableComponent(x, y);
            if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
            {
                this.downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            for (int new_tab = 0; new_tab < this.tabButtons.Count; ++new_tab)
            {
                if (this.tabButtons[new_tab].containsPoint(x, y))
                    this.switchTab(new_tab);
            }
            this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
            if (this.heldItem == null)
            {
                Item obj1 = this.inventory.leftClick(x, y, (Item)null, false);
                if (obj1 != null)
                {
                    Item obj2;
                    if (this.onSell != null)
                    {
                        if (this.onSell((ISalable)obj1))
                            obj2 = (Item)null;
                    }
                    else
                    {
                        ShopMenu.chargePlayer(Game1.player, this.currency, -((obj1 is StardewValley.Object ? (int)((double)(obj1 as StardewValley.Object).sellToStorePrice(-1L) * (double)this.sellPercentage) : (int)((double)(obj1.salePrice() / 2) * (double)this.sellPercentage)) * obj1.Stack));
                        int num = obj1.Stack / 8 + 2;
                        for (int index = 0; index < num; ++index)
                        {
                            this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                alphaFade = 0.025f,
                                motion = new Vector2((float)Game1.random.Next(-3, 4), -4f),
                                acceleration = new Vector2(0.0f, 0.5f),
                                delayBeforeAnimationStart = index * 25,
                                scale = 2f
                            });
                            this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                scale = 4f,
                                alphaFade = 0.025f,
                                delayBeforeAnimationStart = index * 50,
                                motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 8f),
                                acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 0.5f)
                            });
                        }
                        if (obj1 is StardewValley.Object && (int)((NetFieldBase<int, NetInt>)(obj1 as StardewValley.Object).edibility) != -300)
                        {
                            Item one = obj1.getOne();
                            one.Stack = obj1.Stack;
                            if (Game1.currentLocation is ShopLocation)
                                (Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Add(one);
                        }
                        obj2 = (Item)null;
                        Game1.playSound("sell");
                        Game1.playSound("purchase");
                        if (this.inventory.getItemAt(x, y) == null)
                            this.animations.Add(new TemporaryAnimatedSprite(5, clickableComponent + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                            {
                                motion = new Vector2(0.0f, -0.5f)
                            });
                    }
                    this.updateSaleButtonNeighbors();
                }
            }
            else
                this.heldItem = (ISalable)this.inventory.leftClick(x, y, this.heldItem as Item, true);
            for (int index1 = 0; index1 < this.forSaleButtons.Count; ++index1)
            {
                if (this.currentItemIndex + index1 < this.forSale.Count && this.forSaleButtons[index1].containsPoint(x, y))
                {
                    int index2 = this.currentItemIndex + index1;
                    if (this.forSale[index2] != null)
                    {
                        int numberToBuy = Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / Math.Max(1, this.itemPriceAndStock[this.forSale[index2]].Price)), Math.Max(1, this.itemPriceAndStock[this.forSale[index2]].Stock)) : 1, this.forSale[index2].maximumStackSize());
                        if (numberToBuy == -1)
                            numberToBuy = 1;
                        if (this.canPurchaseCheck != null && !this.canPurchaseCheck(index2))
                            return;
                        if (numberToBuy > 0 && this.tryToPurchaseItem(this.forSale[index2], this.heldItem, numberToBuy, x, y, index2))
                        {
                            this.itemPriceAndStock.Remove(this.forSale[index2]);
                            this.forSale.RemoveAt(index2);
                        }
                        else if (numberToBuy <= 0)
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                            Game1.playSound("cancel");
                        }
                        if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus || Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.heldItem.maximumStackSize() == 1) && (Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item, false)))
                        {
                            this.heldItem = (ISalable)null;
                            DelayedAction.playSoundAfterDelay("coin", 100, (GameLocation)null, -1);
                        }
                    }
                    this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
                    this.updateSaleButtonNeighbors();
                    this.setScrollBarToCurrentIndex();
                    return;
                }
            }
            if (!this.readyToClose() || x >= this.xPositionOnScreen - 64 && y >= this.yPositionOnScreen - 64 && (x <= this.xPositionOnScreen + this.width + 128 && y <= this.yPositionOnScreen + this.height + 64))
                return;
            this.exitThisMenu(true);
        }

        public override bool IsAutomaticSnapValid(
            int direction,
            ClickableComponent a,
            ClickableComponent b)
        {
            if (direction == 1 && ((IEnumerable<ClickableComponent>)this.tabButtons).Contains<ClickableComponent>(a) && ((IEnumerable<ClickableComponent>)this.tabButtons).Contains<ClickableComponent>(b))
                return false;
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        public virtual void switchTab(int new_tab)
        {
            this.currentTab = new_tab;
            Game1.playSound("shwip");
            this.applyTab();
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.snapCursorToCurrentSnappedComponent();
        }

        public virtual void applyTab()
        {

                if (this.currentTab == 0)
                {
                    this.forSale = this.itemPriceAndStock.Keys.ToList<ISalable>();
                }
                else
                {
                    this.forSale.Clear();
                    foreach (ISalable key in this.itemPriceAndStock.Keys)
                    {
                        Item obj = key as Item;
                        if (obj != null)
                        {
                            if (this.currentTab == 1)
                            {
                                if (obj.Category == -95)
                                    this.forSale.Add((ISalable)obj);
                            }
                            else if (this.currentTab == 2)
                            {
                                if (obj is Clothing && (obj as Clothing).clothesType.Value == 0)
                                    this.forSale.Add((ISalable)obj);
                            }
                            else if (this.currentTab == 3)
                            {
                                if (obj is Clothing && (obj as Clothing).clothesType.Value == 1)
                                    this.forSale.Add((ISalable)obj);
                            }
                            else if (this.currentTab == 4)
                            {
                                if (obj.Category == -97)
                                    this.forSale.Add((ISalable)obj);
                            }
                            else if (this.currentTab == 5 && obj.Category == -96)
                                this.forSale.Add((ISalable)obj);
                        }
                    }
                }
            this.currentItemIndex = 0;
            this.setScrollBarToCurrentIndex();
            this.updateSaleButtonNeighbors();
        }

        public override bool readyToClose()
        {
            if (this.heldItem == null)
                return this.animations.Count == 0;
            return false;
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (this.heldItem == null)
                return;
            Game1.player.addItemToInventoryBool(this.heldItem as Item, false);
            Game1.playSound("coin");
        }

        public static void chargePlayer(Farmer who, int currencyType, int amount)
        {
            switch (currencyType)
            {
                case 0:
                    who.Money -= amount;
                    break;
                case 1:
                    who.festivalScore -= amount;
                    break;
                case 2:
                    who.clubCoins -= amount;
                    break;
            }
        }

        private bool tryToPurchaseItem(
            ISalable item,
            ISalable held_item,
            int numberToBuy,
            int x,
            int y,
            int indexInForSaleList)
        {
            if (held_item == null)
            {
                if (this.itemPriceAndStock[item].Stock == 0)
                {
                    this.hoveredItem = (ISalable)null;
                    return true;
                }
                if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
                    numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
                int amount = this.itemPriceAndStock[item].Price * numberToBuy;

                // this code broke in Stardew Valley 1.6, but it's redundant anyway since the
                // dresser doesn't have item trade prices.
                //int num1 = -1;
                //int num2 = 5;
                //if (this.itemPriceAndStock[item].Length > 2)
                //{
                //    num1 = this.itemPriceAndStock[item][2];
                //    if (this.itemPriceAndStock[item].Length > 3)
                //        num2 = this.itemPriceAndStock[item][3];
                //    num2 *= numberToBuy;
                //}
                if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= amount /*&& (num1 == -1 || Game1.player.hasItemInInventory(num1, num2, 0))*/)
                {
                    this.heldItem = item.GetSalableInstance();
                    this.heldItem.Stack = numberToBuy;
                    if (this.itemPriceAndStock[item].Stock == int.MaxValue && item.Stack != int.MaxValue)
                        this.heldItem.Stack *= item.Stack;
                    if (!this.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && (!(this.heldItem is StardewValley.Object) || !(bool)((NetFieldBase<bool, NetBool>)(this.heldItem as StardewValley.Object).isRecipe)))
                    {
                        Game1.playSound("smallSelect");
                        this.heldItem = (ISalable)null;
                        return false;
                    }
                    if (this.itemPriceAndStock[item].Stock != int.MaxValue && !item.IsInfiniteStock())
                    {
                        var stockInfo = this.itemPriceAndStock[item];
                        this.itemPriceAndStock[item] = stockInfo with { Stock = stockInfo.Stock - numberToBuy };
                        this.forSale[indexInForSaleList].Stack -= numberToBuy;
                    }
                    ShopMenu.chargePlayer(Game1.player, this.currency, amount);
                    //if (num1 != -1)
                    //    Game1.player.removeItemsFromInventory(num1, num2);
                    if (item.actionWhenPurchased(GetShopMenuContext()))
                    {
                        if (this.heldItem is StardewValley.Object && (bool)((NetFieldBase<bool, NetBool>)(this.heldItem as StardewValley.Object).isRecipe))
                        {
                            string key = this.heldItem.Name.Substring(0, this.heldItem.Name.IndexOf("Recipe") - 1);
                            try
                            {
                                if ((this.heldItem as StardewValley.Object).Category == -7)
                                    Game1.player.cookingRecipes.Add(key, 0);
                                else
                                    Game1.player.craftingRecipes.Add(key, 0);
                                Game1.playSound("newRecipe");
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        held_item = (ISalable)null;
                        this.heldItem = (ISalable)null;
                    }
                    else if (Game1.mouseClickPolling > 300)
                    {
                        if (this.purchaseRepeatSound != null)
                            Game1.playSound(this.purchaseRepeatSound);
                    }
                    else if (this.purchaseSound != null)
                        Game1.playSound(this.purchaseSound);
                    if (this.onPurchase != null && this.onPurchase(item, Game1.player, numberToBuy))
                        this.exitThisMenu(true);
                }
                else
                {
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    Game1.playSound("cancel");
                }
            }
            else if (held_item.canStackWith(item))
            {
                numberToBuy = Math.Min(numberToBuy, held_item.maximumStackSize() - held_item.Stack);
                if (numberToBuy > 0)
                {
                    int amount = this.itemPriceAndStock[item].Price * numberToBuy;
                    //int num1 = -1;
                    //int num2 = 5;
                    //if (this.itemPriceAndStock[item].Length > 2)
                    //{
                    //    num1 = this.itemPriceAndStock[item][2];
                    //    if (this.itemPriceAndStock[item].Length > 3)
                    //        num2 = this.itemPriceAndStock[item][3];
                    //    num2 *= numberToBuy;
                    //}
                    int stack = item.Stack;
                    item.Stack = numberToBuy + this.heldItem.Stack;
                    if (!item.CanBuyItem(Game1.player))
                    {
                        item.Stack = stack;
                        Game1.playSound("cancel");
                        return false;
                    }
                    item.Stack = stack;
                    if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= amount /*&& (num1 == -1 || Game1.player.hasItemInInventory(num1, num2, 0))*/)
                    {
                        int num3 = numberToBuy;
                        if (this.itemPriceAndStock[item].Stock == int.MaxValue && item.Stack != int.MaxValue)
                            num3 *= item.Stack;
                        this.heldItem.Stack += num3;
                        if (this.itemPriceAndStock[item].Stock != int.MaxValue && !item.IsInfiniteStock())
                        {
                            var stockInfo = this.itemPriceAndStock[item];
                            this.itemPriceAndStock[item] = stockInfo with { Stock = stockInfo.Stock - numberToBuy };
                            this.forSale[indexInForSaleList].Stack -= numberToBuy;
                        }
                        ShopMenu.chargePlayer(Game1.player, this.currency, amount);
                        if (Game1.mouseClickPolling > 300)
                        {
                            if (this.purchaseRepeatSound != null)
                                Game1.playSound(this.purchaseRepeatSound);
                        }
                        else if (this.purchaseSound != null)
                            Game1.playSound(this.purchaseSound);
                        //if (num1 != -1)
                        //    Game1.player.removeItemsFromInventory(num1, num2);
                        if (item.actionWhenPurchased(GetShopMenuContext()))
                            this.heldItem = (ISalable)null;
                        if (this.onPurchase != null && this.onPurchase(item, Game1.player, numberToBuy))
                            this.exitThisMenu(true);
                    }
                    else
                    {
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        Game1.playSound("cancel");
                    }
                }
            }
            if (this.itemPriceAndStock[item].Stock > 0)
                return false;
            this.hoveredItem = (ISalable)null;
            return true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Vector2 clickableComponent = this.inventory.snapToClickableComponent(x, y);
            if (this.heldItem == null)
            {
                ISalable salable1 = (ISalable)this.inventory.rightClick(x, y, (Item)null, false, false);
                if (salable1 != null)
                {
                    if (this.onSell != null)
                    {
                        if (this.onSell(salable1))
                            ;
                    }
                    else
                    {
                        ShopMenu.chargePlayer(Game1.player, this.currency, -((salable1 is StardewValley.Object ? (int)((double)(salable1 as StardewValley.Object).sellToStorePrice(-1L) * (double)this.sellPercentage) : (int)((double)(salable1.salePrice() / 2) * (double)this.sellPercentage)) * salable1.Stack));
                        ISalable salable2 = (ISalable)null;
                        if (Game1.mouseClickPolling > 300)
                        {
                            if (this.purchaseRepeatSound != null)
                                Game1.playSound(this.purchaseRepeatSound);
                        }
                        else if (this.purchaseSound != null)
                            Game1.playSound(this.purchaseSound);
                        int num = 2;
                        for (int index = 0; index < num; ++index)
                        {
                            this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                alphaFade = 0.025f,
                                motion = new Vector2((float)Game1.random.Next(-3, 4), -4f),
                                acceleration = new Vector2(0.0f, 0.5f),
                                delayBeforeAnimationStart = index * 25,
                                scale = 2f
                            });
                            this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                scale = 4f,
                                alphaFade = 0.025f,
                                delayBeforeAnimationStart = index * 50,
                                motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 8f),
                                acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.xPositionOnScreen - 36), (float)(this.yPositionOnScreen + this.height - this.inventory.height - 16)), 0.5f)
                            });
                        }
                        if (salable2 is StardewValley.Object && (int)((NetFieldBase<int, NetInt>)(salable2 as StardewValley.Object).edibility) != -300 && (Game1.random.NextDouble() < 0.0399999991059303 && Game1.currentLocation is ShopLocation))
                            (Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Add((Item)salable2.GetSalableInstance());
                        if (this.inventory.getItemAt(x, y) == null)
                        {
                            Game1.playSound("sell");
                            this.animations.Add(new TemporaryAnimatedSprite(5, clickableComponent + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                            {
                                motion = new Vector2(0.0f, -0.5f)
                            });
                        }
                    }
                }
            }
            else
                this.heldItem = (ISalable)this.inventory.rightClick(x, y, this.heldItem as Item, true, false);
            for (int index1 = 0; index1 < this.forSaleButtons.Count; ++index1)
            {
                if (this.currentItemIndex + index1 < this.forSale.Count && this.forSaleButtons[index1].containsPoint(x, y))
                {
                    int index2 = this.currentItemIndex + index1;
                    if (this.forSale[index2] == null)
                        break;
                    int numberToBuy = 1;
                    if (this.itemPriceAndStock[this.forSale[index2]].Price > 0)
                        numberToBuy = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / this.itemPriceAndStock[this.forSale[index2]].Price), this.itemPriceAndStock[this.forSale[index2]].Stock) : 1;
                    if (this.canPurchaseCheck != null && !this.canPurchaseCheck(index2))
                        break;
                    if (numberToBuy > 0 && this.tryToPurchaseItem(this.forSale[index2], this.heldItem, numberToBuy, x, y, index2))
                    {
                        this.itemPriceAndStock.Remove(this.forSale[index2]);
                        this.forSale.RemoveAt(index2);
                    }
                    if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus) && (Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item, false)))
                    {
                        this.heldItem = (ISalable)null;
                        DelayedAction.playSoundAfterDelay("coin", 100, (GameLocation)null, -1);
                    }
                    this.setScrollBarToCurrentIndex();
                    break;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.descriptionText = "";
            this.hoverText = "";
            this.hoveredItem = (ISalable)null;
            this.hoverPrice = -1;
            this.boldTitleText = "";
            this.upArrow.tryHover(x, y, 0.1f);
            this.downArrow.tryHover(x, y, 0.1f);
            this.scrollBar.tryHover(x, y, 0.1f);
            if (this.scrolling)
                return;
            for (int index = 0; index < this.forSaleButtons.Count; ++index)
            {
                if (this.currentItemIndex + index < this.forSale.Count && this.forSaleButtons[index].containsPoint(x, y))
                {
                    ISalable key = this.forSale[this.currentItemIndex + index];
                    if (this.canPurchaseCheck == null || this.canPurchaseCheck(this.currentItemIndex + index))
                    {
                        this.hoverText = key.getDescription();
                        this.boldTitleText = key.DisplayName;
                        if (!this._isStorageShop)
                            this.hoverPrice = this.itemPriceAndStock == null || !this.itemPriceAndStock.ContainsKey(key) ? key.salePrice() : this.itemPriceAndStock[key].Price;
                        this.hoveredItem = key;
                        this.forSaleButtons[index].scale = Math.Min(this.forSaleButtons[index].scale + 0.03f, 1.1f);
                    }
                }
                else
                    this.forSaleButtons[index].scale = Math.Max(1f, this.forSaleButtons[index].scale - 0.03f);
            }
            if (this.heldItem != null)
                return;
            foreach (ClickableComponent c in this.inventory.inventory)
            {
                if (c.containsPoint(x, y))
                {
                    Item clickableComponent = this.inventory.getItemFromClickableComponent(c);
                    if (clickableComponent != null && (this.inventory.highlightMethod == null || this.inventory.highlightMethod(clickableComponent)))
                    {
                        if (this._isStorageShop)
                        {
                            this.hoverText = clickableComponent.getDescription();
                            this.boldTitleText = clickableComponent.DisplayName;
                            this.hoveredItem = (ISalable)clickableComponent;
                        }
                        else
                        {
                            this.hoverText = clickableComponent.DisplayName + " x" + (object)clickableComponent.Stack;
                            if (clickableComponent is StardewValley.Object @object && @object.needsToBeDonated())
                                this.hoverText = this.hoverText + "\n\n" + clickableComponent.getDescription() + "\n";
                            this.hoverPrice = (clickableComponent is StardewValley.Object ? (int)((double)(clickableComponent as StardewValley.Object).sellToStorePrice(-1L) * (double)this.sellPercentage) : (int)((double)(clickableComponent.salePrice() / 2) * (double)this.sellPercentage)) * clickableComponent.Stack;
                        }
                    }
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (this.poof != null && this.poof.update(time))
                this.poof = (TemporaryAnimatedSprite)null;
            this.repositionTabs();
        }

        public void drawCurrency(SpriteBatch b)
        {
            if (this._isStorageShop)
                return;
            if (this.currency == 0)
                Game1.dayTimeMoneyBox.drawMoneyBox(b, this.xPositionOnScreen - 36, this.yPositionOnScreen + this.height - this.inventory.height - 12);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.RightTrigger && b != Buttons.LeftTrigger)
                return;
            if (this.currentlySnappedComponent != null && this.currentlySnappedComponent.myID >= 3546)
            {
                int num = -1;
                for (int index = 0; index < 12; ++index)
                {
                    this.inventory.inventory[index].upNeighborID = 3546 + this.forSaleButtons.Count - 1;
                    if (num == -1 && this.heldItem != null && (this.inventory.actualInventory != null && this.inventory.actualInventory.Count > index) && this.inventory.actualInventory[index] == null)
                        num = index;
                }
                this.currentlySnappedComponent = this.getComponentWithID(num != -1 ? num : 0);
                this.snapCursorToCurrentSnappedComponent();
            }
            else
                this.snapToDefaultClickableComponent();
            Game1.playSound("shiny4");
        }

        private int getHoveredItemExtraItemIndex()
        {
            //if (this.itemPriceAndStock != null && this.hoveredItem != null && (this.itemPriceAndStock.ContainsKey(this.hoveredItem) && this.itemPriceAndStock[this.hoveredItem].Length > 2))
            //    return this.itemPriceAndStock[this.hoveredItem][2];
            return -1;
        }

        private int getHoveredItemExtraItemAmount()
        {
            //if (this.itemPriceAndStock != null && this.hoveredItem != null && (this.itemPriceAndStock.ContainsKey(this.hoveredItem) && this.itemPriceAndStock[this.hoveredItem].Length > 3))
            //    return this.itemPriceAndStock[this.hoveredItem][3];
            return 5;
        }

        public void updatePosition()
        {
            this.width = 1000 + IClickableMenu.borderWidth * 2;
            this.height = 600 + IClickableMenu.borderWidth * 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
            int num = this.xPositionOnScreen - 320;
            bool flag = false;
            if (this.portraitPerson != null)
                flag = true;
            if (this.potraitPersonDialogue != null && this.potraitPersonDialogue != "")
                flag = true;
            if (((num <= 0 ? 0 : (Game1.options.showMerchantPortraits ? 1 : 0)) & (flag ? 1 : 0)) != 0)
                return;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - (1000 + IClickableMenu.borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.updatePosition();
            this.initializeUpperRightCloseButton();
            Game1.player.forceCanMove();
            this.inventory = new InventoryMenu(this.xPositionOnScreen + this.width, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, false, (IList<Item>)null, new InventoryMenu.highlightThisItem(this.highlightItemToSell), -1, 3, 0, 0, true)
            {
                showGrayedOutSlots = true
            };
            this.inventory.movePosition(-this.inventory.width - 32, 0);
            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 64 - this.upArrow.bounds.Height - 28);
            this.forSaleButtons.Clear();
            for (int index = 0; index < 4; ++index)
                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16, this.yPositionOnScreen + 16 + index * ((this.height - 256) / 4), this.width - 32, (this.height - 256) / 4 + 4), string.Concat((object)index)));
            if (this.tabButtons.Count > 0)
            {
                foreach (ClickableComponent forSaleButton in this.forSaleButtons)
                    forSaleButton.leftNeighborID = -99998;
            }
            this.repositionTabs();
            foreach (ClickableComponent clickableComponent in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
                clickableComponent.upNeighborID = -99998;
        }

        public void setItemPriceAndStock(Dictionary<ISalable, ItemStockInformation> new_stock)
        {
            this.itemPriceAndStock = new_stock;
            this.forSale = this.itemPriceAndStock.Keys.ToList<ISalable>();
            this.applyTab();
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen + this.width - this.inventory.width - 32 - 24, this.yPositionOnScreen + this.height - 256 + 40, this.inventory.width + 56, this.height - 448 + 20, Color.White, 4f, true);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height - 256 + 32 + 4, Color.White, 4f, true);
            this.drawCurrency(b);
            for (int index1 = 0; index1 < this.forSaleButtons.Count; ++index1)
            {
                if (this.currentItemIndex + index1 < this.forSale.Count)
                {
                    bool flag1 = false;
                    if (this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + index1))
                        flag1 = true;
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.forSaleButtons[index1].bounds.X, this.forSaleButtons[index1].bounds.Y, this.forSaleButtons[index1].bounds.Width, this.forSaleButtons[index1].bounds.Height, !this.forSaleButtons[index1].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) || this.scrolling ? Color.White : Color.Wheat, 4f, false);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(this.forSaleButtons[index1].bounds.X + 32 - 12), (float)(this.forSaleButtons[index1].bounds.Y + 24 - 4)), new Rectangle?(new Rectangle(296, 363, 18, 18)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    ISalable index2 = this.forSale[this.currentItemIndex + index1];
                    int num1 = index2.Stack <= 1 || index2.Stack == int.MaxValue ? 0 : (this.itemPriceAndStock[index2].Stock == int.MaxValue ? 1 : 0);
                    StackDrawType drawStackNumber;
                    if (this.itemPriceAndStock[index2].Stock == int.MaxValue)
                    {
                        drawStackNumber = StackDrawType.Hide;
                    }
                    else
                    {
                        drawStackNumber = StackDrawType.Draw_OneInclusive;
                        if (this._isStorageShop)
                            drawStackNumber = StackDrawType.Draw;
                    }
                    this.forSale[this.currentItemIndex + index1].drawInMenu(b, new Vector2((float)(this.forSaleButtons[index1].bounds.X + 32 - 8), (float)(this.forSaleButtons[index1].bounds.Y + 24)), 1f, 1f, 0.9f, drawStackNumber, Color.White * (!flag1 ? 1f : 0.25f), true);
                    string s = index2.DisplayName;
                    if (num1 != 0)
                        s = s + " x" + (object)index2.Stack;
                    SpriteText.drawString(b, s, this.forSaleButtons[index1].bounds.X + 96 + 8, this.forSaleButtons[index1].bounds.Y + 28, 999999, -1, 999999, flag1 ? 0.5f : 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                    if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Price > 0)
                    {
                        SpriteText.drawString(b, this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Price.ToString() + " ", this.forSaleButtons[index1].bounds.Right - SpriteText.getWidthOfString(this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Price.ToString() + " ", 999999) - 60, this.forSaleButtons[index1].bounds.Y + 28, 999999, -1, 999999, ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) < this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Price || flag1 ? 0.5f : 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                        Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(this.forSaleButtons[index1].bounds.Right - 52), (float)(this.forSaleButtons[index1].bounds.Y + 40 - 4)), new Rectangle(193 + this.currency * 9, 373, 9, 10), Color.White * (!flag1 ? 1f : 0.25f), 0.0f, Vector2.Zero, 4f, false, 1f, -1, -1, !flag1 ? 0.35f : 0.0f);
                    }
                    //else if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Length > 2)
                    //{
                    //    int quantity = 5;
                    //    int num2 = this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]][2];
                    //    if (this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]].Length > 3)
                    //        quantity = this.itemPriceAndStock[this.forSale[this.currentItemIndex + index1]][3];
                    //    bool flag2 = Game1.player.hasItemInInventory(num2, quantity, 0);
                    //    if (this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + index1))
                    //        flag2 = false;
                    //    float widthOfString = (float)SpriteText.getWidthOfString("x" + (object)quantity, 999999);
                    //    Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(this.forSaleButtons[index1].bounds.Right - 88) - widthOfString, (float)(this.forSaleButtons[index1].bounds.Y + 28 - 4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, num2, 16, 16), Color.White * (flag2 ? 1f : 0.25f), 0.0f, Vector2.Zero, -1f, false, -1f, -1, -1, flag2 ? 0.35f : 0.0f);
                    //    SpriteText.drawString(b, "x" + (object)quantity, this.forSaleButtons[index1].bounds.Right - (int)widthOfString - 16, this.forSaleButtons[index1].bounds.Y + 44, 999999, -1, 999999, flag2 ? 1f : 0.5f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                    //}
                }
            }
            if (this.forSale.Count == 0 && !this._isStorageShop)
                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), this.xPositionOnScreen + this.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), 999999) / 2, this.yPositionOnScreen + this.height / 2 - 128, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
            this.inventory.draw(b);
            for (int index = this.animations.Count - 1; index >= 0; --index)
            {
                if (this.animations[index].update(Game1.currentGameTime))
                    this.animations.RemoveAt(index);
                else
                    this.animations[index].draw(b, true, 0, 0, 1f);
            }
            if (this.poof != null)
                this.poof.draw(b, false, 0, 0, 1f);
            this.upArrow.draw(b);
            this.downArrow.draw(b);
            for (int index = 0; index < this.tabButtons.Count; ++index)
                this.tabButtons[index].draw(b);
            if (this.forSale.Count > 4)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true);
                this.scrollBar.draw(b);
            }
            if (!this.hoverText.Equals(""))
                IClickableMenu.drawToolTip(b, this.hoverText, this.boldTitleText, this.hoveredItem as Item, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), (CraftingRecipe)null, this.hoverPrice > 0 ? this.hoverPrice : -1);
            if (this.heldItem != null)
                this.heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
            base.draw(b);
            int num = this.xPositionOnScreen - 320;
            if (num > 0 && Game1.options.showMerchantPortraits)
            {
                if (this.portraitPerson != null)
                {
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)num, (float)this.yPositionOnScreen), new Rectangle(603, 414, 74, 74), Color.White, 0.0f, Vector2.Zero, 4f, false, 0.91f, -1, -1, 0.35f);
                    if (this.portraitPerson.Portrait != null)
                        b.Draw(this.portraitPerson.Portrait, new Vector2((float)(num + 20), (float)(this.yPositionOnScreen + 20)), new Rectangle?(new Rectangle(0, 0, 64, 64)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
                }
                if (this.potraitPersonDialogue != null)
                {
                    int overrideX = this.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(this.potraitPersonDialogue).X - 64;
                    if (overrideX > 0)
                        IClickableMenu.drawHoverText(b, this.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, overrideX, this.yPositionOnScreen + (this.portraitPerson != null ? 312 : 0), 1f, (CraftingRecipe)null, (IList<Item>)null);
                }
            }
            this.drawMouse(b);
        }

        public virtual string GetShopMenuContext()
        {
            return "Dresser";
        }
    }
}