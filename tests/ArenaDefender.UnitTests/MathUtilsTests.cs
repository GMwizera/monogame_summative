using System.Numerics;
using ArenaDefender.Core.Mathematics;
using Xunit;

namespace ArenaDefender.UnitTests;

/// <summary>Tests for the maths helpers: distance, dot product, cross product, lerp and clamping.</summary>
public class MathUtilsTests
{
    [Fact]
    public void Distance_ReturnsEuclideanDistance()
    {
        float d = MathUtils.Distance(new Vector2(0f, 0f), new Vector2(3f, 4f));
        Assert.Equal(5f, d, precision: 4);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero_WithoutNaN()
    {
        Vector2 result = MathUtils.Normalize(Vector2.Zero);
        Assert.Equal(Vector2.Zero, result);
        Assert.False(float.IsNaN(result.X));
    }

    [Fact]
    public void Normalize_ProducesUnitLength()
    {
        Vector2 result = MathUtils.Normalize(new Vector2(0f, 10f));
        Assert.Equal(1f, result.Length(), precision: 4);
    }

    [Fact]
    public void Dot_OfPerpendicularVectors_IsZero()
    {
        float dot = MathUtils.Dot(new Vector2(1f, 0f), new Vector2(0f, 1f));
        Assert.Equal(0f, dot, precision: 5);
    }

    [Fact]
    public void Dot_OfSameDirection_IsPositive_OppositeIsNegative()
    {
        Assert.True(MathUtils.Dot(new Vector2(1f, 0f), new Vector2(1f, 0f)) > 0f);
        Assert.True(MathUtils.Dot(new Vector2(1f, 0f), new Vector2(-1f, 0f)) < 0f);
    }

    [Fact]
    public void Cross_SignIndicatesTurnDirection()
    {
        Assert.True(MathUtils.Cross(new Vector2(1f, 0f), new Vector2(0f, 1f)) > 0f);
        Assert.True(MathUtils.Cross(new Vector2(1f, 0f), new Vector2(0f, -1f)) < 0f);
    }

    [Fact]
    public void Cross_OfParallelVectors_IsZero()
    {
        Assert.Equal(0f, MathUtils.Cross(new Vector2(2f, 4f), new Vector2(1f, 2f)), precision: 5);
    }

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(1f, 10f)]
    [InlineData(0.5f, 5f)]
    public void Lerp_ReturnsExpectedValueAcrossRange(float t, float expected)
    {
        Assert.Equal(expected, MathUtils.Lerp(0f, 10f, t), precision: 4);
    }

    [Theory]
    [InlineData(-1f, 0f)]
    [InlineData(2f, 10f)]
    public void Lerp_ClampsInterpolant(float t, float expected)
    {
        Assert.Equal(expected, MathUtils.Lerp(0f, 10f, t), precision: 4);
    }

    [Fact]
    public void Rotate_NinetyDegrees_MapsXAxisToYAxis()
    {
        Vector2 rotated = MathUtils.Rotate(new Vector2(1f, 0f), MathF.PI / 2f);
        Assert.Equal(0f, rotated.X, precision: 4);
        Assert.Equal(1f, rotated.Y, precision: 4);
    }

    [Fact]
    public void Clamp_ThrowsWhenMinExceedsMax()
    {
        Assert.Throws<ArgumentException>(() => MathUtils.Clamp(5f, 10f, 0f));
    }
}
