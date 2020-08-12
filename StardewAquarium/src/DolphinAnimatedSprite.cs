using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewAquarium
{
    class DolphinAnimatedSprite : TemporaryAnimatedSprite
    {
        private int holdFrames = -1;

        public DolphinAnimatedSprite(Vector2 position, Texture2D tex)
            : base(-666, 250, 16, 1, position, false, false)
        {
            texture = tex;
            Game1.playSound("pullItemFromWater");
            currentParentTileIndex = 0;
        }

        public override void draw(
            SpriteBatch spriteBatch,
            bool localPosition = false,
            int xOffset = 0,
            int yOffset = 0,
            float extraAlpha = 1f)
        {
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Position), new Rectangle(currentParentTileIndex * 16 * 4, 0, 16 * 4, 16 * 2), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((Position.Y + 32.0) / 10000.0));
        }

        public override bool update(GameTime time)
        {
            timer += time.ElapsedGameTime.Milliseconds;

            if (timer > (double)interval)
            {
                ++currentParentTileIndex;
                timer = 0.0f;
            }

            return false;
        }
    }
}
