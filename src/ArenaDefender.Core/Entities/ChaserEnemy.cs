using System.Numerics;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// Fast, weak enemy that curves toward the player. It uses the cross product to
/// decide whether to turn left or right, and a limited turn rate so it swerves
/// instead of snapping straight at the player.
/// </summary>
public sealed class ChaserEnemy : Enemy
{
    /// <summary>Collision radius for chasers.</summary>
    public const float BodyRadius = 14f;

    private const float MaxTurnRate = 5.5f;

    /// <summary>Creates a chaser with the given (already difficulty-scaled) stats.</summary>
    public ChaserEnemy(Vector2 position, int maxHealth, float speed, int contactDamage, int scoreValue)
        : base(EnemyKind.Chaser, position, BodyRadius, maxHealth, speed, contactDamage, scoreValue)
    {
    }

    /// <summary>Turns toward the player and moves forward. Never fires a ranged attack.</summary>
    public override EnemyAttack? Think(Player player, float deltaSeconds)
    {
        float dt = MathF.Max(0f, deltaSeconds);
        Vector2 toPlayer = MathUtils.Direction(Position, player.Position);
        if (toPlayer == Vector2.Zero)
        {
            Velocity = Vector2.Zero;
            return null;
        }

        float alignment = MathUtils.Dot(Facing, toPlayer);
        float angleToTarget = MathF.Acos(MathUtils.Clamp(alignment, -1f, 1f));

        float turnSign = MathF.Sign(MathUtils.Cross(Facing, toPlayer));

        float step = MathF.Min(angleToTarget, MaxTurnRate * dt) * turnSign;
        Facing = MathUtils.Normalize(MathUtils.Rotate(Facing, step));

        Velocity = Facing * Speed;
        return null;
    }
}
