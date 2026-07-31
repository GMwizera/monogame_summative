# Arena Defender — Architecture & Design Document

This document explains how Arena Defender is put together, the reasoning behind
the main decisions, and how the code applies object-oriented programming and
SOLID principles.

---

## 1. High-level idea

Arena Defender is a 2D wave-survival game. The player defends a fixed arena
against an endless, intensifying stream of enemies, scoring by shooting enemies
and by staying alive. The whole program is organised around one central idea:

> **The rules of the game are completely separated from how the game is drawn and
> controlled.**

Everything that decides *what happens* lives in a plain C# library with no
knowledge of MonoGame. MonoGame is only used at the very edge, to open a window,
read the keyboard/mouse, and paint the current state on screen.

## 2. Solution layout

The solution has three projects:

```
ArenaDefender.Core        (class library)   -> game logic + mathematics
ArenaDefender.Desktop     (MonoGame exe)     -> window, input, rendering
ArenaDefender.UnitTests   (xUnit)            -> tests, references Core only
```

Dependencies flow in one direction only:

```
        ┌─────────────────────────┐
        │  ArenaDefender.Desktop   │  (MonoGame: input + rendering)
        └───────────┬─────────────┘
                    │ references
                    ▼
        ┌─────────────────────────┐        ┌──────────────────────────┐
        │   ArenaDefender.Core     │◄───────│ ArenaDefender.UnitTests  │
        │  (no MonoGame reference) │references└──────────────────────────┘
        └─────────────────────────┘
```

Because `Core` has no MonoGame dependency, the tests can create a `GameWorld`,
push input into it, and assert on the result — no graphics device, no window.

## 3. The core loop and data flow

Each frame the desktop layer does three things:

1. **Read input** and translate it into an engine-agnostic `PlayerInput`
   (`Rendering/InputMapper.cs`).
2. **Advance the simulation**: `GameWorld.Update(input, deltaTime)`.
3. **Draw** whatever the world now reports (player, enemies, projectiles,
   power-ups, HUD).

```
Keyboard/Mouse ──► InputMapper ──► PlayerInput ──► GameWorld.Update ──► world state ──► Renderer
        (Desktop)                    (struct)          (Core)                              (Desktop)
```

`PlayerInput` is a small immutable `record struct` carrying a movement direction,
an aim direction, and two flags (firing, confirm). Passing *intent as data* is the
seam that decouples the rules from the input API and makes them testable.

## 4. Object model

### Entities (`Core/Entities`)

```
Entity (abstract)                     IDamageable (interface)
  ├─ Position, Radius, IsActive         ├─ Health / MaxHealth / IsAlive
  └─ Update(dt), ClampToArena()         └─ TakeDamage(), Heal()

Player            : Entity, IDamageable
Projectile        : Entity
PowerUp           : Entity
Enemy (abstract)  : Entity, IDamageable
   ├─ ChaserEnemy   (cross-product steering)
   ├─ BruteEnemy    (straight-line charge)
   └─ SniperEnemy   (dot-product field-of-view beam)
```

- **Abstraction** — `Entity` captures what every object has in common (a circular
  body and a per-frame update); `IDamageable` captures "has health and can be hurt".
- **Inheritance & polymorphism** — the three enemies inherit shared stats/health
  from `Enemy` and override one method, `Think(player, dt)`, to express their
  behaviour. `GameWorld` calls `Think` on every enemy without knowing its type.
- **Encapsulation** — health, cooldowns and power-up timers are private; they can
  only change through methods (`TakeDamage`, `Heal`, `ApplyPowerUp`, `Update`) that
  enforce the rules (clamping to `[0, MaxHealth]`, refusing negative amounts, etc.).

### Systems (`Core/Systems`)

Each system has a single responsibility:

