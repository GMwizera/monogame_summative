using System.Numerics;
using ArenaDefender.Core.Configuration;
using ArenaDefender.Core.Entities;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Decides when new enemies appear, which kind they are, and where on the arena
/// edge they spawn. Tougher enemy types become more common as the difficulty
/// rises. A random number generator can be passed in so tests are repeatable.
/// </summary>
public sealed class EnemySpawner
{
    private const float EdgeMargin = 30f;
    private const int MaxSpawnsPerFrame = 8;

    private readonly IEnemyFactory _factory;
    private readonly GameSettings _settings;
    private readonly Random _random;

    private float _timeUntilNextSpawn;

    /// <summary>Creates a spawner using the given factory, settings and random source.</summary>
    public EnemySpawner(IEnemyFactory factory, GameSettings settings, Random? random = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _random = random ?? new Random();
        _timeUntilNextSpawn = settings.BaseSpawnInterval;
    }

    /// <summary>Seconds left until the next enemy spawns.</summary>
    public float TimeUntilNextSpawn => _timeUntilNextSpawn;

    /// <summary>Counts down the timer and returns any enemies that spawned this frame.</summary>
    public IReadOnlyList<Enemy> Update(float deltaSeconds, float difficultyMultiplier, float spawnInterval)
    {
        if (spawnInterval <= 0f)
        {
            spawnInterval = _settings.MinSpawnInterval;
        }

        var spawned = new List<Enemy>();
        _timeUntilNextSpawn -= MathF.Max(0f, deltaSeconds);

        int guard = 0;
        while (_timeUntilNextSpawn <= 0f && guard < MaxSpawnsPerFrame)
        {
            spawned.Add(SpawnOne(difficultyMultiplier));
            _timeUntilNextSpawn += spawnInterval;
            guard++;
        }

        if (_timeUntilNextSpawn < 0f)
        {
            _timeUntilNextSpawn = spawnInterval;
        }

        return spawned;
    }

    /// <summary>Creates a single enemy of a randomly chosen kind at a random edge position.</summary>
    public Enemy SpawnOne(float difficultyMultiplier)
    {
        EnemyKind kind = ChooseKind(difficultyMultiplier);
        Vector2 position = RandomEdgePosition();
        return _factory.Create(kind, position, difficultyMultiplier);
    }

    /// <summary>Resets the spawn timer for a new run.</summary>
    public void Reset() => _timeUntilNextSpawn = _settings.BaseSpawnInterval;

    private EnemyKind ChooseKind(float mult)
    {
        float chaser = 60f;
        float brute = 15f + 15f * (mult - 1f);
        float sniper = 10f + 12f * (mult - 1f);
        float total = chaser + brute + sniper;

        float roll = (float)_random.NextDouble() * total;
        if (roll < chaser) return EnemyKind.Chaser;
        if (roll < chaser + brute) return EnemyKind.Brute;
        return EnemyKind.Sniper;
    }

    private Vector2 RandomEdgePosition()
    {
        float w = _settings.ArenaWidth;
        float h = _settings.ArenaHeight;
        int edge = _random.Next(4);

        if (edge == 0)
        {
            return new Vector2((float)_random.NextDouble() * w, -EdgeMargin);
        }
        if (edge == 1)
        {
            return new Vector2((float)_random.NextDouble() * w, h + EdgeMargin);
        }
        if (edge == 2)
        {
            return new Vector2(-EdgeMargin, (float)_random.NextDouble() * h);
        }
        return new Vector2(w + EdgeMargin, (float)_random.NextDouble() * h);
    }
}
