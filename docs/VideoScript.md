# Arena Defender — Video Walkthrough Script (10–15 minutes)

This is a talking script for the recording. Each section says **[SHOW]** (what to
have on screen) and **[SAY]** (what to say, in your own words — don't read it
robotically). The times are a guide; the whole thing lands around **12 minutes**.

Before you start recording:
- Have the game built and ready to run.
- Open the solution in VS Code with these files ready in tabs: `GameWorld.cs`,
  `Player.cs`, `Enemy.cs`, `SniperEnemy.cs`, `ChaserEnemy.cs`, `MathUtils.cs`,
  `EnemyFactory.cs`, and one test file (`EnemyTests.cs`).
- Have a terminal open at the project root.

---

## 0. Intro — 30 seconds

**[SHOW]** Your face or just the title screen of the game.

**[SAY]**
> "Hi, I'm Gisele. This is my summative project, **Arena Defender** — a 2D survival
> game I built from scratch in C# with MonoGame. In this video I'll first play it,
> then walk through the code to show how it applies object-oriented programming,
> SOLID principles, the maths, and unit testing. Let's start with the gameplay."

---

## 1. Gameplay demonstration — 2.5 minutes

**[SHOW]** Run the game live:
```bash
dotnet run --project src/ArenaDefender.Desktop
```
(If your machine needs it, prefix with `DOTNET_ROLL_FORWARD=Major`.)

**[SAY]** while playing — hit each of these points on screen:
> - "This is the **start screen** — the game name and 'press enter to play'." *(Press Enter.)*
> - "I move with **WASD**, aim with the **mouse**, and shoot with **left-click or space**."
> - "Notice the **three enemy types**: the **orange chasers** are fast and curve
>   toward me, the **red brutes** are slow but tough, and the **purple snipers**
>   hang back and only fire a beam when I drift into their line of sight."
> - "Top-left is my **health bar** and **lives**; top-right is my **score**; the
>   centre shows the **time survived** and the **danger level**, which rises as the
>   game gets harder — more enemies, tougher enemies."
> - "When I shoot enemies I earn points, and they sometimes drop **power-ups** —
>   here's the green **health** pickup, and there's a **shield**." *(Grab one.)*
> - "If I let enemies touch me I lose health. Let me take some hits…" *(Let yourself
>   die.)* "…that costs a life, and when my lives run out I get the **game-over
>   screen** with my final score. Pressing Enter **restarts**."

**Tip:** keep moving in circles while shooting — it shows the game off far better
than standing still.

---

## 2. The big idea: logic separated from UI — 1.5 minutes

**[SHOW]** The Solution Explorer / folder tree, pointing at the three projects.

**[SAY]**
> "Now the code. The most important decision in the whole project is that it's split
> into **three projects**:
> - **`ArenaDefender.Core`** — all the game rules and maths. It has **no reference
>   to MonoGame** at all.
> - **`ArenaDefender.Desktop`** — the MonoGame part: the window, input, and drawing.
> - **`ArenaDefender.UnitTests`** — the tests, which reference only Core.
>
> Because Core doesn't know MonoGame exists, the rules **can't** touch the screen or
> the keyboard. That's how I **separated game logic from UI logic** — not just by
> being tidy, but physically, at the project level. And it's exactly what lets me
> unit-test the whole game with no window open."

**This answers rubric questions:** *"Did you separate game logic from UI logic?"*

---

## 3. How the UI talks to the logic — 1 minute

**[SHOW]** `PlayerInput.cs`, then `GameWorld.Update(...)` in `GameWorld.cs`.

**[SAY]**
> "So how do they communicate? Every frame the desktop layer reads the keyboard and
> mouse and packs them into this one small object, **`PlayerInput`** — a move
> direction, an aim direction, and two flags for firing and confirm. It hands that
> to **`GameWorld.Update(input, deltaTime)`**. The world updates everything, and then
> the desktop layer **reads back** the state — player, enemies, score — and draws it.
> So it's one-way in, one-way out: **input goes in as data, state comes out.**"

