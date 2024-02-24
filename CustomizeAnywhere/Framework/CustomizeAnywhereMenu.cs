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

namespace CustomizeAnywhere.Framework
{
    public class CustomizeAnywhereMenu : IClickableMenu
    {
        public List<ClickableComponent> labels = new List<ClickableComponent>();
        public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();
        public List<ClickableComponent> genderButtons = new List<ClickableComponent>();
        public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

        private int colorPickerTimer;
        public ColorPicker pantsColorPicker;
        public ColorPicker hairColorPicker;
        public ColorPicker eyeColorPicker;
        public ClickableTextureComponent okButton;

        public ClickableTextureComponent randomButton;

        private string hoverText;
        private string hoverTitle;

        public ClickableComponent nameBoxCC;
        public ClickableComponent farmnameBoxCC;
        public ClickableComponent favThingBoxCC;
        public ClickableComponent backButton;
        private ClickableComponent shirtLabel;
        private ClickableComponent skinLabel;
        private ClickableComponent hairLabel;
        private ClickableComponent accLabel;
        private ClickableComponent pantsStyleLabel;
        private ColorPicker _sliderOpTarget;
        private Action _sliderAction;
        private readonly Action _recolorEyesAction;
        private readonly Action _recolorPantsAction;
        private readonly Action _recolorHairAction;
        protected Farmer _displayFarmer;
        public Rectangle portraitBox;
        private ColorPicker lastHeldColorPicker;
        private int timesRandom;

        public CustomizeAnywhereMenu()
            : base(Game1.viewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64, false)
        {
            ModEntry.saveCurrentAppearance();
            setUpPositions();
            _recolorEyesAction = () => Game1.player.changeEyeColor(eyeColorPicker.getSelectedColor());
            _recolorPantsAction = () => Game1.player.changePants(pantsColorPicker.getSelectedColor());
            _recolorHairAction = () => Game1.player.changeHairColor(hairColorPicker.getSelectedColor());

            _displayFarmer = GetOrCreateDisplayFarmer();
        }


        public Farmer GetOrCreateDisplayFarmer()
        {
            if (_displayFarmer == null)
            {
                _displayFarmer = Game1.player;
                _displayFarmer.faceDirection(2);
                _displayFarmer.FarmerSprite.StopAnimation();
            }
            return _displayFarmer;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            this.xPositionOnScreen = Game1.viewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
            setUpPositions();
        }

        private void setUpPositions()
        {
            bool flag1 = true;
            bool flag2 = true;
            labels.Clear();
            genderButtons.Clear();
            leftSelectionButtons.Clear();
            rightSelectionButtons.Clear();
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
            assignComponentID(textureComponent1, 505);
            okButton = textureComponent1;

            int num1 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt ? -4 : 0;
            int num2 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? -16 : 0;
            int num3 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? 48 : 0;
            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f, false);
            assignComponentID(textureComponent2, 507);
            randomButton = textureComponent2;

