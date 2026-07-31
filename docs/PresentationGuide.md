# Arena Defender — Presentation & Study Guide

This is a plain-language guide to your own project so you can explain and defend
every part in the presentation. It answers the exact questions from the brief.
Read it, then say the ideas in your own words — that matters more than memorising.

---

## 1. The project in one paragraph

Arena Defender is a 2D survival game. You control a circle (the defender) in the
middle of an arena. Enemies keep spawning from the edges and come after you. You
aim with the mouse and shoot them for points, grab power-ups they drop, and try
to survive as long as possible while the game slowly gets harder. If you lose all
your health you lose a life; lose all your lives and it's game over.

I split the project into **three parts**:
1. **Core** – all the rules and maths of the game (a class library).
2. **Desktop** – the MonoGame part that opens the window, reads the keyboard/mouse
   and draws everything.
3. **UnitTests** – automated tests that check the rules are correct.

---

## 2. Answers to the presentation questions

### "How does your UI communicate with your game logic?"
Every frame, the Desktop part reads the keyboard and mouse and packs it into one
small object called `PlayerInput` (a move direction, an aim direction, and two
true/false flags for *firing* and *confirm*). It hands that to
`GameWorld.Update(input, deltaTime)`. The `GameWorld` updates everything and the
Desktop part then **reads back** the world's state (player, enemies, score, …) and
draws it. So the communication is one-way in, one-way out: *input goes in, state
comes out.*

### "Did you separate game logic from UI logic? If yes, how?"
Yes. All the logic lives in the **Core** project, and **Core does not reference
MonoGame at all.** The Desktop project references Core, not the other way round.
That physical separation is the proof: the rules literally can't touch the screen
or the keyboard, because they don't even know MonoGame exists. It's also why I can
test the whole game without opening a window (see the tests).

### "How did you apply encapsulation in your classes?"
Encapsulation = hiding the inside of an object and only letting people change it
through safe methods. Examples:
- In `Player`, `Health` has a **private setter**. Nobody can write
  `player.Health = -999`. They must call `TakeDamage()` or `Heal()`, which keep
  health between 0 and the maximum.
- Power-up timers (`_shieldTimer`, etc.) are **private fields**. The outside world
  only sees read-only properties like `HasShield`.
- The lists of enemies/projectiles are exposed as `IReadOnlyList`, so outside code
  can look at them but can't secretly add or remove items.

### "Did you use inheritance or interfaces anywhere?"
Both.
- **Inheritance:** `Entity` is a base class for everything in the arena (it has a
  position, a radius and an update method). `Player`, `Enemy`, `Projectile` and
  `PowerUp` all inherit from it. `Enemy` is itself a base class, and
  `ChaserEnemy`, `BruteEnemy` and `SniperEnemy` inherit from it and each override
  the `Think()` method to behave differently. That's polymorphism: `GameWorld`
  calls `Think()` on every enemy without caring which type it is.
- **Interfaces:** `IDamageable` (has `Health`, `TakeDamage`, `Heal`) is implemented
  by both `Player` and `Enemy`. `IEnemyFactory` is the interface my enemy factory
  implements, which lets the tests swap in their own version.

### "Demonstrate the use of Abstraction."
Abstraction = describing the important idea while ignoring the details. My
`Entity` base class is abstraction: everything in the arena is "a circle at a
position that updates each frame", so I wrote that once in `Entity` and every
object shares it. The collision code then treats *everything* as a circle and
doesn't care whether it's a player or a bullet. `IDamageable` is another
abstraction: "a thing with health that can be hurt".

### "Why did you choose the Data Structure(s) that you used?"
I used **`List<T>`** for enemies, projectiles and power-ups because:
- The number of them changes constantly (spawning and dying), and a `List` grows
  and shrinks easily.
- I loop over all of them every frame, and a `List` is fast and simple to loop.
- I remove "dead" objects **after** the loop with `RemoveAll`, so I never change
  the list while I'm looping over it (which would crash).
An array would be a bad choice because its size is fixed; a dictionary would be
pointless because I don't look things up by a key — I just process all of them.

### "Where did you use an Enum, and what problem did it solve?"
Three places:
- `GameState` (`Menu`, `Playing`, `GameOver`) – runs the screen flow. Without it
  I'd have messy true/false flags like `isPlaying`, `isGameOver`.
- `EnemyKind` (`Chaser`, `Brute`, `Sniper`) – so the factory, scoring and renderer
  all name the type safely instead of using numbers like 0/1/2.
- `PowerUpType` (`Health`, `Shield`, `RapidFire`, `SpeedBoost`).
The problem enums solve: a fixed set of named options the **compiler checks**, so
I can't accidentally use an invalid value.

