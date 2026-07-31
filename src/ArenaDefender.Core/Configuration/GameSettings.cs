namespace ArenaDefender.Core.Configuration;

/// <summary>
/// All the tunable numbers for the game in one place: arena size, player stats,
/// projectile stats, spawn rates, difficulty ramp and power-up values. Keeping
/// them here means the balance can be changed without touching the game logic.
/// </summary>
public sealed class GameSettings
{

    /// <summary>Arena width in pixels.</summary>
    public float ArenaWidth { get; set; } = 1280f;

    /// <summary>Arena height in pixels.</summary>
    public float ArenaHeight { get; set; } = 720f;

    /// <summary>Player collision radius in pixels.</summary>
    public float PlayerRadius { get; set; } = 18f;

    /// <summary>Player starting and maximum health.</summary>
    public int PlayerMaxHealth { get; set; } = 100;

    /// <summary>Player movement speed in pixels per second.</summary>
    public float PlayerSpeed { get; set; } = 260f;

    /// <summary>Seconds between shots.</summary>
    public float PlayerFireCooldown { get; set; } = 0.28f;

    /// <summary>Projectile speed in pixels per second.</summary>
    public float ProjectileSpeed { get; set; } = 620f;

    /// <summary>Projectile collision radius in pixels.</summary>
    public float ProjectileRadius { get; set; } = 6f;

    /// <summary>Damage a projectile deals to an enemy.</summary>
    public int ProjectileDamage { get; set; } = 34;

    /// <summary>How long a projectile lives before disappearing, in seconds.</summary>
    public float ProjectileLifetime { get; set; } = 1.6f;

    /// <summary>Starting time between enemy spawns, in seconds.</summary>
    public float BaseSpawnInterval { get; set; } = 1.6f;

    /// <summary>Fastest the game will ever spawn enemies, in seconds.</summary>
    public float MinSpawnInterval { get; set; } = 0.35f;

    /// <summary>How much the spawn interval shrinks each second.</summary>
    public float SpawnRampPerSecond { get; set; } = 0.02f;

    /// <summary>How much the difficulty multiplier grows each second.</summary>
    public float DifficultyRampPerSecond { get; set; } = 0.015f;

    /// <summary>Upper limit on the difficulty multiplier.</summary>
    public float MaxDifficultyMultiplier { get; set; } = 4.0f;

    /// <summary>Chance (0..1) that a defeated enemy drops a power-up.</summary>
    public float PowerUpDropChance { get; set; } = 0.22f;

    /// <summary>Power-up collision radius in pixels.</summary>
    public float PowerUpRadius { get; set; } = 14f;

    /// <summary>How long a dropped power-up stays before disappearing, in seconds.</summary>
    public float PowerUpLifetime { get; set; } = 8f;

    /// <summary>How long a timed power-up effect (shield, rapid fire, speed) lasts.</summary>
    public float PowerUpEffectDuration { get; set; } = 6f;

    /// <summary>Health restored by a health pickup.</summary>
    public int HealthPickupAmount { get; set; } = 30;

    /// <summary>Points awarded per second of survival.</summary>
    public int SurvivalPointsPerSecond { get; set; } = 5;
}
