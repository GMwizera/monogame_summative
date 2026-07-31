using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Mathematics;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Tracks how long the run has lasted and turns that into a rising difficulty
/// multiplier (tougher enemies) and a shrinking spawn interval (more enemies),
/// both kept within safe limits.
/// </summary>
public sealed class DifficultyManager
{
    private readonly GameSettings _settings;

    /// <summary>Creates a difficulty manager driven by the given settings.</summary>
    public DifficultyManager(GameSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>How long the current run has lasted, in seconds.</summary>
    public float ElapsedSeconds { get; private set; }

    /// <summary>The current difficulty multiplier, from 1 up to the configured maximum.</summary>
    public float DifficultyMultiplier =>
        MathUtils.Clamp(
            1f + _settings.DifficultyRampPerSecond * ElapsedSeconds,
            1f,
            _settings.MaxDifficultyMultiplier);

    /// <summary>The current time between spawns, shrinking over time toward the minimum.</summary>
    public float CurrentSpawnInterval =>
        MathUtils.Clamp(
            _settings.BaseSpawnInterval - _settings.SpawnRampPerSecond * ElapsedSeconds,
            _settings.MinSpawnInterval,
            _settings.BaseSpawnInterval);

    /// <summary>Adds elapsed time for this frame.</summary>
    public void Update(float deltaSeconds) => ElapsedSeconds += MathF.Max(0f, deltaSeconds);

    /// <summary>Resets the timer for a new run.</summary>
    public void Reset() => ElapsedSeconds = 0f;
}
