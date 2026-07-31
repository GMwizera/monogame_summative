using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// The heart of the game. It owns the player, enemies, projectiles and power-ups
/// and updates them every frame from a single <see cref="PlayerInput"/>. It knows
/// nothing about drawing, so the whole game can be run and tested without a window.
/// </summary>
public sealed class GameWorld
{
    private const float ProjectileDespawnMargin = 60f;

    private readonly GameSettings _settings;
    private readonly IEnemyFactory _factory;
    private readonly Random _random;

    private readonly List<Enemy> _enemies = new();
    private readonly List<Projectile> _projectiles = new();
    private readonly List<PowerUp> _powerUps = new();
    private readonly List<EnemyAttack> _beamsThisFrame = new();

    private readonly DifficultyManager _difficulty;
    private readonly EnemySpawner _spawner;
    private readonly ScoreManager _score;

    /// <summary>
    /// Creates a game world. The settings, enemy factory and random source can be
    /// supplied (mainly for tests) or left out to use sensible defaults.
    /// </summary>
    public GameWorld(GameSettings? settings = null, IEnemyFactory? factory = null, Random? random = null)
    {
        _settings = settings ?? new GameSettings();
        _factory = factory ?? new EnemyFactory();
        _random = random ?? new Random();

        _difficulty = new DifficultyManager(_settings);
        _spawner = new EnemySpawner(_factory, _settings, _random);
        _score = new ScoreManager(_settings);

        Player = new Player(_settings);
        State = GameState.Menu;
    }

    /// <summary>Which screen the game is on (menu, playing, game over).</summary>
    public GameState State { get; private set; }

    /// <summary>The player character.</summary>
    public Player Player { get; private set; }

    /// <summary>The tunable settings this world runs with.</summary>
    public GameSettings Settings => _settings;

    /// <summary>All active enemies (read-only view).</summary>
    public IReadOnlyList<Enemy> Enemies => _enemies;

    /// <summary>All active projectiles (read-only view).</summary>
    public IReadOnlyList<Projectile> Projectiles => _projectiles;

    /// <summary>All active power-ups (read-only view).</summary>
    public IReadOnlyList<PowerUp> PowerUps => _powerUps;

    /// <summary>Sniper beams fired this frame, used only for drawing them.</summary>
    public IReadOnlyList<EnemyAttack> BeamsThisFrame => _beamsThisFrame;

    /// <summary>Current run score.</summary>
    public int Score => _score.Score;

    /// <summary>Best score so far.</summary>
    public int HighScore => _score.HighScore;

    /// <summary>How long the current run has lasted, in seconds.</summary>
    public float ElapsedSeconds => _difficulty.ElapsedSeconds;

    /// <summary>The current difficulty multiplier.</summary>
    public float DifficultyMultiplier => _difficulty.DifficultyMultiplier;

    /// <summary>True if the player was hurt this frame (used for the damage flash).</summary>
    public bool PlayerTookDamageThisFrame { get; private set; }

    /// <summary>Advances the whole game by one frame based on the given input.</summary>
    public void Update(PlayerInput input, float deltaSeconds)
    {
        float dt = MathF.Max(0f, deltaSeconds);
        switch (State)
        {
            case GameState.Menu:
            case GameState.GameOver:
                if (input.Confirm)
                {
                    StartNewRun();
                }
                break;

            case GameState.Playing:
                UpdatePlaying(input, dt);
                break;
        }
    }

    /// <summary>Clears everything and starts a fresh run.</summary>
    public void StartNewRun()
    {
        _enemies.Clear();
        _projectiles.Clear();
        _powerUps.Clear();
        _beamsThisFrame.Clear();
        _difficulty.Reset();
        _spawner.Reset();
        _score.ResetForNewRun();
        Player = new Player(_settings);
        PlayerTookDamageThisFrame = false;
        State = GameState.Playing;
    }

