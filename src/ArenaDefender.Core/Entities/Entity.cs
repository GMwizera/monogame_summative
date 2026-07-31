using System.Numerics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// Base class for everything in the arena that is drawn as a circle: the player,
/// enemies, projectiles and power-ups. It holds the state they all share (a
/// position, a radius and an "alive" flag) so the collision and drawing code can
/// treat them the same way.
/// </summary>
public abstract class Entity
{
    /// <summary>Centre of the entity in the arena, in pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Radius of the circular body, in pixels.</summary>
    public float Radius { get; protected set; }

    /// <summary>When false the entity is removed from the world at the end of the frame.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Advances the entity by the given number of seconds.</summary>
    public abstract void Update(float deltaSeconds);

    /// <summary>Keeps the entity inside the arena so it can never leave the play field.</summary>
    public void ClampToArena(float arenaWidth, float arenaHeight)
    {
        float x = Position.X < Radius ? Radius : Position.X > arenaWidth - Radius ? arenaWidth - Radius : Position.X;
        float y = Position.Y < Radius ? Radius : Position.Y > arenaHeight - Radius ? arenaHeight - Radius : Position.Y;
        Position = new Vector2(x, y);
    }
}
