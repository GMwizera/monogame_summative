namespace ArenaDefender.Core.Entities;

/// <summary>The kinds of power-up the player can pick up.</summary>
public enum PowerUpType
{
    /// <summary>Restores some health.</summary>
    Health,

    /// <summary>Blocks all damage for a short time.</summary>
    Shield,

    /// <summary>Shortens the time between shots for a short time.</summary>
    RapidFire,

    /// <summary>Increases movement speed for a short time.</summary>
    SpeedBoost
}
