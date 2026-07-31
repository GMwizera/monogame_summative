using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;
using Xunit;

namespace ArenaDefender.UnitTests;

/// <summary>Tests for the player: movement, health, damage, healing, lives and power-up effects.</summary>
public class PlayerTests
{
    private static GameSettings Settings() => new();

    [Fact]
    public void TakeDamage_ReducesHealth_AndReturnsAmountDealt()
    {
        var player = new Player(Settings());
        int dealt = player.TakeDamage(30);

        Assert.Equal(30, dealt);
        Assert.Equal(player.MaxHealth - 30, player.Health);
    }

    [Fact]
    public void TakeDamage_NeverDropsBelowZero()
    {
        var player = new Player(Settings());
        player.TakeDamage(10_000);

        Assert.Equal(0, player.Health);
        Assert.False(player.IsAlive);
    }

    [Fact]
    public void Heal_DoesNotExceedMaxHealth()
    {
        var player = new Player(Settings());
        player.TakeDamage(20);
        int restored = player.Heal(1000);

        Assert.Equal(20, restored);
        Assert.Equal(player.MaxHealth, player.Health);
    }

    [Fact]
    public void TakeDamage_NegativeAmount_Throws()
    {
        var player = new Player(Settings());
        Assert.Throws<ArgumentOutOfRangeException>(() => player.TakeDamage(-5));
    }

    [Fact]
    public void Shield_PowerUp_AbsorbsAllDamage()
    {
        var player = new Player(Settings());
        player.ApplyPowerUp(PowerUpType.Shield);

        int dealt = player.TakeDamage(50);

        Assert.Equal(0, dealt);
        Assert.Equal(player.MaxHealth, player.Health);
        Assert.True(player.HasShield);
    }

    [Fact]
    public void SpeedBoost_PowerUp_IncreasesEffectiveSpeed()
    {
        var player = new Player(Settings());
        float baseSpeed = player.EffectiveSpeed;

        player.ApplyPowerUp(PowerUpType.SpeedBoost);

        Assert.True(player.EffectiveSpeed > baseSpeed);
    }

    [Fact]
    public void RapidFire_PowerUp_ShortensCooldown()
    {
        var player = new Player(Settings());
        float baseCooldown = player.EffectiveFireCooldown;

        player.ApplyPowerUp(PowerUpType.RapidFire);

        Assert.True(player.EffectiveFireCooldown < baseCooldown);
    }

    [Fact]
    public void PowerUpEffects_ExpireAfterTheirDuration()
    {
        var settings = Settings();
        var player = new Player(settings);
        player.ApplyPowerUp(PowerUpType.Shield);

        player.Update(settings.PowerUpEffectDuration + 0.1f);

        Assert.False(player.HasShield);
    }

    [Fact]
    public void FireCooldown_GatesShooting()
    {
        var settings = Settings();
        var player = new Player(settings);

        Assert.True(player.CanFire);
        player.RegisterShot();
        Assert.False(player.CanFire);

        player.Update(settings.PlayerFireCooldown + 0.01f);
        Assert.True(player.CanFire);
    }

    [Fact]
    public void Move_ClampsPlayerInsideArena()
    {
        var settings = new GameSettings { ArenaWidth = 200f, ArenaHeight = 200f };
        var player = new Player(settings);

        player.Move(new Vector2(-1f, 0f), 100f);

        Assert.True(player.Position.X >= player.Radius);
    }

    [Fact]
    public void ConsumeLifeAndRespawn_RevivesUntilLivesRunOut()
    {
        var player = new Player(Settings(), lives: 2);
        player.TakeDamage(10_000);

        bool revived = player.ConsumeLifeAndRespawn();
        Assert.True(revived);
        Assert.Equal(1, player.Lives);
        Assert.True(player.IsAlive);

        player.TakeDamage(10_000);
        bool revivedAgain = player.ConsumeLifeAndRespawn();
        Assert.False(revivedAgain);
    }

    [Fact]
    public void Constructor_WithZeroLives_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Player(Settings(), lives: 0));
    }
}