    private void UpdatePlaying(PlayerInput input, float dt)
    {
        PlayerTookDamageThisFrame = false;
        _beamsThisFrame.Clear();

        _difficulty.Update(dt);

        Player.Aim(input.AimDirection);
        Player.Move(input.MoveDirection, dt);
        Player.Update(dt);
        if (input.Firing && Player.CanFire)
        {
            FireProjectile();
            Player.RegisterShot();
        }

        foreach (Enemy spawned in _spawner.Update(dt, _difficulty.DifficultyMultiplier, _difficulty.CurrentSpawnInterval))
        {
            _enemies.Add(spawned);
        }

        foreach (Enemy enemy in _enemies)
        {
            EnemyAttack? attack = enemy.Think(Player, dt);
            enemy.Update(dt);
            if (attack != null)
            {
                _beamsThisFrame.Add(attack);
                ApplyDamageToPlayer(attack.Damage);
            }
        }

        foreach (Projectile projectile in _projectiles)
        {
            projectile.Update(dt);
            if (IsOutsideArena(projectile.Position, ProjectileDespawnMargin))
            {
                projectile.IsActive = false;
            }
        }

        foreach (PowerUp powerUp in _powerUps)
        {
            powerUp.Update(dt);
        }

        ResolveProjectileHits();
        ResolvePlayerContact();
        ResolvePowerUpPickups();

        if (!Player.IsAlive)
        {
            if (!Player.ConsumeLifeAndRespawn())
            {
                State = GameState.GameOver;
            }
        }

        _score.AddSurvivalTime(dt);
        RemoveInactive();
    }

    private void FireProjectile()
    {
        Vector2 muzzle = Player.Position + Player.AimDirection * (Player.Radius + _settings.ProjectileRadius);
        _projectiles.Add(new Projectile(
            muzzle,
            Player.AimDirection,
            _settings.ProjectileSpeed,
            _settings.ProjectileRadius,
            _settings.ProjectileDamage,
            _settings.ProjectileLifetime));
    }

    private void ResolveProjectileHits()
    {
        foreach (Projectile projectile in _projectiles)
        {
            if (!projectile.IsActive)
            {
                continue;
            }

            foreach (Enemy enemy in _enemies)
            {
                if (!enemy.IsActive || !CollisionSystem.Overlap(projectile, enemy))
                {
                    continue;
                }

                projectile.IsActive = false;
                enemy.TakeDamage(projectile.Damage);
                if (!enemy.IsAlive)
                {
                    _score.AddEnemyDefeat(enemy.ScoreValue);
                    MaybeDropPowerUp(enemy.Position);
                }

                break;
            }
        }
    }

    private void ResolvePlayerContact()
    {
        foreach (Enemy enemy in _enemies)
        {
            if (!enemy.IsActive || !CollisionSystem.Overlap(Player, enemy))
            {
                continue;
            }

            ApplyDamageToPlayer(enemy.ContactDamage);
            enemy.IsActive = false;
        }
    }

    private void ResolvePowerUpPickups()
    {
        foreach (PowerUp powerUp in _powerUps)
        {
            if (powerUp.IsActive && CollisionSystem.Overlap(Player, powerUp))
            {
                Player.ApplyPowerUp(powerUp.Type);
                powerUp.IsActive = false;
            }
        }
    }

    private void ApplyDamageToPlayer(int amount)
    {
        int dealt = Player.TakeDamage(amount);
        if (dealt > 0)
        {
            PlayerTookDamageThisFrame = true;
        }
    }

    private void MaybeDropPowerUp(Vector2 position)
    {
        if (_random.NextDouble() > _settings.PowerUpDropChance)
        {
            return;
        }

        int typeCount = 4;
        PowerUpType type = (PowerUpType)_random.Next(typeCount);
        _powerUps.Add(new PowerUp(type, position, _settings.PowerUpRadius, _settings.PowerUpLifetime));
    }

    private bool IsOutsideArena(Vector2 position, float margin) =>
        position.X < -margin || position.X > _settings.ArenaWidth + margin ||
        position.Y < -margin || position.Y > _settings.ArenaHeight + margin;

    private void RemoveInactive()
    {
        _enemies.RemoveAll(e => !e.IsActive);
        _projectiles.RemoveAll(p => !p.IsActive);
        _powerUps.RemoveAll(p => !p.IsActive);
    }
}
