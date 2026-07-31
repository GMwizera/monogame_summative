using System.Numerics;
using ArenaDefender.Core.Entities;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Builds enemies of each kind and scales their stats up with the difficulty.
/// Keeping enemy creation in one place means the rest of the game does not need
/// to know how each enemy type is put together.
/// </summary>
public class EnemyFactory : IEnemyFactory
{
    /// <summary>Creates an enemy of the requested kind with difficulty-scaled stats.</summary>
    public Enemy Create(EnemyKind kind, Vector2 position, float difficultyMultiplier)
    {
        if (difficultyMultiplier < 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(difficultyMultiplier), difficultyMultiplier, "Difficulty multiplier cannot be below 1.");
        }

        float mult = difficultyMultiplier;

        switch (kind)
        {
            case EnemyKind.Chaser:
                return new ChaserEnemy(position, ScaleHealth(30, mult), ScaleSpeed(165f, mult), ScaleDamage(8, mult), scoreValue: 10);

            case EnemyKind.Brute:
                return new BruteEnemy(position, ScaleHealth(120, mult), ScaleSpeed(70f, mult), ScaleDamage(22, mult), scoreValue: 25);

            case EnemyKind.Sniper:
                return new SniperEnemy(position, ScaleHealth(55, mult), ScaleSpeed(95f, mult), ScaleDamage(6, mult), scoreValue: 20, beamDamage: ScaleDamage(14, mult));

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown enemy kind.");
        }
    }

    private static int ScaleHealth(int baseHealth, float mult)
    {
        return Math.Max(1, (int)MathF.Round(baseHealth * mult));
    }

    private static float ScaleSpeed(float baseSpeed, float mult)
    {
        return baseSpeed * (1f + 0.20f * (mult - 1f));
    }

    private static int ScaleDamage(int baseDamage, float mult)
    {
        return Math.Max(1, (int)MathF.Round(baseDamage * (1f + 0.25f * (mult - 1f))));
    }
}
