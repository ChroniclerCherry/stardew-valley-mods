using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace UpgradeEmptyCabins
{
    public class CabinQuestionsBox : IClickableMenu
    {
        private List<string> dialogues = new List<string>();
        private List<Response> responses = new List<Response>();
        private Rectangle friendshipJewel = Rectangle.Empty;
        private int transitionX = -1;
        private int safetyTimer = 750;
        private int selectedResponse = -1;
        private bool transitioning = true;
        private bool transitioningBigger = true;
        private string hoverText = "";
        public const int portraitBoxSize = 74;
        public const int nameTagWidth = 102;
        public const int nameTagHeight = 18;
        public const int portraitPlateWidth = 115;
        public const int nameTagSideMargin = 5;
        public const float transitionRate = 3f;
        public const int characterAdvanceDelay = 30;
        public const int safetyDelay = 750;
        private int questionFinishPauseTimer;
        protected bool _showedOptions;
        public List<ClickableComponent> responseCC;
        private int x;
        private int y;
        private int transitionY;
        private int transitionWidth;
        private int transitionHeight;
        private int characterAdvanceTimer;
        private int characterIndexInDialogue;
        private int heightForQuestions;
        private int newPortaitShakeTimer;
        private bool transitionInitialized;
        private bool dialogueFinished;
        private bool isQuestion;
        private TemporaryAnimatedSprite dialogueIcon;
        public CabinQuestionsBox(string dialogue, List<Response> responses, int width = 1200)
        {
            if (Game1.options.SnappyMenus)
                Game1.mouseCursorTransparency = 0.0f;
            this.dialogues.Add(dialogue);
            this.responses = responses;
            this.isQuestion = true;
            this.width = width;
            this.setUpQuestions();
            this.height = this.heightForQuestions;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - 64;
            this.setUpIcons();
            this.characterIndexInDialogue = dialogue.Length;
            if (responses == null)
                return;
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override bool autoCenterMouseCursorForGamepad()
        {
            return false;
        }

        private void playOpeningSound()
        {
            Game1.playSound("breathin");
        }

        public override void setUpForGamePadMode()
        {
        }

        public void closeDialogue()
        {
            if (Game1.activeClickableMenu.Equals(this))
            {
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;
                if (Game1.messagePause)
                    Game1.pauseTime = 500f;
                if (Game1.currentObjectDialogue.Count > 0)
                    Game1.currentObjectDialogue.Dequeue();
                Game1.currentDialogueCharacterIndex = 0;
                if (Game1.currentObjectDialogue.Count > 0)
                {
                    Game1.dialogueUp = true;
                    Game1.questionChoices.Clear();
                    Game1.dialogueTyping = true;
                }
                Game1.tvStation = -1;
                Game1.currentSpeaker = (NPC)null;
                if (!Game1.eventUp)
                {
                    if (!Game1.isWarping)
                        Game1.player.CanMove = true;
                    Game1.player.movementDirections.Clear();
                }
                else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
                {
                    if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
                        ++Game1.currentLocation.currentEvent.CurrentCommand;
                    else
                        Game1.player.CanMove = true;
                }
                Game1.questionChoices.Clear();
            }
            if (Game1.afterDialogues == null)
                return;
            Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
            Game1.afterDialogues = (Game1.afterFadeFunction)null;
            afterDialogues();
        }

        public void finishTyping()
        {
            this.characterIndexInDialogue = this.getCurrentString().Length;
        }

        public void beginOutro()
        {
            this.transitioning = true;
            this.transitioningBigger = false;
            Game1.playSound("breathout");
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.receiveLeftClick(x, y, playSound);
        }

        private void tryOutro()
        {
            if (Game1.activeClickableMenu == null || !Game1.activeClickableMenu.Equals((object)this))
                return;
            this.beginOutro();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.transitioning)
                return;

            if (!Game1.options.SnappyMenus || !this.isQuestion || Game1.options.doesInputListContain(Game1.options.menuButton, key))
                return;
            base.receiveKeyPress(key);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.transitioning)
                return;
            if (this.characterIndexInDialogue < this.getCurrentString().Length - 1)
            {
                this.characterIndexInDialogue = this.getCurrentString().Length - 1;
            }
            else
            {
                if (this.safetyTimer > 0)
                    return;
                if (this.isQuestion)
                {
                    if (this.selectedResponse == -1)
                        return;
                    this.questionFinishPauseTimer = Game1.eventUp ? 600 : 200;
                    this.transitioning = true;
                    this.transitionInitialized = false;
                    this.transitioningBigger = true;

                    Game1.dialogueUp = false;

                    UpgradeCabins.houseUpgradeAccept(ModUtility.GetCabin(this.responses[this.selectedResponse].responseKey));
                    this.selectedResponse = -1;
                    this.tryOutro();
                    return;

                }

                this.characterIndexInDialogue = 0;

                if (!this.transitioning)
                    Game1.playSound("smallSelect");
                this.setUpIcons();
                this.safetyTimer = 750;
                if (this.getCurrentString() == null || this.getCurrentString().Length > 20)
                    return;
                this.safetyTimer -= 200;
            }
        }

        private void setUpIcons()
        {
            this.dialogueIcon = (TemporaryAnimatedSprite)null;
            if (this.isQuestion)
                this.setUpQuestionIcon();
            this.setUpForGamePadMode();
            if (this.getCurrentString() == null || this.getCurrentString().Length > 20)
                return;
            this.safetyTimer -= 200;
        }

        public override void performHoverAction(int mouseX, int mouseY)
        {
            this.hoverText = "";
            if (!this.transitioning && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
            {
                base.performHoverAction(mouseX, mouseY);
                if (this.isQuestion)
                {
                    int selectedResponse = this.selectedResponse;
                    int num = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - 16) + 48;
                    for (int index = 0; index < this.responses.Count; ++index)
                    {
                        if (mouseY >= num && mouseY < num + SpriteText.getHeightOfString(this.responses[index].responseText, this.width - 16))
                        {
                            this.selectedResponse = index;
                            if (this.responseCC != null && index < this.responseCC.Count)
                            {
                                this.currentlySnappedComponent = this.responseCC[index];
                                break;
                            }
                            break;
                        }
                        num += SpriteText.getHeightOfString(this.responses[index].responseText, this.width - 16) + 16;
                    }
                    if (this.selectedResponse != selectedResponse)
                        Game1.playSound("Cowboy_gunshot");
                }
            }
            if (!Game1.options.SnappyMenus || this.currentlySnappedComponent == null)
                return;
            this.selectedResponse = this.currentlySnappedComponent.myID;
        }

        public bool shouldDrawFriendshipJewel()
        {
            return false;
        }

        private void setUpQuestionIcon()
        {
            this.dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(330, 357, 7, 13), 100f, 6, 999999, new Vector2((float)(this.x + this.width - 40), (float)(this.y + this.height - 44)), false, false, 0.89f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f, true)
            {
                yPeriodic = true,
                yPeriodicLoopTime = 1500f,
                yPeriodicRange = 8f
            };
        }


        private void setUpQuestions()
        {
            int widthConstraint = this.width - 16;
            this.heightForQuestions = SpriteText.getHeightOfString(this.getCurrentString(), widthConstraint);
            foreach (Response response in this.responses)
                this.heightForQuestions += SpriteText.getHeightOfString(response.responseText, widthConstraint) + 16;
            this.heightForQuestions += 40;
        }

        public bool isPortraitBox()
        {
            return false;
        }

        public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            if (!this.transitionInitialized)
                return;
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle?(new Rectangle(306, 320, 16, 16)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos - 28)), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos - 28)), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos + boxWidth - 8), (float)(yPos + boxHeight - 8)), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(xPos - 44), (float)(yPos + boxHeight - 4)), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        }

        private bool shouldPortraitShake(Dialogue d)
        {
            int portraitIndex = d.getPortraitIndex();
            if (d.speaker.Name.Equals("Pam") && portraitIndex == 3 || d.speaker.Name.Equals("Abigail") && portraitIndex == 7 || (d.speaker.Name.Equals("Haley") && portraitIndex == 5 || d.speaker.Name.Equals("Maru") && portraitIndex == 9))
                return true;
            return this.newPortaitShakeTimer > 0;
        }

        public string getCurrentString()
        {

            if (this.dialogues.Count > 0)
                return this.dialogues[0].Trim().Replace(Environment.NewLine, "");
            return "";
        }

        public override void update(GameTime time)
        {
            base.update(time);
            Game1.mouseCursorTransparency = !Game1.options.SnappyMenus || Game1.lastCursorMotionWasMouse ? 1f : 0.0f;
            if (this.isQuestion && this.characterIndexInDialogue >= this.getCurrentString().Length - 1 && !this.transitioning)
            {
                Game1.mouseCursorTransparency = 1f;
                if (!this._showedOptions)
                {
                    this._showedOptions = true;
                    if (this.responses != null)
                    {
                        this.responseCC = new List<ClickableComponent>();
                        int y = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width) + 48;
                        for (int index = 0; index < this.responses.Count; ++index)
                        {
                            this.responseCC.Add(new ClickableComponent(new Rectangle(this.x + 8, y, this.width - 8, SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + 16), "")
                            {
                                myID = index,
                                downNeighborID = index < this.responses.Count - 1 ? index + 1 : -1,
                                upNeighborID = index > 0 ? index - 1 : -1
                            });
                            y += SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + 16;
                        }
                    }
                    this.populateClickableComponentList();
                    if (Game1.options.gamepadControls)
                    {
                        this.snapToDefaultClickableComponent();
                        this.selectedResponse = this.currentlySnappedComponent.myID;
                    }
                }
            }
            if (this.safetyTimer > 0)
                this.safetyTimer -= time.ElapsedGameTime.Milliseconds;
            if (this.questionFinishPauseTimer > 0)
            {
                this.questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
            }
            else
            {
                TimeSpan elapsedGameTime;
                if (this.transitioning)
                {
                    if (!this.transitionInitialized)
                    {
                        this.transitionInitialized = true;
                        this.transitionX = this.x + this.width / 2;
                        this.transitionY = this.y + this.height / 2;
                        this.transitionWidth = 0;
                        this.transitionHeight = 0;
                    }
                    if (this.transitioningBigger)
                    {
                        int transitionWidth1 = this.transitionWidth;
                        this.transitionX -= (int)((double)time.ElapsedGameTime.Milliseconds * 3.0);
                        this.transitionY -= (int)((double)time.ElapsedGameTime.Milliseconds * 3.0 * ((this.isQuestion ? (double)this.heightForQuestions : (double)this.height) / (double)this.width));
                        this.transitionX = Math.Max(this.x, this.transitionX);
                        this.transitionY = Math.Max(this.isQuestion ? this.y + this.height - this.heightForQuestions : this.y, this.transitionY);
                        int transitionWidth2 = this.transitionWidth;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num1 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * 2.0);
                        this.transitionWidth = transitionWidth2 + num1;
                        int transitionHeight = this.transitionHeight;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num2 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * ((this.isQuestion ? (double)this.heightForQuestions : (double)this.height) / (double)this.width) * 2.0);
                        this.transitionHeight = transitionHeight + num2;
                        this.transitionWidth = Math.Min(this.width, this.transitionWidth);
                        this.transitionHeight = Math.Min(this.isQuestion ? this.heightForQuestions : this.height, this.transitionHeight);
                        if (transitionWidth1 == 0 && this.transitionWidth > 0)
                            this.playOpeningSound();
                        if (this.transitionX == this.x && this.transitionY == (this.isQuestion ? this.y + this.height - this.heightForQuestions : this.y))
                        {
                            this.transitioning = false;
                            this.characterAdvanceTimer = 90;
                            this.setUpIcons();
                            this.transitionX = this.x;
                            this.transitionY = this.y;
                            this.transitionWidth = this.width;
                            this.transitionHeight = this.height;
                        }
                    }
                    else
                    {
                        this.transitionX += (int)((double)time.ElapsedGameTime.Milliseconds * 3.0);
                        this.transitionY += (int)((double)time.ElapsedGameTime.Milliseconds * 3.0 * ((double)this.height / (double)this.width));
                        this.transitionX = Math.Min(this.x + this.width / 2, this.transitionX);
                        this.transitionY = Math.Min(this.y + this.height / 2, this.transitionY);
                        int transitionWidth = this.transitionWidth;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num1 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * 2.0);
                        this.transitionWidth = transitionWidth - num1;
                        int transitionHeight = this.transitionHeight;
                        elapsedGameTime = time.ElapsedGameTime;
                        int num2 = (int)((double)elapsedGameTime.Milliseconds * 3.0 * ((double)this.height / (double)this.width) * 2.0);
                        this.transitionHeight = transitionHeight - num2;
                        this.transitionWidth = Math.Max(0, this.transitionWidth);
                        this.transitionHeight = Math.Max(0, this.transitionHeight);
                        if (this.transitionWidth == 0 && this.transitionHeight == 0)
                            this.closeDialogue();
                    }
                }
                if (!this.transitioning && this.characterIndexInDialogue < this.getCurrentString().Length)
                {
                    int characterAdvanceTimer = this.characterAdvanceTimer;
                    elapsedGameTime = time.ElapsedGameTime;
                    int milliseconds = elapsedGameTime.Milliseconds;
                    this.characterAdvanceTimer = characterAdvanceTimer - milliseconds;
                    if (this.characterAdvanceTimer <= 0)
                    {
                        this.characterAdvanceTimer = 30;
                        int characterIndexInDialogue = this.characterIndexInDialogue;
                        this.characterIndexInDialogue = Math.Min(this.characterIndexInDialogue + 1, this.getCurrentString().Length);
                        if (this.characterIndexInDialogue != characterIndexInDialogue && this.characterIndexInDialogue == this.getCurrentString().Length)
                            Game1.playSound("dialogueCharacterClose");
                        if (this.characterIndexInDialogue > 1 && this.characterIndexInDialogue < this.getCurrentString().Length && Game1.options.dialogueTyping)
                            Game1.playSound("dialogueCharacter");
                    }
                }
                if (!this.transitioning && this.dialogueIcon != null)
                    this.dialogueIcon.update(time);
                if (this.transitioning || this.newPortaitShakeTimer <= 0)
                    return;
                int portaitShakeTimer = this.newPortaitShakeTimer;
                elapsedGameTime = time.ElapsedGameTime;
                int milliseconds1 = elapsedGameTime.Milliseconds;
                this.newPortaitShakeTimer = portaitShakeTimer - milliseconds1;
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.width = 1200;
            this.height = 384;
            this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height, 0, 0).X;
            this.y = Game1.viewport.Height - this.height - 64;
            this.friendshipJewel = new Rectangle(this.x + this.width - 64, this.y + 256, 44, 44);
            this.setUpIcons();
        }

        public override void draw(SpriteBatch b)
        {
            if (this.width < 16 || this.height < 16)
                return;
            if (this.transitioning)
            {
                this.drawBox(b, this.transitionX, this.transitionY, this.transitionWidth, this.transitionHeight);
                this.drawMouse(b);
            }
            else
            {
                this.drawBox(b, this.x, this.y - (this.heightForQuestions - this.height), this.width, this.heightForQuestions);
                SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 12 - (this.heightForQuestions - this.height), this.characterIndexInDialogue, this.width - 16, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                if (this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
                {
                    int y = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(this.getCurrentString(), this.width - 16) + 48;
                    for (int index = 0; index < this.responses.Count; ++index)
                    {
                        if (index == this.selectedResponse)
                            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.x + 4, y - 8, this.width - 8, SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + 16, Color.White, 4f, false);
                        SpriteText.drawString(b, this.responses[index].responseText, this.x + 8, y, 999999, this.width, 999999, this.selectedResponse == index ? 1f : 0.6f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                        y += SpriteText.getHeightOfString(this.responses[index].responseText, this.width) + 16;
                    }
                }
                if (this.dialogueIcon != null && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
                    this.dialogueIcon.draw(b, true, 0, 0, 1f);
                if (this.hoverText.Length > 0)
                    SpriteText.drawStringWithScrollBackground(b, this.hoverText, this.friendshipJewel.Center.X - SpriteText.getWidthOfString(this.hoverText, 999999) / 2, this.friendshipJewel.Y - 64, "", 1f, -1, SpriteText.ScrollTextAlignment.Left);
                this.drawMouse(b);
            }
        }
    }
}
