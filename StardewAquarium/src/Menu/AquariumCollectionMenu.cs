using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace StardewAquarium.Menu
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
            _title = title;
            this.width = 700 + IClickableMenu.borderWidth * 2;
            this.height = (IsAndroid? 550 : 600) + IClickableMenu.borderWidth * 2;

            if (this.IsAndroid)
                this.initializeUpperRightCloseButton();

            this.xPositionOnScreen = Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;

            CollectionsPage.widthToMoveActiveTab = 8;
            ClickableTextureComponent textureComponent9 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false);
            textureComponent9.myID = 706;
            textureComponent9.rightNeighborID = -7777;
            backButton = textureComponent9;
            ClickableTextureComponent textureComponent10 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width - 32 - 60, this.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false);
            textureComponent10.myID = 707;
            textureComponent10.leftNeighborID = -7777;
            forwardButton = textureComponent10;
            int[] numArray = new int[8];
            int num2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            int num3 = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
            int num4 = 10;
            List<KeyValuePair<int, string>> keyValuePairList = new List<KeyValuePair<int, string>>((IEnumerable<KeyValuePair<int, string>>)Game1.objectInformation);
            keyValuePairList.Sort((Comparison<KeyValuePair<int, string>>)((a, b) => a.Key.CompareTo(b.Key)));
            int index = 0;
            foreach (KeyValuePair<int, string> keyValuePair in keyValuePairList)
            {
                string str = keyValuePair.Value.Split('/')[3];
                bool drawShadow = false;

                if (str.Contains("Fish") && !str.Contains("-20"))
                {
                    string name = keyValuePair.Value.Split('/')[0].Replace(" ", String.Empty);
                    if (!Utils.IsUnDonatedFish(name))
                    {
                        drawShadow = true;
                    }
                }
                else
                    continue;

                int x1 = num2 + index % num4 * 68;
                int y1 = num3 + index / num4 * 68;
                if (y1 > this.yPositionOnScreen + height - 128)
                {
                    this.collections.Add(new List<ClickableTextureComponent>());
                    index = 0;
                    x1 = num2;
                    y1 = num3;
                }
                if (this.collections.Count == 0)
                    this.collections.Add(new List<ClickableTextureComponent>());
                List<ClickableTextureComponent> textureComponentList = this.collections.Last<List<ClickableTextureComponent>>();
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent(keyValuePair.Key.ToString() + " " + drawShadow.ToString(), new Rectangle(x1, y1, 64, 64), (string)null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, keyValuePair.Key, 16, 16), 4f, drawShadow)
                {
                    myID = this.collections.Last<List<ClickableTextureComponent>>().Count,
                    rightNeighborID = (this.collections.Last<List<ClickableTextureComponent>>().Count + 1) % num4 == 0 ? -1 : this.collections.Last<List<ClickableTextureComponent>>().Count + 1,
                    leftNeighborID = this.collections.Last<List<ClickableTextureComponent>>().Count % num4 == 0 ? 7001 : this.collections.Last<List<ClickableTextureComponent>>().Count - 1,
                    downNeighborID = y1 + 68 > this.yPositionOnScreen + height - 128 ? -7777 : this.collections.Last<List<ClickableTextureComponent>>().Count + num4,
                    upNeighborID = this.collections.Last<List<ClickableTextureComponent>>().Count < num4 ? 12345 : this.collections.Last<List<ClickableTextureComponent>>().Count - num4,
                    fullyImmutable = true
                };
                textureComponentList.Add(textureComponent8);
                index++;
            }
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
            if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
            {
                --this.currentPage;
                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == 0)
                {
                    this.currentlySnappedComponent = (ClickableComponent) this.forwardButton;
                    Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                }
            }

            if (this.currentPage < this.collections.Count - 1 && this.forwardButton.containsPoint(x, y))
            {
                ++this.currentPage;
                Game1.playSound("shwip");
                this.forwardButton.scale = this.forwardButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls &&
                    this.currentPage == this.collections.Count - 1)
                {
                    this.currentlySnappedComponent = (ClickableComponent) this.backButton;
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
                    this.hoverText = this.createDescription(Convert.ToInt32(textureComponent.name.Split(' ')[0]));
                }
                else
                    textureComponent.scale = Math.Max(textureComponent.scale - 0.02f, textureComponent.baseScale);
            }

            this.forwardButton.tryHover(x, y, 0.5f);
            this.backButton.tryHover(x, y, 0.5f);
        }

        public string createDescription(int index)
        {
            string[] strArray = Game1.objectInformation[index].Split('/');

            string name = strArray[0];
            if (Utils.IsUnDonatedFish(name.Replace(" ", String.Empty)))
                return "???";

            string returnStr = strArray[4] + Environment.NewLine + Environment.NewLine + Game1.parseText(strArray[5], Game1.smallFont, 256) + Environment.NewLine;
            return returnStr;
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                base.drawBackground(b);

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

            if (this.currentPage > 0)
                this.backButton.draw(b);
            if (this.currentPage < this.collections.Count - 1)
                this.forwardButton.draw(b);

            SpriteText.drawStringWithScrollCenteredAt(b, _title, Game1.viewport.Width / 2 - 50, Game1.viewport.Height / 2 - 310, _title, 1f, -1, 0, 0.88f, false);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            foreach (ClickableTextureComponent textureComponent in this.collections[this.currentPage])
            {
                bool boolean = Convert.ToBoolean(textureComponent.name.Split(' ')[1]);
                textureComponent.draw(b, boolean ? Color.White : Color.Black * 0.2f, 0.86f, 0);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (!this.hoverText.Equals(""))
            {
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0);
            }

            this.drawMouse(b);
        }
    }
}
