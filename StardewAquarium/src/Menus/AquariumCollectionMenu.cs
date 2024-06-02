using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewAquarium.Menus
{
    class AquariumCollectionMenu : IClickableMenu
    {
        private string hoverText = "";
        public List<List<ClickableTextureComponent>> collections = new List<List<ClickableTextureComponent>>();
        public ClickableTextureComponent backButton;
        public ClickableTextureComponent forwardButton;
        private int currentPage;
        private string _title;
        private readonly bool IsAndroid = Constants.TargetPlatform == GamePlatform.Android;

        public AquariumCollectionMenu(string title)
        {
            this._title = title;
            this.width = 700 + borderWidth * 2;
            this.height = (this.IsAndroid ? 550 : 600) + borderWidth * 2;

            if (this.IsAndroid)
            {
                this.initializeUpperRightCloseButton();
            }

            this.xPositionOnScreen = Game1.viewport.Width / 2 - (800 + borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (600 + borderWidth * 2) / 2;

            CollectionsPage.widthToMoveActiveTab = 8;
            ClickableTextureComponent textureComponent9 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + this.height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false);
            textureComponent9.myID = 706;
            textureComponent9.rightNeighborID = -7777;
            this.backButton = textureComponent9;
            ClickableTextureComponent textureComponent10 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 60, this.yPositionOnScreen + this.height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false);
            textureComponent10.myID = 707;
            textureComponent10.leftNeighborID = -7777;
            this.forwardButton = textureComponent10;
            int[] numArray = new int[8];
            int num2 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder;
            int num3 = this.yPositionOnScreen + borderWidth + spaceToClearTopBorder - 16;
            int num4 = 10;
            List<KeyValuePair<string, ObjectData>> keyValuePairList = new List<KeyValuePair<string, ObjectData>>(Game1.objectData);
            keyValuePairList.Sort((a, b) => a.Key.CompareTo(b.Key));
            int index = 0;
            foreach (KeyValuePair<string, ObjectData> keyValuePair in keyValuePairList)
            {
                int category = keyValuePair.Value.Category;
                bool drawColour = false;
                bool drawColorFaded = false;

                if (category == -4)
                {
                    string name = keyValuePair.Value.Name;
                    if (Game1.player.fishCaught.ContainsKey(keyValuePair.Key))
                    {
                        drawColorFaded = true;
                    }

                    if (!Utils.IsUnDonatedFish(Utils.InternalNameToDonationName[name]))
                    {
                        drawColour = true;
                        drawColorFaded = false;
                    }
                }
                else
                    continue;

                int x1 = num2 + index % num4 * 68;
                int y1 = num3 + index / num4 * 68;
                if (y1 > this.yPositionOnScreen + this.height - 128)
                {
                    this.collections.Add(new List<ClickableTextureComponent>());
                    index = 0;
                    x1 = num2;
                    y1 = num3;
                }
                if (this.collections.Count == 0)
                    this.collections.Add(new List<ClickableTextureComponent>());
                List<ClickableTextureComponent> textureComponentList = this.collections.Last();
                var texture = Game1.content.Load<Texture2D>(keyValuePair.Value.Texture ?? Game1.objectSpriteSheetName);
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent(keyValuePair.Key + " " + drawColour + " " + drawColorFaded, new Rectangle(x1, y1, 64, 64), null, "", texture, Game1.getSourceRectForStandardTileSheet(texture, keyValuePair.Value.SpriteIndex, 16, 16), 4f, drawColour)
                {
                    myID = this.collections.Last().Count,
                    rightNeighborID = (this.collections.Last().Count + 1) % num4 == 0 ? -1 : this.collections.Last().Count + 1,
                    leftNeighborID = this.collections.Last().Count % num4 == 0 ? 7001 : this.collections.Last().Count - 1,
                    downNeighborID = y1 + 68 > this.yPositionOnScreen + this.height - 128 ? -7777 : this.collections.Last().Count + num4,
                    upNeighborID = this.collections.Last().Count < num4 ? 12345 : this.collections.Last().Count - num4,
                    fullyImmutable = true
                };
                textureComponentList.Add(textureComponent8);
                index++;
            }

            this.initializeUpperRightCloseButton();
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
            switch (direction)
            {
                case 1:
                    if (oldID != 706 || this.collections.Count <= this.currentPage + 1)
                        break;
                    this.currentlySnappedComponent = this.getComponentWithID(707);
                    break;
                case 2:
                    if (this.currentPage > 0)
                        this.currentlySnappedComponent = this.getComponentWithID(706);
                    else if (this.currentPage == 0 && this.collections.Count > 1)
                        this.currentlySnappedComponent = this.getComponentWithID(707);
                    this.backButton.upNeighborID = oldID;
                    this.forwardButton.upNeighborID = oldID;
                    break;
                case 3:
                    if (oldID != 707 || this.currentPage <= 0)
                        break;
                    this.currentlySnappedComponent = this.getComponentWithID(706);
                    break;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.snapToDefaultClickableComponent();
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);

            if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
            {
                --this.currentPage;
                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == 0)
                {
                    this.currentlySnappedComponent = this.forwardButton;
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                }
            }

            if (this.currentPage < this.collections.Count - 1 && this.forwardButton.containsPoint(x, y))
            {
                ++this.currentPage;
                Game1.playSound("shwip");
                this.forwardButton.scale = this.forwardButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == this.collections.Count - 1)
                {
                    this.currentlySnappedComponent = this.backButton;
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";

            foreach (ClickableTextureComponent textureComponent in this.collections[this.currentPage])
            {
                if (textureComponent.containsPoint(x, y))
                {
                    textureComponent.scale =
                        Math.Min(textureComponent.scale + 0.02f, textureComponent.baseScale + 0.1f);
                    this.hoverText = this.createDescription(textureComponent.name.Split(' ')[0]);
                }
                else
                    textureComponent.scale = Math.Max(textureComponent.scale - 0.02f, textureComponent.baseScale);
            }

            this.forwardButton.tryHover(x, y, 0.5f);
            this.backButton.tryHover(x, y, 0.5f);
        }

        public string createDescription(string key)
        {
            ParsedItemData data = ItemRegistry.GetDataOrErrorItem(key);

            string name = data.InternalName;
            if (Utils.IsUnDonatedFish(Utils.InternalNameToDonationName[name]) && !Game1.player.fishCaught.ContainsKey(key))
                return "???";

            string returnStr = data.DisplayName + Environment.NewLine + Environment.NewLine + Game1.parseText(data.Description, Game1.smallFont, 256) + Environment.NewLine;
            return returnStr;
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                base.drawBackground(b);

            base.draw(b);

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

            if (this.currentPage > 0)
                this.backButton.draw(b);
            if (this.currentPage < this.collections.Count - 1)
                this.forwardButton.draw(b);

            SpriteText.drawStringWithScrollCenteredAt(b, this._title, Game1.viewport.Width / 2 - 50, Game1.viewport.Height / 2 - 310, SpriteText.getWidthOfString(this._title) + 16, 1f, null, 0, 0.88f, false);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            foreach (ClickableTextureComponent textureComponent in this.collections[this.currentPage])
            {
                /*
                 *bool drawColor = Convert.ToBoolean(c.name.Split(' ')[1]);
                bool drawColorFaded = this.currentTab == 4 && Convert.ToBoolean(c.name.Split(' ')[2]);
                c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f);
                 */
                bool drawColor = Convert.ToBoolean(textureComponent.name.Split(' ')[1]);
                bool drawColorFaded = Convert.ToBoolean(textureComponent.name.Split(' ')[2]);
                textureComponent.draw(b, drawColorFaded ? Color.DimGray * 0.4f : drawColor ? Color.White : Color.Black * 0.2f, drawColorFaded ? 0 : 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (!this.hoverText.Equals(""))
            {
                drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0);
            }

            this.drawMouse(b);
        }
    }
}
