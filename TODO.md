# TODO

## How to use this file

- Keep immediate work here and long-term outcomes in `ROADMAP.md`.
- Complete items only after their acceptance criteria and applicable validation are satisfied.
- Preserve manual Unity setup and Play Mode work as real task scope.

---

## P0 — Blockers

_No confirmed implementation blocker._

---

## P1 — Critical next work

Tasks are ordered by dependency and prototype value.

### 0. Validate the project-owned C# correctness and maintainability pass

**Outcome:** The cleaned combat, spawning, worker, resource, health, and Editor-tooling foundations are confirmed safe before further prototype integration builds on them.

**Acceptance criteria:**

- [x] All project-owned C# receives a coordinated architecture, correctness, lifecycle, naming, and maintainability audit.
- [x] Runtime and Editor assemblies compile externally with zero warnings and errors against the current Unity references; the stale generated entry for the deleted script was excluded only for this check.
- [x] Dead `ProductionSlot` source and its unreferenced `.meta` file are removed together.
- [x] Unity imports the changes with no missing-script or serialization errors and refreshes its generated project files.
- [x] The cleanup pass receives a representative Play Mode smoke check covering the active spawning, combat, base, and worker loop.
- [ ] Projectile attacks are verified against destroyed targets and with travel times over two seconds.
- [ ] Worker slot contention, disable-during-mining cleanup, failed purchases, and the worker cap are verified.
- [ ] The duplicate/stale serialized fields on unit prefabs are cleaned by opening and resaving them in Unity; the non-animated archer retains a valid `visualTransform`.
- [x] The stray `GoldVein` component on `Tilemap_Water` is removed in Unity and the scene contains only the intended left and right veins.

**Progress:** The code audit, corrections, refactors, dead-source removal, static reference checks, and external runtime/Editor compilation are complete. The user verified the current behavior in Unity on 2026-07-16, and the scene now contains only the two intended gold veins. Targeted projectile/worker edge cases and stale prefab serialization cleanup remain.

**Status:** Partially implemented

### 1. Integrate player gold, workers, and independent unit production

**Outcome:** The player can make the prototype's core economy-versus-pressure decision with gold.

**Acceptance criteria:**

- [ ] Player gold is the authoritative currency for worker purchases, all five production types, and upgrades.
- [x] Buying a worker uses the existing worker limit and updates visible gold/worker state.
- [ ] Every unit type follows the approved independent recurring-production contract without a shared FIFO queue.
- [ ] A role begins locked; its first purchase unlocks continuous one-star production and its next two purchases upgrade only future spawns to two and three stars.
- [ ] Favourable matchups apply a 1.2× damage multiplier.
- [ ] Star tiers multiply every configured unit stat except purchase cost by 1×/1.5×/2× and affect future spawns only.
- [x] Each `UnitData` asset owns that role's purchase cost and recurring production cadence; the global spawn interval no longer defines all roles.
- [ ] Production stops correctly when the match ends and resets correctly for a new match.
- [ ] Costs, failed purchases, and production state are visibly communicated.
- [ ] Deterministic economy and production rules have focused automated tests where practical.
- [ ] Compilation and relevant Play Mode paths are verified.

**Relevant systems:** `WorkerManager`, `GameManager`, `UnitData`, faction data, battle UI, scene wiring

**Dependencies:** `DEC-004` and `DEC-007`; initial values require balance tuning during implementation

**Progress:** `UnitData` owns each role's cost and cadence. Player production purchasing is now implemented in source: all roles begin locked, a successful purchase atomically spends player gold, the first purchase starts that role's fresh recurring timer, the next two purchases snapshot 1.5×/2× stats onto future units, and a fourth purchase is rejected. The five production cards now show live locked/producing state, tier, unlock/upgrade/max actions, affordability, and greyed locked art; the selected-role panel mirrors the chosen role. The user Play Mode verified clickable production controls, successful melee unlock/upgrade processing through the three-purchase cap, and live 0/3 → 1/3 → 2/3 → 3/3 tier-counter updates on 2026-07-17. Both teams start locked, so the enemy intentionally produces no units until Task 2 supplies AI purchasing. Runtime and Editor assemblies compile externally with zero warnings and errors. Recurring spawn cadence, stat scaling, insufficient-funds behavior, all five roles, and restart reset still require targeted Play Mode verification; focused automated tests remain absent.

