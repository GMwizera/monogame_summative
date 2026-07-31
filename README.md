# Arena Defender

A 2D wave-survival game built in C# with MonoGame. You control a defender in a
closed arena and survive escalating waves of enemies for as long as possible —
shooting them for points, grabbing power-ups, and staying alive as the difficulty
rises. Losing all your health costs a life; lose all your lives and it's game over.

## How to play

| Action | Keys |
| --- | --- |
| Move | **W A S D** or **Arrow keys** |
| Aim | **Mouse** |
| Fire | **Left mouse button** or **Space** |
| Start / Restart | **Enter** |
| Quit | **Esc** |

Shoot enemies to earn points and survive to earn more. Enemies come in three types
(a fast chaser, a slow brute, and a ranged sniper) and can drop power-ups: health,
shield, rapid fire, and speed boost. Avoid letting enemies touch you.

## How to run

**Prerequisite:** the [.NET SDK](https://dotnet.microsoft.com/download) **9.0 or
later**. Nothing else is needed. The game generates all its graphics at runtime, so
there is no MonoGame content pipeline to install.

**1. Clone the repository and enter the folder**

```bash
git clone https://github.com/GMwizera/monogame_summative.git
cd monogame_summative
```

**2. Run the game**

```bash
dotnet run --project src/ArenaDefender.Desktop
```

**3. Run the unit tests (optional)**

```bash
dotnet test
```

> The projects target **.NET 9**. If your machine only has a newer runtime (for
> example .NET 10) installed, prefix the commands in steps 2 and 3 with
> `DOTNET_ROLL_FORWARD=Major`, like this:
>
> ```bash
> DOTNET_ROLL_FORWARD=Major dotnet run --project src/ArenaDefender.Desktop
> ```

## Project structure

```
src/ArenaDefender.Core       game logic and mathematics (no MonoGame dependency)
src/ArenaDefender.Desktop    MonoGame layer: window, input, rendering
tests/ArenaDefender.UnitTests xUnit tests
```

All game logic lives in `ArenaDefender.Core`, which has no MonoGame reference — this
keeps the rules separate from the UI and makes them fully unit-testable.