### "Which functions did you test, and why those?"
I tested the **logic and maths**, not the drawing (you can't meaningfully unit-test
pixels). Specifically:
- The maths (`Distance`, `Dot`, `Cross`, `Lerp`, `Normalize`) — because everything
  else is built on them, so a bug there would break the whole game.
- `Player` health/damage/heal/power-ups — the core of staying alive.
- Each enemy's behaviour — that the chaser turns toward the player, the sniper only
  fires when the player is in front, etc.
- Difficulty scaling, scoring, collision, and spawning — the rules that decide the
  game's fairness.
- The whole `GameWorld` together (integration) — starting a run, firing, spawning,
  scoring, game over and restart.
I chose these because they're the parts where a mistake changes whether the game
is *correct* — exactly what tests are for.

### "Can you show an example of a failing test and how you fixed it?"
Yes — this really happened. My chaser test checks that the chaser turns to face
the player. In my first version I put the player **exactly behind** the enemy. The
test failed, because when two directions point exactly opposite, the **cross
product is zero**, so the enemy couldn't tell whether to turn left or right and it
just stood there. I fixed the test by placing the player slightly *off to the
side* (which is what happens in a real game anyway), and the chaser then turned
correctly. It taught me an edge case in the cross-product maths.

### "How could you implement leaderboards?"
Right now I keep a `HighScore` in memory in `ScoreManager` for the current
session. To make a real leaderboard I would:
1. When the game ends, save the player's name and score to a file (for example a
   small JSON or CSV file on disk), appending to the existing list.
2. On the menu, load that file, sort the scores from highest to lowest, and show
   the top 10.
Because scoring already lives in one place (`ScoreManager`) and is separate from
the UI, I'd only add a tiny "save/load" class and a screen — nothing else changes.
For an online leaderboard I'd send the score to a small web API instead of a file.

---

## 3. How to explain the maths (the 8-point part)

Keep it simple and point at the mechanic:

- **Distance** — "I measure the straight-line distance between two circle centres
  to check collisions and ranges." (`MathUtils.Distance`)
- **Direction / vectors** — "To move an enemy toward the player I subtract
  positions and normalise it to get a direction, then multiply by speed."
- **Algebra** — "Difficulty, spawn rate and score are just formulas of time, e.g.
  difficulty = 1 + rate × seconds, capped at a maximum."
- **Dot product** — "The sniper only fires when the player is in front of it. I
  take the dot product of the sniper's facing and the direction to the player; if
  it's bigger than the cosine of the cone angle, the player is inside its view."
- **Cross product** — "The chaser decides whether to turn left or right by the
  **sign** of the cross product of its facing and the direction to the player."
- **Lerp (3 places)** — "The health bar, the red damage flash, and the game-over
  fade all slide smoothly using linear interpolation instead of snapping."

If asked "why not just use the built-in `Vector2` methods?" say: *"I wrote the
maths myself in `MathUtils` so I understand exactly what each one does, and so I
could unit-test them."*

---

## 4. A 2-minute demo script (for the video / live demo)

1. Start the game → show the **menu** with the title and "press enter".
2. Press Enter → move with WASD, aim with the mouse, shoot a few enemies.
3. Point out the **three enemy colours** and how they behave differently (the
   orange chasers curve toward you, the red brutes are slow and tanky, the purple
   snipers hang back and shoot a beam).
4. Grab a **power-up** and mention which one (e.g. the shield ring appears).
5. Show the **HUD**: health bar, lives, score, timer, danger level.
6. Let yourself die → show the **game-over screen** and final score.
7. Press Enter → show it **restarts**.
8. Switch to the code: open `GameWorld.cs`, `Player.cs`, one enemy, and
   `MathUtils.cs`, and open the test file to run the tests live
   (`dotnet test`).

---

## 5. Likely follow-up questions and short answers

- **"What happens if two enemies touch you at once?"** Both deal their damage that
  frame; each is a separate collision check.
- **"Why do melee enemies die when they touch you?"** Design choice: contact is a
  punishment (you lose health and get no points), which pushes the player to shoot
  from a distance.
- **"How does difficulty increase?"** Over time the difficulty multiplier rises
  (tougher enemies) and the spawn interval shrinks (more enemies), both clamped so
  it never becomes unfair.
- **"Why three projects?"** So the rules are separate from the graphics and can be
  tested on their own. It's the same shape as a real production codebase.
- **"What is `deltaTime`?"** The seconds since the last frame. I multiply movement
  by it so the game runs at the same speed regardless of frame rate.
