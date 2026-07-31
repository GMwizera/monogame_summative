using System.Numerics;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Entities;

/// <summary>
/// Ranged enemy that keeps its distance and shoots a beam. It only fires when the
/// player is inside a narrow cone in front of it, tested with the dot product, and
/// it turns slowly so the player has a moment to dodge out of its line of sight.
/// </summary>
public sealed class SniperEnemy : Enemy
{
    /// <summary>Collision radius for snipers.</summary>
    public const float BodyRadius = 16f;

    private const float HalfFovRadians = 22f * (MathF.PI / 180f);
    private const float EngageRange = 460f;
    private const float PreferredMinRange = 280f;
    private const float TurnRate = 2.2f;
    private const float FireCooldownSeconds = 1.8f;

    private float _fireCooldownRemaining;

    /// <summary>Creates a sniper with the given (already difficulty-scaled) stats.</summary>
    public SniperEnemy(Vector2 position, int maxHealth, float speed, int contactDamage, int scoreValue, int beamDamage)
        : base(EnemyKind.Sniper, position, BodyRadius, maxHealth, speed, contactDamage, scoreValue)
    {
        if (beamDamage <= 0) throw new ArgumentOutOfRangeException(nameof(beamDamage), beamDamage, "Beam damage must be positive.");
        BeamDamage = beamDamage;
    }

    /// <summary>Damage dealt when a beam connects.</summary>
    public int BeamDamage { get; }

    /// <summary>True when the player is currently inside the firing cone (drives the aim line).</summary>
    public bool IsTargetInSight { get; private set; }

    /// <summary>Keeps its distance, aims toward the player, and fires when the player is in the cone.</summary>
    public override EnemyAttack? Think(Player player, float deltaSeconds)
    {
        float dt = MathF.Max(0f, deltaSeconds);
        _fireCooldownRemaining = MathF.Max(0f, _fireCooldownRemaining - dt);

        Vector2 toPlayer = player.Position - Position;
        float distance = toPlayer.Length();
        Vector2 dirToPlayer = MathUtils.Normalize(toPlayer);
        if (dirToPlayer == Vector2.Zero)
        {
            Velocity = Vector2.Zero;
            IsTargetInSight = false;
            return null;
        }

        float angleToTarget = MathF.Acos(MathUtils.Clamp(MathUtils.Dot(Facing, dirToPlayer), -1f, 1f));
        float turnSign = MathF.Sign(MathUtils.Cross(Facing, dirToPlayer));
        float step = MathF.Min(angleToTarget, TurnRate * dt) * turnSign;
        Facing = MathUtils.Normalize(MathUtils.Rotate(Facing, step));

        if (distance > EngageRange)
        {
            Velocity = dirToPlayer * Speed;
        }
        else if (distance < PreferredMinRange)
        {
            Velocity = -dirToPlayer * Speed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        bool inRange = distance <= EngageRange;
        bool inCone = MathUtils.Dot(Facing, dirToPlayer) >= MathF.Cos(HalfFovRadians);
        IsTargetInSight = inRange && inCone;

        if (IsTargetInSight && _fireCooldownRemaining <= 0f)
        {
            _fireCooldownRemaining = FireCooldownSeconds;
            return new EnemyAttack(BeamDamage, Position, player.Position);
        }

        return null;
    }
}