**This answers:** *"How does your UI communicate with your game logic?"*

---

## 4. OOP: abstraction, inheritance, encapsulation — 2.5 minutes

**[SHOW]** `Enemy.cs` (base class + the three subclasses in the tree), then
`Player.cs`.

**[SAY] — abstraction:**
> "Everything in the arena is a circle at a position that updates each frame, so I
> wrote that **once** in an abstract base class called **`Entity`**. The player,
> enemies, projectiles and power-ups all inherit from it. That's **abstraction** —
> the collision code treats everything as a circle without caring what it really is.
> I have a second abstraction, the **`IDamageable`** interface — 'a thing with health
> that can be hurt' — implemented by both the player and enemies."

**[SAY] — inheritance & polymorphism:** *(show the three enemies)*
> "**`Enemy`** is itself an abstract base with shared health and stats. The three
> enemy types **inherit** from it and each **override one method, `Think`**, to
> behave differently — the chaser steers, the brute charges, the sniper aims and
> fires. In the game loop I just call `Think` on every enemy without checking its
> type — that's **polymorphism**. It's also why adding a fourth enemy wouldn't
> require changing the game loop."

**[SAY] — encapsulation:** *(show `Player.Health` and `TakeDamage`/`Heal`)*
> "For **encapsulation**: the player's `Health` has a **private setter** — nothing
> outside can just set it to a silly value. You have to go through `TakeDamage` or
> `Heal`, which keep it between zero and the maximum and reject negative numbers.
> The power-up timers are private too; the outside world only sees read-only
> properties like `HasShield`."

**This answers:** *"encapsulation", "inheritance or interfaces", "abstraction".*

---

## 5. SOLID, data structures and enums — 1.5 minutes

**[SHOW]** `EnemyFactory.cs` + `IEnemyFactory.cs`, then `GameWorld.cs` fields (the
`List<>`s), then the enum files.

**[SAY] — SOLID (keep it brief):**
> "On **SOLID**: each system has a **single responsibility** — spawning, difficulty,
> scoring and collision are separate classes. `GameWorld` depends on the
> **`IEnemyFactory` interface**, not a concrete factory — that's **dependency
> inversion**, and it's what lets my tests inject their own. Adding a new enemy or
> power-up **extends** the code without modifying the game loop — **open/closed**."

**[SAY] — data structures:**
> "For **data structures** I used **`List<T>`** for enemies, projectiles and
> power-ups, because their number changes constantly and I loop over all of them
> every frame — a list grows and shrinks easily and iterates fast. I remove dead
> objects **after** the loop with `RemoveAll` so I never modify the list while
> looping. An array would be wrong because its size is fixed; a dictionary would be
> pointless because I don't look things up by a key."

**[SAY] — enums:**
> "And I used **enums** — `GameState`, `EnemyKind` and `PowerUpType` — for fixed sets
> of named options the compiler checks, instead of loose strings or magic numbers.
> `GameState` for example drives the whole menu → playing → game-over flow."

**This answers:** *"Why did you choose the Data Structure(s)?", "Where did you use
an Enum?", "inheritance or interfaces".*

---

## 6. The mathematics — 2 minutes  *(worth the most marks — don't rush)*

**[SHOW]** `MathUtils.cs`, then jump to `SniperEnemy.Think` and `ChaserEnemy.Think`.

**[SAY]**
> "The maths all lives in **`MathUtils`**, which I wrote by hand so I understand each
> piece and can test it.
> - **Distance** — straight-line distance between two circle centres, used for
>   collisions and ranges.
> - **Direction and vectors** — to move an enemy toward the player I subtract
>   positions, **normalise** to a direction, and multiply by speed.
> - **Algebra** — difficulty, spawn rate and score are just formulas of time; for
>   example difficulty is `1 + rate × seconds`, clamped to a maximum.
> - **Dot product** — *(show `SniperEnemy`)* the sniper only fires when the player is
>   in front of it. For unit vectors the dot product is the cosine of the angle
>   between them, so I check `Dot(facing, directionToPlayer)` against the cosine of
>   its view cone. Move out of the cone and it can't fire.
> - **Cross product** — *(show `ChaserEnemy`)* the chaser decides whether to turn
>   left or right from the **sign** of `Cross(facing, directionToPlayer)`.
> - **Lerp** — I use linear interpolation in several places: the health bar sliding,
>   the red damage flash fading, and the game-over fade — so the UI animates smoothly
>   instead of snapping."

