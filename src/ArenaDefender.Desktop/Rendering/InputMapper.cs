using System.Numerics;
using ArenaDefender.Core.Systems;
using Microsoft.Xna.Framework.Input;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace ArenaDefender.Desktop.Rendering;

/// <summary>
/// Turns the raw keyboard and mouse state into a <see cref="PlayerInput"/> for the
/// game logic. This is the one place that knows the actual key and button bindings.
/// </summary>
public static class InputMapper
{
    /// <summary>Reads WASD/arrows for movement, the mouse for aim, and click/space/enter for actions.</summary>
    public static PlayerInput Map(KeyboardState keyboard, MouseState mouse, Vector2 playerPosition)
    {
        var move = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) move.Y -= 1f;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) move.Y += 1f;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) move.X -= 1f;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) move.X += 1f;

        var aim = new Vector2(mouse.X - playerPosition.X, mouse.Y - playerPosition.Y);

        bool firing = mouse.LeftButton == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Space);

        bool confirm = keyboard.IsKeyDown(Keys.Enter);

        return new PlayerInput(move, aim, firing, confirm);
    }

    /// <summary>Converts a System.Numerics vector to the MonoGame (XNA) vector type.</summary>
    public static XnaVector2 ToXna(Vector2 v) => new(v.X, v.Y);
}
