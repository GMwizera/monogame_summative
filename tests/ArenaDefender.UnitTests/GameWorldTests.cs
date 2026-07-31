using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Systems;
using Xunit;

namespace ArenaDefender.UnitTests;

/// <summary>Integration tests that drive the whole game through its public surface, with no rendering.</summary>
public class GameWorldTests
{
    private static GameWorld NewWorld(int seed = 7) =>
        new(new GameSettings(), new EnemyFactory(), new Random(seed));

    private static PlayerInput Confirm => new(Vector2.Zero, new Vector2(0f, -1f), false, true);
    private static PlayerInput Idle => PlayerInput.None;

    [Fact]
    public void NewWorld_StartsInMenu()
    {
        var world = NewWorld();
        Assert.Equal(GameState.Menu, world.State);
    }

    [Fact]
    public void Confirm_FromMenu_StartsARun()
    {
        var world = NewWorld();

        world.Update(Confirm, 1f / 60f);

        Assert.Equal(GameState.Playing, world.State);
        Assert.Equal(world.Player.MaxHealth, world.Player.Health);
        Assert.Equal(0, world.Score);
        Assert.Empty(world.Enemies);
    }

    [Fact]
    public void Firing_CreatesAProjectile()
    {
        var world = NewWorld();
        world.Update(Confirm, 1f / 60f);

        var fire = new PlayerInput(Vector2.Zero, new Vector2(0f, -1f), true, false);
        world.Update(fire, 1f / 60f);

        Assert.Single(world.Projectiles);
    }

    [Fact]
    public void EnemiesSpawn_AsTimePasses()
    {
        var world = NewWorld();
        world.Update(Confirm, 1f / 60f);

        for (int i = 0; i < 240; i++)
        {
            world.Update(Idle, 1f / 60f);
        }

        Assert.NotEmpty(world.Enemies);
    }

    [Fact]
    public void Score_RisesFromSurvivalTime()
    {
        var world = NewWorld();
        world.Update(Confirm, 1f / 60f);

        for (int i = 0; i < 120; i++)
        {
            world.Update(Idle, 1f / 60f);
        }

        Assert.True(world.Score > 0, "Survival should award score over time.");
    }

    [Fact]
    public void Difficulty_RisesDuringPlay()
    {
        var world = NewWorld();
        world.Update(Confirm, 1f / 60f);
        float startMultiplier = world.DifficultyMultiplier;

        for (int i = 0; i < 600; i++)
        {
            world.Update(Idle, 1f / 60f);
        }

        Assert.True(world.DifficultyMultiplier > startMultiplier);
    }

    [Fact]
    public void RestartFromGameOver_ResetsScore()
    {
        var settings = new GameSettings { ArenaWidth = 80f, ArenaHeight = 80f, BaseSpawnInterval = 0.05f, MinSpawnInterval = 0.05f };
        var world = new GameWorld(settings, new EnemyFactory(), new Random(1));
        world.Update(Confirm, 1f / 60f);

        int guard = 0;
        while (world.State == GameState.Playing && guard < 100_000)
        {
            world.Update(Idle, 1f / 60f);
            guard++;
        }

        Assert.Equal(GameState.GameOver, world.State);

        world.Update(Confirm, 1f / 60f);
        Assert.Equal(GameState.Playing, world.State);
        Assert.Equal(0, world.Score);
    }
}
