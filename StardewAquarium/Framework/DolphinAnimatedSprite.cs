using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewAquarium.Framework;

internal class DolphinAnimatedSprite : TemporaryAnimatedSprite
{
    /*********
    ** Fields
    *********/
    private const int SpriteWidth = 64;


    /*********
    ** Public methods
    *********/
    public DolphinAnimatedSprite(Vector2 position, Texture2D tex)
        : base(-666, 250, tex.Width / SpriteWidth, 1, position, false, false)
    {
        this.texture = tex;
        Game1.playSound("pullItemFromWater");
        this.currentParentTileIndex = 0;
    }

    public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
    {
        spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.Position), new Rectangle(this.currentParentTileIndex * 16 * 4, 0, 16 * 4, 16 * 2), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.Position.Y + 32.0) / 10000.0));
    }

    public override bool update(GameTime time)
    {
        this.timer += time.ElapsedGameTime.Milliseconds;

        if (this.timer <= this.interval)
            return false;
        ++this.currentParentTileIndex;
        this.timer = 0.0f;

        return false;
    }
}
