using System.Numerics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// Describes a single attack an enemy makes this frame (currently the sniper's
/// beam): how much damage it deals and the line it is drawn along.
/// </summary>
public class EnemyAttack
{
    /// <summary>Creates an attack with the given damage and start/end points.</summary>
    public EnemyAttack(int damage, Vector2 origin, Vector2 target)
    {
        Damage = damage;
        Origin = origin;
        Target = target;
    }

    /// <summary>Damage this attack deals to the player.</summary>
    public int Damage { get; }

    /// <summary>Where the attack starts (the enemy).</summary>
    public Vector2 Origin { get; }

    /// <summary>Where the attack is aimed (the player).</summary>
    public Vector2 Target { get; }
}

/// <summary>
/// Base class for all enemies. It stores the shared stats (health, speed, damage,
/// score) and defines <see cref="Think"/>, which each enemy type overrides to
/// decide how it moves and attacks.
/// </summary>
public abstract class Enemy : Entity, IDamageable
{
    /// <summary>Sets up the shared enemy stats and validates them.</summary>
    protected Enemy(EnemyKind kind, Vector2 position, float radius, int maxHealth, float speed, int contactDamage, int scoreValue)
    {
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be positive.");
        if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Max health must be positive.");
        if (speed < 0f) throw new ArgumentOutOfRangeException(nameof(speed), speed, "Speed cannot be negative.");
        if (contactDamage < 0) throw new ArgumentOutOfRangeException(nameof(contactDamage), contactDamage, "Contact damage cannot be negative.");
        if (scoreValue < 0) throw new ArgumentOutOfRangeException(nameof(scoreValue), scoreValue, "Score value cannot be negative.");

        Kind = kind;
        Position = position;
        Radius = radius;
        MaxHealth = maxHealth;
        Health = maxHealth;
        Speed = speed;
        ContactDamage = contactDamage;
        ScoreValue = scoreValue;
        Facing = new Vector2(0f, 1f);
    }

    /// <summary>Which type of enemy this is.</summary>
    public EnemyKind Kind { get; }

    /// <summary>Current health. Zero means defeated.</summary>
    public int Health { get; private set; }

    /// <summary>The most health this enemy can have.</summary>
    public int MaxHealth { get; }

    /// <summary>True while health is above zero.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Movement speed in pixels per second.</summary>
    public float Speed { get; }

    /// <summary>Damage dealt to the player on contact.</summary>
    public int ContactDamage { get; }

    /// <summary>Points awarded when this enemy is defeated.</summary>
    public int ScoreValue { get; }

    /// <summary>Current movement, applied each frame in <see cref="Update"/>.</summary>
    public Vector2 Velocity { get; protected set; }

    /// <summary>The direction the enemy is facing.</summary>
    public Vector2 Facing { get; protected set; }

    /// <summary>Applies damage and deactivates the enemy if it dies. Returns the damage removed.</summary>
    public int TakeDamage(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage cannot be negative.");
        int before = Health;
        Health = Math.Max(0, Health - amount);
        if (Health == 0)
        {
            IsActive = false;
        }

        return before - Health;
    }

    /// <summary>Restores health without going over the maximum and returns how much was healed.</summary>
    public int Heal(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Heal amount cannot be negative.");
        int before = Health;
        Health = Math.Min(MaxHealth, Health + amount);
        return Health - before;
    }

    /// <summary>
    /// Decides how the enemy moves and whether it attacks this frame. Each enemy
    /// type provides its own behaviour. Returns an attack, or null if none.
    /// </summary>
    public abstract EnemyAttack? Think(Player player, float deltaSeconds);

    /// <summary>Moves the enemy by its current velocity.</summary>
    public override void Update(float deltaSeconds)
    {
        Position += Velocity * MathF.Max(0f, deltaSeconds);
    }
}
