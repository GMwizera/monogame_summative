// Entry point. Creates the game and runs it, with a safety net that logs any
// unexpected crash instead of letting the window vanish silently.
using ArenaDefender.Desktop;

try
{
    using var game = new ArenaGame();
    game.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("Arena Defender crashed unexpectedly.");
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
