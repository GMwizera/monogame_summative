using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;
using ArenaDefender.Core.Mathematics;
using ArenaDefender.Core.Systems;
using Xunit;

namespace ArenaDefender.UnitTests;

/// <summary>Tests for enemy behaviour (movement, firing, damage) and the enemy factory.</summary>
public class EnemyTests
{
    private static Player PlayerAt(Vector2 position)
    {
        var player = new Player(new GameSettings());
        player.Position = position;
        return player;
    }

    [Fact]
    public void Chaser_TurnsToFaceThePlayer_OverTime()
    {
        var chaser = new ChaserEnemy(new Vector2(0f, 0f), maxHealth: 30, speed: 150f, contactDamage: 8, scoreValue: 10);
        Player player = PlayerAt(new Vector2(30f, -100f));

        for (int i = 0; i < 120; i++)
        {
            chaser.Think(player, 1f / 60f);
        }

        Vector2 toPlayer = MathUtils.Direction(chaser.Position, player.Position);
        float alignment = MathUtils.Dot(chaser.Facing, toPlayer);
        Assert.True(alignment > 0.99f, $"Chaser failed to align with player (dot = {alignment}).");
    }

    [Fact]
    public void Brute_MovesTowardThePlayer()
    {
        var brute = new BruteEnemy(new Vector2(100f, 0f), maxHealth: 120, speed: 70f, contactDamage: 22, scoreValue: 25);
        Player player = PlayerAt(new Vector2(0f, 0f));

        brute.Think(player, 0.1f);
        brute.Update(0.1f);

        Assert.True(brute.Position.X < 100f, "Brute did not advance toward the player.");
    }

    [Fact]
    public void Sniper_Fires_WhenPlayerIsInFieldOfViewAndRange()
    {
        var sniper = new SniperEnemy(new Vector2(0f, 0f), maxHealth: 55, speed: 95f, contactDamage: 6, scoreValue: 20, beamDamage: 14);
        Player player = PlayerAt(new Vector2(0f, 350f));

        EnemyAttack? attack = sniper.Think(player, 1f / 60f);

        Assert.NotNull(attack);
        Assert.Equal(14, attack!.Damage);
        Assert.True(sniper.IsTargetInSight);
    }

    [Fact]
    public void Sniper_DoesNotFire_WhenPlayerIsBehindIt()
    {
        var sniper = new SniperEnemy(new Vector2(0f, 0f), maxHealth: 55, speed: 95f, contactDamage: 6, scoreValue: 20, beamDamage: 14);
        Player player = PlayerAt(new Vector2(0f, -350f));

        EnemyAttack? attack = sniper.Think(player, 1f / 60f);

        Assert.Null(attack);
        Assert.False(sniper.IsTargetInSight);
    }

    [Fact]
    public void Enemy_TakeDamage_KillsAndDeactivates()
    {
        var chaser = new ChaserEnemy(new Vector2(0f, 0f), maxHealth: 30, speed: 150f, contactDamage: 8, scoreValue: 10);

        chaser.TakeDamage(30);

        Assert.Equal(0, chaser.Health);
        Assert.False(chaser.IsAlive);
        Assert.False(chaser.IsActive);
    }

    [Fact]
    public void Enemy_ConstructedWithInvalidStats_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ChaserEnemy(new Vector2(0f, 0f), maxHealth: 0, speed: 150f, contactDamage: 8, scoreValue: 10));
    }

    [Theory]
    [InlineData(EnemyKind.Chaser)]
    [InlineData(EnemyKind.Brute)]
    [InlineData(EnemyKind.Sniper)]
    public void Factory_CreatesRequestedKind(EnemyKind kind)
    {
        var factory = new EnemyFactory();
        Enemy enemy = factory.Create(kind, new Vector2(10f, 10f), difficultyMultiplier: 1f);
        Assert.Equal(kind, enemy.Kind);
    }

    [Fact]
    public void Factory_ScalesHealthWithDifficulty()
    {
        var factory = new EnemyFactory();
        Enemy easy = factory.Create(EnemyKind.Chaser, Vector2.Zero, difficultyMultiplier: 1f);
        Enemy hard = factory.Create(EnemyKind.Chaser, Vector2.Zero, difficultyMultiplier: 3f);

        Assert.True(hard.MaxHealth > easy.MaxHealth,
            $"Expected scaled health to grow (easy {easy.MaxHealth}, hard {hard.MaxHealth}).");
    }

    [Fact]
    public void Factory_RejectsMultiplierBelowOne()
    {
        var factory = new EnemyFactory();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => factory.Create(EnemyKind.Chaser, Vector2.Zero, difficultyMultiplier: 0.5f));
    }
}