**Status:** Partially implemented

### 2. Implement the single-player AI economy and production opponent

**Outcome:** The opposing side makes strategic worker, production, composition, and upgrade decisions under the same rules as the player.

**Acceptance criteria:**

- [ ] AI purchases consume its gold and respect the same costs, limits, and production rules.
- [ ] AI can buy workers, use all required roles, and buy star upgrades.
- [ ] A simple documented strategy creates credible pressure within a five-minute match.
- [ ] AI stops on match end and resets on restart.
- [ ] AI decisions are observable enough to debug and tune.
- [ ] Compilation and representative Play Mode matches are verified.

**Relevant systems:** future AI policy, economy/production interfaces, match state

**Dependencies:** Task 1

**Progress:** Shared team/role production state and purchase rules now exist, but no AI policy or AI economy purchasing has been implemented. Per the user-approved temporary behavior, every enemy role starts locked and remains inactive until this task is completed; there is no automatic enemy-production bootstrap in the active per-role mode.

**Status:** Not started

### 3. Complete match timing, result resolution, and restart

**Outcome:** Every match reaches a deterministic result and can be replayed without Editor intervention.

**Acceptance criteria:**

- [x] Match duration is configured around the five-minute target.
- [x] Stronghold destruction ends the match immediately.
- [x] Timeout compares stronghold health, then lower total value of lost units when health is equal.
- [x] Unit losses and their approved values are tracked per side.
- [x] Lost value uses production-slot purchase cost multiplied by the number of that unit type destroyed.
- [x] Exact equality after both timeout comparisons produces a draw with no winner or loser.
- [x] Result UI identifies the winner and resolution reason.
- [ ] Restart resets time, gold, workers, production, upgrades, units, AI, strongholds, and UI.
- [ ] Base-destruction, health-tiebreak, value-tiebreak, and exact-equality behavior have tests or explicit verification cases.
- [ ] Compilation and full Play Mode result/restart paths are verified.

**Relevant systems:** `GameManager`, `Base`, `BaseUnit`, economy/production, result UI

**Dependencies:** Tasks 1–2

**Progress:** `GameManager` now owns a configurable 300-second countdown, typed match-end reasons, per-team/per-role death counts, total purchase-value losses, deterministic health/value/draw timeout resolution, and active-scene reload restart. Lethal unit damage records the destroyed unit's production-slot cost exactly once. The battle HUD shows live remaining time, player-relative victory/defeat/draw state, the resolution reason, loss totals for value tiebreaks, and a functional restart action in source. Runtime and Editor assemblies compile externally with zero warnings and errors. The user Play Mode verified countdown expiry and correct lost-gold resolution with a shortened 20-second match on 2026-07-17. Targeted coverage of base destruction, the health tiebreak, exact draw, overlay input, and complete reset behavior remains required, so this task is not complete.

**Status:** Partially implemented

### 4. Replace the static HUD with a functional prototype HUD and validate the loop

**Outcome:** A new player can read and complete the entire prototype loop against AI.

**Acceptance criteria:**

- [ ] HUD shows live gold, workers, five independent production states, star tiers, timer, both stronghold health values, and results.
- [ ] Favour, essence, and shared FIFO queue presentation are removed or clearly excluded from the functional prototype UI.
- [ ] Player purchase and upgrade controls provide success, failure, affordability, and cooldown/cadence feedback.
- [ ] Critical state does not rely on red/blue colour alone and text is readable at the chosen prototype resolution.
- [ ] At least one complete economy → production → combat → result → restart match is verified in Play Mode.
- [ ] Playtesting confirms at least one meaningful economy-versus-pressure choice and identifies required balance follow-up separately.

