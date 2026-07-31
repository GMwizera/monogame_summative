using System.Numerics;

namespace ArenaDefender.Core.Systems;

/// <summary>
/// One frame's worth of player input, passed from the desktop layer into the game
/// logic. This is the only thing the logic knows about the keyboard and mouse,
/// which keeps the rules separate from the input code.
/// </summary>
public class PlayerInput
{
    /// <summary>Creates an input snapshot from a move direction, aim direction and two buttons.</summary>
    public PlayerInput(Vector2 moveDirection, Vector2 aimDirection, bool firing, bool confirm)
    {
        MoveDirection = moveDirection;
        AimDirection = aimDirection;
        Firing = firing;
        Confirm = confirm;
    }

    /// <summary>Direction the player wants to move.</summary>
    public Vector2 MoveDirection { get; }

    /// <summary>Direction the player is aiming.</summary>
    public Vector2 AimDirection { get; }

    /// <summary>True while the fire button is held.</summary>
    public bool Firing { get; }

    /// <summary>True while the confirm/start button is held.</summary>
    public bool Confirm { get; }

    /// <summary>An input with no movement and no buttons pressed.</summary>
    public static PlayerInput None => new PlayerInput(Vector2.Zero, new Vector2(0f, -1f), false, false);
}
