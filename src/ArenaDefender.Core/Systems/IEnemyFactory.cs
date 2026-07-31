using System.Numerics;
using ArenaDefender.Core.Entities;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Creates enemies. Having an interface lets the game and the tests supply their
/// own version (for example a predictable factory in unit tests).
/// </summary>
public interface IEnemyFactory
{
    /// <summary>Creates an enemy of the given kind, scaled by the current difficulty.</summary>
    Enemy Create(EnemyKind kind, Vector2 position, float difficultyMultiplier);
}
