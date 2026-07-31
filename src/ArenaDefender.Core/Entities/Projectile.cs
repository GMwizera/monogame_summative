using System.Numerics;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// A bullet fired by the player. It travels in a straight line and disappears
/// when it hits an enemy, leaves the arena, or its lifetime runs out.
/// </summary>
public sealed class Projectile : Entity
{
    private float _remainingLifetime;

    /// <summary>Creates a projectile moving in the given direction at the given speed.</summary>
    public Projectile(Vector2 position, Vector2 direction, float speed, float radius, int damage, float lifetime)
    {
        if (speed <= 0f) throw new ArgumentOutOfRangeException(nameof(speed), speed, "Speed must be positive.");
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be positive.");
        if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage must be positive.");
        if (lifetime <= 0f) throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Lifetime must be positive.");

        Position = position;
        Radius = radius;
        Damage = damage;
        Velocity = MathUtils.Normalize(direction) * speed;
        _remainingLifetime = lifetime;
    }

    /// <summary>How fast and in which direction the projectile travels.</summary>
    public Vector2 Velocity { get; }

    /// <summary>Damage dealt to an enemy on hit.</summary>
    public int Damage { get; }

    /// <summary>Moves the projectile and removes it when its lifetime runs out.</summary>
    public override void Update(float deltaSeconds)
    {
        float dt = MathF.Max(0f, deltaSeconds);
        Position += Velocity * dt;
        _remainingLifetime -= dt;
        if (_remainingLifetime <= 0f)
        {
            IsActive = false;
        }
    }
}