**This answers:** the entire **Mathematics** requirement (the 8-point criterion).

---

## 7. Exception handling — 45 seconds

**[SHOW]** A guard in `Enemy` or `Player` (a `throw new ArgumentOutOfRangeException`),
then the `try/catch` in `Program.cs`.

**[SAY]**
> "For **exception handling** I use two layers. Inside Core I **fail fast** —
> constructors and methods throw on impossible input like negative health or zero
> lives, and my tests check that. At the edge, in `Program.cs` and the update loop,
> I wrap things in **try/catch** so a problem logs a clean message instead of
> crashing the window with a raw stack trace."

**This answers:** the **Exception Handling** criterion.

---

## 8. Unit tests — run them live — 1.5 minutes

**[SHOW]** Run the tests in the terminal:
```bash
dotnet test
```
Wait for the green **Passed! Failed: 0, Passed: 58** line. Then open `EnemyTests.cs`.

**[SAY]**
> "Finally, testing. I have **58 unit tests** in xUnit, and here they all pass. I
> tested the **logic and maths**, not the drawing — you can't meaningfully unit-test
> pixels. So I tested the maths helpers, the player's health and power-ups, each
> enemy's behaviour, difficulty and scoring, collisions, and the whole `GameWorld`
> together."

**[SAY] — the failing-test story (they specifically ask for this):**
> "Here's a test that **failed** at first — the chaser turning test. Originally I put
> the player **exactly behind** the enemy, and it failed, because when two directions
> point exactly opposite the **cross product is zero**, so there's no left-or-right
> sign and the enemy couldn't decide which way to turn. I fixed the test by placing
> the player slightly **to the side**, which is what happens in real play anyway, and
> it passed. It taught me a real edge case in the cross-product maths."

**This answers:** *"Which functions did you test and why?", "Can you show a failing
test and how you fixed it?"*

---

## 9. Leaderboards + close — 45 seconds

**[SHOW]** `ScoreManager.cs`.

**[SAY]**
> "One thing they ask is how I'd add **leaderboards**. Right now `ScoreManager` keeps
> a high score in memory. To make a real leaderboard I'd, on game over, save the
> player's name and score to a small file — JSON or CSV — and on the menu load it,
> sort highest-first, and show the top ten. Because scoring already lives in one
> place and is separate from the UI, I'd only add a small save/load class and a
> screen — nothing else would change. For an online board I'd send the score to a
> small web API instead of a file."
>
> "That's Arena Defender — thanks for watching. All the code, the tests, and the
> design report are in the GitHub repository linked in my submission."

**This answers:** *"How could you implement leaderboards?"*

---

## Rubric coverage checklist (make sure you hit each on camera)

- [ ] Gameplay: player, movement, health, lives, score, 3 enemy types, power-ups, difficulty, game over → **§1**
- [ ] Separated game logic from UI → **§2**
- [ ] UI ↔ logic communication → **§3**
- [ ] Abstraction, inheritance/polymorphism, encapsulation → **§4**
- [ ] SOLID, data-structure choice, enums, interfaces → **§5**
- [ ] Distance, vectors, algebra, **dot**, **cross**, **lerp** → **§6**
- [ ] Exception handling → **§7**
- [ ] Unit tests run + which functions + a failing test fixed → **§8**
- [ ] Leaderboards → **§9**

Keep the total between **10 and 15 minutes**. If you're short on time, the sections
that carry the most marks are **§6 (maths)** and **§8 (tests)** — never rush those.
