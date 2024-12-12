using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewAquarium.Menus;

internal sealed class AquariumClickableComponent : ClickableTextureComponent
{
    public AquariumClickableComponent(Rectangle bounds, ParsedItemData data, float scale, bool drawShadow = false)
        : base(bounds, data.GetTexture(), data.GetSourceRect(), scale, drawShadow)
    {
    }

    public AquariumClickableComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
        : base(name, bounds, label, hoverText, texture, sourceRect, scale, drawShadow)
    {
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);
    }
}
