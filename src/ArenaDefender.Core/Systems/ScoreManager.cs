using ArenaDefender.Core.Configuration;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// Keeps the score for the current run and the best score so far. Points come
/// from defeating enemies and from surviving over time.
/// </summary>
public sealed class ScoreManager
{
    private readonly int _survivalPointsPerSecond;
    private float _survivalCarrySeconds;

    /// <summary>Creates a score manager using the survival rate from the settings.</summary>
    public ScoreManager(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _survivalPointsPerSecond = settings.SurvivalPointsPerSecond;
    }

    /// <summary>Score for the current run.</summary>
    public int Score { get; private set; }

    /// <summary>Highest score reached so far.</summary>
    public int HighScore { get; private set; }

    /// <summary>Adds the points for defeating an enemy.</summary>
    public void AddEnemyDefeat(int enemyScoreValue)
    {
        if (enemyScoreValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(enemyScoreValue), enemyScoreValue, "Score value cannot be negative.");
        }

        Score += enemyScoreValue;
        UpdateHighScore();
    }

    /// <summary>Adds survival points for the time that passed, one whole second at a time.</summary>
    public void AddSurvivalTime(float deltaSeconds)
    {
        _survivalCarrySeconds += MathF.Max(0f, deltaSeconds);
        while (_survivalCarrySeconds >= 1f)
        {
            _survivalCarrySeconds -= 1f;
            Score += _survivalPointsPerSecond;
        }

        UpdateHighScore();
    }

    /// <summary>Resets the score to zero for a new run (the high score is kept).</summary>
    public void ResetForNewRun()
    {
        Score = 0;
        _survivalCarrySeconds = 0f;
    }

    private void UpdateHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }
}
