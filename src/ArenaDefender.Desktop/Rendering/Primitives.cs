using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaDefender.Desktop.Rendering;

/// <summary>
/// Draws simple shapes (rectangles, circles and lines) from a single white pixel
/// texture and one circle texture built at runtime. This is why the game needs no
/// image files or MonoGame content pipeline.
/// </summary>
public sealed class Primitives : IDisposable
{
    private readonly Texture2D _pixel;
    private readonly Texture2D _circle;
    private readonly int _circleDiameter;

    /// <summary>Builds the pixel and circle textures on the given graphics device.</summary>
    public Primitives(GraphicsDevice graphicsDevice, int circleDiameter = 64)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        _circleDiameter = circleDiameter;

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _circle = CreateCircleTexture(graphicsDevice, circleDiameter);
    }

    /// <summary>The 1x1 white texture, handy for drawing solid colours.</summary>
    public Texture2D Pixel => _pixel;

    /// <summary>Fills a rectangle with a colour.</summary>
    public void FillRectangle(SpriteBatch batch, Rectangle rectangle, Color color)
        => batch.Draw(_pixel, rectangle, color);

    /// <summary>Fills a rectangle (given as x, y, width, height) with a colour.</summary>
    public void FillRectangle(SpriteBatch batch, float x, float y, float width, float height, Color color)
        => batch.Draw(_pixel, new Rectangle((int)x, (int)y, (int)width, (int)height), color);

    /// <summary>Draws just the border of a rectangle at the given thickness.</summary>
    public void DrawRectangleOutline(SpriteBatch batch, Rectangle rectangle, int thickness, Color color)
    {
        FillRectangle(batch, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thickness), color);
        FillRectangle(batch, new Rectangle(rectangle.Left, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        FillRectangle(batch, new Rectangle(rectangle.Left, rectangle.Top, thickness, rectangle.Height), color);
        FillRectangle(batch, new Rectangle(rectangle.Right - thickness, rectangle.Top, thickness, rectangle.Height), color);
    }

    /// <summary>Draws a filled circle by scaling the circle texture to the wanted radius.</summary>
    public void FillCircle(SpriteBatch batch, Vector2 center, float radius, Color color)
    {
        float scale = radius * 2f / _circleDiameter;
        var origin = new Vector2(_circleDiameter / 2f, _circleDiameter / 2f);
        batch.Draw(_circle, center, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    /// <summary>Draws a line between two points by stretching and rotating the pixel texture.</summary>
    public void DrawLine(SpriteBatch batch, Vector2 start, Vector2 end, float thickness, Color color)
    {
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length < 0.001f)
        {
            return;
        }

        float angle = MathF.Atan2(delta.Y, delta.X);
        batch.Draw(
            _pixel,
            start,
            null,
            color,
            angle,
            new Vector2(0f, 0.5f),
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }

    private static Texture2D CreateCircleTexture(GraphicsDevice device, int diameter)
    {
        var texture = new Texture2D(device, diameter, diameter);
        var data = new Color[diameter * diameter];
        float radius = diameter / 2f;
        Vector2 centre = new(radius, radius);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), centre);
                float alpha = MathHelper.Clamp(radius - distance, 0f, 1f);
                data[y * diameter + x] = Color.White * alpha;
            }
        }

        texture.SetData(data);
        return texture;
    }

    /// <summary>Releases the textures.</summary>
    public void Dispose()
    {
        _pixel.Dispose();
        _circle.Dispose();
    }
}