            portraitBox = new Rectangle(this.xPositionOnScreen + 64 + 42 - 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
            portraitBox.X = this.xPositionOnScreen + (this.width - portraitBox.Width) / 2;
            randomButton.bounds.X = portraitBox.X - 56;

            int num4 = 128;
            List<ClickableComponent> selectionButtons1 = leftSelectionButtons;
            ClickableTextureComponent textureComponent3 = new ClickableTextureComponent("Direction", new Rectangle(portraitBox.X - 32, portraitBox.Y + 144, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent3, 520);
            selectionButtons1.Add((ClickableComponent)textureComponent3);
            List<ClickableComponent> selectionButtons2 = rightSelectionButtons;
            ClickableTextureComponent textureComponent4 = new ClickableTextureComponent("Direction", new Rectangle(portraitBox.Right - 32, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent4, 521);
            selectionButtons2.Add((ClickableComponent)textureComponent4);
            int num5 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt ? -20 : 0;

            List<ClickableComponent> genderButtons1 = genderButtons;
            ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Male", new Rectangle(portraitBox.X, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), (string)null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f, false);
            assignComponentID(textureComponent5, 508);
            genderButtons1.Add((ClickableComponent)textureComponent5);
            List<ClickableComponent> genderButtons2 = genderButtons;
            ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Female", new Rectangle(portraitBox.X + 64 + 24, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), (string)null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f, false);
            assignComponentID(textureComponent6, 509);
            genderButtons2.Add((ClickableComponent)textureComponent6);

            num4 = 256;

            num5 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr ? -20 : 0;
            List<ClickableComponent> selectionButtons3 = leftSelectionButtons;
            ClickableTextureComponent textureComponent7 = new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent7, 518);
            selectionButtons3.Add((ClickableComponent)textureComponent7);
            labels.Add(skinLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
            List<ClickableComponent> selectionButtons4 = rightSelectionButtons;
            ClickableTextureComponent textureComponent8 = new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent8, 519);
            selectionButtons4.Add((ClickableComponent)textureComponent8);

            Point point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
            int x1 = this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;

            labels.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
            eyeColorPicker = new ColorPicker("Eyes", point1.X, point1.Y);
            eyeColorPicker.setColor((Color)(NetFieldBase<Color, NetColor>)Game1.player.newEyeColor);
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
            {
                myID = 522,
                downNeighborID = -99998,
                upNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
            {
                myID = 523,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
            {
                myID = 524,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            num4 += 68;
            selectionButtons3 = leftSelectionButtons;
            textureComponent5 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent5, 514);
            selectionButtons3.Add((ClickableComponent)textureComponent5);
            labels.Add(hairLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
            selectionButtons4 = rightSelectionButtons;
            textureComponent6 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent6, 515);
            selectionButtons4.Add((ClickableComponent)textureComponent6);

            point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);

            labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
            hairColorPicker = new ColorPicker("Hair", point1.X, point1.Y);
            hairColorPicker.setColor((Color)(NetFieldBase<Color, NetColor>)Game1.player.hairstyleColor);
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
            {
                myID = 525,
                downNeighborID = -99998,
                upNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
            {
                myID = 526,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
            {
                myID = 527,
                upNeighborID = -99998,
                downNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborImmutable = true
            });



            num4 += 68;
            int num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 8 : 0;
            selectionButtons3 = leftSelectionButtons;
            textureComponent5 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent5, 512);
            selectionButtons3.Add((ClickableComponent)textureComponent5);
            labels.Add(shirtLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
            selectionButtons4 = rightSelectionButtons;
            textureComponent6 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent6, 513);
            selectionButtons4.Add((ClickableComponent)textureComponent6);
            int num7 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? -16 : 0;
            labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16 + num7, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
            point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
            pantsColorPicker = new ColorPicker("Pants", point1.X, point1.Y);
            pantsColorPicker.setColor((Color)(NetFieldBase<Color, NetColor>)Game1.player.pantsColor);
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
            {
                myID = 528,
                downNeighborID = -99998,
                upNeighborID = -99998,
                rightNeighborImmutable = true,
                leftNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
            {
                myID = 529,
                downNeighborID = -99998,
                upNeighborID = -99998,
                rightNeighborImmutable = true,
                leftNeighborImmutable = true
            });
            colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
            {
                myID = 530,
                downNeighborID = -99998,
                upNeighborID = -99998,
                rightNeighborImmutable = true,
                leftNeighborImmutable = true
            });

            num4 += 68;
            selectionButtons3 = leftSelectionButtons;
            textureComponent5 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent5, 629);
            selectionButtons3.Add((ClickableComponent)textureComponent5);
            labels.Add(pantsStyleLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
            selectionButtons4 = rightSelectionButtons;
            textureComponent6 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent6, 517);
            selectionButtons4.Add((ClickableComponent)textureComponent6);

            int num8 = num4 + 68;

            num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 32 : 0;
            selectionButtons3 = leftSelectionButtons;
            textureComponent5 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            assignComponentID(textureComponent5, 516);
            selectionButtons3.Add((ClickableComponent)textureComponent5);
            labels.Add(accLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
            selectionButtons4 = rightSelectionButtons;
            textureComponent6 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            assignComponentID(textureComponent6, 517);
            selectionButtons4.Add((ClickableComponent)textureComponent6);
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(521);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void gamePadButtonHeld(Buttons b)
        {
            base.gamePadButtonHeld(b);
            if (this.currentlySnappedComponent == null)
                return;
            switch (b)
            {
                case Buttons.DPadLeft:
                case Buttons.LeftThumbstickLeft:
                    switch (this.currentlySnappedComponent.myID)
                    {
                        case 522:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeHue(-1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 523:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeSaturation(-1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 524:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeValue(-1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 525:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeHue(-1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 526:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeSaturation(-1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 527:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeValue(-1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 528:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeHue(-1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        case 529:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeSaturation(-1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        case 530:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeValue(-1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        default:
                            return;
                    }
                case Buttons.DPadRight:
                case Buttons.LeftThumbstickRight:
                    switch (this.currentlySnappedComponent.myID)
                    {
                        case 522:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeHue(1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 523:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeSaturation(1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 524:
                            eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
                            eyeColorPicker.changeValue(1);
                            eyeColorPicker.Dirty = true;
                            _sliderOpTarget = eyeColorPicker;
                            _sliderAction = _recolorEyesAction;
                            return;
                        case 525:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeHue(1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 526:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeSaturation(1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 527:
                            hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
                            hairColorPicker.changeValue(1);
                            hairColorPicker.Dirty = true;
                            _sliderOpTarget = hairColorPicker;
                            _sliderAction = _recolorHairAction;
                            return;
                        case 528:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeHue(1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        case 529:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeSaturation(1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        case 530:
                            pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
                            pantsColorPicker.changeValue(1);
                            pantsColorPicker.Dirty = true;
                            _sliderOpTarget = pantsColorPicker;
                            _sliderAction = _recolorPantsAction;
                            return;
                        default:
                            return;
                    }
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (this.currentlySnappedComponent == null)
                return;
            switch (b)
            {
                case Buttons.RightTrigger:
                    if ((uint)(this.currentlySnappedComponent.myID - 512) > 9U)
                        break;
                    this.selectionClick(this.currentlySnappedComponent.name, 1);
                    break;
                case Buttons.LeftTrigger:
                    if ((uint)(this.currentlySnappedComponent.myID - 512) > 9U)
                        break;
                    this.selectionClick(this.currentlySnappedComponent.name, -1);
                    break;
            }
        }

        private void optionButtonClick(string name)
        {
            switch (name)
            {
                case "Female":
                    Game1.player.changeGender(false);
                    Game1.player.changeHairStyle(16);
                    break;

                case "Male":

                    Game1.player.changeGender(true);
                    Game1.player.changeHairStyle(0);
                    break;

                case "OK":
                    Game1.player.isCustomized.Value = true;
                    Game1.player.ConvertClothingOverrideToClothesItems();

                    Game1.exitActiveMenu();
                    break;
            }
            Game1.playSound("coin");
        }


        private void selectionClick(string name, int change)
        {
            switch (name)
            {
                case "Pants Style":
                    int whichPants = (int)(NetFieldBase<int, NetInt>)Game1.player.pants + change;

                    if (whichPants < -1)
                    {
                        whichPants = 14;
                    }
                    else if (whichPants >= 14)
                    {
                        whichPants = -1;
                    }

                    Game1.player.changePantStyle(whichPants, false);
                    Game1.playSound("coin");
                    break;
                case "Hair":
                    Game1.player.changeHairStyle((int)(NetFieldBase<int, NetInt>)Game1.player.hair + change);
                    Game1.playSound("grassyStep");
                    break;
                case "Direction":
                    _displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
                    _displayFarmer.FarmerSprite.StopAnimation();
                    _displayFarmer.completelyStopAnimatingOrDoingAction();
                    Game1.playSound("pickUpItem");
                    break;
                case "Acc":
                    Game1.player.changeAccessory((int)(NetFieldBase<int, NetInt>)Game1.player.accessory + change);
                    Game1.playSound("purchase");
                    break;
                case "Skin":
                    Game1.player.changeSkinColor((int)(NetFieldBase<int, NetInt>)Game1.player.skin + change, false);
                    Game1.playSound("skeletonStep");
                    break;
                case "Shirt":

                    int whichShirt = (int)(NetFieldBase<int, NetInt>)Game1.player.shirt + change;

                    if (whichShirt < -1)
                    {
                        whichShirt = 291;
                    }
                    else if (whichShirt >= 291)
                    {
                        whichShirt = -1;
                    }

                    Game1.player.changeShirt(whichShirt, false);
                    Game1.playSound("coin");
                    break;
            }
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {

            if (genderButtons.Count > 0)
            {
                foreach (ClickableComponent genderButton in genderButtons)
                {
                    if (genderButton.containsPoint(x, y))
                    {
                        this.optionButtonClick(genderButton.name);
                        genderButton.scale -= 0.5f;
                        genderButton.scale = Math.Max(3.5f, genderButton.scale);
                    }
                }
            }

            if (leftSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent leftSelectionButton in leftSelectionButtons)
                {
                    if (leftSelectionButton.containsPoint(x, y))
                    {
                        this.selectionClick(leftSelectionButton.name, -1);
                        if ((double)leftSelectionButton.scale != 0.0)
                        {
                            leftSelectionButton.scale -= 0.25f;
                            leftSelectionButton.scale = Math.Max(0.75f, leftSelectionButton.scale);
                        }
                    }
                }
            }
            if (rightSelectionButtons.Count > 0)
            {
                foreach (ClickableComponent rightSelectionButton in rightSelectionButtons)
                {
                    if (rightSelectionButton.containsPoint(x, y))
                    {
                        this.selectionClick(rightSelectionButton.name, 1);
                        if ((double)rightSelectionButton.scale != 0.0)
                        {
                            rightSelectionButton.scale -= 0.25f;
                            rightSelectionButton.scale = Math.Max(0.75f, rightSelectionButton.scale);
                        }
                    }
                }
            }
            if (okButton.containsPoint(x, y) && canLeaveMenu())
            {
                this.optionButtonClick(okButton.name);
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
            }
            if (hairColorPicker != null && hairColorPicker.containsPoint(x, y))
            {
                Color c = hairColorPicker.click(x, y);
                Game1.player.changeHairColor(c);
                lastHeldColorPicker = hairColorPicker;
            }
            else if (pantsColorPicker != null && pantsColorPicker.containsPoint(x, y))
            {
                Color color = pantsColorPicker.click(x, y);
                Game1.player.changePants(color);
                lastHeldColorPicker = pantsColorPicker;
            }
            else if (eyeColorPicker != null && eyeColorPicker.containsPoint(x, y))
            {
                Game1.player.changeEyeColor(eyeColorPicker.click(x, y));
                lastHeldColorPicker = eyeColorPicker;
            }

            if (!randomButton.containsPoint(x, y))
                return;
            string cueName = "drumkit6";
            if (timesRandom > 0)
            {
                switch (Game1.random.Next(15))
                {
                    case 0:
                        cueName = "drumkit1";
                        break;
                    case 1:
                        cueName = "dirtyHit";
                        break;
                    case 2:
                        cueName = "axchop";
                        break;
                    case 3:
                        cueName = "hoeHit";
                        break;
                    case 4:
                        cueName = "fishSlap";
                        break;
                    case 5:
                        cueName = "drumkit6";
                        break;
                    case 6:
                        cueName = "drumkit5";
                        break;
                    case 7:
                        cueName = "drumkit6";
                        break;
                    case 8:
                        cueName = "junimoMeep1";
                        break;
                    case 9:
                        cueName = "coin";
                        break;
                    case 10:
                        cueName = "axe";
                        break;
                    case 11:
                        cueName = "hammer";
                        break;
                    case 12:
                        cueName = "drumkit2";
                        break;
                    case 13:
                        cueName = "drumkit4";
                        break;
                    case 14:
                        cueName = "drumkit3";
                        break;
                }
            }
            Game1.playSound(cueName);
            ++timesRandom;
            if (accLabel != null && accLabel.visible)
            {
                if (Game1.random.NextDouble() < 0.33)
                {
                    if (Game1.player.IsMale)
                        Game1.player.changeAccessory(Game1.random.Next(19));
                    else
                        Game1.player.changeAccessory(Game1.random.Next(6, 19));
                }
                else
                    Game1.player.changeAccessory(-1);
            }
            if (hairLabel != null && hairLabel.visible)
            {
                if (Game1.player.IsMale)
                    Game1.player.changeHairStyle(Game1.random.Next(16));
                else
                    Game1.player.changeHairStyle(Game1.random.Next(16, 32));
                Color c = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
                if (Game1.random.NextDouble() < 0.5)
                {
                    c.R /= 2;
                    c.G /= 2;
                    c.B /= 2;
                }
                if (Game1.random.NextDouble() < 0.5)
                    c.R = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    c.G = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    c.B = (byte)Game1.random.Next(15, 50);
                Game1.player.changeHairColor(c);
            }
            if (shirtLabel != null && shirtLabel.visible)
                Game1.player.changeShirt(Game1.random.Next(291), false);
            if (skinLabel != null && skinLabel.visible)
            {
                Game1.player.changeSkinColor(Game1.random.Next(6), false);
                if (Game1.random.NextDouble() < 0.25)
                    Game1.player.changeSkinColor(Game1.random.Next(24), false);
            }
            if (pantsStyleLabel != null && pantsStyleLabel.visible)
            {
                Color color = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
                if (Game1.random.NextDouble() < 0.5)
                {
                    color.R /= 2;
                    color.G /= 2;
                    color.B /= 2;
                }
                if (Game1.random.NextDouble() < 0.5)
                    color.R = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    color.G = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    color.B = (byte)Game1.random.Next(15, 50);
                Game1.player.changePants(color);
                pantsColorPicker.setColor((Color)(NetFieldBase<Color, NetColor>)Game1.player.pantsColor);
            }
            if (eyeColorPicker != null)
            {
                Color c = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
                c.R /= 2;
                c.G /= 2;
                c.B /= 2;
                if (Game1.random.NextDouble() < 0.5)
                    c.R = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    c.G = (byte)Game1.random.Next(15, 50);
                if (Game1.random.NextDouble() < 0.5)
                    c.B = (byte)Game1.random.Next(15, 50);
                Game1.player.changeEyeColor(c);
                eyeColorPicker.setColor((Color)(NetFieldBase<Color, NetColor>)Game1.player.newEyeColor);
            }
            randomButton.scale = 3.5f;
        }

        public override void leftClickHeld(int x, int y)
        {
            colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            if (colorPickerTimer > 0)
                return;
            if (lastHeldColorPicker != null && !Game1.options.SnappyMenus)
            {
                if (lastHeldColorPicker.Equals((object)hairColorPicker))
                {
                    Color c = hairColorPicker.clickHeld(x, y);
                    Game1.player.changeHairColor(c);
                }
                if (lastHeldColorPicker.Equals((object)pantsColorPicker))
                {
                    Color color = pantsColorPicker.clickHeld(x, y);
                    Game1.player.changePants(color);
                }
                if (lastHeldColorPicker.Equals((object)eyeColorPicker))
                    Game1.player.changeEyeColor(eyeColorPicker.clickHeld(x, y));
            }
            colorPickerTimer = 100;
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (hairColorPicker != null)
                hairColorPicker.releaseClick();
            if (pantsColorPicker != null)
                pantsColorPicker.releaseClick();
            if (eyeColorPicker != null)
                eyeColorPicker.releaseClick();
            lastHeldColorPicker = (ColorPicker)null;
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";
            hoverTitle = "";
            foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
            {
                if (leftSelectionButton.containsPoint(x, y))
                    leftSelectionButton.scale = Math.Min(leftSelectionButton.scale + 0.02f, leftSelectionButton.baseScale + 0.1f);
                else
                    leftSelectionButton.scale = Math.Max(leftSelectionButton.scale - 0.02f, leftSelectionButton.baseScale);
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
            {
                if (rightSelectionButton.containsPoint(x, y))
                    rightSelectionButton.scale = Math.Min(rightSelectionButton.scale + 0.02f, rightSelectionButton.baseScale + 0.1f);
                else
                    rightSelectionButton.scale = Math.Max(rightSelectionButton.scale - 0.02f, rightSelectionButton.baseScale);
            }

            foreach (ClickableTextureComponent genderButton in genderButtons)
            {
                if (genderButton.containsPoint(x, y))
                    genderButton.scale = Math.Min(genderButton.scale + 0.05f, genderButton.baseScale + 0.5f);
                else
                    genderButton.scale = Math.Max(genderButton.scale - 0.05f, genderButton.baseScale);
            }

            if (okButton.containsPoint(x, y) && canLeaveMenu())
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
            else
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);


            randomButton.tryHover(x, y, 0.25f);
            randomButton.tryHover(x, y, 0.25f);
            if (hairColorPicker != null && hairColorPicker.containsPoint(x, y) || pantsColorPicker != null && pantsColorPicker.containsPoint(x, y) || eyeColorPicker != null && eyeColorPicker.containsPoint(x, y))
                Game1.SetFreeCursorDrag();
        }

        public bool canLeaveMenu()
        {
            return true;
        }

        public override void draw(SpriteBatch b)
        {
            bool ignoreTitleSafe = true;

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false, ignoreTitleSafe, -1, -1, -1);

            b.Draw(Game1.daybg, new Vector2(portraitBox.X, portraitBox.Y), Color.White);
            foreach (ClickableTextureComponent genderButton in genderButtons)
            {
                if (genderButton.visible)
                {
                    genderButton.draw(b);
                    if (genderButton.name.Equals("Male") && Game1.player.IsMale || genderButton.name.Equals("Female") && !Game1.player.IsMale)
                        b.Draw(Game1.mouseCursors, genderButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
                }
            }

            foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
                leftSelectionButton.draw(b);
            foreach (ClickableComponent label in labels)
            {
                if (label.visible)
                {
                    string text = "";
                    float num1 = 0.0f;
                    float num2 = 0.0f;
                    Color color = Game1.textColor;
                    if (label == shirtLabel)
                    {
                        num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                        text = string.Concat((object)((int)(NetFieldBase<int, NetInt>)Game1.player.shirt + 1));
                    }
                    else if (label == skinLabel)
                    {
                        num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                        text = string.Concat((object)((int)(NetFieldBase<int, NetInt>)Game1.player.skin + 1));
                    }
                    else if (label == hairLabel)
                    {
                        num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                        if (!label.name.Contains("Color"))
                            text = string.Concat((object)((int)(NetFieldBase<int, NetInt>)Game1.player.hair + 1));
                    }
                    else if (label == accLabel)
                    {
                        num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                        text = string.Concat((object)((int)(NetFieldBase<int, NetInt>)Game1.player.accessory + 2));
                    }
                    else if (label == pantsStyleLabel)
                    {
                        num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                        text = string.Concat((object)((int)(NetFieldBase<int, NetInt>)Game1.player.pants + 1));
                    }
                    else
                        color = Game1.textColor;
                    Utility.drawTextWithShadow(b, label.name, Game1.smallFont, new Vector2((float)label.bounds.X + num1, (float)label.bounds.Y), color, 1f, -1f, -1, -1, 1f, 3);
                    if (text.Length > 0)
                        Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2((float)(label.bounds.X + 21) - Game1.smallFont.MeasureString(text).X / 2f, (float)(label.bounds.Y + 32) + num2), color, 1f, -1f, -1, -1, 1f, 3);
                }
            }
            foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
                rightSelectionButton.draw(b);

            okButton.draw(b, Color.White, 0.75f, 0);


            if (hairColorPicker != null)
                hairColorPicker.draw(b);
            if (pantsColorPicker != null)
                pantsColorPicker.draw(b);
            if (eyeColorPicker != null)
                eyeColorPicker.draw(b);

            randomButton.draw(b);
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            _displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(portraitBox.Center.X - 32, portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0.0f, 1f, _displayFarmer);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (hoverText != null && hoverTitle != null && hoverText.Count() > 0)
                IClickableMenu.drawHoverText(b, Game1.parseText(hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, hoverTitle, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null, (IList<Item>)null);
            this.drawMouse(b);
        }

        private void assignComponentID(ClickableComponent component, int id)
        {
            component.myID = id;
            component.upNeighborID = -99998;
            component.leftNeighborID = -99998;
            component.rightNeighborID = -99998;
            component.downNeighborID = -99998;
        }

    }
}