| System | Responsibility |
| --- | --- |
| `EnemySpawner` | *when* and *where* enemies appear, and *which* type |
| `IEnemyFactory` / `EnemyFactory` | *how* an enemy of a given type is built and stat-scaled |
| `DifficultyManager` | turns elapsed time into a difficulty multiplier and spawn interval |
| `CollisionSystem` | pure circle-overlap geometry |
| `ScoreManager` | score and high-score bookkeeping |
| `GameWorld` | orchestrates all of the above and owns the `GameState` machine |

### State machine

`GameState` is an enum: `Menu → Playing → GameOver → Playing …`. `GameWorld.Update`
dispatches on it, so the start screen, the run, and the game-over screen are all
driven by the same entry point.

## 5. How the design satisfies SOLID

- **S — Single Responsibility.** Spawning, difficulty, scoring, collision and
  orchestration are separate classes; rendering is a separate project entirely.
- **O — Open/Closed.** Adding a fourth enemy is a new `Enemy` subclass plus a
  branch in the factory; nothing else changes. Adding a power-up is a new enum
  value plus its effect.
- **L — Liskov Substitution.** Any `Enemy` can be used wherever an `Enemy` is
  expected; `GameWorld` never checks concrete types except for the optional sniper
  aim-laser *visual*.
- **I — Interface Segregation.** `IDamageable` is a tiny, focused contract;
  `IEnemyFactory` exposes a single method.
- **D — Dependency Inversion.** `GameWorld` and `EnemySpawner` depend on the
  `IEnemyFactory` abstraction, and the `Random` source is injected — which is
  exactly what lets tests run deterministically.

## 6. Data-structure choices

- **`List<T>`** holds enemies, projectiles and power-ups. The collections are
  iterated in full every frame and grow/shrink constantly; a `List<T>` gives O(1)
  amortised append and cache-friendly iteration. Dead objects are flagged
  (`IsActive = false`) during iteration and swept afterwards with `RemoveAll`,
  which avoids mutating a collection while enumerating it.
- **`enum`** for `GameState`, `EnemyKind` and `PowerUpType` — a closed set of
  named options that the compiler can check, instead of strings or ints.
- **`record struct`** for `PlayerInput` and `EnemyAttack` — small, immutable,
  value-semantic data with no identity, so a struct is the natural fit.
- **`GameSettings`** centralises every tunable number, so balancing the game (or
  constructing a deterministic one in a test) never means hunting for magic values.

## 7. Rendering without the content pipeline

A deliberate choice was to depend on **no MonoGame content** (`.mgcb`/`.xnb`):

- `Rendering/Primitives.cs` builds a 1×1 white pixel and a soft circle texture in
  code at load time and draws every shape by tinting/scaling them.
- `Rendering/PixelFont.cs` is a hand-authored 5×7 bitmap font drawn from that same
  pixel, so all UI text needs no font asset.

The benefit: the game builds and runs anywhere the framework NuGet package
restores, with `dotnet run` and nothing else — no content build step to fail.

## 8. Exception handling

- **Core** validates aggressively at its boundaries: constructors and mutators
  throw `ArgumentOutOfRangeException` / `ArgumentException` / `ArgumentNullException`
  for impossible inputs (negative health, zero lives, `min > max`, null factory).
  This fails fast and is asserted by tests.
- **Desktop** is defensive at runtime: `GameWorld.Update` is wrapped in a
  `try/catch` so a logic error logs and the window keeps running, and `Program.cs`
  wraps the whole game in a `try/catch` that reports a clean message instead of an
  unhandled stack trace.

## 9. Where each mathematical concept lives

| Concept | Location |
| --- | --- |
| Distance | `MathUtils.Distance`, used by `CollisionSystem`, `SniperEnemy` |
| Direction/vectors | `MathUtils.Direction/Normalize`, used by movement & projectiles |
| Algebra | `DifficultyManager`, `EnemyFactory`, `ScoreManager` |
| Dot product | `MathUtils.Dot`, used by `SniperEnemy` field-of-view |
| Cross product | `MathUtils.Cross`, used by `ChaserEnemy`/`SniperEnemy` steering |
| Lerp | `MathUtils.Lerp`, used by HUD/health-bar/flash/fade in `ArenaGame` |
