using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
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
            _title = title;
            width = 700 + borderWidth * 2;
            height = (IsAndroid ? 550 : 600) + borderWidth * 2;

            if (IsAndroid)
            {
                initializeUpperRightCloseButton();
            }
                

            xPositionOnScreen = Game1.viewport.Width / 2 - (800 + borderWidth * 2) / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - (600 + borderWidth * 2) / 2;

            CollectionsPage.widthToMoveActiveTab = 8;
            ClickableTextureComponent textureComponent9 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false);
            textureComponent9.myID = 706;
            textureComponent9.rightNeighborID = -7777;
            backButton = textureComponent9;
            ClickableTextureComponent textureComponent10 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 60, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false);
            textureComponent10.myID = 707;
            textureComponent10.leftNeighborID = -7777;
            forwardButton = textureComponent10;
            int[] numArray = new int[8];
            int num2 = xPositionOnScreen + borderWidth + spaceToClearSideBorder;
            int num3 = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 16;
            int num4 = 10;
            List<KeyValuePair<int, string>> keyValuePairList = new List<KeyValuePair<int, string>>(Game1.objectInformation);
            keyValuePairList.Sort((a, b) => a.Key.CompareTo(b.Key));
            int index = 0;
            foreach (KeyValuePair<int, string> keyValuePair in keyValuePairList)
            {
                string str = keyValuePair.Value.Split('/')[3];
                bool drawColour = false;
                bool drawColorFaded = false;

                if (str.Contains("-4"))
                {
                    string name = keyValuePair.Value.Split('/')[0];
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
                if (y1 > yPositionOnScreen + height - 128)
                {
                    collections.Add(new List<ClickableTextureComponent>());
                    index = 0;
                    x1 = num2;
                    y1 = num3;
                }
                if (collections.Count == 0)
                    collections.Add(new List<ClickableTextureComponent>());
                List<ClickableTextureComponent> textureComponentList = collections.Last();
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent(keyValuePair.Key + " " + drawColour + " " + drawColorFaded, new Rectangle(x1, y1, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, keyValuePair.Key, 16, 16), 4f, drawColour)
                {
                    myID = collections.Last().Count,
                    rightNeighborID = (collections.Last().Count + 1) % num4 == 0 ? -1 : collections.Last().Count + 1,
                    leftNeighborID = collections.Last().Count % num4 == 0 ? 7001 : collections.Last().Count - 1,
                    downNeighborID = y1 + 68 > yPositionOnScreen + height - 128 ? -7777 : collections.Last().Count + num4,
                    upNeighborID = collections.Last().Count < num4 ? 12345 : collections.Last().Count - num4,
                    fullyImmutable = true
                };
                textureComponentList.Add(textureComponent8);
                index++;
            }

            initializeUpperRightCloseButton();
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
            switch (direction)
            {
                case 1:
                    if (oldID != 706 || collections.Count <= currentPage + 1)
                        break;
                    currentlySnappedComponent = getComponentWithID(707);
                    break;
                case 2:
                    if (currentPage > 0)
                        currentlySnappedComponent = getComponentWithID(706);
                    else if (currentPage == 0 && collections.Count > 1)
                        currentlySnappedComponent = getComponentWithID(707);
                    backButton.upNeighborID = oldID;
                    forwardButton.upNeighborID = oldID;
                    break;
                case 3:
                    if (oldID != 707 || currentPage <= 0)
                        break;
                    currentlySnappedComponent = getComponentWithID(706);
                    break;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.snapToDefaultClickableComponent();
            currentlySnappedComponent = getComponentWithID(0);
            snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);

            if (currentPage > 0 && backButton.containsPoint(x, y))
            {
                --currentPage;
                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == 0)
                {
                    currentlySnappedComponent = forwardButton;
                    Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
                }
            }

            if (currentPage < collections.Count - 1 && forwardButton.containsPoint(x, y))
            {
                ++currentPage;
                Game1.playSound("shwip");
                forwardButton.scale = forwardButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls &&
                    currentPage == collections.Count - 1)
                {
                    currentlySnappedComponent = backButton;
                    Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            foreach (ClickableTextureComponent textureComponent in collections[currentPage])
            {
                if (textureComponent.containsPoint(x, y))
                {
                    textureComponent.scale =
                        Math.Min(textureComponent.scale + 0.02f, textureComponent.baseScale + 0.1f);
                    hoverText = createDescription(Convert.ToInt32(textureComponent.name.Split(' ')[0]));
                }
                else
                    textureComponent.scale = Math.Max(textureComponent.scale - 0.02f, textureComponent.baseScale);
            }

            forwardButton.tryHover(x, y, 0.5f);
            backButton.tryHover(x, y, 0.5f);
        }

        public string createDescription(int index)
        {
            string[] strArray = Game1.objectInformation[index].Split('/');

            string name = strArray[0];
            if (Utils.IsUnDonatedFish(Utils.InternalNameToDonationName[name]) && !Game1.player.fishCaught.ContainsKey(index))
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

            base.draw(b);

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            if (currentPage > 0)
                backButton.draw(b);
            if (currentPage < collections.Count - 1)
                forwardButton.draw(b);

            SpriteText.drawStringWithScrollCenteredAt(b, _title, Game1.viewport.Width / 2 - 50, Game1.viewport.Height / 2 - 310, _title, 1f, -1, 0, 0.88f, false);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            foreach (ClickableTextureComponent textureComponent in collections[currentPage])
            {
                /*
                 *bool drawColor = Convert.ToBoolean(c.name.Split(' ')[1]);
                bool drawColorFaded = this.currentTab == 4 && Convert.ToBoolean(c.name.Split(' ')[2]);
                c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f);
                 */
                bool drawColor = Convert.ToBoolean(textureComponent.name.Split(' ')[1]);
                bool drawColorFaded = Convert.ToBoolean(textureComponent.name.Split(' ')[2]);
                textureComponent.draw(b, drawColorFaded ? Color.DimGray * 0.4f : drawColor ? Color.White : Color.Black * 0.2f, drawColorFaded? 0 : 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (!hoverText.Equals(""))
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0);
            }

            drawMouse(b);
        }
    }
}
