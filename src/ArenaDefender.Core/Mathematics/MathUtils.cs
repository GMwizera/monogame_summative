using System.Numerics;

namespace ArenaDefender.Core.Mathematics;

/// <summary>
/// Helper maths used across the game: distance, direction, dot product,
/// cross product and linear interpolation. Written by hand (instead of using the
/// built-in <see cref="Vector2"/> helpers) so the concepts are easy to see and test.
/// </summary>
public static class MathUtils
{
    /// <summary>Small tolerance used when comparing floating-point numbers.</summary>
    public const float Epsilon = 1e-6f;

    /// <summary>Straight-line distance between two points. Used for collisions and ranges.</summary>
    public static float Distance(Vector2 a, Vector2 b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>Distance without the square root. Cheaper when only comparing ranges.</summary>
    public static float DistanceSquared(Vector2 a, Vector2 b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>Returns a length-1 version of the vector, or zero if it is too short.</summary>
    public static Vector2 Normalize(Vector2 v)
    {
        float length = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        if (length < Epsilon)
        {
            return Vector2.Zero;
        }

        return new Vector2(v.X / length, v.Y / length);
    }

    /// <summary>Unit direction pointing from one point toward another.</summary>
    public static Vector2 Direction(Vector2 from, Vector2 to) => Normalize(to - from);

    /// <summary>Dot product. For unit vectors this is the cosine of the angle between them.</summary>
    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

    /// <summary>2D cross product. Its sign tells us whether to turn left or right.</summary>
    public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

    /// <summary>Linear interpolation between two numbers (t is clamped to 0..1).</summary>
    public static float Lerp(float start, float end, float t)
    {
        t = Clamp01(t);
        return start + (end - start) * t;
    }

    /// <summary>Linear interpolation between two vectors (t is clamped to 0..1).</summary>
    public static Vector2 Lerp(Vector2 start, Vector2 end, float t)
    {
        t = Clamp01(t);
        return new Vector2(
            start.X + (end.X - start.X) * t,
            start.Y + (end.Y - start.Y) * t);
    }

    /// <summary>Keeps a value within the given range.</summary>
    public static float Clamp(float value, float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException($"min ({min}) must not exceed max ({max}).", nameof(min));
        }

        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>Keeps a value within the range 0..1.</summary>
    public static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    /// <summary>Rotates a vector by an angle in radians.</summary>
    public static Vector2 Rotate(Vector2 v, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos);
    }
}
