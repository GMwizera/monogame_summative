using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// The character the person plays. Handles movement, aiming, health, lives and
/// the temporary power-up effects (shield, rapid fire and speed boost).
/// </summary>
public sealed class Player : Entity, IDamageable
{
    private readonly GameSettings _settings;

    private float _shieldTimer;
    private float _rapidFireTimer;
    private float _speedTimer;

    private float _fireCooldownRemaining;

    private const float SpeedBoostFactor = 1.6f;

    private const float RapidFireFactor = 0.45f;

    /// <summary>Creates a player at the centre of the arena with full health.</summary>
    public Player(GameSettings settings, int lives = 3)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        if (lives < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lives), lives, "A player must start with at least one life.");
        }

        Radius = settings.PlayerRadius;
        MaxHealth = settings.PlayerMaxHealth;
        Health = settings.PlayerMaxHealth;
        Lives = lives;
        Position = new Vector2(settings.ArenaWidth * 0.5f, settings.ArenaHeight * 0.5f);
        AimDirection = new Vector2(0f, -1f);
    }

    /// <summary>Current health. Never negative; zero means the current life is lost.</summary>
    public int Health { get; private set; }

    /// <summary>The most health the player can have.</summary>
    public int MaxHealth { get; private set; }

    /// <summary>True while health is above zero.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Lives remaining. The game ends when this runs out.</summary>
    public int Lives { get; private set; }

    /// <summary>The direction the player is aiming (where shots go).</summary>
    public Vector2 AimDirection { get; private set; }

    /// <summary>True while the shield power-up is active (blocks all damage).</summary>
    public bool HasShield => _shieldTimer > 0f;

    /// <summary>True while the rapid-fire power-up is active.</summary>
    public bool HasRapidFire => _rapidFireTimer > 0f;

    /// <summary>True while the speed-boost power-up is active.</summary>
    public bool HasSpeedBoost => _speedTimer > 0f;

    /// <summary>Current movement speed, faster while the speed boost is active.</summary>
    public float EffectiveSpeed => _settings.PlayerSpeed * (HasSpeedBoost ? SpeedBoostFactor : 1f);

    /// <summary>Current time between shots, shorter while rapid fire is active.</summary>
    public float EffectiveFireCooldown => _settings.PlayerFireCooldown * (HasRapidFire ? RapidFireFactor : 1f);

    /// <summary>True when enough time has passed to shoot again.</summary>
    public bool CanFire => _fireCooldownRemaining <= 0f;

    /// <summary>Moves the player in the given direction and keeps them inside the arena.</summary>
    public void Move(Vector2 inputDirection, float deltaSeconds)
    {
        Vector2 dir = MathUtils.Normalize(inputDirection);
        Position += dir * EffectiveSpeed * MathF.Max(0f, deltaSeconds);
        ClampToArena(_settings.ArenaWidth, _settings.ArenaHeight);
    }

    /// <summary>Points the player toward the given direction (ignored if it is zero).</summary>
    public void Aim(Vector2 direction)
    {
        Vector2 normalized = MathUtils.Normalize(direction);
        if (normalized != Vector2.Zero)
        {
            AimDirection = normalized;
        }
    }

    /// <summary>Starts the shooting cooldown after a shot is fired.</summary>
    public void RegisterShot() => _fireCooldownRemaining = EffectiveFireCooldown;

    /// <summary>Applies damage (unless shielded) and returns how much was removed.</summary>
    public int TakeDamage(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage cannot be negative.");
        }

        if (HasShield || amount == 0)
        {
            return 0;
        }

        int before = Health;
        Health = Math.Max(0, Health - amount);
        return before - Health;
    }

    /// <summary>Restores health without going over the maximum and returns how much was healed.</summary>
    public int Heal(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Heal amount cannot be negative.");
        }

        int before = Health;
        Health = Math.Min(MaxHealth, Health + amount);
        return Health - before;
    }

    /// <summary>Uses up a life and respawns with full health. Returns false when no lives remain.</summary>
    public bool ConsumeLifeAndRespawn()
    {
        if (Lives <= 0)
        {
            return false;
        }

        Lives--;
        if (Lives <= 0)
        {
            return false;
        }

        Health = MaxHealth;
        _shieldTimer = _rapidFireTimer = _speedTimer = 0f;
        Position = new Vector2(_settings.ArenaWidth * 0.5f, _settings.ArenaHeight * 0.5f);
        return true;
    }

    /// <summary>Applies a collected power-up (heals or starts a timed effect).</summary>
    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Health:
                Heal(_settings.HealthPickupAmount);
                break;
            case PowerUpType.Shield:
                _shieldTimer = _settings.PowerUpEffectDuration;
                break;
            case PowerUpType.RapidFire:
                _rapidFireTimer = _settings.PowerUpEffectDuration;
                break;
            case PowerUpType.SpeedBoost:
                _speedTimer = _settings.PowerUpEffectDuration;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown power-up type.");
        }
    }

    /// <summary>Counts down the shooting cooldown and the power-up timers each frame.</summary>
    public override void Update(float deltaSeconds)
    {
        float dt = MathF.Max(0f, deltaSeconds);
        _fireCooldownRemaining = MathF.Max(0f, _fireCooldownRemaining - dt);
        _shieldTimer = MathF.Max(0f, _shieldTimer - dt);
        _rapidFireTimer = MathF.Max(0f, _rapidFireTimer - dt);
        _speedTimer = MathF.Max(0f, _speedTimer - dt);
    }
}
