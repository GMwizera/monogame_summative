# Arena Defender

A 2D wave-survival game built from scratch in **C# and MonoGame**. You control a
lone defender in a closed arena and must survive escalating waves of enemies for
as long as possible — shooting them down, grabbing power-ups, and racking up a
score while the difficulty steadily climbs.

> Module summative project: *Arena Defender — A 2D Survival Game with MonoGame*.

---

## Table of contents
- [Gameplay](#gameplay)
- [Controls](#controls)
- [How to run](#how-to-run)
- [Project structure](#project-structure)
- [Feature checklist](#feature-checklist)
- [Mathematics in the game](#mathematics-in-the-game)
- [Testing](#testing)
- [Design notes](#design-notes)

---

## Gameplay

- **Survive the waves.** Enemies spawn continuously from the edges of the arena
  and hunt you down. The longer you last, the harder it gets.
- **Three enemy types**, each with distinct behaviour:
  - **Chaser** — fast but fragile; *homes* in on you with smooth, steered turns.
  - **Brute** — slow, heavily armoured, and hits hard on contact; charges straight at you.
  - **Sniper** — keeps its distance and fires a beam, but only once you drift into
    its field of view, so you can break its aim by moving.
- **Shoot to score.** Destroying an enemy with a shot earns points. Letting an
  enemy *touch* you costs health and awards nothing — so keep your distance.
- **Power-ups** drop from defeated enemies: **Health**, **Shield**, **Rapid Fire**
  and **Speed Boost**.
- **Lives.** Losing all your health costs a life and revives you; run out of lives
  and it's game over.
- **Score** comes from defeating enemies *and* from surviving — a steady trickle
  of points per second.

## Controls

| Action | Keys |
| --- | --- |
| Move | **W A S D** or **Arrow keys** |
| Aim | **Mouse** |
| Fire | **Left mouse button** or **Space** |
| Start / Restart | **Enter** |
| Quit | **Esc** |

## How to run

**Prerequisites:** the [.NET SDK](https://dotnet.microsoft.com/download) **9.0 or
later**. Nothing else — the game generates all of its graphics at runtime, so
there is no MonoGame content pipeline to install or build.

```bash
# from the repository root
dotnet run --project src/ArenaDefender.Desktop
```

To build the whole solution or run the tests:

```bash
dotnet build ArenaDefender.sln          # build everything
dotnet test  ArenaDefender.sln          # run the unit tests
```

> The projects target **.NET 9**. This repository was developed on a machine that
> only had the **.NET 10** runtime installed, so the test project sets
> `RollForward=Major` to allow running on a newer runtime. On a machine with the
> .NET 9 runtime it simply runs on .NET 9.

## Project structure

```
monogame_summative/
├── ArenaDefender.sln
├── README.md
├── docs/
│   ├── Architecture.md            # architecture & design decisions
│   └── TestingStrategy.md         # testing approach, edge cases, results
├── src/
│   ├── ArenaDefender.Core/        # ALL game logic & maths — no MonoGame dependency
│   │   ├── Mathematics/           # MathUtils: distance, dot, cross, lerp, ...
│   │   ├── Entities/              # Player, Enemy hierarchy, Projectile, PowerUp
│   │   ├── Systems/               # spawning, difficulty, collision, scoring, GameWorld
│   │   └── Configuration/         # GameSettings (all tunable values)
│   └── ArenaDefender.Desktop/     # MonoGame layer: window, input, rendering
│       └── Rendering/             # Primitives, PixelFont, InputMapper
└── tests/
    └── ArenaDefender.UnitTests/   # xUnit tests (58 test cases)
```

The single most important design decision is that **all game logic lives in
`ArenaDefender.Core`, which does not reference MonoGame at all.** The desktop
project feeds it a per-frame `PlayerInput` and reads back state to draw. This is
what makes the rules fully unit-testable and cleanly separates *game logic* from
*UI logic*. See [docs/Architecture.md](docs/Architecture.md) for the full write-up.

## Feature checklist

| Requirement | Where |
| --- | --- |
| Player: movement, health, score, lives, graphics | `Entities/Player.cs`, `ArenaGame.cs` |
| ≥2 enemy types with different behaviour | `ChaserEnemy`, `BruteEnemy`, `SniperEnemy` |
| Continuous enemy spawning | `Systems/EnemySpawner.cs` |
| Projectile / attack system | `Entities/Projectile.cs`, `GameWorld.FireProjectile` |
| Collisions (player–enemy, projectile–enemy, player–power-up) | `Systems/CollisionSystem.cs`, `GameWorld` |
| ≥1 power-up (has 4) | `Entities/PowerUp.cs`, `Player.ApplyPowerUp` |
| Scoring + live display | `Systems/ScoreManager.cs`, HUD in `ArenaGame.cs` |
| UI: start screen, health, score, lives, game over | `ArenaGame.DrawMenu/DrawHud/DrawGameOver` |
| Increasing difficulty | `Systems/DifficultyManager.cs` |
| Exception handling | guards throughout Core; `try/catch` in `Program.cs` & `ArenaGame.Update` |
| Unit tests (≥10) | `tests/ArenaDefender.UnitTests` (58 cases) |
| XML documentation | `///` comments throughout `ArenaDefender.Core` |

## Mathematics in the game

Every mathematical concept required by the brief is used in a real gameplay
mechanic (implemented in `Mathematics/MathUtils.cs` and exercised by the systems):

| Concept | Where it is used |
| --- | --- |
| **Distance** | collision detection, sniper engagement range, power-up pickup range |
| **Direction & vectors** | player movement, enemy steering, projectile velocity |
| **Algebra** | health/damage, difficulty scaling, spawn-interval and score formulas |
| **Dot product** | the **Sniper's field-of-view**: it only fires when `Dot(facing, toPlayer) ≥ cos(halfFov)` |
| **Cross product** | the **Chaser's steering**: the *sign* of `Cross(facing, toPlayer)` decides whether to turn left or right |
| **Lerp (≥3)** | health-bar animation, damage-flash fade, game-over fade, difficulty colour shift |

## Testing

The `ArenaDefender.UnitTests` project uses **xUnit** and contains **58 test
cases** covering the mathematics, player rules, each enemy behaviour, the enemy
factory, difficulty scaling, scoring, collision geometry, spawning, and full
`GameWorld` integration. All tests pass:

```
Passed!  - Failed: 0, Passed: 58, Skipped: 0, Total: 58
```

Read the full approach in [docs/TestingStrategy.md](docs/TestingStrategy.md).

## Design notes

- **SOLID / OOP:** an `Entity` base class and an `IDamageable` interface provide
  abstraction; the three enemies use inheritance and polymorphism; `IEnemyFactory`
  applies dependency inversion; each system has a single responsibility.
- **Enums** (`GameState`, `EnemyKind`, `PowerUpType`) replace fragile magic values.
- **No content pipeline:** all shapes are drawn from a runtime-generated pixel and
  circle texture, and text from a hand-coded 5×7 bitmap font (`Rendering/PixelFont.cs`),
  so the game builds and runs anywhere the framework package restores.
