using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using Object = StardewValley.Object;

using SObject = StardewValley.Object;

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
            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + this.height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
            {
                myID = 706,
                rightNeighborID = -7777
            };

            this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 32 - 60, this.yPositionOnScreen + this.height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
            {
                myID = 707,
                leftNeighborID = -7777
            };

            int top_left_x = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder;
            int top_left_y = this.yPositionOnScreen + borderWidth + spaceToClearTopBorder - 16;
            const int square_size = 10;

            // apply sort earlier.
            IOrderedEnumerable<KeyValuePair<string, ObjectData>> fishes = Game1.objectData.Where(kvp => kvp.Value?.Category == SObject.FishCategory).OrderBy(kvp => kvp.Key);

            this.collections.Add([]);
            List<ClickableTextureComponent> textureComponentList = this.collections.Last();
            int index = 0;
            foreach (var data in ItemRegistry.GetObjectTypeDefinition().GetAllData().Where(static data => data.Category == SObject.FishCategory).OrderBy(static data => (data.TextureName, data.ItemId)))
            {
                bool drawColor = false;
                bool drawColorFaded = false;

                if (!Utils.IsUnDonatedFish(data.InternalName))
                {
                    drawColor = true;
                    drawColorFaded = false;
                }
                else if (Game1.player.fishCaught.ContainsKey(data.QualifiedItemId))
                {
                    drawColorFaded = true;
                }

                int x1 = top_left_x + index % square_size * 68;
                int y1 = top_left_y + index / square_size * 68;
                if (y1 > this.yPositionOnScreen + this.height - 128)
                {
                    this.collections.Add([]);
                    index = 0;
                    x1 = top_left_x;
                    y1 = top_left_y;
                    textureComponentList = this.collections.Last();
                }
                Texture2D texture = data.GetTexture();
                ClickableTextureComponent textureComponent8 = new($"{data.ItemId} {drawColor} {drawColorFaded}", new Rectangle(x1, y1, 64, 64), null, "", texture, Game1.getSourceRectForStandardTileSheet(texture, data.SpriteIndex, 16, 16), 4f, drawColor)
                {
                    myID = this.collections.Last().Count,
                    rightNeighborID = (this.collections.Last().Count + 1) % square_size == 0 ? -1 : this.collections.Last().Count + 1,
                    leftNeighborID = this.collections.Last().Count % square_size == 0 ? 7001 : this.collections.Last().Count - 1,
                    downNeighborID = y1 + 68 > this.yPositionOnScreen + this.height - 128 ? -7777 : this.collections.Last().Count + square_size,
                    upNeighborID = this.collections.Last().Count < square_size ? 12345 : this.collections.Last().Count - square_size,
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
            return CreateDescription(ItemRegistry.GetDataOrErrorItem(key));
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
                bool drawColor = Convert.ToBoolean(textureComponent.name.Split(' ')[1]);
                bool drawColorFaded = Convert.ToBoolean(textureComponent.name.Split(' ')[2]);
                textureComponent.draw(b, drawColorFaded ? Color.DimGray * 0.4f : drawColor ? Color.White : Color.Black * 0.2f, drawColorFaded ? 0 : 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (!string.IsNullOrEmpty(this.hoverText))
            {
                drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0);
            }

            this.drawMouse(b);
        }

        internal static string CreateDescription(ParsedItemData data)
        {
            string name = data.InternalName;
            if (Utils.IsUnDonatedFish(Utils.InternalNameToDonationName[name]) && !Game1.player.fishCaught.ContainsKey(data.QualifiedItemId))
                return "???";

            string returnStr = data.DisplayName + Environment.NewLine + Environment.NewLine + Game1.parseText(data.Description, Game1.smallFont, 256) + Environment.NewLine;
            return returnStr;
        }
    }
}
