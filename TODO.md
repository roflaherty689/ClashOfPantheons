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

### 1. Integrate player gold, workers, and independent unit production

**Outcome:** The player can make the prototype's core economy-versus-pressure decision with gold.

**Acceptance criteria:**

- [ ] Player gold is the authoritative currency for worker purchases, all five production types, and upgrades.
- [ ] Buying a worker uses the existing worker limit and updates visible gold/worker state.
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

**Progress:** `UnitData` now owns a serialized spawn interval, and `GameManager` has selectable legacy-global and independent-per-role test patterns. The five initial role cadences are configured and user-verified in Play Mode on 2026-07-16. Purchase/unlock state, star tiers, UI, and tests remain.

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

**Status:** Not started

### 3. Complete match timing, result resolution, and restart

**Outcome:** Every match reaches a deterministic result and can be replayed without Editor intervention.

**Acceptance criteria:**

- [ ] Match duration is configured around the five-minute target.
- [ ] Stronghold destruction ends the match immediately.
- [ ] Timeout compares stronghold health, then lower total value of lost units when health is equal.
- [ ] Unit losses and their approved values are tracked per side.
- [ ] Lost value uses production-slot purchase cost multiplied by the number of that unit type destroyed.
- [ ] Exact equality after both timeout comparisons produces a draw with no winner or loser.
- [ ] Result UI identifies the winner and resolution reason.
- [ ] Restart resets time, gold, workers, production, upgrades, units, AI, strongholds, and UI.
- [ ] Base-destruction, health-tiebreak, value-tiebreak, and exact-equality behavior have tests or explicit verification cases.
- [ ] Compilation and full Play Mode result/restart paths are verified.

**Relevant systems:** `GameManager`, `Base`, `BaseUnit`, economy/production, result UI

**Dependencies:** Tasks 1–2

**Status:** Not started

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

**Status:** Not started

---

## P2 — Important after the critical loop

- Define and tune the exact five-role counter model and three-star stat curves after functional production enables representative playtests.
- Add basic onboarding for workers, production, upgrades, timeout rules, and restart.

---

## P3 — Later

_Deferred scope is tracked in `ROADMAP.md` and `DEC-005`._

---

## Blocked

_No items currently blocked by external state._

---

## Discovered follow-up work

- Tune initial purchase costs, recurring production cadences, role matchups, and stat values through representative playtests.

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
