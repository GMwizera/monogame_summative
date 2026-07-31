namespace ArenaDefender.Core.Entities;

/// <summary>The kinds of enemy in the game. Used by the factory, scoring and drawing.</summary>
public enum EnemyKind
{
    /// <summary>Fast, weak, curves toward the player.</summary>
    Chaser,

    /// <summary>Slow, tough, walks straight at the player.</summary>
    Brute,

    /// <summary>Ranged, keeps its distance and shoots a beam.</summary>
    Sniper
}