**Relevant systems:** battle HUD, UI bindings, safe area, all critical-loop systems

**Dependencies:** Tasks 1–3

**Progress:** The editor-authored battle HUD has been redesigned around the accepted Tiny Swords direction. Gold, workers, both strongholds, timer, results, restart, and all five independent production cards are now bound in source. Production cards display live locked/producing state, tier, affordability, greyed locked art, and unlock/upgrade/max actions; the selected-role panel follows the last production card hovered over, clicked, tapped, or purchased from and remains pinned after the pointer leaves. Its icon mirrors the selected card's unit artwork, the redundant production/independent row has been removed, and the role-icon container was doubled from 54×54 to 108×108. The user verified the complete selected-role interaction and visual update in Play Mode on 2026-07-17. Worker, stronghold, countdown-expiry, and lost-gold paths were previously user-verified. Broader production purchasing and the remaining result/restart paths still need targeted Play Mode verification. The generated HUD requires multi-resolution Unity inspection and complete-loop Play Mode verification. The attempted three-part Tiny Swords stronghold health-bar frame did not produce an acceptable result; its stretched middle segment is temporarily commented out pending a later UI pass.

**Status:** Partially implemented

---

## P2 — Important after the critical loop

- Define and tune the exact five-role counter model and three-star stat curves after functional production enables representative playtests.
- Add basic onboarding for workers, production, upgrades, timeout rules, and restart.
- [x] Black, blue, purple, red, and yellow melee/archer variants show their matching idle, run, and attack animations in Play Mode; the user verified every colour on 2026-07-17 after loop and transition settings were matched to black.
- [ ] Verify the shared cavalry, siege, and mythic units still spawn correctly with each team-colour faction. Confirm that switching both team factions does not produce missing references, controller warnings, or unintended mechanical differences.

---

## P3 — Later

_Deferred scope is tracked in `ROADMAP.md` and `DEC-005`._

---

## Blocked

_No items currently blocked by external state._

---

## Discovered follow-up work

- Revisit the editor-generated stronghold health-bar frame in `ClashBattleUIBuilder.CreateHealthBarFrame`. Determine the correct use of the Tiny Swords bar assets or replace the frame treatment; the final frame must fully contain the health fill without distortion or overflow for both teams at QHD 2560×1440 and representative narrower/wider aspect ratios. The current stretched middle segment is intentionally commented out until this is addressed.
- Tune initial purchase costs, recurring production cadences, role matchups, and stat values through representative playtests.
- Resolve projectile targeting semantics before changing combat behavior: current projectiles aim at the launch position and may miss moving targets; choose deliberate misses, target leading, or live-target tracking, then document and verify the approved rule.
- Profile a representative 60-versus-60 battle before changing target-query or unit-count architecture; if measurements confirm pressure, replace allocating physics queries and scene-wide unit scans with proportionate non-allocating queries and lifecycle-owned counts.

---

## Completed

### Resolve the independent production contract

**Completed:** 2026-07-16

**Evidence:** User-approved answers are recorded in accepted `DEC-007`: first purchase unlocks one continuously producing role track; later purchases advance future spawns to two and three stars; fielded units are unchanged. `DEC-003`, `DEC-004`, and `DEC-008` also record the resolved timeout valuation, initial multipliers, and PC-first priority.

This completes the design dependency only; production and upgrades are not implemented.

### Define and reconcile the current game direction

**Completed:** 2026-07-16

**Evidence:** `GAME_DESIGN.md` defines the pitch, fantasy, core loop, pillars, scope, and success criteria; `DEC-002` through `DEC-006` record direct user-approved constraints; the Prototype milestone is game-specific.

This documentation completion does not claim that the gameplay loop is implemented or validated.
