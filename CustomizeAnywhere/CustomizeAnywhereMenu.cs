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
        private TextBox nameBox;
        private TextBox farmnameBox;
        private TextBox favThingBox;
        private ClickableComponent nameLabel;
        private ClickableComponent farmLabel;
        private ClickableComponent favoriteLabel;
        private ClickableComponent shirtLabel;
        private ClickableComponent skinLabel;
        private ClickableComponent hairLabel;
        private ClickableComponent accLabel;
        private ClickableComponent pantsStyleLabel;
        private ClickableComponent startingCabinsLabel;
        private ClickableComponent cabinLayoutLabel;
        private ClickableComponent separateWalletLabel;
        private ClickableComponent difficultyModifierLabel;
        private string hoverText;
        private string hoverTitle;
        private string coopHelpString;
        private string noneString;
        private string normalDiffString;
        private string toughDiffString;
        private string hardDiffString;
        private string superDiffString;
        private string sharedWalletString;
        private string separateWalletString;

        public CustomizeAnywhereMenu() : base(Source.NewGame)
        {
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
            if (this.source == CharacterCustomization.Source.ClothesDye && this._itemToDye == null)
                return;
            bool flag1 = true;
            bool flag2 = true;
            if (this.source == CharacterCustomization.Source.Wizard || this.source == CharacterCustomization.Source.ClothesDye || this.source == CharacterCustomization.Source.DyePots)
                flag2 = false;
            if (this.source == CharacterCustomization.Source.ClothesDye || this.source == CharacterCustomization.Source.DyePots)
                flag1 = false;
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
            this.backButton = new ClickableComponent(new Rectangle(Game1.viewport.Width - 198 - 48, Game1.viewport.Height - 81 - 24, 198, 81), "")
            {
                myID = 81114,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            this.nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
            {
                X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
                Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
                Text = Game1.player.Name
            };
            this.nameBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
            {
                myID = 536,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            int num1 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt ? -4 : 0;
            this.labels.Add(this.nameLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + num1 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));
            this.farmnameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
            {
                X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
                Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
                Text = (string)((NetFieldBase<string, NetString>)Game1.MasterPlayer.farmName)
            };
            this.farmnameBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
            {
                myID = 537,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            int num2 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? -16 : 0;
            this.labels.Add(this.farmLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + num1 * 3 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4 + num2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Farm")));
            int num3 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? 48 : 0;
            this.favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
            {
                X = this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + num3,
                Y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128,
                Text = (string)((NetFieldBase<string, NetString>)Game1.player.favoriteThing)
            };
            this.favThingBoxCC = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 192, 48), "")
            {
                myID = 538,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            this.labels.Add(this.favoriteLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + num1 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
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

            if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm || (this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard))
            {
                if (this.source != CharacterCustomization.Source.Wizard)
                {
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
                }
                num4 = 256;
                if (this.source == CharacterCustomization.Source.Wizard)
                    num4 = 192;
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
            }
            if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm)
            {
                Game1.startingCabins = 0;
                if (this.source == CharacterCustomization.Source.HostNewFarm)
                    Game1.startingCabins = 1;
                Game1.player.difficultyModifier = 1f;
                Game1.player.team.useSeparateWallets.Value = false;
                Point point = new Point(this.xPositionOnScreen + this.width + 4 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth);
                List<ClickableTextureComponent> farmTypeButtons1 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Standard", new Rectangle(point.X, point.Y + 88, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmStandard"), Game1.mouseCursors, new Rectangle(0, 324, 22, 20), 4f, false);
                textureComponent5.myID = 531;
                textureComponent5.downNeighborID = 532;
                textureComponent5.leftNeighborID = 537;
                farmTypeButtons1.Add(textureComponent5);
                List<ClickableTextureComponent> farmTypeButtons2 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Riverland", new Rectangle(point.X, point.Y + 176, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmFishing"), Game1.mouseCursors, new Rectangle(22, 324, 22, 20), 4f, false);
                textureComponent6.myID = 532;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                farmTypeButtons2.Add(textureComponent6);
                List<ClickableTextureComponent> farmTypeButtons3 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent7 = new ClickableTextureComponent("Forest", new Rectangle(point.X, point.Y + 264, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmForaging"), Game1.mouseCursors, new Rectangle(44, 324, 22, 20), 4f, false);
                textureComponent7.myID = 533;
                textureComponent7.upNeighborID = -99998;
                textureComponent7.leftNeighborID = -99998;
                textureComponent7.rightNeighborID = -99998;
                textureComponent7.downNeighborID = -99998;
                farmTypeButtons3.Add(textureComponent7);
                List<ClickableTextureComponent> farmTypeButtons4 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent("Hills", new Rectangle(point.X, point.Y + 352, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmMining"), Game1.mouseCursors, new Rectangle(66, 324, 22, 20), 4f, false);
                textureComponent8.myID = 534;
                textureComponent8.upNeighborID = -99998;
                textureComponent8.leftNeighborID = -99998;
                textureComponent8.rightNeighborID = -99998;
                textureComponent8.downNeighborID = -99998;
                farmTypeButtons4.Add(textureComponent8);
                List<ClickableTextureComponent> farmTypeButtons5 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent9 = new ClickableTextureComponent("Wilderness", new Rectangle(point.X, point.Y + 440, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmCombat"), Game1.mouseCursors, new Rectangle(88, 324, 22, 20), 4f, false);
                textureComponent9.myID = 535;
                textureComponent9.upNeighborID = -99998;
                textureComponent9.leftNeighborID = -99998;
                textureComponent9.rightNeighborID = -99998;
                textureComponent9.downNeighborID = -99998;
                farmTypeButtons5.Add(textureComponent9);
                List<ClickableTextureComponent> farmTypeButtons6 = this.farmTypeButtons;
                ClickableTextureComponent textureComponent10 = new ClickableTextureComponent("Four Corners", new Rectangle(point.X, point.Y + 528, 88, 80), (string)null, Game1.content.LoadString("Strings\\UI:Character_FarmFourCorners"), Game1.mouseCursors, new Rectangle(0, 345, 22, 20), 4f, false);
                textureComponent10.myID = 545;
                textureComponent10.upNeighborID = -99998;
                textureComponent10.leftNeighborID = -99998;
                textureComponent10.rightNeighborID = -99998;
                textureComponent10.downNeighborID = -99998;
                farmTypeButtons6.Add(textureComponent10);
            }
            if (this.source == CharacterCustomization.Source.HostNewFarm)
            {
                this.labels.Add(this.startingCabinsLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 84, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Cabins", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 621;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Cabins", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 622;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
                this.labels.Add(this.cabinLayoutLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 128 - (int)((double)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X / 2.0), this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 120 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
                List<ClickableTextureComponent> cabinLayoutButtons1 = this.cabinLayoutButtons;
                ClickableTextureComponent textureComponent7 = new ClickableTextureComponent("Close", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), (string)null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f, false);
                textureComponent7.myID = 623;
                textureComponent7.upNeighborID = -99998;
                textureComponent7.leftNeighborID = -99998;
                textureComponent7.rightNeighborID = -99998;
                textureComponent7.downNeighborID = -99998;
                cabinLayoutButtons1.Add(textureComponent7);
                List<ClickableTextureComponent> cabinLayoutButtons2 = this.cabinLayoutButtons;
                ClickableTextureComponent textureComponent8 = new ClickableTextureComponent("Separate", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), (string)null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f, false);
                textureComponent8.myID = 624;
                textureComponent8.upNeighborID = -99998;
                textureComponent8.leftNeighborID = -99998;
                textureComponent8.rightNeighborID = -99998;
                textureComponent8.downNeighborID = -99998;
                cabinLayoutButtons2.Add(textureComponent8);
                this.labels.Add(this.difficultyModifierLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 56, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
                List<ClickableComponent> selectionButtons5 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent9 = new ClickableTextureComponent("Difficulty", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent9.myID = 627;
                textureComponent9.upNeighborID = -99998;
                textureComponent9.leftNeighborID = -99998;
                textureComponent9.rightNeighborID = -99998;
                textureComponent9.downNeighborID = -99998;
                selectionButtons5.Add((ClickableComponent)textureComponent9);
                List<ClickableComponent> selectionButtons6 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent10 = new ClickableTextureComponent("Difficulty", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent10.myID = 628;
                textureComponent10.upNeighborID = -99998;
                textureComponent10.leftNeighborID = -99998;
                textureComponent10.rightNeighborID = -99998;
                textureComponent10.downNeighborID = -99998;
                selectionButtons6.Add((ClickableComponent)textureComponent10);
                int y = this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 320 + 100;
                this.labels.Add(this.separateWalletLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 21 - 128, y - 24, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Wallets")));
                List<ClickableComponent> selectionButtons7 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent11 = new ClickableTextureComponent("Wallets", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, y, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent11.myID = 631;
                textureComponent11.upNeighborID = -99998;
                textureComponent11.leftNeighborID = -99998;
                textureComponent11.rightNeighborID = -99998;
                textureComponent11.downNeighborID = -99998;
                selectionButtons7.Add((ClickableComponent)textureComponent11);
                List<ClickableComponent> selectionButtons8 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent12 = new ClickableTextureComponent("Wallets", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, y, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent12.myID = 632;
                textureComponent12.upNeighborID = -99998;
                textureComponent12.leftNeighborID = -99998;
                textureComponent12.rightNeighborID = -99998;
                textureComponent12.downNeighborID = -99998;
                selectionButtons8.Add((ClickableComponent)textureComponent12);
                ClickableTextureComponent textureComponent13 = new ClickableTextureComponent("CoopHelp", new Rectangle(this.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 448 + 40, 64, 64), (string)null, Game1.content.LoadString("Strings\\UI:Character_CoopHelp"), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f, false);
                textureComponent13.myID = 625;
                textureComponent13.upNeighborID = -99998;
                textureComponent13.leftNeighborID = -99998;
                textureComponent13.rightNeighborID = -99998;
                textureComponent13.downNeighborID = -99998;
                this.coopHelpButton = textureComponent13;
                ClickableTextureComponent textureComponent14 = new ClickableTextureComponent("CoopHelpOK", new Rectangle(this.xPositionOnScreen - 256 - 12, this.yPositionOnScreen + this.height - 64, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
                textureComponent14.myID = 626;
                textureComponent14.region = 635;
                textureComponent14.upNeighborID = -99998;
                textureComponent14.leftNeighborID = -99998;
                textureComponent14.rightNeighborID = -99998;
                textureComponent14.downNeighborID = -99998;
                this.coopHelpOkButton = textureComponent14;
                this.noneString = Game1.content.LoadString("Strings\\UI:Character_none");
                this.normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
                this.toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
                this.hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
                this.superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
                this.separateWalletString = Game1.content.LoadString("Strings\\UI:Character_SeparateWallet");
                this.sharedWalletString = Game1.content.LoadString("Strings\\UI:Character_SharedWallet");
                ClickableTextureComponent textureComponent15 = new ClickableTextureComponent("CoopHelpRight", new Rectangle(this.xPositionOnScreen + this.width, this.yPositionOnScreen + this.height, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent15.myID = 633;
                textureComponent15.region = 635;
                textureComponent15.upNeighborID = -99998;
                textureComponent15.leftNeighborID = -99998;
                textureComponent15.rightNeighborID = -99998;
                textureComponent15.downNeighborID = -99998;
                this.coopHelpRightButton = textureComponent15;
                ClickableTextureComponent textureComponent16 = new ClickableTextureComponent("CoopHelpLeft", new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + this.height, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent16.myID = 634;
                textureComponent16.region = 635;
                textureComponent16.upNeighborID = -99998;
                textureComponent16.leftNeighborID = -99998;
                textureComponent16.rightNeighborID = -99998;
                textureComponent16.downNeighborID = -99998;
                this.coopHelpLeftButton = textureComponent16;
            }
            Point point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
            int x1 = this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;
            if (this._isDyeMenu)
                x1 = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
            if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm || (this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard))
            {
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
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 514;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.hairLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Hair", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 515;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
            }
            point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
            if (this.source == CharacterCustomization.Source.NewGame || this.source == CharacterCustomization.Source.HostNewFarm || (this.source == CharacterCustomization.Source.NewFarmhand || this.source == CharacterCustomization.Source.Wizard))
            {
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
            }
            if (this.source == CharacterCustomization.Source.DyePots)
            {
                num4 += 68;
                if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
                {
                    point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
                    point1.X = this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
                    this.labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_ShirtColor")));
                    this.hairColorPicker = new ColorPicker("Hair", point1.X, point1.Y);
                    this.hairColorPicker.setColor(Game1.player.GetShirtColor());
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
                    num4 += 64;
                }
                if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
                {
                    point1 = new Point(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4);
                    point1.X = this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
                    int num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? -16 : 0;
                    this.labels.Add(new ClickableComponent(new Rectangle(x1, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16 + num6, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
                    this.pantsColorPicker = new ColorPicker("Pants", point1.X, point1.Y);
                    this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
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
                }
            }
            else if (flag2)
            {
                num4 += 68;
                int num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 8 : 0;
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 512;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.shirtLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Shirt", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
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
            }
            ClickableTextureComponent textureComponent17 = new ClickableTextureComponent("Skip Intro", new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 - 48 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 80, 36, 36), (string)null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f, false);
            textureComponent17.myID = 506;
            textureComponent17.upNeighborID = 530;
            textureComponent17.leftNeighborID = 517;
            textureComponent17.rightNeighborID = 505;
            this.skipIntroButton = textureComponent17;
            if (flag2)
            {
                num4 += 68;
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 629;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.pantsStyleLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Pants Style", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num4, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 517;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
            }
            int num8 = num4 + 68;
            if (flag1)
            {
                int num6 = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr ? 32 : 0;
                List<ClickableComponent> selectionButtons3 = this.leftSelectionButtons;
                ClickableTextureComponent textureComponent5 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + num5 - num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false);
                textureComponent5.myID = 516;
                textureComponent5.upNeighborID = -99998;
                textureComponent5.leftNeighborID = -99998;
                textureComponent5.rightNeighborID = -99998;
                textureComponent5.downNeighborID = -99998;
                selectionButtons3.Add((ClickableComponent)textureComponent5);
                this.labels.Add(this.accLabel = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + num5 / 2, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8 + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
                List<ClickableComponent> selectionButtons4 = this.rightSelectionButtons;
                ClickableTextureComponent textureComponent6 = new ClickableTextureComponent("Acc", new Rectangle(this.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + num6, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + num8, 64, 64), (string)null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false);
                textureComponent6.myID = 517;
                textureComponent6.upNeighborID = -99998;
                textureComponent6.leftNeighborID = -99998;
                textureComponent6.rightNeighborID = -99998;
                textureComponent6.downNeighborID = -99998;
                selectionButtons4.Add((ClickableComponent)textureComponent6);
            }

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
