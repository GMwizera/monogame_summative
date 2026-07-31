using System.Numerics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// A pickup dropped by defeated enemies. It sits in the arena for a while, then
/// disappears if the player does not collect it in time.
/// </summary>
public sealed class PowerUp : Entity
{
    private float _remainingLifetime;

    /// <summary>Creates a power-up of the given type that lasts for the given lifetime.</summary>
    public PowerUp(PowerUpType type, Vector2 position, float radius, float lifetime)
    {
        if (radius <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be positive.");
        }

        if (lifetime <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Lifetime must be positive.");
        }

        Type = type;
        Position = position;
        Radius = radius;
        _remainingLifetime = lifetime;
        InitialLifetime = lifetime;
    }

    /// <summary>Which effect this power-up gives.</summary>
    public PowerUpType Type { get; }

    /// <summary>How long the power-up lasts in total, in seconds.</summary>
    public float InitialLifetime { get; }

    /// <summary>How much of its lifetime remains, from 1 down to 0 (used for fading it out).</summary>
    public float LifetimeFraction => _remainingLifetime / InitialLifetime;

    /// <summary>Counts down the lifetime and removes the power-up when it runs out.</summary>
    public override void Update(float deltaSeconds)
    {
        _remainingLifetime -= MathF.Max(0f, deltaSeconds);
        if (_remainingLifetime <= 0f)
        {
            IsActive = false;
        }
    }
}
