# Arena Defender — Testing Strategy

## 1. Goal and philosophy

The aim of the test suite is to prove that the **game logic and mathematics** are
correct, independently of rendering. Because all rules live in
`ArenaDefender.Core` (which has no MonoGame dependency), every rule can be tested
as an ordinary object with ordinary assertions — no graphics device, no window,
no frame loop.

The suite follows the **F.I.R.S.T.** qualities:

- **Fast** — the whole suite runs in ~25 ms.
- **Independent** — each test constructs its own objects; there is no shared state.
- **Repeatable** — anything random is driven by a **seeded `Random`**, so results
  are deterministic every run.
- **Self-validating** — each test asserts a clear pass/fail; no manual inspection.
- **Timely** — tests were written alongside the code and drove several design
  choices (e.g. injecting `Random` and `IEnemyFactory` exists *so that* the world
  can be tested deterministically).

## 2. Tooling

- **xUnit** as the test framework (`[Fact]` for single cases, `[Theory]` +
  `[InlineData]` for parameterised cases).
- Run with:

  ```bash
  dotnet test ArenaDefender.sln
  ```

## 3. What is tested

The suite has **58 test cases** across five files.

### `MathUtilsTests` — the mathematics (12 cases)
The foundation everything else relies on:
- `Distance` returns the Euclidean distance (3-4-5 triangle).
- `Normalize` of the zero vector returns zero **without producing NaN** (an
  important edge case — dividing by a zero length would otherwise poison every
  downstream calculation).
- `Normalize` produces unit length.
- `Dot` is zero for perpendicular vectors, positive for aligned, negative for opposed.
- `Cross` sign indicates turn direction, and is zero for parallel vectors.
- `Lerp` returns the right value across the range **and clamps** out-of-range `t`.
- `Rotate` by 90° maps the X-axis to the Y-axis.
- `Clamp` throws when `min > max`.

### `PlayerTests` — player rules (13 cases)
- Damage reduces health and reports the amount dealt.
- Health never drops below zero; the player is then "not alive".
- Healing never exceeds max health.
- Negative damage/heal is rejected (`ArgumentOutOfRangeException`).
- The **Shield** power-up absorbs all damage.
- **Speed** and **Rapid-Fire** power-ups change effective speed/cooldown.
- Timed power-ups **expire** after their duration.
- The fire cooldown gates shooting until enough time passes.
- Movement is **clamped inside the arena**.
- Lives are consumed on death and the player revives until they run out.
- Constructing a player with zero lives throws.

### `EnemyTests` — enemy behaviour & factory (11 cases)
- The **Chaser** steers until it faces the player (dot-product alignment → ~1).
- The **Brute** advances toward the player.
- The **Sniper fires** when the player is inside its field of view and range.
- The **Sniper does not fire** when the player is behind it (the dot-product gate).
- Taking lethal damage kills an enemy and flags it inactive.
- Invalid enemy stats throw.
- The factory creates the requested `EnemyKind`, **scales health with difficulty**,
  and rejects a difficulty multiplier below 1.

### `SystemsTests` — supporting systems (14 cases)
- **Difficulty** multiplier rises with time and is clamped to its maximum.
- **Spawn interval** shrinks over time but never below the minimum.
- **Score** accumulates from enemy defeats and tracks a high score.
- **Survival** points are awarded per whole second (with fractional carry-over).
- Resetting a run keeps the high score.
- Negative score values throw.
- **Collision**: overlapping circles collide, separated ones don't, and exactly
  touching circles count as a collision (boundary case).
- `Overlap` throws on a null entity.
- The **spawner** doesn't spawn before its interval, does after, and places
  enemies outside the arena edges.

### `GameWorldTests` — full integration (7 cases)
Drives the whole simulation through its public surface — the same `PlayerInput`
the desktop layer uses:
- A new world starts in the **Menu**.
- Confirm from the menu **starts a run** (Playing, full health, zero score).
- Firing **creates a projectile**.
- Enemies **spawn as time passes**.
- Score **rises from survival**.
- Difficulty **rises during play**.
- After a forced **game over**, restarting resets the score (the loop is bounded so
  the test can never hang).

## 4. Notable edge cases deliberately covered

- **Zero-length normalization** (NaN guard) — the most dangerous silent bug in
  vector maths.
- **Lerp interpolant clamping** — `t` outside `[0,1]` must not overshoot.
- **Exactly-touching circles** — the boundary between "collision" and "no collision".
- **Antiparallel steering** — when the player is exactly behind an enemy the cross
  product is zero; the steering code picks a side instead of stalling (guarded in
  `ChaserEnemy`/`SniperEnemy`).
- **Clamp with inverted bounds** — asserts the code throws rather than silently
  returning nonsense.

## 5. Results

```
Passed!  - Failed: 0, Passed: 58, Skipped: 0, Total: 58, Duration: 25 ms
```

## 6. What is intentionally *not* unit-tested

Rendering (`ArenaDefender.Desktop`) is not unit-tested: drawing shapes and text to
a graphics device is inherently visual and is verified by running the game. This
is a deliberate boundary — the logic that *decides* what to draw is in `Core` and
is tested; the code that *performs* the drawing is thin and side-effecting.
