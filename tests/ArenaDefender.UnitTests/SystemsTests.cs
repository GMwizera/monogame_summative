using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;
using ArenaDefender.Core.Systems;
using Xunit;

namespace ArenaDefender.UnitTests;

/// <summary>Tests for the game systems: collisions, difficulty scaling, scoring and spawning.</summary>
public class SystemsTests
{

    [Fact]
    public void Difficulty_MultiplierIncreasesWithTime()
    {
        var difficulty = new DifficultyManager(new GameSettings());
        float start = difficulty.DifficultyMultiplier;

        difficulty.Update(30f);

        Assert.True(difficulty.DifficultyMultiplier > start);
    }

    [Fact]
    public void Difficulty_MultiplierIsClampedToMaximum()
    {
        var settings = new GameSettings { MaxDifficultyMultiplier = 2f };
        var difficulty = new DifficultyManager(settings);

        difficulty.Update(100_000f);

        Assert.Equal(2f, difficulty.DifficultyMultiplier, precision: 4);
    }

    [Fact]
    public void Difficulty_SpawnIntervalShrinks_ButNeverBelowMinimum()
    {
        var settings = new GameSettings();
        var difficulty = new DifficultyManager(settings);
        float initialInterval = difficulty.CurrentSpawnInterval;

        difficulty.Update(100_000f);

        Assert.True(difficulty.CurrentSpawnInterval < initialInterval);
        Assert.True(difficulty.CurrentSpawnInterval >= settings.MinSpawnInterval);
    }

    [Fact]
    public void Score_AddEnemyDefeat_AccumulatesAndTracksHighScore()
    {
        var score = new ScoreManager(new GameSettings());

        score.AddEnemyDefeat(10);
        score.AddEnemyDefeat(25);

        Assert.Equal(35, score.Score);
        Assert.Equal(35, score.HighScore);
    }

    [Fact]
    public void Score_SurvivalTime_AwardsPointsPerWholeSecond()
    {
        var settings = new GameSettings { SurvivalPointsPerSecond = 5 };
        var score = new ScoreManager(settings);

        score.AddSurvivalTime(2.5f);

        Assert.Equal(10, score.Score);
    }

    [Fact]
    public void Score_ResetForNewRun_KeepsHighScore()
    {
        var score = new ScoreManager(new GameSettings());
        score.AddEnemyDefeat(40);

        score.ResetForNewRun();

        Assert.Equal(0, score.Score);
        Assert.Equal(40, score.HighScore);
    }

    [Fact]
    public void Score_NegativeEnemyValue_Throws()
    {
        var score = new ScoreManager(new GameSettings());
        Assert.Throws<ArgumentOutOfRangeException>(() => score.AddEnemyDefeat(-1));
    }

    [Fact]
    public void Collision_OverlappingCircles_ReturnTrue()
    {
        bool overlap = CollisionSystem.CirclesOverlap(new Vector2(0f, 0f), 10f, new Vector2(15f, 0f), 10f);
        Assert.True(overlap);
    }

    [Fact]
    public void Collision_SeparatedCircles_ReturnFalse()
    {
        bool overlap = CollisionSystem.CirclesOverlap(new Vector2(0f, 0f), 5f, new Vector2(100f, 0f), 5f);
        Assert.False(overlap);
    }

    [Fact]
    public void Collision_ExactlyTouchingCircles_ReturnTrue()
    {
        bool overlap = CollisionSystem.CirclesOverlap(new Vector2(0f, 0f), 10f, new Vector2(20f, 0f), 10f);
        Assert.True(overlap);
    }

    [Fact]
    public void Collision_Overlap_NullEntity_Throws()
    {
        var player = new Player(new GameSettings());
        Assert.Throws<ArgumentNullException>(() => CollisionSystem.Overlap(player, null!));
    }

    [Fact]
    public void Spawner_DoesNotSpawnBeforeIntervalElapses()
    {
        var settings = new GameSettings();
        var spawner = new EnemySpawner(new EnemyFactory(), settings, new Random(1));

        IReadOnlyList<Enemy> spawned = spawner.Update(0.1f, 1f, settings.BaseSpawnInterval);

        Assert.Empty(spawned);
    }

    [Fact]
    public void Spawner_SpawnsAfterIntervalElapses()
    {
        var settings = new GameSettings();
        var spawner = new EnemySpawner(new EnemyFactory(), settings, new Random(1));

        IReadOnlyList<Enemy> spawned = spawner.Update(settings.BaseSpawnInterval + 0.01f, 1f, settings.BaseSpawnInterval);

        Assert.NotEmpty(spawned);
    }

    [Fact]
    public void Spawner_SpawnOne_PlacesEnemyOnAnArenaEdge()
    {
        var settings = new GameSettings();
        var spawner = new EnemySpawner(new EnemyFactory(), settings, new Random(42));

        Enemy enemy = spawner.SpawnOne(1f);

        bool onEdge =
            enemy.Position.X < 0f || enemy.Position.X > settings.ArenaWidth ||
            enemy.Position.Y < 0f || enemy.Position.Y > settings.ArenaHeight;
        Assert.True(onEdge, $"Enemy spawned at {enemy.Position}, expected outside the arena bounds.");
    }
}
