using System.Numerics;
using ArenaDefender.Core.Entities;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Circle-based collision helpers. Everything in the game is a circle, so two
/// objects touch when the distance between their centres is less than the sum of
/// their radii (compared using squared distance to avoid a square root).
/// </summary>
public static class CollisionSystem
{
    /// <summary>True when two circles overlap.</summary>
    public static bool CirclesOverlap(Vector2 centreA, float radiusA, Vector2 centreB, float radiusB)
    {
        if (radiusA < 0f) throw new ArgumentOutOfRangeException(nameof(radiusA), radiusA, "Radius cannot be negative.");
        if (radiusB < 0f) throw new ArgumentOutOfRangeException(nameof(radiusB), radiusB, "Radius cannot be negative.");

        float radiusSum = radiusA + radiusB;
        return MathUtils.DistanceSquared(centreA, centreB) <= radiusSum * radiusSum;
    }

    /// <summary>True when two entities' circular bodies overlap.</summary>
    public static bool Overlap(Entity a, Entity b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return CirclesOverlap(a.Position, a.Radius, b.Position, b.Radius);
    }

    /// <summary>True when a point is within a given range of a centre.</summary>
    public static bool WithinRange(Vector2 centre, Vector2 point, float range)
    {
        if (range < 0f) throw new ArgumentOutOfRangeException(nameof(range), range, "Range cannot be negative.");
        return MathUtils.DistanceSquared(centre, point) <= range * range;
    }
}
