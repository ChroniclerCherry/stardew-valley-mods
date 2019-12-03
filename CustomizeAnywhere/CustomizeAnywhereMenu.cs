using StardewValley;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CustomizeAnywhere
{
    class CustomizeAnywhereMenu : CharacterCustomization
    {
        private ClickableComponent shirtLabel;
        private ClickableComponent skinLabel;
        private ClickableComponent hairLabel;
        private ClickableComponent accLabel;
        private ClickableComponent pantsStyleLabel;
        private string hoverText;
        private string hoverTitle;


        public CustomizeAnywhereMenu() : base(Source.Wizard)
        {
            this.height = 648 + IClickableMenu.borderWidth * 2 + 64;
            this.yPositionOnScreen = (Game1.viewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64);
            this.setUpPositions();
        }

        public override void draw(SpriteBatch b)
        {
            bool ignoreTitleSafe = true;
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false, ignoreTitleSafe, -1, -1, -1);

                b.Draw(Game1.daybg, new Vector2((float)this.portraitBox.X, (float)this.portraitBox.Y), Color.White);
                foreach (ClickableTextureComponent genderButton in this.genderButtons)
                {
                    if (genderButton.visible)
                    {
                        genderButton.draw(b);
                        if (genderButton.name.Equals("Male") && Game1.player.IsMale || genderButton.name.Equals("Female") && !Game1.player.IsMale)
                            b.Draw(Game1.mouseCursors, genderButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
                    }
                }

            foreach (ClickableTextureComponent leftSelectionButton in this.leftSelectionButtons)
                if (!leftSelectionButton.name.Contains("Pet"))
                        leftSelectionButton.draw(b);
            foreach (ClickableComponent label in this.labels)
                {
                    if (label.visible && !label.name.Contains("Name") && !label.name.Contains("Farm") && !label.name.Contains("Fav") && !label.name.Contains("Animal"))
                    {
                        string text = "";
                        float num1 = 0.0f;
                        float num2 = 0.0f;
                        Color color = Game1.textColor;
                        if (label == this.shirtLabel)
                        {
                            num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                            text = string.Concat((object)((int)((NetFieldBase<int, NetInt>)Game1.player.shirt) + 1));
                        }
                        else if (label == this.skinLabel)
                        {
                            num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                            text = string.Concat((object)((int)((NetFieldBase<int, NetInt>)Game1.player.skin) + 1));
                        }
                        else if (label == this.hairLabel)
                        {
                            num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                            if (!label.name.Contains("Color"))
                                text = string.Concat((object)((int)((NetFieldBase<int, NetInt>)Game1.player.hair) + 1));
                        }
                        else if (label == this.accLabel)
                        {
                            num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                            text = string.Concat((object)((int)((NetFieldBase<int, NetInt>)Game1.player.accessory) + 2));
                        }
                        else if (label == this.pantsStyleLabel)
                        {
                            num1 = (float)(21.0 - (double)Game1.smallFont.MeasureString(label.name).X / 2.0);
                            text = string.Concat((object)((int)((NetFieldBase<int, NetInt>)Game1.player.pants) + 1));
                        }
                        else
                            color = Game1.textColor;
                        Utility.drawTextWithShadow(b, label.name, Game1.smallFont, new Vector2((float)label.bounds.X + num1, (float)label.bounds.Y), color, 1f, -1f, -1, -1, 1f, 3);
                        if (text.Length > 0)
                            Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2((float)(label.bounds.X + 21) - Game1.smallFont.MeasureString(text).X / 2f, (float)(label.bounds.Y + 32) + num2), color, 1f, -1f, -1, -1, 1f, 3);
                    }
                }
                foreach (ClickableTextureComponent rightSelectionButton in this.rightSelectionButtons)
                    if (!rightSelectionButton.name.Contains("Pet"))
                        rightSelectionButton.draw(b);
                if (this.hairColorPicker != null)
                    this.hairColorPicker.draw(b);
                if (this.pantsColorPicker != null)
                    this.pantsColorPicker.draw(b);
                if (this.eyeColorPicker != null)
                    this.eyeColorPicker.draw(b);

                this.okButton.draw(b, Color.White, 0.75f, 0);
            this.randomButton.draw(b);
                b.End();
                b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                this._displayFarmer.FarmerRenderer.draw(b, this._displayFarmer.FarmerSprite.CurrentAnimationFrame, this._displayFarmer.FarmerSprite.CurrentFrame, this._displayFarmer.FarmerSprite.SourceRect, new Vector2((float)(this.portraitBox.Center.X - 32), (float)(this.portraitBox.Bottom - 160)), Vector2.Zero, 0.8f, Color.White, 0.0f, 1f, this._displayFarmer);
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                if (this.hoverText != null && this.hoverTitle != null && this.hoverText.Count<char>() > 0)
                    IClickableMenu.drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, this.hoverTitle, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null, (IList<Item>)null);
                this.drawMouse(b);
        }

        private void setUpPositions()
        {

            this.labels.Clear();
            this.petButtons.Clear();
            this.genderButtons.Clear();
            this.cabinLayoutButtons.Clear();
            this.leftSelectionButtons.Clear();
            this.rightSelectionButtons.Clear();
            this.farmTypeButtons.Clear();
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
            textureComponent1.myID = 505;
            textureComponent1.upNeighborID = -99998;
            textureComponent1.leftNeighborID = -99998;
            textureComponent1.rightNeighborID = -99998;
            textureComponent1.downNeighborID = -99998;
            this.okButton = textureComponent1;

            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f, false);
            textureComponent2.myID = 507;
            textureComponent2.upNeighborID = -99998;
            textureComponent2.leftNeighborID = -99998;
            textureComponent2.rightNeighborID = -99998;
            textureComponent2.downNeighborID = -99998;
            this.randomButton = textureComponent2;

            this.portraitBox = new Rectangle(this.xPositionOnScreen + 64 + 42 - 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);

                //this centers the portraits
                this.portraitBox.X = this.xPositionOnScreen + (this.width - this.portraitBox.Width) / 2;
                this.randomButton.bounds.X = this.portraitBox.X - 56;

            int num4 = 128;
            List<ClickableComponent> selectionButtons1 = this.leftSelectionButtons;
            ClickableTextureComponent textureComponent3 = new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.X - 32, this.portraitBox.Y + 144, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
            textureComponent3.myID = 520;
            textureComponent3.upNeighborID = -99998;
            textureComponent3.leftNeighborID = -99998;
            textureComponent3.rightNeighborID = -99998;
            textureComponent3.downNeighborID = -99998;
            selectionButtons1.Add((ClickableComponent)textureComponent3);
            List<ClickableComponent> selectionButtons2 = this.rightSelectionButtons;
            ClickableTextureComponent textureComponent4 = new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.Right - 32, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
            textureComponent4.myID = 521;
            textureComponent4.upNeighborID = -99998;
            textureComponent4.leftNeighborID = -99998;
            textureComponent4.rightNeighborID = -99998;
            textureComponent4.downNeighborID = -99998;
            selectionButtons2.Add((ClickableComponent)textureComponent4);
            int num5 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt ? -20 : 0;
            this.isModifyingExistingPet = false;



                    int genderButtonOffsetX = 50;
                    List<ClickableComponent> genderButtons1 = this.genderButtons;
                    ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Male", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8  + (this.portraitBox.Width+ genderButtonOffsetX), this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), (string)null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f, false);
                    textureComponent5.myID = 508;
                    textureComponent5.upNeighborID = -99998;
                    textureComponent5.leftNeighborID = -99998;
                    textureComponent5.rightNeighborID = -99998;
                    textureComponent5.downNeighborID = -99998;
                    genderButtons1.Add((ClickableComponent)textureComponent5);
                    List<ClickableComponent> genderButtons2 = this.genderButtons;
                    ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Female", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24 + (this.portraitBox.Width+ genderButtonOffsetX), this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), (string)null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f, false);
                    textureComponent6.myID = 509;
                    textureComponent6.upNeighborID = -99998;
                    textureComponent6.leftNeighborID = -99998;
                    textureComponent6.rightNeighborID = -99998;
                    textureComponent6.downNeighborID = -99998;
                    genderButtons2.Add((ClickableComponent)textureComponent6);

                num4 = 256;

                num5 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? -20 : 0;
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent7 = new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent7.myID = 518;
                textureComponent7.upNeighborID = -99998;
                textureComponent7.leftNeighborID = -99998;
                textureComponent7.rightNeighborID = -99998;
                textureComponent7.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent7);
                this.labels.Add(this.skinLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent("Skin", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent8.myID = 519;
                textureComponent8.upNeighborID = -99998;
                textureComponent8.leftNeighborID = -99998;
                textureComponent8.rightNeighborID = -99998;
                textureComponent8.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent8);

            Point point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
            int x1 = this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;

                this.labels.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
                this.eyeColorPicker = new ColorPicker("Eyes", point1.X, point1.Y);
                this.eyeColorPicker.setColor((Color)((NetFieldBase<Color, NetColor>)Game1.player.newEyeColor));
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
                {
                    myID = 522,
                    downNeighborID = -99998,
                    upNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
                {
                    myID = 523,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
                {
                    myID = 524,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
                num4 += 68;
                selectionButtons3 = this.leftSelectionButtons;
                textureComponent5 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 514;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.hairLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
                selectionButtons4 = this.rightSelectionButtons;
                textureComponent6 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 515;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
            point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);


                this.labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
                this.hairColorPicker = new ColorPicker("Hair", point1.X, point1.Y);
                this.hairColorPicker.setColor((Color)((NetFieldBase<Color, NetColor>)Game1.player.hairstyleColor));
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
                {
                    myID = 525,
                    downNeighborID = -99998,
                    upNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
                {
                    myID = 526,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
                {
                    myID = 527,
                    upNeighborID = -99998,
                    downNeighborID = -99998,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });


                num4 += 68;
                int num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 8 : 0;
                selectionButtons3 = this.leftSelectionButtons;
                textureComponent5 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 512;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.shirtLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
                selectionButtons4 = this.rightSelectionButtons;
                textureComponent6 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 513;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
                int num7 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? -16 : 0;
                this.labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16 + num7, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
                point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
                this.pantsColorPicker = new ColorPicker("Pants", point1.X, point1.Y);
                this.pantsColorPicker.setColor((Color)((NetFieldBase<Color, NetColor>)Game1.player.pantsColor));
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y, 128, 20), "")
                {
                    myID = 528,
                    downNeighborID = -99998,
                    upNeighborID = -99998,
                    rightNeighborImmutable = true,
                    leftNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 20, 128, 20), "")
                {
                    myID = 529,
                    downNeighborID = -99998,
                    upNeighborID = -99998,
                    rightNeighborImmutable = true,
                    leftNeighborImmutable = true
                });
                this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(point1.X, point1.Y + 40, 128, 20), "")
                {
                    myID = 530,
                    downNeighborID = -99998,
                    upNeighborID = -99998,
                    rightNeighborImmutable = true,
                    leftNeighborImmutable = true
                });


                num4 += 68;
                selectionButtons3 = this.leftSelectionButtons;
                textureComponent5 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 629;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.pantsStyleLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
                selectionButtons4 = this.rightSelectionButtons;
                textureComponent6 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 517;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);

            int num8 = num4 + 68;

                num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 32 : 0;
                selectionButtons3 = this.leftSelectionButtons;
                textureComponent5 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 516;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.accLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
                selectionButtons4 = this.rightSelectionButtons;
                textureComponent6 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 517;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);


            this._shouldShowBackButton = false;

            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public new bool canLeaveMenu()
        {
            return true;
        }

    }
}
