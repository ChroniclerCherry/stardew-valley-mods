using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace StardewAquarium.Framework.Menus;

internal class DonateFishMenu : InventoryMenu
{
    /*********
    ** Fields
    *********/
    private bool Donated;
    private bool PufferchickDonated;

    private readonly string Title;


    /*********
    ** Public methods
    *********/
    public DonateFishMenu()
        : base(Game1.viewport.Width / 2 - 768 / 2, Game1.viewport.Height / 2 + 36, false, null, Utils.IsUnDonatedFish, 36)
    {
        this.showGrayedOutSlots = true;
        this.exitFunction = () => Utils.DonationMenuExit(this.Donated, this.PufferchickDonated);

        this.Title = ContentPackHelper.LoadString("DonationMenuTitle");
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        Item item = this.getItemAt(x, y);
        if (!Utils.IsUnDonatedFish(item))
            return;

        if (Utils.DonateFish(item))
        {
            this.Donated = true;
            Game1.playSound("newArtifact");
            item.Stack--;
            if (item.Stack <= 0)
                Game1.player.removeItemFromInventory(item);

            if (item.QualifiedItemId == ContentPackHelper.PufferchickQualifiedId)
            {
                Game1.playSound("openChest");
                this.PufferchickDonated = true;
            }

            Game1.Multiplayer.globalChatInfoMessage($"{ContentPackHelper.ContentPackId}_FishDonated", Game1.player.Name, item.DisplayName); // TokenStringBuilder.ItemNameFor(item)
        }
    }

    public override void draw(SpriteBatch b)
    {
        // base.draw(b);
        if (!Game1.options.showMenuBackground)
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        else
            base.drawBackground(b);

        SpriteText.drawStringWithScrollCenteredAt(b, this.Title, Game1.viewport.Width / 2,
            Game1.viewport.Height / 2 - 128, SpriteText.getWidthOfString(this.Title) + 16);

        Game1.drawDialogueBox(this.xPositionOnScreen - 64, this.yPositionOnScreen - 128, this.width + 128, this.height + 176, false, true);

        base.draw(b);
        this.drawMouse(b);
    }
}
