using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;
using ArenaDefender.Core.Mathematics;
using ArenaDefender.Core.Systems;
using ArenaDefender.Desktop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NVector2 = System.Numerics.Vector2;

namespace ArenaDefender.Desktop;

/// <summary>
/// The MonoGame entry class. It owns the window and game loop: it feeds input into
/// the <see cref="GameWorld"/>, then draws whatever state the world reports. All
/// the game rules live in the Core library; this class only handles input and drawing.
/// </summary>
public sealed class ArenaGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly GameSettings _settings = new();

    private SpriteBatch _spriteBatch = null!;
    private Primitives _primitives = null!;
    private PixelFont _font = null!;
    private GameWorld _world = null!;

    private float _displayedHealth;
    private float _damageFlash;
    private float _gameOverFade;
    private float _titleTime;

    private static readonly Color Background = new(16, 18, 28);
    private static readonly Color BackgroundDanger = new(38, 16, 20);
    private static readonly Color PlayerColor = new(80, 220, 210);
    private static readonly Color ChaserColor = new(240, 140, 60);
    private static readonly Color BruteColor = new(210, 70, 70);
    private static readonly Color SniperColor = new(190, 110, 230);
    private static readonly Color ProjectileColor = new(250, 240, 140);
    private static readonly Color BeamColor = new(255, 90, 90);
    private static readonly Color HudColor = new(230, 235, 245);
    private static readonly Color HudDim = new(120, 130, 150);

    /// <summary>Sets up the window size and title.</summary>
    public ArenaGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = (int)_settings.ArenaWidth,
            PreferredBackBufferHeight = (int)_settings.ArenaHeight
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Arena Defender";
    }

    /// <summary>One-time setup before the game starts.</summary>
    protected override void Initialize()
    {
        Window.AllowUserResizing = false;
        base.Initialize();
    }

    /// <summary>Creates the drawing helpers and the game world.</summary>
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _primitives = new Primitives(GraphicsDevice);
        _font = new PixelFont(_primitives);
        _world = new GameWorld(_settings);
        _displayedHealth = _world.Player.Health;
    }

    /// <summary>Reads input, advances the game world, and updates the on-screen effects.</summary>
    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        try
        {
            MouseState mouse = Mouse.GetState();
            PlayerInput input = InputMapper.Map(keyboard, mouse, _world.Player.Position);
            _world.Update(input, dt);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ArenaDefender] Update error: {ex}");
        }

        UpdatePresentation(dt);
        base.Update(gameTime);
    }

    // Smoothly animates the health bar, damage flash and game-over fade using Lerp.
    private void UpdatePresentation(float dt)
    {
        _displayedHealth = MathUtils.Lerp(_displayedHealth, _world.Player.Health, dt * 6f);

        if (_world.PlayerTookDamageThisFrame)
        {
            _damageFlash = 1f;
        }
        _damageFlash = MathUtils.Lerp(_damageFlash, 0f, dt * 4f);

        float fadeTarget = _world.State == GameState.GameOver ? 1f : 0f;
        _gameOverFade = MathUtils.Lerp(_gameOverFade, fadeTarget, dt * 3f);

        _titleTime += dt;
    }

    /// <summary>Draws the current screen: menu, the playing arena, or game over.</summary>
    protected override void Draw(GameTime gameTime)
    {
        float dangerT = (_world.DifficultyMultiplier - 1f) / (_settings.MaxDifficultyMultiplier - 1f);
        GraphicsDevice.Clear(Color.Lerp(Background, BackgroundDanger, MathUtils.Clamp01(dangerT)));

        _spriteBatch.Begin();
        switch (_world.State)
        {
            case GameState.Menu:
                DrawMenu();
                break;
            case GameState.Playing:
                DrawArena();
                DrawHud();
                break;
            case GameState.GameOver:
                DrawArena();
                DrawHud();
                DrawGameOver();
                break;
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Draws the power-ups, enemy beams, enemies, projectiles and the player.
    private void DrawArena()
    {
        foreach (PowerUp powerUp in _world.PowerUps)
        {
            float alpha = MathUtils.Clamp01(powerUp.LifetimeFraction + 0.2f);
            _primitives.FillCircle(_spriteBatch, ToXna(powerUp.Position), powerUp.Radius, PowerUpColor(powerUp.Type) * alpha);
        }

        foreach (EnemyAttack beam in _world.BeamsThisFrame)
        {
            _primitives.DrawLine(_spriteBatch, ToXna(beam.Origin), ToXna(beam.Target), 3f, BeamColor);
        }

        foreach (Enemy enemy in _world.Enemies)
        {
            Color color = Color.White;
            if (enemy.Kind == EnemyKind.Chaser) color = ChaserColor;
            else if (enemy.Kind == EnemyKind.Brute) color = BruteColor;
            else if (enemy.Kind == EnemyKind.Sniper) color = SniperColor;

            _primitives.FillCircle(_spriteBatch, ToXna(enemy.Position), enemy.Radius, color);

            if (enemy is SniperEnemy sniper && sniper.IsTargetInSight)
            {
                NVector2 tip = sniper.Position + sniper.Facing * 520f;
                _primitives.DrawLine(_spriteBatch, ToXna(sniper.Position), ToXna(tip), 1f, BeamColor * 0.5f);
            }
        }

        foreach (Projectile projectile in _world.Projectiles)
        {
            _primitives.FillCircle(_spriteBatch, ToXna(projectile.Position), projectile.Radius, ProjectileColor);
        }

        DrawPlayer();

        if (_damageFlash > 0.01f)
        {
            _primitives.FillRectangle(_spriteBatch, GraphicsDevice.Viewport.Bounds, new Color(255, 40, 40) * (_damageFlash * 0.35f));
        }
    }

    // Draws the player circle, its aim line and the shield ring when active.
    private void DrawPlayer()
    {
        Player player = _world.Player;
        _primitives.FillCircle(_spriteBatch, ToXna(player.Position), player.Radius, PlayerColor);

        NVector2 muzzle = player.Position + player.AimDirection * (player.Radius + 12f);
        _primitives.DrawLine(_spriteBatch, ToXna(player.Position), ToXna(muzzle), 3f, HudColor);

        if (player.HasShield)
        {
            float pulse = 0.6f + 0.4f * MathF.Sin(_titleTime * 8f);
            _primitives.FillCircle(_spriteBatch, ToXna(player.Position), player.Radius + 6f, new Color(90, 160, 255) * 0.25f * pulse);
        }
    }

    // Draws the heads-up display: health bar, lives, active buffs, score, time and danger.
    private void DrawHud()
    {
        Player player = _world.Player;

        const int barX = 24, barY = 24, barW = 260, barH = 22;
        _primitives.FillRectangle(_spriteBatch, new Rectangle(barX, barY, barW, barH), new Color(40, 44, 58));
        float healthFraction = MathUtils.Clamp01(_displayedHealth / player.MaxHealth);
        Color healthColor = Color.Lerp(new Color(220, 60, 60), new Color(80, 210, 120), healthFraction);
        _primitives.FillRectangle(_spriteBatch, new Rectangle(barX, barY, (int)(barW * healthFraction), barH), healthColor);
        _primitives.DrawRectangleOutline(_spriteBatch, new Rectangle(barX, barY, barW, barH), 2, HudDim);
        _font.Draw(_spriteBatch, $"HP {Math.Max(0, player.Health)}/{player.MaxHealth}", new Vector2(barX + 6, barY + 4), HudColor, 2);

        _font.Draw(_spriteBatch, $"LIVES X{player.Lives}", new Vector2(barX, barY + 34), HudColor, 2);

        string buffs = "";
        if (player.HasShield) buffs += "SHIELD ";
        if (player.HasRapidFire) buffs += "RAPID ";
        if (player.HasSpeedBoost) buffs += "SPEED ";
        if (buffs.Length > 0)
        {
            _font.Draw(_spriteBatch, buffs.TrimEnd(), new Vector2(barX, barY + 58), new Color(120, 200, 255), 2);
        }

        DrawRightAligned($"SCORE {_world.Score}", 24, HudColor, 3);
        DrawRightAligned($"BEST {_world.HighScore}", 58, HudDim, 2);

        int seconds = (int)_world.ElapsedSeconds;
        _font.DrawCentered(_spriteBatch, $"TIME {seconds}S", _settings.ArenaWidth / 2f, 24, HudColor, 2);
        _font.DrawCentered(_spriteBatch, $"DANGER X{_world.DifficultyMultiplier:0.0}", _settings.ArenaWidth / 2f, 46, HudDim, 2);
    }

    private void DrawRightAligned(string text, float y, Color color, int scale)
    {
        Microsoft.Xna.Framework.Vector2 size = _font.Measure(text, scale);
        _font.Draw(_spriteBatch, text, new Vector2(_settings.ArenaWidth - size.X - 24, y), color, scale);
    }

    // Draws the title screen with the game name and the controls.
    private void DrawMenu()
    {
        float cx = _settings.ArenaWidth / 2f;
        float pulse = 0.7f + 0.3f * MathF.Sin(_titleTime * 3f);

        _font.DrawCentered(_spriteBatch, "ARENA DEFENDER", cx, 180, HudColor, 8);
        _font.DrawCentered(_spriteBatch, "SURVIVE THE WAVES", cx, 300, new Color(120, 200, 255), 3);
        _font.DrawCentered(_spriteBatch, "PRESS ENTER TO PLAY", cx, 400, HudColor * pulse, 4);

        _font.DrawCentered(_spriteBatch, "MOVE WASD OR ARROWS    AIM MOUSE    FIRE CLICK OR SPACE", cx, 500, HudDim, 2);
        _font.DrawCentered(_spriteBatch, "SHOOT ENEMIES FOR POINTS    GRAB POWER-UPS    AVOID CONTACT", cx, 530, HudDim, 2);
        _font.DrawCentered(_spriteBatch, "ESC TO QUIT", cx, 560, HudDim, 2);
    }

    // Draws the game-over overlay with the final score.
    private void DrawGameOver()
    {
        _primitives.FillRectangle(_spriteBatch, GraphicsDevice.Viewport.Bounds, Color.Black * (_gameOverFade * 0.7f));

        float cx = _settings.ArenaWidth / 2f;
        Color textColor = HudColor * MathUtils.Clamp01(_gameOverFade);
        _font.DrawCentered(_spriteBatch, "GAME OVER", cx, 220, new Color(240, 90, 90) * MathUtils.Clamp01(_gameOverFade), 8);
        _font.DrawCentered(_spriteBatch, $"SCORE {_world.Score}", cx, 340, textColor, 4);
        _font.DrawCentered(_spriteBatch, $"BEST {_world.HighScore}", cx, 390, textColor, 3);
        _font.DrawCentered(_spriteBatch, $"YOU SURVIVED {(int)_world.ElapsedSeconds} SECONDS", cx, 440, textColor, 2);
        _font.DrawCentered(_spriteBatch, "PRESS ENTER TO PLAY AGAIN", cx, 500, textColor, 3);
    }

    private static Color PowerUpColor(PowerUpType type)
    {
        if (type == PowerUpType.Health) return new Color(90, 220, 120);
        if (type == PowerUpType.Shield) return new Color(90, 160, 255);
        if (type == PowerUpType.RapidFire) return new Color(255, 170, 70);
        if (type == PowerUpType.SpeedBoost) return new Color(90, 230, 230);
        return Color.White;
    }

    private static Microsoft.Xna.Framework.Vector2 ToXna(NVector2 v)
    {
        return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
    }

    /// <summary>Releases the drawing resources when the game closes.</summary>
    protected override void UnloadContent()
    {
        _primitives.Dispose();
        _spriteBatch.Dispose();
        base.UnloadContent();
    }
}
