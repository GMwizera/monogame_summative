namespace ArenaDefender.Core.Systems;

/// <summary>The screen the game is currently on. Drives the menu / play / game-over flow.</summary>
public enum GameState
{
    /// <summary>Title screen, waiting for the player to start.</summary>
    Menu,

    /// <summary>A run is in progress.</summary>
    Playing,

    /// <summary>The run has ended, showing the final score.</summary>
    GameOver
}
