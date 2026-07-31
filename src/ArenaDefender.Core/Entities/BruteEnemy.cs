using System.Numerics;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// Slow but tough enemy that walks straight at the player. High health and heavy
/// contact damage, but no ranged attack and no clever steering.
/// </summary>
public sealed class BruteEnemy : Enemy
{
    /// <summary>Collision radius for brutes.</summary>
    public const float BodyRadius = 26f;

    /// <summary>Creates a brute with the given (already difficulty-scaled) stats.</summary>
    public BruteEnemy(Vector2 position, int maxHealth, float speed, int contactDamage, int scoreValue)
        : base(EnemyKind.Brute, position, BodyRadius, maxHealth, speed, contactDamage, scoreValue)
    {
    }

    /// <summary>Heads straight toward the player. Never fires a ranged attack.</summary>
    public override EnemyAttack? Think(Player player, float deltaSeconds)
    {
        Vector2 toPlayer = MathUtils.Direction(Position, player.Position);
        if (toPlayer != Vector2.Zero)
        {
            Facing = toPlayer;
            Velocity = toPlayer * Speed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        return null;
    }
}
