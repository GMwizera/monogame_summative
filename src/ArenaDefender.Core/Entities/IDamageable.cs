namespace ArenaDefender.Core.Entities;

/// <summary>
/// Anything that has health and can be damaged or healed. Both the player and
/// every enemy implement this, so the combat code can hurt them the same way.
/// </summary>
public interface IDamageable
{
    /// <summary>Current health. Never negative; zero means defeated.</summary>
    int Health { get; }

    /// <summary>The most health this entity can have.</summary>
    int MaxHealth { get; }

    /// <summary>True while health is above zero.</summary>
    bool IsAlive { get; }

    /// <summary>Applies damage and returns how much health was actually removed.</summary>
    int TakeDamage(int amount);

    /// <summary>Restores health (never above the maximum) and returns how much was healed.</summary>
    int Heal(int amount);
}
